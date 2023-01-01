using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using UnityEngine.UI;

public class MenuManager : Manager<MenuManager>
{

    [Header("MenuManager")]

    #region Panels
    [Header("Panels")]
    [SerializeField] GameObject m_PanelMainMenu;
    [SerializeField] GameObject m_PanelInGameMenu;
    [SerializeField] GameObject m_PanelGameVictory;
    [SerializeField] GameObject m_PanelGameOver;
    [SerializeField] GameObject m_PanelSelectLevel;
    [SerializeField] GameObject m_PanelCredit;
    [SerializeField] GameObject m_PanelEditorLevel;
    List<GameObject> m_AllPanels;

    #endregion

    #region Events' subscription
    public override void SubscribeEvents()
    {
        base.SubscribeEvents();


    }

    public override void UnsubscribeEvents()
    {
        base.UnsubscribeEvents();

    }
    #endregion

    #region Manager implementation
    protected override IEnumerator InitCoroutine()
    {
        yield break;
    }
    #endregion

    #region Monobehaviour lifecycle
    protected override void Awake()
    {
        base.Awake();
        RegisterPanels();
    }


    #endregion
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscapeButtonHasBeenClicked();
        }
    }
    #region Panel Methods
    void RegisterPanels()
    {
        m_AllPanels = new List<GameObject>();
        m_AllPanels.Add(m_PanelMainMenu);
        m_AllPanels.Add(m_PanelInGameMenu);
        m_AllPanels.Add(m_PanelGameVictory);
        m_AllPanels.Add(m_PanelGameOver);
        m_AllPanels.Add(m_PanelSelectLevel);
        m_AllPanels.Add(m_PanelCredit);
        m_AllPanels.Add(m_PanelEditorLevel);
    }

    void OpenPanel(GameObject panel)
    {
        foreach (var item in m_AllPanels)
            if (item) item.SetActive(item == panel);
    }
    void OpenOnePanel(GameObject panel)
    {
        panel.SetActive(true);
    }
    void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    #endregion

    #region UI OnClick Events
    public void EscapeButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new EscapeButtonClickedEvent());
    }
    public void PlayButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new PlayButtonClickedEvent());
    }
    public void PlayButtonSelectLevelHasBeenClicked()
    {
        EventManager.Instance.Raise(new PlayButtonSelectLevelClickedEvent());
    }
    public void SelectLevelButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new SelectLevelButtonHasBeenClickedEvent());
        OpenPanel(m_PanelSelectLevel);
    }
    public void ResumeButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new ResumeButtonClickedEvent());
    }

    public void MainMenuButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new MainMenuButtonClickedEvent());
    }

    public void QuitButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new QuitButtonClickedEvent());
    }
    public void CreditButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new CreditButtonClickedEvent());
    }
    public void EditLevelButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new EditLevelButtonHasBeenClickedEvent());
    }
    public void ButtonMuteHasBeenClicked(Button button)
    {
        EventManager.Instance.Raise(new ClickButtonMuteEvent() { button = button });
    }
    #endregion

    #region Callbacks to GameManager events


    protected override void GameMenu(GameMenuEvent e)
    {
        OpenPanel(m_PanelMainMenu);
    }
    protected override void GamePlay(GamePlayEvent e)
    {
        ClosePanel(m_PanelMainMenu);
        ClosePanel(m_PanelSelectLevel);
    }

    protected override void GamePause(GamePauseEvent e)
    {
        OpenOnePanel(m_PanelInGameMenu);
    }

    protected override void GameResume(GameResumeEvent e)
    {
        ClosePanel(m_PanelInGameMenu);
    }

    protected override void GameOver(GameOverEvent e)
    {
        OpenPanel(m_PanelGameOver);
    }
    protected override void GameVictory(GameVictoryEvent e)
    {
        OpenPanel(m_PanelGameVictory);
    }
    protected override void GameCredit(GameCreditEvent e)
    {
        OpenPanel(m_PanelCredit);
    }
    protected override void GameEditorLevel(GameEditorLevelEvent e)
    {
        OpenPanel(m_PanelEditorLevel);
    }
    #endregion
}
