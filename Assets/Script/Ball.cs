using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Vector3 shootDir;

    public void Setup(Vector3 shootDir)
    {
        this.shootDir = shootDir;
    }
    private void Update()
    {
        float moveSpeed = 20f;
        transform.position += shootDir * moveSpeed * Time.deltaTime;

    }
    void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }
}
