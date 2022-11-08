using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class EditorLevelManager : MonoBehaviour
{
    [SerializeField] private GameObject _EditorLevel;
    GameObject _EditorLevelInstance;

    #region Event listener
    private void OnEnable()
    {
        SubscribeEvents();
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


}
