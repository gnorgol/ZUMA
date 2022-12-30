using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class test : MonoBehaviour
{
    private string fileLocation = "Assets/Resources/SaveCurves/";
    public GameData loadedObject;
    public List<GameObject> listBall;
    public Material _materialLineRenderer;
    private void Start()
    {
        Load("Curves Perso 4.dat");
    }
    public void Load(string fileName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(fileLocation + fileName, FileMode.Open);

        loadedObject = formatter.Deserialize(stream) as GameData;
        stream.Close();
        //create empty GameObject name from file
        GameObject loadedGameObject = new GameObject(fileName);
        //set position to 0 1 0
        loadedGameObject.transform.position = new Vector3(0, 1, 0);
        //add LineRenderer
        LineRenderer lineRenderer = loadedGameObject.AddComponent<LineRenderer>();
        //set material
        lineRenderer.material = _materialLineRenderer;
        List<Transform> m_CtrlTransform = new List<Transform>();
        foreach (var item in loadedObject.transformList)
        {
            //Create spehere with the name and the possition
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = item.name;
            //set sphere in child of the loadedGameObject
            sphere.transform.parent = loadedGameObject.transform;
            sphere.transform.position = new Vector3(item.posX, item.posY, item.posZ);
            //add the sphere to the list
            m_CtrlTransform.Add(sphere.transform);
        }
        //BezierSpline bezierSplineCompenent = new BezierSpline(m_CtrlTransform, loadedObject.m_NbPtsOnSpline, loadedObject.m_IsClosed, loadedObject.m_PtsDensity, loadedObject.m_TranslationSpeed, loadedObject.m_Direction, loadedObject._Repeat, loadedObject._nbBall, loadedObject._idLevel);
        //add the bezierSplineCompenent to the loadedGameObject
        BezierSpline bezierSplineCompenent = loadedGameObject.AddComponent<BezierSpline>();
        bezierSplineCompenent.CtrlTransform = m_CtrlTransform;
        bezierSplineCompenent.NbPtsOnSpline = loadedObject.m_NbPtsOnSpline;
        bezierSplineCompenent.IsClosed = loadedObject.m_IsClosed;
        bezierSplineCompenent.PtsDensity = loadedObject.m_PtsDensity;
        bezierSplineCompenent.TranslationSpeed = loadedObject.m_TranslationSpeed;
        bezierSplineCompenent.Direction = loadedObject.m_Direction;
        bezierSplineCompenent.Repeat = loadedObject._Repeat;
        bezierSplineCompenent.NbBall = loadedObject._nbBall;
        bezierSplineCompenent.IdLevel = loadedObject._idLevel;
        bezierSplineCompenent.ListBall = listBall;
        



    }

}
