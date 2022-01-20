using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BezierSpline : MonoBehaviour
{
    [SerializeField] List<Transform> m_CtrlTransforms;
    [SerializeField] int m_NbPtsOnSpline;
    [SerializeField] bool m_IsClosed = false;
    [SerializeField] float m_PtsDensity;

    List<Vector3> m_Pts = new List<Vector3>();

    LinearInterpoCurve m_MyCurve;

    [SerializeField] List<Transform> m_MovingObjects;
    [SerializeField] float m_TranslationSpeed;
    float m_TranslatedDistance = 0;
    float m_SpeedRandomVariation = 0;
    [SerializeField] int m_RndRefreshFrameRate = 10; //commentaire
    [SerializeField] float m_RndAmplitude = .7f;

    Vector3 ComputeBezierPos(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return (.5f * (
            (-a + 3f * b - 3f * c + d) * (t * t * t)
            + (2f * a - 5f * b + 4f * c - d) * (t * t)
            + (-a + c) * t
            + 2f * b));
    }

    Vector3 ComputeBezierTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return (.5f * (
        3 * (-a + 3f * b - 3f * c + d) * (t * t)
        + 2 * (2f * a - 5f * b + 4f * c - d) * t
        + (-a + c)
        )).normalized;
    }

    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> positions = m_CtrlTransforms.Select(item => item.position).ToList();

        //      Vector3 p1 = positions[0];
        //      Vector3 p2 = positions[1];
        //      Vector3 p3 = positions[2];
        //      Vector3 p4 = positions[3];

        //for (int i = 0; i < m_NbPtsOnSpline; i++)
        //{
        //          float t = (float)i / (m_NbPtsOnSpline - 1);
        //          m_Pts.Add(ComputeBezierPos(p1, p2, p3, p4, t));
        //}

        if (m_IsClosed)
        {
            Vector3 ctrlPt0 = positions[0];
            Vector3 ctrlPt1 = positions[1];
            Vector3 ctrlPtNMinus1 = positions[positions.Count - 1];
            positions.Add(ctrlPt0);
            positions.Add(ctrlPt1);
            positions.Insert(0, ctrlPtNMinus1);
        }

        for (int i = 1; i < positions.Count - 2; i++)
        {
            Vector3 P0 = positions[i - 1];
            Vector3 P1 = positions[i];
            Vector3 P2 = positions[i + 1];
            Vector3 P3 = positions[i + 2];
            float distance = Vector3.Distance(P1, P2);
            int nPts = (int)Mathf.Max(3, distance * m_PtsDensity);

            for (int j = 0; j < nPts; j++)
            {
                int nPtsDenominator = (i == positions.Count - 3) && !m_IsClosed ? nPts - 1 : nPts;
                float k = (float)j / nPtsDenominator;
                Vector3 pt = ComputeBezierPos(P0, P1, P2, P3, k);
                m_Pts.Add(pt);
            }
        }

        m_MyCurve = new LinearInterpoCurve(m_Pts, null, m_IsClosed);

    }

    // Update is called once per frame
    void Update()
    {
        if (m_MyCurve != null && m_MyCurve.IsValid)
        {
            Vector3 pos = Vector3.zero, normal, tangent;

            //float offset = 0;
            float prevRadius = 0;
            Vector3 prevPos = Vector3.zero;
            int prevIndex = -1;

            for (int i = 0; i < m_MovingObjects.Count; i++)
            {
                //offset += prevRadius;
                Transform movingObject = m_MovingObjects[i];
                float currRadius = movingObject.GetComponent<SphereCollider>().radius * movingObject.localScale.x;

                //if(i>0) offset += currRadius;

                int currIndex = -1;
                //1ère balle
                if (i == 0 && m_MyCurve.GetPositionNormalTangent(Mathf.Repeat((m_TranslatedDistance/*-offset*/) / m_MyCurve.Length, 1), out pos, out normal, out tangent, out currIndex))
                    movingObject.position = pos;

                //balles suivantes
                else if (m_MyCurve.GetSphereSplineIntersection(prevPos, prevRadius + currRadius, prevIndex, -1, out pos, out normal, out tangent, out currIndex))
                    movingObject.position = pos;

                prevRadius = currRadius;
                prevPos = pos;
                prevIndex = currIndex;
            }

            if (Time.frameCount % m_RndRefreshFrameRate == 0) m_SpeedRandomVariation = m_RndAmplitude * 2 * (Random.value - .5f);
            m_TranslatedDistance += m_TranslationSpeed * (1 + m_SpeedRandomVariation) * Time.deltaTime;
        }
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