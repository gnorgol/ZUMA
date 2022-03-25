using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SDD.Events;
using TMPro;
public class HudManager : Manager<HudManager>
{

	[Header("HudManager")]
	#region Labels & Values
	[Header("Texts")]
	[SerializeField] private GameObject m_HUD;
    [SerializeField] private TextMeshProUGUI m_TxtBestScore;
	[SerializeField] private TextMeshProUGUI m_TxtScore;
	[SerializeField] private TextMeshProUGUI m_TxtLevel;



	#endregion

	#region Manager implementation
	protected override IEnumerator InitCoroutine()
	{
		yield break;
	}
	#endregion

	#region Events subscription
	public override void SubscribeEvents()
	{
		base.SubscribeEvents();
	}
	public override void UnsubscribeEvents()
	{
		base.UnsubscribeEvents();
	}
	#endregion
	#region Callbacks to GameManager events
	protected override void GameStatisticsChanged(GameStatisticsChangedEvent e)
	{
        //change the text of the score and the best score
        m_TxtBestScore.text = "Best Score : " + e.eBestScore.ToString();
		m_TxtScore.text = "Score : " + e.eScore.ToString();
	}
    protected override void GamePlay(GamePlayEvent e)
    {
        //Show HUD
        m_HUD.SetActive(true);
    }
    protected override void GameOver(GameOverEvent e)
    {
        //Hide HUD
        m_HUD.SetActive(false);
    }
    protected override void GameMenu(GameMenuEvent e)
    {
        //Hide HUD
        m_HUD.SetActive(false);
    }
    
    #endregion
}
