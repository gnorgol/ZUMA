using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameData 
{
    public List<TransformData> transformList;
    public int m_NbPtsOnSpline;
    public bool m_IsClosed = true;
    public float m_PtsDensity;
    public float m_TranslationSpeed;
    public bool m_Direction = false;
    public bool _Repeat = false;
    public int _nbBall;
    public int _idLevel;


    //Constructor
    public GameData(BezierSpline bezierSpline)
    {
        
        transformList = new List<TransformData>();
        foreach (var item in bezierSpline.CtrlTransform)
        {
            transformList.Add(new TransformData(item));
        }
        m_NbPtsOnSpline = bezierSpline.NbPtsOnSpline;
        m_IsClosed = bezierSpline.IsClosed;
        m_PtsDensity = bezierSpline.PtsDensity;
        m_TranslationSpeed = bezierSpline.TranslationSpeed;
        m_Direction = bezierSpline.Direction;
        _Repeat = bezierSpline.Repeat;
        _nbBall = bezierSpline.NbBall;
        _idLevel = bezierSpline.IdLevel;
    }

}
