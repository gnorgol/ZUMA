using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BezierSplineEditor : MonoBehaviour
{
    [SerializeField] List<Transform> m_CtrlTransform;
    [SerializeField] int m_NbPtsOnSpline;
    [SerializeField] bool m_IsClosed = true;
    [SerializeField] float m_PtsDensity;
    List<Vector3> m_Pts = new List<Vector3>();
    CurveLinearInterpo m_MyCurve;
    [SerializeField] List<Transform> ListeMovingObject;
    [SerializeField] float m_TranslationSpeed;
    [SerializeField] bool m_Direction = true;
    [SerializeField] bool _Repeat = false;
    float m_TranslatedDistance = 0;
    [SerializeField] List<GameObject> ListBall;
    [SerializeField] int nbBall;
    [SerializeField] int idLevel;
    LineRenderer lineRenderer;
    private bool IsSelected = false;
    private bool isExemple = true;

    #region Event listener

    private void OnEnable()
    {
        SubscribeEvents();
    }
    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<putBallForwardEvent>(putBallForward);
        EventManager.Instance.AddListener<putBallBackEvent>(putBallBack);
        EventManager.Instance.AddListener<CheckMatchBallsEvent>(CheckMatchBalls);
        EventManager.Instance.AddListener<MainMenuButtonClickedEvent>(GameMenu);
        EventManager.Instance.AddListener<InstantiateLevelExempleEvent>(InstantiateLevelExemple);
        EventManager.Instance.AddListener<DestroyLevelExempleEvent>(DestroyLevelExemple);
        EventManager.Instance.AddListener<AddSphereEvent>(AddSphere);
        EventManager.Instance.AddListener<DeleteSphereEvent>(RemoveSphere);
        EventManager.Instance.AddListener<MoveSphereEvent>(MoveSphere);
        EventManager.Instance.AddListener<PlayButtonEditorLevelClickedEvent>(PlayButtonEditorLevelClicked);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<putBallForwardEvent>(putBallForward);
        EventManager.Instance.RemoveListener<putBallBackEvent>(putBallBack);
        EventManager.Instance.RemoveListener<CheckMatchBallsEvent>(CheckMatchBalls);
        EventManager.Instance.RemoveListener<MainMenuButtonClickedEvent>(GameMenu);
        EventManager.Instance.RemoveListener<InstantiateLevelExempleEvent>(InstantiateLevelExemple);
        EventManager.Instance.RemoveListener<DestroyLevelExempleEvent>(DestroyLevelExemple);
        EventManager.Instance.RemoveListener<AddSphereEvent>(AddSphere);
        EventManager.Instance.RemoveListener<DeleteSphereEvent>(RemoveSphere);
        EventManager.Instance.RemoveListener<MoveSphereEvent>(MoveSphere);
        EventManager.Instance.RemoveListener<PlayButtonEditorLevelClickedEvent>(PlayButtonEditorLevelClicked);
    }
    #endregion
    private void GameMenu(MainMenuButtonClickedEvent e)
    {
        DestroyAllMovingObject();
    }
    private void putBallForward(putBallForwardEvent e)
    {
        int index = ListeMovingObject.IndexOf(e.target.transform);
        ListeMovingObject.Insert(index, e.ball.transform);
        GetAllColorsMovingObject();
    }
    private void putBallBack(putBallBackEvent e)
    {
        int index = ListeMovingObject.IndexOf(e.target.transform);
        ListeMovingObject.Insert(index + 1, e.ball.transform);
        GetAllColorsMovingObject();
    }
    private void CheckMatchBalls(CheckMatchBallsEvent e)
    {
        int index = ListeMovingObject.IndexOf(e.ball.transform);
        int CountFrontSameColor = 0;
        int CountBackSameColor = 0;
        Color ballColor = e.ball.GetComponent<Renderer>().material.GetColor("_Color");
        /*        Debug.Log(index);
                Debug.Log(ListeMovingObject.Count);*/

        for (int i = index - 1; i >= 0; i--)
        {
            /*Debug.Log("Front : " + ListeMovingObject[i].name);*/
            Color currrentBallColor = ListeMovingObject[i].GetComponent<Renderer>().material.GetColor("_Color");

            if (ballColor == currrentBallColor)
            {
                CountFrontSameColor = CountFrontSameColor + 1;
            }
            else
            {
                break;
            }
        }

        for (int i = index + 1; i < ListeMovingObject.Count; i++)
        {
            /*Debug.Log("Back : " + ListeMovingObject[i].name);*/
            Color currrentBallColor = ListeMovingObject[i].GetComponent<Renderer>().material.GetColor("_Color");

            if (ballColor == currrentBallColor)
            {
                CountBackSameColor = CountBackSameColor + 1;
            }
            else
            {
                break;
            }
        }
        /*        Debug.Log("Count front : " + CountFrontSameColor);
                Debug.Log("Count back : " + CountBackSameColor);*/
        if (CountFrontSameColor + CountBackSameColor >= 2)
        {
            EventManager.Instance.Raise(new GainScoreEvent() { eScore = 10 * (CountFrontSameColor + CountBackSameColor + 1) });
            RemoveBalls(index - CountFrontSameColor, CountFrontSameColor + CountBackSameColor + 1);
        }

    }
    private void RemoveBalls(int index, int range)
    {
        foreach (var item in ListeMovingObject.GetRange(index, range))
        {
            Destroy(item.gameObject);
        }
        ListeMovingObject.RemoveRange(index, range);
        GetAllColorsMovingObject();
    }


    Vector3 ComputeBezierPos(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return (.5f * (
            (-a + 3f * b - 3f * c + d) * (t * t * t)
            + (2f * a - 5f * b + 4f * c - d) * (t * t)
            + (-a + c) * t
            + 2f * b));
    }

    private void Awake()
    {
        lineRenderer = this.GetComponent<LineRenderer>();

    }
    private void GetAllColorsMovingObject()
    {
        List<Color> ListeColor = new List<Color>();
        foreach (var item in ListeMovingObject)
        {
            ListeColor.Add(item.GetComponent<Renderer>().material.GetColor("_Color"));
        }
        EventManager.Instance.Raise(new AllColorsBallsCurveEvent() { ListColorsCurve = ListeColor });
    }
    private void Start()
    {
        //List<Vector3> positions = m_CtrlTransform.Select(item => item.position).ToList();
        /*
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
                }*/

/*        m_MyCurve = new CurveLinearInterpo(m_CtrlTransform, m_PtsDensity, m_IsClosed);
        lineRenderer.positionCount = m_MyCurve.ListPts.Count;
        lineRenderer.SetPositions(m_MyCurve.ListPts.ToArray());
        if (isExemple)
        {
            _Repeat = true;
        }*/
    }

    private void Update()
    {
        Vector3 currentposition;


        float previousRadius = 0;
        Vector3 previousPosition = Vector3.zero;
        int previousIndex = -1;
        int currentIndex = -1;
        if (EditorLevelManager.Instance.FunctionEditorLevelActive == FunctionEditorLevel.Play && m_CtrlTransform.Count >= 4)
        {
            for (int i = 0; i < ListeMovingObject.Count; i++)
            {

                Transform item = ListeMovingObject[i];
                float currentRadious = item.GetComponent<SphereCollider>().radius * item.localScale.x;

                float distance = _Repeat ? Mathf.Repeat(m_TranslatedDistance, m_MyCurve.Length) : m_TranslatedDistance;
                if (i == 0 && m_MyCurve.GetPositionFromDistance(distance, out currentposition, out currentIndex))
                {

                    if (currentIndex == m_MyCurve.NPoints - 2 && !isExemple)
                    {
                        DestroyAllMovingObject();
                        EventManager.Instance.Raise(new FinishCurveEvent());

                    }
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
        else
        {
            if (ListeMovingObject.Count != 0)
            {
                DeleteBallCurve();
            }
            m_TranslatedDistance = 0;

        }



    }
    private void InstantiateLevelExemple(InstantiateLevelExempleEvent e)
    {
        isExemple = true;
    }
    private void DestroyLevelExemple(DestroyLevelExempleEvent e)
    {
        DestroyAllMovingObject();
    }
    public void DestroyAllMovingObject()
    {
        foreach (var destroyFinishCurve in ListeMovingObject)
        {
            Destroy(destroyFinishCurve.gameObject);
        }
        ListeMovingObject.Clear();
        GetAllColorsMovingObject();
    }
    private void AddSphere(AddSphereEvent e)
    {
        m_CtrlTransform.Add(e.Sphere.transform);
        UpdateCurve();
    }
    private void RemoveSphere(DeleteSphereEvent e)
    {
        m_CtrlTransform.Remove(e.Sphere.transform);
        UpdateCurve();
    }
    private void MoveSphere(MoveSphereEvent e)
    {
        UpdateCurve();
    }
    private void UpdateCurve()
    {
        if (m_CtrlTransform.Count >= 4)
        {
            m_Pts = new List<Vector3>();
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
            lineRenderer.positionCount = m_MyCurve.ListPts.Count;
            lineRenderer.SetPositions(m_MyCurve.ListPts.ToArray());
            if (isExemple)
            {
                _Repeat = true;
            }
        }
        else
        {
            m_Pts = new List<Vector3>();
            //reset lineRenderer
            lineRenderer.positionCount = 0;
        }
    }
    private void CreateBall()
    {
        Debug.Log("CreateBall");
        int PreviousR = 0;
        int r;
        for (int i = 0; i < nbBall; i++)
        {
            r = Random.Range(0, 4);
            while (r == PreviousR)
            {
                r = Random.Range(0, 4);
            }
            PreviousR = r;
            GameObject clone;
            clone = Instantiate(ListBall[r], Vector3.zero, Quaternion.identity);
            ListeMovingObject.Add(clone.transform);
        }
        GetAllColorsMovingObject();
    }
    private void DeleteBallCurve()
    {
        foreach (var item in ListeMovingObject)
        {
            Destroy(item.gameObject);
        }
        ListeMovingObject.Clear();

    }
    private void PlayButtonEditorLevelClicked(PlayButtonEditorLevelClickedEvent e)
    {
        if (EditorLevelManager.Instance.FunctionEditorLevelActive == FunctionEditorLevel.Play)
        {
            CreateBall();
        }

    }
}
