using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class QuadTree : MonoBehaviour
{

    public Vector2 center;
    public float halfDimension;
    public GameObject prefab;
    public float minSize = 0.2f;

    // Arbitrary constant to indicate how many elements can be stored in this quad tree node
    public const int qtNodeCapacity = 4;
    // Axis-aligned bounding box stored as a center with half-dimensions
    // to represent the boundaries of this quad tree
    public AABB boundary;
    // Points in this quad tree node
    private Vector2?[] points;
    private int nextIn;
    
    private QuadTree northWest;
    private QuadTree northEast;
    private QuadTree southWest;
    private QuadTree southEast;
    
    private MeshRenderer renderer;



    
    // Start is called before the first frame update
    void Start()
    {
        nextIn = 0;
        renderer = GetComponent<MeshRenderer>();
        renderer.enabled = true;
        points = new Vector2?[qtNodeCapacity];
        boundary = new AABB(center, halfDimension);
        var transform1 = transform;
        transform1.position = center;
        transform1.localScale = new Vector3(1 * (halfDimension * 2), 1 * (halfDimension * 2), 1);
    }

    // Update is called once per frame
    void OnEnable()
    {
        nextIn = 0;
        renderer = GetComponent<MeshRenderer>();
        renderer.enabled = true;
        points = new Vector2?[qtNodeCapacity];
        boundary = new AABB(center, halfDimension);
        var transform1 = transform;
        transform1.position = center;
        transform1.localScale = new Vector3(1 * (halfDimension * 2), 1 * (halfDimension * 2), 1);
    }
    

    

    public bool Insert(Vector2 p)
    {


        // Ignore objects that do not belong in this quad tree
        if (!boundary.containsPoint(p))
        {
            return false;
        }
        
        if (halfDimension <= minSize)
        {
            return false;
        }
        
        // If there is space in this quad tree and if doesn't have subdivisions, add the object here
        if (nextIn < qtNodeCapacity && northWest == null)
        {
            points[nextIn++] = p;
            return true;
        }
        // Otherwise, subdivide and then add the point to whichever node will accept it
        if (northWest == null)
        {
            Subdivide();
        }
        //We have to add the points/data contained into this quad array to the new quads if we only want 
        //the last node to hold the data 

        if (northWest.Insert(p)) return true;
        if (northEast.Insert(p)) return true;
        if (southWest.Insert(p)) return true;
        if (southEast.Insert(p)) return true;
        
        // Otherwise, the point cannot be inserted for some unknown reason (this should never happen)
        return false;
    }
    
    

    private void Subdivide()
    {
        var newHalfDimensions = halfDimension / 2;
        var northEastCenter = new Vector2(center.x - newHalfDimensions, center.y + newHalfDimensions);
        var northWestCenter = new Vector2(center.x + newHalfDimensions , center.y + newHalfDimensions );
        var southWestCenter = new Vector2(center.x + newHalfDimensions , center.y - newHalfDimensions );
        var southEastCenter = new Vector2(center.x - newHalfDimensions , center.y - newHalfDimensions );

        InstantiateNode(newHalfDimensions, ref northEast, northEastCenter);
        InstantiateNode(newHalfDimensions, ref northWest, northWestCenter);
        InstantiateNode(newHalfDimensions, ref southWest, southWestCenter);
        InstantiateNode(newHalfDimensions, ref southEast, southEastCenter);

        gameObject.SetActive(false);

    }

    private void InstantiateNode(float newHalfDimensions, ref QuadTree quadTree, Vector2 newCenter)
    {
        var go = Instantiate(prefab);
        quadTree = go.GetComponent<QuadTree>();
        quadTree.boundary = new AABB(newCenter, newHalfDimensions);
        quadTree.points = points = new Vector2?[qtNodeCapacity];
        quadTree.center = newCenter;
        quadTree.halfDimension = newHalfDimensions;
        quadTree.minSize = minSize;
    }

    public Vector2[] QueryRange(AABB range)
    {
        List<Vector2> pointsInRange = new List<Vector2>();

        if (!boundary.intersectsAABB(range))
            return pointsInRange.ToArray();
        for (int p = 0; p < points.Length; p++)
        {
            var currentPoint = points[p];
            if (!currentPoint.HasValue) continue;
            if (range.containsPoint((Vector2)currentPoint))
                pointsInRange.Add((Vector2) currentPoint);
        }

        if (northWest == null)
            return pointsInRange.ToArray();

        pointsInRange.AddRange(northWest.QueryRange(range));
        pointsInRange.AddRange(northEast.QueryRange(range));
        pointsInRange.AddRange(southWest.QueryRange(range));
        pointsInRange.AddRange(southEast.QueryRange(range));

        return pointsInRange.ToArray();
    }

    public QuadTree[] DestroyNodesInRange(AABB range)
    {
        var nodes = new List<QuadTree>();

 
        if (northWest == null) return nodes.ToArray();
        nodes.AddRange(northWest.DestroyNodesInRange(range));
        nodes.AddRange(northEast.DestroyNodesInRange(range));
        nodes.AddRange(southWest.DestroyNodesInRange(range));
        nodes.AddRange(southEast.DestroyNodesInRange(range));

        return nodes.ToArray();
    }


    // Axis-aligned bounding box
    public struct AABB
    {
        public Vector2 center;
        public float halfDimension;

        public AABB(Vector2 center, float halfDimension)
        {
            this.center = center;
            this.halfDimension = halfDimension;
        }

        public bool containsPoint(Vector2 point)
        {
            return (point.x >= center.x - halfDimension && point.x <= center.x + halfDimension) &&
                   (point.y >= center.y - halfDimension && point.y <= center.y + halfDimension);
        }

        public bool intersectsAABB(AABB other)
        {
            var otherCenter = other.center;
            return (Mathf.Abs(center.x - otherCenter.x) * 2 < (halfDimension * 2 + other.halfDimension * 2) &&
                    Mathf.Abs(center.y - otherCenter.y) * 2 < (halfDimension * 2 + other.halfDimension * 2));
        }
    }
}
