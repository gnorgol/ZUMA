using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLauncher : MonoBehaviour
{
    private Vector3 lookPos;



    void Update()
    {


        RotateLauncherMousePosition();

    }


    private void RotateLauncherMousePosition()
    {
        /*        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    lookPos = hit.point;

                }
                Vector3 target = new Vector3(lookPos.x, lookPos.y, transform.position.z);
                transform.LookAt(target);*/

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
            lookPos = hit.point;

        Vector3 lookDir = lookPos - transform.position;
        lookDir.z = 0;

        transform.LookAt(transform.position + lookDir, Vector3.forward);
    }

}
