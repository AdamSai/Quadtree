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
    private Vector2[] points;

    private QuadTree northWest;
    private QuadTree northEast;
    private QuadTree southWest;
    private QuadTree southEast;

    private MeshRenderer renderer;



    
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        renderer.enabled = true;
        points = new Vector2[qtNodeCapacity];
        boundary = new AABB(center, halfDimension);
        var transform1 = transform;
        transform1.position = center;
        transform1.localScale = new Vector3(1 * (halfDimension * 2), 1 * (halfDimension * 2), 1);
    }

    // Update is called once per frame
    void OnEnable()
    {
        renderer = GetComponent<MeshRenderer>();
        renderer.enabled = true;
        points = new Vector2[qtNodeCapacity];
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
        // If there is space in this quad tree and if doesn't have subdivisions, add the object here
        if (points.Length < qtNodeCapacity && northWest == null)
        {

            points.Append(p);
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
        
        var nwGo = Instantiate(prefab);
        northWest = nwGo.GetComponent<QuadTree>();
        var northWestCenter = new Vector2(center.x + newHalfDimensions , center.y + newHalfDimensions );
        northWest.boundary = new AABB(northWestCenter, newHalfDimensions);
        northWest.points = points = new Vector2[qtNodeCapacity];
        northWest.center = northWestCenter;
        northWest.halfDimension = newHalfDimensions;
        
        var neGO = Instantiate(prefab);
        northEast = neGO.GetComponent<QuadTree>();
        var northEastCenter = new Vector2(center.x - newHalfDimensions , center.y + newHalfDimensions );
        northEast.boundary = new AABB(northEastCenter, newHalfDimensions);
        northEast.points = points = new Vector2[qtNodeCapacity];
        northEast.center = northEastCenter;
        northEast.halfDimension = newHalfDimensions;

        var swGO = Instantiate(prefab);
        southWest = swGO.GetComponent<QuadTree>();
        var southWestCenter = new Vector2(center.x + newHalfDimensions , center.y - newHalfDimensions );
        southWest.boundary = new AABB(southWestCenter, newHalfDimensions);
        southWest.points = points = new Vector2[qtNodeCapacity];
        southWest.center = southWestCenter;
        southWest.halfDimension = newHalfDimensions;

        var seGO = Instantiate(prefab);
        southEast = seGO.GetComponent<QuadTree>();
        var southEastCenter = new Vector2(center.x - newHalfDimensions , center.y - newHalfDimensions );
        southEast.boundary = new AABB(southEastCenter, newHalfDimensions);
        southEast.points = points = new Vector2[qtNodeCapacity];
        southEast.center = southEastCenter;
        southEast.halfDimension = newHalfDimensions;

        gameObject.SetActive(false);

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
