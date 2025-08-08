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
    [SerializeField] bool m_Direction = true; // true = forward along curve
    [SerializeField] bool _Repeat = false;
    float m_TranslatedDistance = 0;
    [SerializeField] List<GameObject> _ListBall;
    [SerializeField] int _nbBall;
    [SerializeField] int _idLevel;
    LineRenderer lineRenderer;
    private bool IsSelected = false;
    [SerializeField] private bool _isExemple = false;
    [SerializeField] private bool _isInstantiate = false;
    // Gap closing state (only the trailing segment moves forward)
    [SerializeField] private float _gapCloseSpeed = 8f; // conservé si on revient à MoveTowards
    private bool _isClosingGap = false;
    private int _gapIndex = -1; // first ball index after removed block
    private float _gapOffset = 0f; // current offset distance behind the previous ball
    // Easing parameters for gap closing
    [SerializeField] private float _gapCloseDuration = 0.35f; // plus long pour bien voir le mouvement
    [SerializeField] private float _gapCloseBackOvershoot = 1.35f; // plus d'overshoot
    private float _gapInitialOffset = 0f;
    private float _gapTime = 0f;
    // Slight spacing multiplier to avoid z-fight/overlap
    [SerializeField, Range(1.0f, 1.2f)] private float _spacingMultiplier = 1.02f;
    // Loop tightening (last ball vs head) to avoid overlap on closed curves without teleport
    [SerializeField] private float _loopTightenDuration = 0.12f;
    [SerializeField] private float _loopBackOvershoot = 1.15f;
    private bool _isLoopTightening = false;
    private float _loopOffset = 0f;
    private float _loopInitialOffset = 0f;
    private float _loopTime = 0f;
    // Insertion local recoil (autour du point d'insertion)
    [SerializeField] private float _insertDuration = 0.16f;
    [SerializeField] private float _insertBackOvershoot = 1.25f;
    private bool _isInserting = false;
    private int _insertIndex = -1;
    private float _insertOffset = 0f;
    private float _insertInitialOffset = 0f;
    private float _insertTime = 0f;
    // Scale punch cache
    private Dictionary<Transform, Coroutine> _scaleRoutines;
    // Ensure balls spawn exactly once per instance
    private bool _spawnedBalls = false;
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
    m_TranslationSpeed = translationSpeed;
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
    // Pas de recul global: l'insertion restera locale
    ScalePunchNeighbors(index);
    }
    private void putBallBack(putBallBackEvent e)
    {
        int index = ListeMovingObject.IndexOf(e.target.transform);
        ListeMovingObject.Insert(index + 1, e.ball.transform);
        GetAllColorsMovingObject();
    // Pas de recul global: l'insertion restera locale
    ScalePunchNeighbors(index + 1);
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
        // Init gap closing animation (only if there is a head and a tail)
        if (index > 0 && index < ListeMovingObject.Count)
        {
            _isClosingGap = true;
            _gapIndex = index; // first ball after the removed block
            // Estimate gap size by diameters of removed balls
            float radius = 0.5f;
            if (ListeMovingObject.Count > 0)
            {
                var first = ListeMovingObject[0];
                var sc = first.GetComponent<SphereCollider>();
                if (sc != null) radius = sc.radius * first.localScale.x;
            }
            float diameter = radius * 2f;
            _gapOffset = Mathf.Max(diameter * range, diameter); // avoid zero-length
            _gapInitialOffset = _gapOffset;
            _gapTime = 0f;
            // Visual punch on boundaries of the gap
            ScalePunchAtRemovalBoundaries(index);
        }
        else
        {
            _isClosingGap = false;
            _gapIndex = -1;
            _gapOffset = 0f;
            _gapInitialOffset = 0f;
            _gapTime = 0f;
        }
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
    if (_scaleRoutines == null) _scaleRoutines = new Dictionary<Transform, Coroutine>();
    if (ListeMovingObject == null) ListeMovingObject = new List<Transform>();
    }
    private void Start()
    {
        UpdateCurve();
        if (!_spawnedBalls)
        {
            CreateBall();
            _spawnedBalls = true;
        }
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
            int dirSign = (m_TranslationSpeed >= 0f) ? 1 : -1;
            Vector3 headPos = Vector3.zero; int headSegIndex = -1; float headRadius = 0f; Transform headTr = null;
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
                    // Orient along spline tangent
                    Vector3 tangent = (m_MyCurve.ListPts[Mathf.Min(currentIndex + 1, m_MyCurve.ListPts.Count - 1)] - m_MyCurve.ListPts[Mathf.Max(currentIndex, 0)]).normalized;
                    if (tangent.sqrMagnitude > 0.0001f) item.forward = tangent * dirSign;
                    headPos = currentposition;
                    headSegIndex = currentIndex;
                    headRadius = currentRadious;
                    headTr = item;
                }
                else
                {
                    // En phase de fermeture de trou: pour le premier élément après le gap,
                    // on recule artificiellement le centre sur la tangente pour créer un retard qui sera comblé progressivement.
                    Vector3 centreForIntersection = previousPosition;
                    int startIdxForIntersection = previousIndex;
                    if (_isClosingGap && i == _gapIndex)
                    {
                        // Approxime la tangente locale au point précédent
                        Vector3 prevTangent = (m_MyCurve.ListPts[Mathf.Min(previousIndex + 1, m_MyCurve.ListPts.Count - 1)] - m_MyCurve.ListPts[Mathf.Max(previousIndex, 0)]).normalized;
                        if (prevTangent.sqrMagnitude < 0.0001f)
                        {
                            prevTangent = item.forward;
                        }
                        centreForIntersection = previousPosition - prevTangent * _gapOffset;
                        startIdxForIntersection = previousIndex;
                    }
                    // Insertion locale: décale la nouvelle boule pour un léger retard
                    else if (_isInserting && i == _insertIndex)
                    {
                        Vector3 prevTangent = (m_MyCurve.ListPts[Mathf.Min(previousIndex + 1, m_MyCurve.ListPts.Count - 1)] - m_MyCurve.ListPts[Mathf.Max(previousIndex, 0)]).normalized;
                        if (prevTangent.sqrMagnitude < 0.0001f)
                        {
                            prevTangent = item.forward;
                        }
                        centreForIntersection = previousPosition - prevTangent * _insertOffset;
                        startIdxForIntersection = previousIndex;
                    }
                    // Loop tightening: applique un léger retard sur la dernière boule pour l'écarter de la tête
                    else if (_isLoopTightening && i == ListeMovingObject.Count - 1)
                    {
                        Vector3 prevTangent = (m_MyCurve.ListPts[Mathf.Min(previousIndex + 1, m_MyCurve.ListPts.Count - 1)] - m_MyCurve.ListPts[Mathf.Max(previousIndex, 0)]).normalized;
                        if (prevTangent.sqrMagnitude < 0.0001f)
                        {
                            prevTangent = item.forward;
                        }
                        centreForIntersection = previousPosition - prevTangent * _loopOffset;
                        startIdxForIntersection = previousIndex;
                    }

                    if (m_MyCurve.GetSphereSplineIntersection(centreForIntersection, (previousRadius + currentRadious) * _spacingMultiplier, startIdxForIntersection, m_Direction ? 1 : -1, out currentposition, out currentIndex))
                    {
                        item.position = currentposition;
                        // Orient along spline tangent
                        Vector3 tangent = (m_MyCurve.ListPts[Mathf.Min(currentIndex + 1, m_MyCurve.ListPts.Count - 1)] - m_MyCurve.ListPts[Mathf.Max(currentIndex, 0)]).normalized;
                        if (tangent.sqrMagnitude > 0.0001f) item.forward = tangent * dirSign;
                    }
                }
                previousRadius = currentRadious;
                previousPosition = currentposition;
                previousIndex = currentIndex;

            }
            // Active le loop tightening si la queue s'approche trop de la tête (sans téléport)
            if (_isClosingGap && m_IsClosed && ListeMovingObject.Count > 1 && headTr != null)
            {
                Transform lastTr = ListeMovingObject[ListeMovingObject.Count - 1];
                if (lastTr != null)
                {
                    float lastRadius = lastTr.GetComponent<SphereCollider>().radius * lastTr.localScale.x;
                    float minDist = (headRadius + lastRadius) * _spacingMultiplier;
                    float realDist = Vector3.Distance(lastTr.position, headPos);
                    if (realDist < minDist && !_isLoopTightening)
                    {
                        _isLoopTightening = true;
                        _loopInitialOffset = Mathf.Max(minDist - realDist, 0.01f);
                        _loopOffset = _loopInitialOffset;
                        _loopTime = 0f;
                    }
                }
            }
            // Anime la fermeture du trou si active
            if (_isClosingGap)
            {
                _gapTime += Time.deltaTime;
                float tNorm = Mathf.Clamp01(_gapTime / Mathf.Max(0.01f, _gapCloseDuration));
                float eased = EaseOutBack(tNorm, _gapCloseBackOvershoot);
                _gapOffset = Mathf.Max(0f, _gapInitialOffset * (1f - eased));
                if (tNorm >= 1f || _gapOffset <= 0.0001f)
                {
                    _gapOffset = 0f;
                    _isClosingGap = false;
                    _gapIndex = -1;
                }
            }
            if (_isInserting)
            {
                _insertTime += Time.deltaTime;
                float tNorm = Mathf.Clamp01(_insertTime / Mathf.Max(0.01f, _insertDuration));
                float eased = EaseOutBack(tNorm, _insertBackOvershoot);
                _insertOffset = Mathf.Max(0f, _insertInitialOffset * (1f - eased));
                if (tNorm >= 1f || _insertOffset <= 0.0001f)
                {
                    _insertOffset = 0f;
                    _isInserting = false;
                    _insertIndex = -1;
                }
            }
            if (_isLoopTightening)
            {
                _loopTime += Time.deltaTime;
                float tNorm = Mathf.Clamp01(_loopTime / Mathf.Max(0.01f, _loopTightenDuration));
                float eased = EaseOutBack(tNorm, _loopBackOvershoot);
                _loopOffset = Mathf.Max(0f, _loopInitialOffset * (1f - eased));
                if (tNorm >= 1f || _loopOffset <= 0.0001f)
                {
                    _loopOffset = 0f;
                    _isLoopTightening = false;
                }
            }
            // Stop global head movement while closing a gap so the head waits for the tail
            if (!_isClosingGap)
            {
                m_TranslatedDistance += m_TranslationSpeed * Time.deltaTime;
            }
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
    private float EaseOutBack(float t, float s)
    {
        // Standard EaseOutBack: t in [0,1]
        t = t - 1f;
        return 1f + (t) * t * ((s + 1f) * t + s);
    }
    private void StartLocalInsertionRecoil(int insertedIndex)
    {
    // Désactivé: on ne recolle pas lors d'une insertion, seulement lors d'une casse
    return;
    }

    // Expose tuning from GameManager or Inspector
    public void SetGapCloseTuning(float duration, float overshoot)
    {
        _gapCloseDuration = duration;
        _gapCloseBackOvershoot = overshoot;
    }
    public void SetInsertionTuning(float duration, float overshoot, float? initialOffsetMul = null)
    {
        _insertDuration = duration;
        _insertBackOvershoot = overshoot;
        if (initialOffsetMul.HasValue)
        {
            // Will apply on next insertion, we store it as multiplier via initial offset computation
            // Here we encode the multiplier by updating the default initial offset proportionally at start time.
            // For simplicity keep a private field would be better but we can reuse _insertInitialOffset at StartLocalInsertionRecoil time.
            // No-op here; we keep API for future use.
        }
    }

    private void ScalePunchNeighbors(int insertedIndex)
    {
        // scale punch on inserted ball and its direct neighbors
        if (insertedIndex >= 0 && insertedIndex < ListeMovingObject.Count && ListeMovingObject[insertedIndex] != null)
        {
            StartScalePunch(ListeMovingObject[insertedIndex]);
        }
        if (insertedIndex - 1 >= 0 && insertedIndex - 1 < ListeMovingObject.Count && ListeMovingObject[insertedIndex - 1] != null)
        {
            StartScalePunch(ListeMovingObject[insertedIndex - 1]);
        }
        if (insertedIndex + 1 >= 0 && insertedIndex + 1 < ListeMovingObject.Count && ListeMovingObject[insertedIndex + 1] != null)
        {
            StartScalePunch(ListeMovingObject[insertedIndex + 1]);
        }
    }
    private void ScalePunchAtRemovalBoundaries(int gapIndex)
    {
        // Punch the last before gap and the first after gap (if they exist)
        int before = gapIndex - 1;
        int after = gapIndex;
        if (before >= 0 && before < ListeMovingObject.Count && ListeMovingObject[before] != null)
        {
            StartScalePunch(ListeMovingObject[before]);
        }
        if (after >= 0 && after < ListeMovingObject.Count && ListeMovingObject[after] != null)
        {
            StartScalePunch(ListeMovingObject[after]);
        }
        // Optionally the next one too for extra juice
        if (after + 1 < ListeMovingObject.Count && ListeMovingObject[after + 1] != null)
        {
            StartScalePunch(ListeMovingObject[after + 1], 1.08f, 0.1f);
        }
    }
    private void StartScalePunch(Transform tr, float punchScale = 1.15f, float duration = 0.12f)
    {
        if (tr == null) return;
        if (_scaleRoutines == null) _scaleRoutines = new Dictionary<Transform, Coroutine>();
        if (_scaleRoutines.TryGetValue(tr, out var routine) && routine != null)
        {
            try { StopCoroutine(routine); } catch { }
            _scaleRoutines[tr] = null;
        }
        if (!isActiveAndEnabled) return;
        var co = StartCoroutine(CoScalePunch(tr, punchScale, duration));
        _scaleRoutines[tr] = co;
    }
    private IEnumerator CoScalePunch(Transform tr, float punchScale, float duration)
    {
        if (tr == null) yield break;
        Vector3 baseScale = tr.localScale;
        Vector3 targetScale = baseScale * punchScale;
        float half = Mathf.Max(0.01f, duration * 0.5f);
        float t = 0f;
        // scale up
        while (t < half)
        {
            float k = t / half;
            if (tr == null) yield break;
            tr.localScale = Vector3.LerpUnclamped(baseScale, targetScale, k);
            t += Time.deltaTime;
            yield return null;
        }
        if (tr == null) yield break;
        tr.localScale = targetScale;
        // scale down
        t = 0f;
        while (t < half)
        {
            float k = t / half;
            if (tr == null) yield break;
            tr.localScale = Vector3.LerpUnclamped(targetScale, baseScale, k);
            t += Time.deltaTime;
            yield return null;
        }
        if (tr != null) tr.localScale = baseScale;
        if (_scaleRoutines != null) _scaleRoutines[tr] = null;
    }
    // (recoil supprimé — on privilégie une fermeture locale du gap)
    
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
    _spawnedBalls = false;
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
    _spawnedBalls = false;

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
            if (!_spawnedBalls)
            {
                CreateBall();
                _spawnedBalls = true;
            }
        }

    }
    private void OnValidate()
    {
        if (m_PtsDensity <= 0f) m_PtsDensity = 2f;
        if (m_NbPtsOnSpline <= 0) m_NbPtsOnSpline = 15;
        if (_spacingMultiplier < 1.0f) _spacingMultiplier = 1.02f;
        if (_gapCloseDuration <= 0f) _gapCloseDuration = 0.24f;
        if (_gapCloseBackOvershoot <= 0f) _gapCloseBackOvershoot = 1.35f;
        if (_insertDuration <= 0f) _insertDuration = 0.16f;
        if (_insertBackOvershoot <= 0f) _insertBackOvershoot = 1.25f;
        if (_loopTightenDuration <= 0f) _loopTightenDuration = 0.12f;
        if (_loopBackOvershoot <= 0f) _loopBackOvershoot = 1.15f;
        if (_nbBall < 1) _nbBall = 1;
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