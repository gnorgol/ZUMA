﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using System;

public abstract class SingletonGameStateObserver<T> :  Singleton<T>,IEventHandler where T:Component
{
	public virtual void SubscribeEvents()
	{
		EventManager.Instance.AddListener<GameMenuEvent>(GameMenu);
		EventManager.Instance.AddListener<GamePlayEvent>(GamePlay);
		EventManager.Instance.AddListener<GamePauseEvent>(GamePause);
		EventManager.Instance.AddListener<GameResumeEvent>(GameResume);
		EventManager.Instance.AddListener<GameOverEvent>(GameOver);
		EventManager.Instance.AddListener<GameVictoryEvent>(GameVictory);
		EventManager.Instance.AddListener<GameCreditEvent>(GameCredit);
		EventManager.Instance.AddListener<GameSettingEvent>(GameOption);
		EventManager.Instance.AddListener<GameStatisticsChangedEvent>(GameStatisticsChanged);
		EventManager.Instance.AddListener<GameEditorLevelEvent>(GameEditorLevel);
	}

	public virtual void UnsubscribeEvents()
	{
		EventManager.Instance.RemoveListener<GameMenuEvent>(GameMenu);
		EventManager.Instance.RemoveListener<GamePlayEvent>(GamePlay);
		EventManager.Instance.RemoveListener<GamePauseEvent>(GamePause);
		EventManager.Instance.RemoveListener<GameResumeEvent>(GameResume);
		EventManager.Instance.RemoveListener<GameOverEvent>(GameOver);
		EventManager.Instance.RemoveListener<GameVictoryEvent>(GameVictory);
        EventManager.Instance.RemoveListener<GameCreditEvent>(GameCredit);
		EventManager.Instance.RemoveListener<GameSettingEvent>(GameOption);
        EventManager.Instance.RemoveListener<GameStatisticsChangedEvent>(GameStatisticsChanged);
        EventManager.Instance.RemoveListener<GameEditorLevelEvent>(GameEditorLevel);
    }

	protected override void Awake()
	{
		base.Awake();
		SubscribeEvents();
	}

	protected virtual void OnDestroy()
	{
		UnsubscribeEvents();
	}

	protected virtual void GameMenu(GameMenuEvent e)
	{
	}

	protected virtual void GamePlay(GamePlayEvent e)
	{
	}

	protected virtual void GamePause(GamePauseEvent e)
	{
	}

	protected virtual void GameResume(GameResumeEvent e)
	{
	}

	protected virtual void GameOver(GameOverEvent e)
	{
	}

	protected virtual void GameVictory(GameVictoryEvent e)
	{
	}

	protected virtual void GameStatisticsChanged(GameStatisticsChangedEvent e)
	{
	}
    protected virtual void GameCredit(GameCreditEvent e)
    {
    }
	protected virtual void GameOption(GameSettingEvent e)
	{
    }
    protected virtual void GameEditorLevel(GameEditorLevelEvent e)
    {
    }

}
