using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class GetThumbnails : MonoBehaviour
{
    [SerializeField] private Object _object;
    [SerializeField] private Texture2D texture;
    void Start()
    {
        //GetMiniThumbnail of the asset bundle of the object



        /*        texture = AssetPreview.GetMiniThumbnail(_object);
                //save Texture to file Image
                byte[] bytes = texture.EncodeToPNG();
                string fileLocation = Application.dataPath + "/Resources/Thumbnails/" + this.gameObject.name + ".png";
                File.WriteAllBytes(fileLocation, bytes);
                Debug.Log("Saved Texture to: " + fileLocation);*/
    }

}
