using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BezierSpline : MonoBehaviour
{
    [SerializeField] List<Transform> m_CtrlTransform;
    [SerializeField] int m_NbPtsOnSpline;
    [SerializeField] bool m_IsClosed = true;
    [SerializeField] float m_PtsDensity;
    List<Vector3> m_Pts = new List<Vector3>();
    CurveLinearInterpo m_MyCurve;
    [SerializeField] List<Transform> ListeMovingObject;
    [SerializeField] float m_TranslationSpeed;
    [SerializeField] bool m_Direction;
    //[SerializeField] bool reapeat = true;
    float m_TranslatedDistance = 0;
    float t = 0f;

    Vector3 ComputeBezierPos(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return (.5f * (
            (-a + 3f * b - 3f * c + d) * (t * t * t)
            + (2f * a - 5f * b + 4f * c - d) * (t * t)
            + (-a + c) * t
            + 2f * b));
    }
    private void Start()
    {
        List<Vector3> positions = m_CtrlTransform.Select(item => item.position).ToList();

        Vector3 p1 = positions[0];
        Vector3 p2 = positions[1];
        Vector3 p3 = positions[2];
        Vector3 p4 = positions[3];

        for (int i = 0; i < m_NbPtsOnSpline; i++)
        {
            float t = (float)i / (m_NbPtsOnSpline - 1);
            m_Pts.Add(ComputeBezierPos(p1, p2, p3, p4, t));
        }
        for (int i = 1; i < positions.Count - 2; i++)
        {
            Vector3 P0 = positions[i - 1];
            Vector3 P1 = positions[i];
            Vector3 P2 = positions[i + 1];
            Vector3 P3 = positions[i + 2];
            float ditance = Vector3.Distance(P1, P2);
            int nPts = (int)Mathf.Max(3, ditance * m_PtsDensity);
            for (int j = 0; j < nPts; j++)
            {
                int nPtsDenominator = (i == positions.Count - 3) && !m_IsClosed ? nPts - 1 : nPts;
                float k = (float)j / nPtsDenominator;
                Vector3 pt = ComputeBezierPos(P0, P1, P2, P3, k);
                m_Pts.Add(pt);
            }
        }

        m_MyCurve = new CurveLinearInterpo(m_CtrlTransform, m_PtsDensity, m_IsClosed);

    }

    private void Update()
    {
        Vector3 currentposition;


        float previousRadius = 0;
        Vector3 previousPosition = Vector3.zero;
        int previousIndex = 0;


        for (int i = 0; i < ListeMovingObject.Count; i++)
        {

            Transform item = ListeMovingObject[i];
            float currentRadious = item.GetComponent<SphereCollider>().radius * item.localScale.x;
            int currentIndex;
            if (i == 0 && m_MyCurve.GetPositionFromDistance(m_TranslatedDistance, out currentposition, out currentIndex))
            {
                item.position = currentposition;
            }
            else if (m_MyCurve.GetSphereSplineIntersection(previousPosition, previousRadius + currentRadious, previousIndex, m_Direction ? 1 : -1, out currentposition, out currentIndex))
            {
                item.position = currentposition;
            }
            previousRadius = currentRadious;
            previousPosition = currentposition;
            previousIndex = currentIndex;

        }
        m_TranslatedDistance += m_TranslationSpeed * Time.deltaTime;
    }


    private void OnDrawGizmos()
    {
        if (m_Pts.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < m_Pts.Count; i++)
            {
                Gizmos.DrawSphere(m_Pts[i], .05f);
            }
        }
    }
}