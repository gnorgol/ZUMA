using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CurveLinearInterpo
{
    List<Transform> controlsPoints = new List<Transform>();
    float _ptsDensity;
    bool _isClosed;
    float _Length = 0;
    bool _IsValid = false;
    int index;
    private List<Vector3> _ListPts = new List<Vector3>();
    List<float> _ListLength = new List<float>();
    List<int> _ListIndex = new List<int>();
    public float Length { get => _Length; set => _Length = value; }
    public int NPoints { get => _ListPts == null ? -1 : _ListPts.Count; }

    public bool IsValid { get => _IsValid; set => _IsValid = value; }
    public List<Vector3> ListPts { get => _ListPts; set => _ListPts = value; }


    public CurveLinearInterpo(List<Transform> controlsPoints, float ptsDensity, bool isClosed = false)
    {
        this.controlsPoints = controlsPoints;
        this._ptsDensity = ptsDensity;
        this._isClosed = isClosed;
        float floorLength;

        List<Vector3> positions = this.controlsPoints.Select(item => item.position).ToList();
        if (isClosed)
        {
            Vector3 ctrlPt0 = positions[0];
            Vector3 ctrlPt1 = positions[1];
            Vector3 ctrlPtMin = positions[positions.Count - 1];
            positions.Add(ctrlPt0);
            positions.Add(ctrlPt1);
            positions.Insert(0, ctrlPtMin);
        }
        Vector3 previousPoint = Vector3.zero;

        for (int i = 1; i < positions.Count - 2; i++)
        {
            Vector3 P0 = positions[i - 1];
            Vector3 P1 = positions[i];
            Vector3 P2 = positions[i + 1];
            Vector3 P3 = positions[i + 2];
            float ditance = Vector3.Distance(P1, P2);
            int nPts = (int)Mathf.Max(3, ditance * ptsDensity);
            if (previousPoint == Vector3.zero)
            {
                previousPoint = ComputeBezierPos(P0, P1, P2, P3, 0);
            }
            for (int j = 0; j < nPts; j++)
            {
                int nPtsDenominator = (i == positions.Count - 3) && !isClosed ? nPts - 1 : nPts;
                float t = (float)j / nPtsDenominator;
                Vector3 currentPoint = ComputeBezierPos(P0, P1, P2, P3, t);
                _Length += Vector3.Distance(currentPoint, previousPoint);
                floorLength = Mathf.FloorToInt(_Length);
                _ListLength.Add(_Length);
                for (int n = _ListIndex.Count; n < floorLength; n++)
                {
                    _ListIndex.Add(Mathf.Max(_ListPts.Count - 1, 0));
                }
                previousPoint = currentPoint;
                _ListPts.Add(currentPoint);
            }

        }
        _IsValid = true;
    }



    public bool GetPositionFromDistance(float dist, out Vector3 position, out int segmentIndex)
    {
        position = Vector3.zero;
        segmentIndex = 0;
        int floorDistance;
        float distance = dist;
        if (!_IsValid)
        {
            return false;
        }


        if (_isClosed)
        {
            while (distance < 0)
            {
                distance = distance + _Length;
            }
            distance %= _Length;
        }
        else
        {
            distance = Mathf.Clamp(distance, 0, _Length);
        }
        floorDistance = Mathf.FloorToInt(distance);

        index = _ListIndex[Mathf.Clamp(floorDistance, 0, _ListIndex.Count - 1)];
        while (_ListLength[index] < distance)
        {
            index = index + 1;
        }
        if (index > 0)
        {
            index = index - 1;
        }
        Vector3 previousPoint = _ListPts[index];
        Vector3 nextPoint = _ListPts[index + 1];

        float previousPointLength = _ListLength[index];
        float nextPointLength = _ListLength[index + 1];
        segmentIndex = index;


        position = previousPoint + (nextPoint - previousPoint) * ((distance - previousPointLength) / (nextPointLength - previousPointLength));


        return true;

    }
    public bool GetPositionFromDistance(float distance, out Vector3 position)
    {
        int segmentIndex;
        return GetPositionFromDistance(distance, out position, out segmentIndex);
    }

    public bool GetSphereSplineIntersection(Vector3 centre, float radius, int startIndex, int direction, out Vector3 position, out int segmentIndex)
    {
        position = Vector3.zero;
        segmentIndex = startIndex;
        if (!IsValid && (direction == 1 || direction == -1))
        {
            return false;
        }

        // Clamp starting index to a valid segment and do NOT skip the first segment.
        int index = Mathf.Clamp(startIndex, 0, _ListPts.Count - 2);

        // Search along the polyline until we find a valid intersection with the circle of radius around 'centre'.
        for (int step = 0; step < _ListPts.Count; step++)
        {
            Vector3 A = _ListPts[index];
            Vector3 B = _ListPts[index + 1];
            Vector3 AB = B - A;
            Vector3 CentreA = A - centre;

            float a = Vector3.Dot(AB, AB);
            float b = 2f * Vector3.Dot(CentreA, AB);
            float c = Vector3.Dot(CentreA, CentreA) - (radius * radius);
            float delta = (b * b) - (4f * a * c);

            if (delta >= 0f && a > 1e-6f)
            {
                float sqrtDelta = Mathf.Sqrt(delta);
                float t1 = (-b - sqrtDelta) / (2f * a);
                float t2 = (-b + sqrtDelta) / (2f * a);

                // Choose the root consistent with the marching direction along AB.
                float tCandidate = -1f;
                if (direction >= 0)
                {
                    // Prefer the intersection closer to B (larger t) within [0,1]
                    bool t1ok = t1 >= 0f && t1 <= 1f;
                    bool t2ok = t2 >= 0f && t2 <= 1f;
                    if (t1ok && t2ok) tCandidate = Mathf.Max(t1, t2);
                    else if (t1ok) tCandidate = t1;
                    else if (t2ok) tCandidate = t2;
                }
                else
                {
                    // Prefer the intersection closer to A (smaller t) within [0,1]
                    bool t1ok = t1 >= 0f && t1 <= 1f;
                    bool t2ok = t2 >= 0f && t2 <= 1f;
                    if (t1ok && t2ok) tCandidate = Mathf.Min(t1, t2);
                    else if (t1ok) tCandidate = t1;
                    else if (t2ok) tCandidate = t2;
                }

                if (tCandidate >= 0f && tCandidate <= 1f)
                {
                    // Found a valid intersection point on this segment.
                    position = A + AB * tCandidate;
                    segmentIndex = index;
                    return true;
                }
            }

            // Advance to next segment according to direction, with wrap-around compatible with previous behavior.
            index += direction >= 0 ? 1 : -1;
            if (index < 0)
                index = _ListPts.Count - 2;
            if (index > _ListPts.Count - 2)
                index = 0;
        }

        // No intersection found (shouldn't happen with reasonable input)
        position = _ListPts[Mathf.Clamp(index, 0, _ListPts.Count - 1)];
        segmentIndex = Mathf.Clamp(index, 0, _ListPts.Count - 2);
        return true;
    }
    Vector3 ComputeBezierPos(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return (.5f * (
            (-a + 3f * b - 3f * c + d) * (t * t * t)
            + (2f * a - 5f * b + 4f * c - d) * (t * t)
            + (-a + c) * t
            + 2f * b));
    }
}



