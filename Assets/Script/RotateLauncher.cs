using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLauncher : MonoBehaviour
{
    private Vector3 lookPos;



    void Update()
    {


        RotatePlayerAlongMousePosition();

    }


    private void RotatePlayerAlongMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 target = new Vector3(hit.point.x, hit.point.y, transform.position.z);
            Debug.Log(target);
            transform.LookAt(target);
        }

    }

}
