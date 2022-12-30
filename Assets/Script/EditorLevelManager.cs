using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using UnityEngine.UI;
using System.Linq;
public enum FunctionEditorLevel
{
    Add,
    Move,
    Delete,
    Save,
    Play,
    None
}
public class EditorLevelManager : Manager<EditorLevelManager>
{
    [SerializeField] private GameObject _EditorLevel;
    GameObject _EditorLevelInstance;
    //[SerializeField] private List<Button> ButtonsSlot;
    private FunctionEditorLevel _FunctionEditorLevelActive = FunctionEditorLevel.None;
    [SerializeField] private Button m_AddButton;
    [SerializeField] private Button m_MoveButton;
    [SerializeField] private Button m_DeleteButton;
    [SerializeField] private Button m_SaveButton;
    [SerializeField] private Button m_PlayButton;
    private Button LastButtonClick;
    private bool _IsButtonClicked = false;
    private GameObject _SphereInstantiate;
    [SerializeField] private GameObject _Player;

    public FunctionEditorLevel FunctionEditorLevelActive { get => _FunctionEditorLevelActive; set => _FunctionEditorLevelActive = value; }

    #region Event listener
    private void OnEnable()
    {
        SubscribeEvents();
        //Add event to the button AddButton and call the function ButtonClicked
        m_AddButton.onClick.AddListener(() => ButtonClicked(FunctionEditorLevel.Add, m_AddButton));
        m_MoveButton.onClick.AddListener(() => ButtonClicked(FunctionEditorLevel.Move, m_MoveButton));
        m_DeleteButton.onClick.AddListener(() => ButtonClicked(FunctionEditorLevel.Delete, m_DeleteButton));
        m_SaveButton.onClick.AddListener(() => ButtonClicked(FunctionEditorLevel.Save, m_SaveButton));
        m_PlayButton.onClick.AddListener(() => ButtonClicked(FunctionEditorLevel.Play, m_PlayButton));

    }
    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    private void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GameEditorLevelEvent>(GameEditorLevel);
        EventManager.Instance.AddListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
    }
    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GameEditorLevelEvent>(GameEditorLevel);
        EventManager.Instance.RemoveListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
    }
    #endregion

    #region Event callback
    private void GameEditorLevel(GameEditorLevelEvent e)
    {
        Debug.Log("Level editor has been instantiated");
        //Instantiate the level editor
        _EditorLevelInstance = Instantiate(_EditorLevel);
        //Set the script saver to the level editor
        _EditorLevelInstance.AddComponent<Saver>();
    }
    private void MainMenuButtonClicked(MainMenuButtonClickedEvent e)
    {
        if (_EditorLevel != null)
        {
            //Destroy the level editor
            Destroy(_EditorLevelInstance);
        }
    }
    public void ButtonClicked(FunctionEditorLevel functionEditorLevel, Button button)
    {

        if (_FunctionEditorLevelActive == functionEditorLevel)
        {
            _FunctionEditorLevelActive = FunctionEditorLevel.None;
            button.image.color = Color.white;
            LastButtonClick = null;
            _IsButtonClicked = false;
        }
        else
        {
            _FunctionEditorLevelActive = functionEditorLevel;
            button.image.color = Color.red;
            if (LastButtonClick != null)
            {
                LastButtonClick.image.color = Color.white;
            }
            LastButtonClick = button;
            _IsButtonClicked = true;
        }
        switch (_FunctionEditorLevelActive)
        {
            case FunctionEditorLevel.Add:

                break;
            case FunctionEditorLevel.Save:
                EventManager.Instance.Raise(new SaveButtonEditorLevelClickedEvent());
                break;
            case FunctionEditorLevel.Play:
                if (_EditorLevelInstance.GetComponent<BezierSpline>().CtrlTransform.Count >= 4)
                {
                    EventManager.Instance.Raise(new PlayButtonEditorLevelClickedEvent());
                }
                else
                {
                    Debug.Log("You need at least 4 points to play");
                }

                break;
            case FunctionEditorLevel.None:
                break;
            case FunctionEditorLevel.Delete:

                break;
            case FunctionEditorLevel.Move:

                break;
            default:
                break;
        }

    }
    #endregion

    #region System
    private void Update()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 1000);
        if (Input.GetMouseButtonDown(0) && _IsButtonClicked)
        {
            Debug.Log("Mouse button down");
            foreach (var item in hits)
            {
                Debug.Log(item.collider.gameObject.name);
            }
            if (hits.Any(x => x.collider.gameObject.layer == 6))
            {
                GameObject EditSphere = null;
                //hit with the tag EditSphere
                if (hits.Any(x => x.collider.gameObject.tag == "EditSphere"))
                {
                    EditSphere = hits.First(x => x.collider.gameObject.tag == "EditSphere").collider.gameObject;
                }

                switch (_FunctionEditorLevelActive)
                {
                    case FunctionEditorLevel.Add:
                        Debug.Log("Add");
                        if (EditSphere == null)
                        {
                            CreateSphere();
                            
                        }
                        break;
                    case FunctionEditorLevel.Move:
                        Debug.Log("Move");
                        if (EditSphere != null)
                        {
                           // MoveSphere(EditSphere);
                        }
                        break;
                    case FunctionEditorLevel.Delete:
                        Debug.Log("Delete");
                        if (EditSphere)
                        {
                            Debug.Log("Destroy");
                            EventManager.Instance.Raise(new DeleteSphereEvent() { Sphere = EditSphere });
                            Destroy(EditSphere);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
/*        if (Input.GetMouseButtonUp(0) && _IsButtonClicked)
        {

            if (_FunctionEditorLevelActive == FunctionEditorLevel.Move)
            {
                if (_SphereInstantiate != null)
                {
                    _SphereInstantiate.GetComponent<Rigidbody>().isKinematic = false;
                    _SphereInstantiate = null;
                }
            }

        }*/


    }
    #endregion

    #region Methods
    private void CreateSphere()
    {

        //Place the sphere on the position of the mouse
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000))
        {
            //if the raycast not near the player
            Debug.Log("Distance player hit : "+Vector3.Distance(hit.point, _Player.transform.position));
            if (Vector3.Distance(hit.point, _Player.transform.position) > 18.7)
            {
                //Instantiate a sphere
                _SphereInstantiate = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _SphereInstantiate.transform.position = new Vector3(0, 0, 0);
                _SphereInstantiate.transform.localScale = new Vector3(1, 1, 1);
                _SphereInstantiate.transform.position = new Vector3(hit.point.x, hit.point.y, 0);
                //remove the collider of the sphere
                Destroy(_SphereInstantiate.GetComponent<SphereCollider>());
                //add tag EditSphere
                _SphereInstantiate.tag = "EditSphere";
                //add script DragSphere
                _SphereInstantiate.AddComponent<DragSphere>();
                //add mesh collider
                _SphereInstantiate.AddComponent<MeshCollider>();
                //Set trigger to true
                _SphereInstantiate.GetComponent<Collider>().isTrigger = true;
                //add rigidbody
                _SphereInstantiate.AddComponent<Rigidbody>();
                //set rigidbody to kinematic
                _SphereInstantiate.GetComponent<Rigidbody>().isKinematic = true;
                //set rigidbody to use gravity
                _SphereInstantiate.GetComponent<Rigidbody>().useGravity = false;
                //set rigidbody Interpolate to true
                _SphereInstantiate.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                //Set parent to the _EditorLevelInstance
                _SphereInstantiate.transform.parent = _EditorLevelInstance.transform;
                EventManager.Instance.Raise(new AddSphereEvent() { Sphere = _SphereInstantiate });
            }            
        }
        


    }
    private void MoveSphere(GameObject EditSphere)
    {
        //Place the sphere on the position of the mouse
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000))
        {
            EditSphere.transform.position = hit.point;
        }
    }

    protected override IEnumerator InitCoroutine()
    {
        yield break;
    }
    #endregion

}

