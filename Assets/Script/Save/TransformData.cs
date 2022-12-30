using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransformData
{
    /*    public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;*/
    public float posX;
    public float posY;
    public float posZ;
    public string name;

    // Constructeur
    public TransformData(Transform t)
    {
        posX = t.position.x;
        posY = t.position.y;
        posZ = t.position.z;
        name = t.name;
    }
}
