using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SDD.Events;
using System;
using System.Linq;
using TMPro;
public enum GameState { gameMenu, gamePlay, gameNextLevel, gamePause, gameOver, gameVictory }

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
    #endregion
    #region Level
    [SerializeField]
    private List<GameObject> m_Level;
    [SerializeField] TMP_Dropdown m_LevelDropdown;
    private int m_SelectLevel = 1;
    private GameObject currentLevel;
    int currentIdLevel = 0;
    private GameObject LevelExemple;
    private bool isPlayingSelectedLevel = false;
    private void GameLevelChanged(GameLevelChangedEvent e)
    {
        if (currentIdLevel != 0)
        {
            DestroyCurrentLevel();
        }
        if (!isPlayingSelectedLevel)
        {
            currentIdLevel = e.eLevel;
            if (currentIdLevel > m_Level.Count)
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
        LevelExemple = m_Level[m_SelectLevel - 1];
        LevelExemple = Instantiate(LevelExemple, LevelExemple.transform.position, Quaternion.identity);
        LevelExemple.name = "LevelExemple_" + m_SelectLevel;
        SetTimeScale(1);
        EventManager.Instance.Raise(new InstantiateLevelExempleEvent());
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
        GameObject level = m_Level[currentIdLevel - 1];
        currentLevel = Instantiate(level, level.transform.position, Quaternion.identity);
    }
    private void SelectLevelButtonHasBeenClicked(SelectLevelButtonHasBeenClickedEvent e)
    {
        InstantiateLevelExemple();
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
        //Score Item
        EventManager.Instance.AddListener<ScoreItemEvent>(ScoreHasBeenGained);

        //Level
        EventManager.Instance.AddListener<GameLevelChangedEvent>(GameLevelChanged);
        EventManager.Instance.AddListener<FinishCurveEvent>(FinishCurve);
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
        //Score Item
        EventManager.Instance.RemoveListener<ScoreItemEvent>(ScoreHasBeenGained);
        //Level
        EventManager.Instance.RemoveListener<GameLevelChangedEvent>(GameLevelChanged);


        EventManager.Instance.RemoveListener<FinishCurveEvent>(FinishCurve);
    }


    #endregion
    #region Manager implementation
    protected override IEnumerator InitCoroutine()
    {
        Menu();
        InitNewGame(); // essentiellement pour que les statistiques du jeu soient mise à jour en HUD
        m_LevelDropdown.ClearOptions();
        for (int i = 0; i < m_Level.Count; i++)
        {
            m_LevelDropdown.options.Add(new TMP_Dropdown.OptionData("Level " + (i + 1)));
        }
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
    private void ScoreHasBeenGained(ScoreItemEvent e)
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
    #endregion

    private void DestroyCurrentLevel()
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }
    }


    #region GameState methods
    private void Menu()
    {
        DestroyLevelExemple();
        DestroyCurrentLevel();
        currentIdLevel = 0;
        isPlayingSelectedLevel = false;
        m_Player.SetActive(false);
        m_GameState = GameState.gameMenu;
        SetTimeScale(0);

        /*if (MusicLoopsManager.Instance) MusicLoopsManager.Instance.PlayMusic(Constants.MENU_MUSIC);*/
        EventManager.Instance.Raise(new GameMenuEvent());
    }

    private void Play(bool IsSelected)
    {
        if (!IsSelected)
        {
            InitNewGame();
            m_Player.SetActive(true);
            SetTimeScale(1);

            EventManager.Instance.Raise(new GameLevelChangedEvent() { eLevel = 1 });
            m_GameState = GameState.gamePlay;
            isPlayingSelectedLevel = false;
            EventManager.Instance.Raise(new GamePlayEvent());
        }
        else
        {
            InitNewGame();
            m_Player.SetActive(true);
            SetTimeScale(1);
            EventManager.Instance.Raise(new GameLevelChangedEvent() { eLevel = m_SelectLevel });
            m_GameState = GameState.gamePlay;
            isPlayingSelectedLevel = true;
            EventManager.Instance.Raise(new GamePlayEvent());
        }


    }

    private void Pause()
    {
        if (!IsPlaying) return;
        SetTimeScale(0);
        m_Player.SetActive(false);
        GameStateBeforePause = m_GameState;
        m_GameState = GameState.gamePause;
        EventManager.Instance.Raise(new GamePauseEvent());
    }

    private void Resume()
    {
        if (IsPlaying) return;
        m_Player.SetActive(true);
        SetTimeScale(1);
        m_GameState = GameStateBeforePause;
        EventManager.Instance.Raise(new GameResumeEvent());
    }

    private void FinishCurve(FinishCurveEvent e)
    {

        Over();
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
        //if(SfxManager.Instance) SfxManager.Instance.PlaySfx2D(Constants.GAMEOVER_SFX);
    }

    #endregion



}
