using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SDD.Events;
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public enum GameState { gameMenu, gamePlay, gameNextLevel, gamePause, gameOver, gameVictory, gameCredit, gameEditLevel }

public class GameManager : Manager<GameManager>
{
    #region Game State
    private GameState m_GameState;

    private GameState GameStateBeforePause;
    public bool IsPlaying { get { return m_GameState == GameState.gamePlay; } }

    public bool IsMenu { get { return m_GameState == GameState.gameMenu; } }

    public bool IsPause { get { return m_GameState == GameState.gamePause; } }
    #endregion
    #region Score
    private float m_Score;
    public float Score
    {
        get { return m_Score; }
        set
        {
            m_Score = value;
            BestScore = Mathf.Max(BestScore, value);
        }
    }

    public float BestScore
    {
        get { return PlayerPrefs.GetFloat("BEST_SCORE", 0); }
        set { PlayerPrefs.SetFloat("BEST_SCORE", value); }
    }


    void IncrementScore(float increment)
    {
        SetScore(m_Score + increment);
    }

    void SetScore(float score, bool raiseEvent = true)
    {
        Score = score;

        if (raiseEvent)
            EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eBestScore = BestScore, eScore = m_Score });
    }
    #endregion
    #region Time
    void SetTimeScale(float newTimeScale)
    {
        Time.timeScale = newTimeScale;
    }
    #endregion
    #region Player
    [SerializeField]
    private GameObject m_Player;
    [SerializeField] private GameObject m_Ground;
    #endregion
    #region Level
    [SerializeField]
    private List<GameObject> m_ListLevel;

    [SerializeField]
    private List<string> _ListSaveLevel;
    [SerializeField]
    private Material _materialLineRenderer;
    [SerializeField] private List<GameObject> listBall;
    public List<GameObject> ListLevel { get => m_ListLevel; set => m_ListLevel = value; }
    public List<string> ListSaveLevel { get => _ListSaveLevel; set => _ListSaveLevel = value; }

    [SerializeField] TMP_Dropdown m_LevelDropdown;
    [SerializeField] private int m_SelectLevel = 1;
    private GameObject currentLevel;
    [SerializeField] int currentIdLevel = 0;
    private GameObject LevelExemple;
    [SerializeField] private bool isPlayingSelectedLevel = false;
    private void GameLevelChanged(GameLevelChangedEvent e)
    {
        if (currentIdLevel != 0)
        {
            DestroyCurrentLevel();
        }
        if (!isPlayingSelectedLevel)
        {
            currentIdLevel = e.eLevel;
            if (currentIdLevel > m_ListLevel.Count + _ListSaveLevel.Count)
            {
                Victory();
            }
            else
            {
                InstantiateLevel();
            }
        }
        else
        {
            Menu();
        }


    }
    public void SelectLevelChanged()
    {
        m_SelectLevel = m_LevelDropdown.value + 1;
        //Show Selected Level
        InstantiateLevelExemple();
    }

    private void InstantiateLevelExemple()
    {
        DestroyLevelExemple();
        if (m_SelectLevel <= m_ListLevel.Count)
        {
            LevelExemple = m_ListLevel[m_SelectLevel - 1];
        }
        else
        {
            LevelExemple = LoadLevel(_ListSaveLevel[m_SelectLevel - m_ListLevel.Count - 1]);            
        }
        LevelExemple = Instantiate(LevelExemple, LevelExemple.transform.position, Quaternion.identity);
        LevelExemple.name = "LevelExemple_" + m_SelectLevel;
        LevelExemple.GetComponent<BezierSpline>().IsExemple = true;
        LevelExemple.GetComponent<BezierSpline>().Repeat = true;
        LevelExemple.GetComponent<BezierSpline>().enabled = true;
        LevelExemple.GetComponent<BezierSpline>().SetInstantiate(true);
        SetTimeScale(1);
    }
    private void DestroyLevelExemple()
    {
        if (LevelExemple != null)
        {
            EventManager.Instance.Raise(new DestroyLevelExempleEvent());
            Destroy(LevelExemple);
        }
    }
    private void InstantiateLevel()
    {
        GameObject level = null;
        if (currentIdLevel <= m_ListLevel.Count)
        {
            level = m_ListLevel[currentIdLevel - 1];
        }
        else
        {
            Debug.Log("Load Level");
            level = LoadLevel(_ListSaveLevel[currentIdLevel - m_ListLevel.Count - 1]);
            
        }

        currentLevel = Instantiate(level, level.transform.position, Quaternion.identity);
        currentLevel.name = "Level_" + currentIdLevel;
        currentLevel.GetComponent<BezierSpline>().enabled = true;
        currentLevel.GetComponent<BezierSpline>().SetInstantiate(true);
    }


    #endregion
    #region Events' subscription
    public override void SubscribeEvents()
    {
        base.SubscribeEvents();

        //MainMenuManager
        EventManager.Instance.AddListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
        EventManager.Instance.AddListener<PlayButtonClickedEvent>(PlayButtonClicked);
        EventManager.Instance.AddListener<PlayButtonSelectLevelClickedEvent>(PlayButtonSelectLevelClicked);
        EventManager.Instance.AddListener<ResumeButtonClickedEvent>(ResumeButtonClicked);
        EventManager.Instance.AddListener<EscapeButtonClickedEvent>(EscapeButtonClicked);
        EventManager.Instance.AddListener<QuitButtonClickedEvent>(QuitButtonClicked);
        EventManager.Instance.AddListener<SelectLevelButtonHasBeenClickedEvent>(SelectLevelButtonHasBeenClicked);
        EventManager.Instance.AddListener<CreditButtonClickedEvent>(CreditButtonClicked);
        EventManager.Instance.AddListener<EditLevelButtonHasBeenClickedEvent>(EditLevelButtonHasBeenClicked);
        //Score Item
        EventManager.Instance.AddListener<GainScoreEvent>(ScoreHasBeenGained);

        //Level
        EventManager.Instance.AddListener<GameLevelChangedEvent>(GameLevelChanged);
        EventManager.Instance.AddListener<FinishCurveEvent>(FinishCurve);
        EventManager.Instance.AddListener<SaveCurveEvent>(SaveCurve);
    }



    public override void UnsubscribeEvents()
    {
        base.UnsubscribeEvents();

        //MainMenuManager
        EventManager.Instance.RemoveListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
        EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(PlayButtonClicked);
        EventManager.Instance.RemoveListener<PlayButtonSelectLevelClickedEvent>(PlayButtonSelectLevelClicked);
        EventManager.Instance.RemoveListener<ResumeButtonClickedEvent>(ResumeButtonClicked);
        EventManager.Instance.RemoveListener<EscapeButtonClickedEvent>(EscapeButtonClicked);
        EventManager.Instance.RemoveListener<QuitButtonClickedEvent>(QuitButtonClicked);
        EventManager.Instance.RemoveListener<SelectLevelButtonHasBeenClickedEvent>(SelectLevelButtonHasBeenClicked);
        EventManager.Instance.RemoveListener<CreditButtonClickedEvent>(CreditButtonClicked);
        EventManager.Instance.RemoveListener<EditLevelButtonHasBeenClickedEvent>(EditLevelButtonHasBeenClicked);
        //Score Item
        EventManager.Instance.RemoveListener<GainScoreEvent>(ScoreHasBeenGained);
        //Level
        EventManager.Instance.RemoveListener<GameLevelChangedEvent>(GameLevelChanged);
        EventManager.Instance.RemoveListener<FinishCurveEvent>(FinishCurve);
        EventManager.Instance.RemoveListener<SaveCurveEvent>(SaveCurve);

    }


    #endregion
    #region Manager implementation
    protected override IEnumerator InitCoroutine()
    {
        Menu();
        InitNewGame(); // essentiellement pour que les statistiques du jeu soient mise à jour en HUD
        GetAllSaveLevel();
        UpdateListDropDown();

        yield break;
    }
    #endregion
    #region Game flow & Gameplay
    void InitNewGame(bool raiseStatsEvent = true)
    {
        SetScore(0);

    }
    #endregion


    #region Callbacks to events issued by Score items
    private void ScoreHasBeenGained(GainScoreEvent e)
    {
        if (IsPlaying)
        {
            IncrementScore(e.eScore);

        }
    }
    #endregion
    #region Callbacks to Events issued by MenuManager
    private void MainMenuButtonClicked(MainMenuButtonClickedEvent e)
    {
        if (!IsPlaying)
            Menu();
    }

    private void PlayButtonClicked(PlayButtonClickedEvent e)
    {
        Play(false);
    }
    private void PlayButtonSelectLevelClicked(PlayButtonSelectLevelClickedEvent e)
    {
        DestroyLevelExemple();
        Play(true);
    }
    private void ResumeButtonClicked(ResumeButtonClickedEvent e)
    {
        Resume();
    }

    private void EscapeButtonClicked(EscapeButtonClickedEvent e)
    {
        if (IsPlaying) Pause();
    }

    private void QuitButtonClicked(QuitButtonClickedEvent e)
    {
        Application.Quit();
    }
    private void SelectLevelButtonHasBeenClicked(SelectLevelButtonHasBeenClickedEvent e)
    {
        UpdateListDropDown();
        InstantiateLevelExemple();
    }
    private void CreditButtonClicked(CreditButtonClickedEvent e)
    {
        Credit();
    }
    private void EditLevelButtonHasBeenClicked(EditLevelButtonHasBeenClickedEvent e)
    {
        EditLevel();
    }

    #endregion

    private void DestroyCurrentLevel()
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }
    }
    private void SaveCurve(SaveCurveEvent e)
    {
        GetAllSaveLevel();
    }


    #region GameState methods
    private void Menu()
    {
        DestroyLevelExemple();
        DestroyCurrentLevel();
        currentIdLevel = 0;
        isPlayingSelectedLevel = false;
        EventManager.Instance.Raise(new DestroyInstanceBallEvent());
        m_Player.SetActive(false);
        m_GameState = GameState.gameMenu;
        SetTimeScale(0);

        EventManager.Instance.Raise(new GameMenuEvent());
    }

    private void Play(bool IsSelected)
    {
        
        m_Ground.SetActive(true);
        InitNewGame();
        m_Player.SetActive(true);
        SetTimeScale(1);
        m_GameState = GameState.gamePlay;
        if (!IsSelected)
        {
            EventManager.Instance.Raise(new GameLevelChangedEvent() { eLevel = 1 });
            isPlayingSelectedLevel = false;
        }
        else
        {
            EventManager.Instance.Raise(new GameLevelChangedEvent() { eLevel = m_SelectLevel });
            isPlayingSelectedLevel = true;
        }
        
        EventManager.Instance.Raise(new GamePlayEvent());
    }

    private void Pause()
    {
        if (!IsPlaying) return;
        SetTimeScale(0);
        //m_Player.SetActive(false);
        m_Ground.SetActive(false);
        GameStateBeforePause = m_GameState;
        m_GameState = GameState.gamePause;
        EventManager.Instance.Raise(new GamePauseEvent());
    }

    private void Resume()
    {
        if (IsPlaying) return;
        m_Player.SetActive(true);
        m_Ground.SetActive(true);
        SetTimeScale(1);
        m_GameState = GameStateBeforePause;
        EventManager.Instance.Raise(new GameResumeEvent());
    }
    private void Credit()
    {
        if (IsPlaying) return;
        m_Player.SetActive(false);
        m_Ground.SetActive(false);
        SetTimeScale(0);
        m_GameState = GameState.gameCredit;
        EventManager.Instance.Raise(new GameCreditEvent());
    }
    private void EditLevel()
    {
        if (IsPlaying) return;
        m_Player.SetActive(false);
        m_Ground.SetActive(false);
        SetTimeScale(1);
        m_GameState = GameState.gameEditLevel;
        EventManager.Instance.Raise(new GameEditorLevelEvent());
    }
    private void FinishCurve(FinishCurveEvent e)
    {
        if (IsPlaying)
        {
            Over();
        }
        
    }
    private void Victory()
    {
        DestroyCurrentLevel();
        m_Player.SetActive(false);
        SetTimeScale(0);
        m_GameState = GameState.gameVictory;
        EventManager.Instance.Raise(new GameVictoryEvent());
    }
    private void Over()
    {
        DestroyCurrentLevel();
        m_Player.SetActive(false);
        SetTimeScale(0);
        m_GameState = GameState.gameOver;
        EventManager.Instance.Raise(new GameOverEvent());
    }

    private void UpdateListDropDown()
    {

        m_LevelDropdown.ClearOptions();
        for (int i = 0; i < m_ListLevel.Count; i++)
        {
            if (m_ListLevel[i].GetComponent<BezierSpline>() != null)
            {
                m_LevelDropdown.options.Add(new TMP_Dropdown.OptionData(m_ListLevel[i].name));
            }

        }
        for (int i = 0; i < _ListSaveLevel.Count; i++)
        {
            m_LevelDropdown.options.Add(new TMP_Dropdown.OptionData(_ListSaveLevel[i]));
        }
    }
    private void GetAllSaveLevel()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.dat");
        _ListSaveLevel = files.Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
    }
    private GameObject LoadLevel(string fileName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        //FileStream stream = new FileStream(PathFileSaveLevel + fileName + ".dat", FileMode.Open);
        FileStream stream = new FileStream(Application.persistentDataPath + "/" + fileName + ".dat", FileMode.Open);
        GameData gameData = formatter.Deserialize(stream) as GameData;
        stream.Close();

        GameObject levelSaved = new GameObject();
        levelSaved.transform.position = new Vector3(0, 1, 0);

        //add lineRenderer
        LineRenderer lineRenderer = levelSaved.AddComponent<LineRenderer>();
        lineRenderer.material = _materialLineRenderer;

        List<Transform> m_CtrlTransform = new List<Transform>();
        foreach (var item in gameData.transformList)
        {
            //Create spehere with the name and the possition
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //disable mesh sphere
            sphere.GetComponent<MeshRenderer>().enabled = false;
            sphere.name = item.name;
            //set sphere in child of the levelSaved
            sphere.transform.parent = levelSaved.transform;
            sphere.transform.position = new Vector3(item.posX, item.posY, item.posZ);
            //add the sphere to the list
            m_CtrlTransform.Add(sphere.transform);
            
        }
        BezierSpline bezierSplineCompenent = levelSaved.AddComponent<BezierSpline>();
        bezierSplineCompenent.enabled = false;
        bezierSplineCompenent.CtrlTransform = m_CtrlTransform;
        bezierSplineCompenent.NbPtsOnSpline = gameData.m_NbPtsOnSpline;
        bezierSplineCompenent.IsClosed = gameData.m_IsClosed;
        bezierSplineCompenent.PtsDensity = gameData.m_PtsDensity;
        bezierSplineCompenent.TranslationSpeed = gameData.m_TranslationSpeed;
        bezierSplineCompenent.Direction = gameData.m_Direction;
        bezierSplineCompenent.Repeat = false;
        bezierSplineCompenent.NbBall = gameData._nbBall;
        bezierSplineCompenent.IdLevel = gameData._idLevel;
        bezierSplineCompenent.ListBall = listBall;
        //active BezierSpline
        Destroy(levelSaved);
        return levelSaved;
    }
    #endregion



}
