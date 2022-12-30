using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SDD.Events;

public class Saver : MonoBehaviour
{
    private string fileLocation = "Assets/Resources/SaveCurves/";
    public void SaveCurve(string fileName)
    {
        Debug.Log("Trying to save");
        GameData gameData = new GameData(GetComponent<BezierSpline>());
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = File.Create(fileLocation + fileName + ".dat");

        formatter.Serialize(stream, gameData);
        stream.Close();
        EventManager.Instance.Raise(new SaveCurveEvent());
    }
    public void Load(string fileName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(fileName, FileMode.Open);

        GameObject loadedObject = formatter.Deserialize(stream) as GameObject;
        stream.Close();
    }

}
