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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
            lookPos = hit.point;


        float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);

    }

}
