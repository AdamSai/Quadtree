using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class PlayerController : MonoBehaviour
{
    private Camera cam;
    public float brushSize = 2f;
    private QuadTree.AABB range;
    public QuadTree root;
    private Collider2D[] _results;
    public GameObject testPrefab;
    private List<GameObject> activePoints;
    public GameObject edgePrefab;
    private List<Vector2> _positons;

    // Start is called before the first frame update
    void Start()
    {
        activePoints = new List<GameObject>();
        range = new QuadTree.AABB();
        cam = Camera.main;
        _results = new Collider2D[2048];
        _positons = new List<Vector2>();
    }

    void Update()
    {
        var mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 22));
        if (Input.GetButtonDown("Fire1"))
        {
            DestroyActivePoints();
            root.Insert(mousePos);
        }

        if (Input.GetButton("Fire2"))
        {
            DestroyActivePoints();
            range.center = mousePos;
            range.halfDimension = brushSize / 2;

            var points = root.QueryRange(range);
            foreach (var point in points)
            {
                var go = Instantiate(edgePrefab, new Vector3(point.x, point.y, -0.4f), Quaternion.identity);
                activePoints.Add(go);
            }
        }

        if (Input.GetButton("Jump"))
        {
                StartCoroutine(DestroyCubes(mousePos));
        }
    }

    private IEnumerator DestroyCubes(Vector3 mousePos)
    {
        SplitTerrain(mousePos);

        yield return new WaitForSeconds(0.2f);
        var size = Physics2D.OverlapCircleNonAlloc(mousePos, range.halfDimension - 0.2f, _results);

        foreach (var obj in _results)
        {
            if(obj != null)
                obj.transform.gameObject.SetActive(false);
        }
        _positons.Clear();
    }

    private void SplitTerrain(Vector3 mousePos)
    {
        range.center = mousePos;
        range.halfDimension = brushSize / 2;

        for (var x = mousePos.x - range.halfDimension; x < mousePos.x + range.halfDimension - 0.2f; x += 0.02f)
        {
            for (var y = mousePos.y - range.halfDimension; y < mousePos.y + range.halfDimension - 0.2f; y += 0.02f)
            {
                var isInsideCircle = CalculateInsideCircle(mousePos.x, mousePos.y, x, y, range.halfDimension);
                if (isInsideCircle)
                {
                    root.Insert(new Vector2(x, y));
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
//            var sprite = obj.GetComponent<SpriteRenderer>().sprite;
//            var verts = sprite.vertices;

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
        return distance <= (radius * radius);
    }
}