using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class PlayerController : MonoBehaviour
{
    private Camera cam;
    public float brushSize = 2f;
    private QuadTree.AABB range;
    public QuadTree root;
    private Collider[] hitColliders;
    public GameObject testPrefab;
    private List<GameObject> activePoints;
    public GameObject edgePrefab;

    // Start is called before the first frame update
    void Start()
    {
        activePoints = new List<GameObject>();
        range = new QuadTree.AABB();
        cam = Camera.main;
        hitColliders = new Collider[1024];
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            DestroyActivePoints();
            var mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 22));
            if (Physics.Raycast(mousePos, Vector3.forward, out RaycastHit hit))
            {
                root.Insert(hit.point);
            }
        }

        if (Input.GetButton("Fire2"))
        {
            DestroyActivePoints();
            var mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 22));
            range.center = mousePos;
            range.halfDimension = brushSize / 2;

            if (Physics.Raycast(mousePos, Vector3.forward, out RaycastHit hit))
            {
                var points = root.QueryRange(range);
                foreach (var point in points)
                {
                    var go = Instantiate(edgePrefab, new Vector3(point.x, point.y, -0.4f), Quaternion.identity);
                    activePoints.Add(go);
                }
            }
        }
    }

    private void DestroyActivePoints()
    {
        if (activePoints.Count <= 0) return;
        //Destroy any active point prefab.
        foreach (var obj in activePoints)
        {
            Destroy(obj);
        }

        activePoints.Clear();
    }


    private static bool CalculateOnCircleEdge(float centerX, float centerY, float xIndex, float yIndex, float radius)
    {
        var sqrdChangeInX = (xIndex - centerX) * (xIndex - centerX);
        var sqrdChangeInY = (yIndex - centerY) * (yIndex - centerY);
        var distance = sqrdChangeInX + sqrdChangeInY;
        return Math.Abs(distance - (radius * radius)) < 0.01f;
    }

    private static bool CalculateInsideCircle(float centerX, float centerY, float xIndex, float yIndex, float radius)
    {
        var sqrdChangeInX = (xIndex - centerX) * (xIndex - centerX);
        var sqrdChangeInY = (yIndex - centerY) * (yIndex - centerY);
        var distance = sqrdChangeInX + sqrdChangeInY;
        return distance < (radius * radius);
    }
}