using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Camera cam;

    public QuadTree root;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;

    }

    void Update()
    {
        
        if (Input.GetButtonDown("Fire1"))
        {
            var mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 22));
            if (Physics.Raycast(mousePos, Vector3.forward, out RaycastHit hit))
            {
                root.Insert(hit.point);
         

            }
        }
    }
}
