using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SDD.Events;
using UnityEditor;
using System.IO;


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
    [SerializeField] bool m_Direction = false;
    [SerializeField] bool _Repeat = false;
    float m_TranslatedDistance = 0;
    [SerializeField] List<GameObject> _ListBall;
    [SerializeField] int _nbBall;
    [SerializeField] int _idLevel;
    LineRenderer lineRenderer;
    private bool IsSelected = false;
    [SerializeField] private bool _isExemple = false;
    [SerializeField] private bool _isInstantiate = false;
    public List<Transform> CtrlTransform { get => m_CtrlTransform; set => m_CtrlTransform = value; }
    public int NbBall { get => _nbBall; set => _nbBall = value; }
    public int IdLevel { get => _idLevel; set => _idLevel = value; }
    public List<GameObject> ListBall { get => _ListBall; set => _ListBall = value; }
    public int NbPtsOnSpline { get => m_NbPtsOnSpline; set => m_NbPtsOnSpline = value; }
    public bool IsClosed { get => m_IsClosed; set => m_IsClosed = value; }
    public float PtsDensity { get => m_PtsDensity; set => m_PtsDensity = value; }
    public float TranslationSpeed { get => m_TranslationSpeed; set => m_TranslationSpeed = value; }
    public bool Direction { get => m_Direction; set => m_Direction = value; }
    public bool Repeat { get => _Repeat; set => _Repeat = value; }
    public List<GameObject> ListBall1 { get => _ListBall; set => _ListBall = value; }
    public bool IsExemple { get => _isExemple; set => _isExemple = value; }


    //Constructeur with parameters
    public BezierSpline(List<Transform> ctrlTransform, int nbPtsOnSpline, bool isClosed, float ptsDensity, float translationSpeed, bool direction, bool repeat, int nbBall, int idLevel)
    {
        m_CtrlTransform = ctrlTransform;
        m_NbPtsOnSpline = nbPtsOnSpline;
        m_IsClosed = isClosed;
        m_PtsDensity = ptsDensity;
        translationSpeed = m_TranslationSpeed;
        m_Direction = direction;
        _Repeat = repeat;
        this._nbBall = nbBall;
        this._idLevel = idLevel;
    }
    public BezierSpline(BezierSpline bezierSpline)
    {
        m_CtrlTransform = bezierSpline.CtrlTransform;
        NbPtsOnSpline = bezierSpline.NbPtsOnSpline;
        IsClosed = bezierSpline.IsClosed;
        m_PtsDensity = bezierSpline.PtsDensity;
        m_TranslationSpeed = bezierSpline.m_TranslationSpeed;
        m_Direction = bezierSpline.Direction;
        _Repeat = bezierSpline.Repeat;
        _nbBall = bezierSpline.NbBall;
        _idLevel = bezierSpline.IdLevel;
    }
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
        EventManager.Instance.AddListener<DestroyLevelExempleEvent>(DestroyLevelExemple);
        EventManager.Instance.AddListener<AddSphereEvent>(AddSphere);
        EventManager.Instance.AddListener<DeleteSphereEvent>(RemoveSphere);
        EventManager.Instance.AddListener<MoveSphereEvent>(MoveSphere);
        EventManager.Instance.AddListener<PlayButtonEditorLevelClickedEvent>(PlayButtonEditorLevelClicked);
        EventManager.Instance.AddListener<SaveButtonEditorLevelClickedEvent>(SaveButtonEditorLevelClicked);

    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<putBallForwardEvent>(putBallForward);
        EventManager.Instance.RemoveListener<putBallBackEvent>(putBallBack);
        EventManager.Instance.RemoveListener<CheckMatchBallsEvent>(CheckMatchBalls);
        EventManager.Instance.RemoveListener<MainMenuButtonClickedEvent>(GameMenu);
        EventManager.Instance.RemoveListener<DestroyLevelExempleEvent>(DestroyLevelExemple);
        EventManager.Instance.RemoveListener<AddSphereEvent>(AddSphere);
        EventManager.Instance.RemoveListener<DeleteSphereEvent>(RemoveSphere);
        EventManager.Instance.RemoveListener<MoveSphereEvent>(MoveSphere);
        EventManager.Instance.RemoveListener<PlayButtonEditorLevelClickedEvent>(PlayButtonEditorLevelClicked);
        EventManager.Instance.RemoveListener<SaveButtonEditorLevelClickedEvent>(SaveButtonEditorLevelClicked);
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
            EventManager.Instance.Raise(new GainScoreEvent() { eScore = 10 * (CountFrontSameColor + CountBackSameColor+1) });
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
    private void GetAllColorsMovingObject()
    {
        if (GameManager.Instance.IsPlaying && _isInstantiate)
        {
            List<Color> ListeColor = new List<Color>();
            foreach (var item in ListeMovingObject)
            {
                ListeColor.Add(item.GetComponent<Renderer>().material.GetColor("_Color"));
            }
            EventManager.Instance.Raise(new AllColorsBallsCurveEvent() { ListColorsCurve = ListeColor });
        }
    }
    public void SetInstantiate(bool instantiate)
    {
        _isInstantiate = instantiate;
        if (_isInstantiate)
        {
            GetAllColorsMovingObject();
        }
        
    }
    private void Awake()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        CreateBall();
    }
    private void Start()
    {
        
        UpdateCurve();

    }

    private void Update()
    {
        Vector3 currentposition;
        float previousRadius = 0;
        Vector3 previousPosition = Vector3.zero;
        int previousIndex = -1;
        int currentIndex = -1;
        if (m_CtrlTransform.Count >= 4 && GameManager.Instance.IsPlaying || _isExemple || EditorLevelManager.Instance.FunctionEditorLevelActive == FunctionEditorLevel.Play)
        {
            for (int i = 0; i < ListeMovingObject.Count; i++)
            {
                Transform item = ListeMovingObject[i];
                float currentRadious = item.GetComponent<SphereCollider>().radius * item.localScale.x;

                float distance = _Repeat ? Mathf.Repeat(m_TranslatedDistance, m_MyCurve.Length) : m_TranslatedDistance;
                if (i == 0 && m_MyCurve.GetPositionFromDistance(distance, out currentposition, out currentIndex))
                {

                    if (currentIndex == m_MyCurve.NPoints - 2 && !_isExemple && EditorLevelManager.Instance.FunctionEditorLevelActive != FunctionEditorLevel.Play)
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

        if (ListeMovingObject.Count == 0 && GameManager.Instance.IsPlaying)
        {
            DestroyAllMovingObject();
            Debug.Log("DestroyAllMovingObject");
            if (GameManager.Instance.IsPlaying)
            {
                EventManager.Instance.Raise(new GameLevelChangedEvent() { eLevel = _idLevel + 1 });
            }
            
        }


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
            if (_isExemple)
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
        int PreviousR = 0;
        int r;
        for (int i = 0; i < _nbBall; i++)
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
    private void DeleteCurve()
    {
        foreach (var item in m_CtrlTransform)
        {
            Destroy(item.gameObject);
        }
        m_CtrlTransform.Clear();
        ResetLineRenderer();
    }
    private void ResetLineRenderer()
    {
        lineRenderer.positionCount = 0;
    }
    private void PlayButtonEditorLevelClicked(PlayButtonEditorLevelClickedEvent e)
    {
        
        if (EditorLevelManager.Instance.FunctionEditorLevelActive == FunctionEditorLevel.Play)
        {
            Debug.Log("PlayButtonEditorLevelClicked");
            CreateBall();
        }

    }
    private void SaveButtonEditorLevelClicked(SaveButtonEditorLevelClickedEvent e)
    {
        SaveCurve();
        DeleteCurve();
    }
    private void SaveCurve()
    {
        IdLevel = GameManager.Instance.ListLevel.Count + GameManager.Instance.ListSaveLevel.Count + 1;
        Saver saver = this.gameObject.GetComponent<Saver>();
        saver.SaveCurve("Curves Perso " + IdLevel);
    }
}