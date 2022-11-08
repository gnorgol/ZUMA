using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class GetThumbnail : MonoBehaviour
{
    [SerializeField] private Object obj;
    [SerializeField] private Texture2D thumbnail;
    private void Start()
    {
        thumbnail = AssetPreview.GetAssetPreview(obj);

        // Save the thumbnail to a file
        byte[] bytes = thumbnail.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/../thumbnail.png", bytes);
    }
}
