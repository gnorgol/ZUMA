using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using UnityEngine.UI;
public enum FunctionEditorLevel
{
    Add,
    Move,
    Delete,
    Save,
    Play,
    None
}
public class EditorLevelManager : MonoBehaviour
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
    private void GameEditorLevel(GameEditorLevelEvent e)
    {
        Debug.Log("Level editor has been instantiated");
        //Instantiate the level editor
        _EditorLevelInstance = Instantiate(_EditorLevel);
    }
    private void MainMenuButtonClicked(MainMenuButtonClickedEvent e)
    {
        if (_EditorLevel != null)
        {
            //Destroy the level editor
            Destroy(_EditorLevelInstance);
        }
    }
    public void ButtonClicked(FunctionEditorLevel functionEditorLevel,Button button)
    {
        Debug.Log("FunctionEditorLevelActive = " + _FunctionEditorLevelActive);
        Debug.Log("functionEditorLevel = " + functionEditorLevel);

        if (_FunctionEditorLevelActive == functionEditorLevel)
        {
            _FunctionEditorLevelActive = FunctionEditorLevel.None;
            button.image.color = Color.white;
            LastButtonClick = null;
            Debug.Log("LastButtonClick2 = " + LastButtonClick);
        }
        else
        {
            Debug.Log("LastButtonClick = " + LastButtonClick);
            _FunctionEditorLevelActive = functionEditorLevel;
            button.image.color = Color.red;
            if (LastButtonClick != null)
            {
                LastButtonClick.image.color = Color.white;
            }
            LastButtonClick = button;
        }
        
    }
    
}

