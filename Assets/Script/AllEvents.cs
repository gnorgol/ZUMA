using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

#region GameManager Events
public class GameMenuEvent : SDD.Events.Event
{
}
public class GamePlayEvent : SDD.Events.Event
{
}
public class GamePlaySelectedEvent : SDD.Events.Event
{
}
public class GamePauseEvent : SDD.Events.Event
{
}
public class GameResumeEvent : SDD.Events.Event
{
}
public class GameOverEvent : SDD.Events.Event
{
}
public class GameVictoryEvent : SDD.Events.Event
{
}
public class GameCreditEvent : SDD.Events.Event
{
}
public class GameEditorLevelEvent : SDD.Events.Event
{
}
public class GameStatisticsChangedEvent : SDD.Events.Event
{
	public float eBestScore { get; set; }
	public float eScore { get; set; }
}
#endregion

#region MenuManager Events
public class EscapeButtonClickedEvent : SDD.Events.Event
{
}
public class PlayButtonClickedEvent : SDD.Events.Event
{
}
public class PlayButtonSelectLevelClickedEvent : SDD.Events.Event
{
}
public class ResumeButtonClickedEvent : SDD.Events.Event
{
}
public class MainMenuButtonClickedEvent : SDD.Events.Event
{
}

public class QuitButtonClickedEvent : SDD.Events.Event
{
}
public class CreditButtonClickedEvent : SDD.Events.Event
{
}
public class EditLevelButtonHasBeenClickedEvent : SDD.Events.Event
{
}
public class SelectLevelButtonHasBeenClickedEvent : SDD.Events.Event
{
}
#endregion

#region Score Event
public class GainScoreEvent : SDD.Events.Event
{
	public float eScore;
}
#endregion


public class putBallForwardEvent : SDD.Events.Event
{
	public GameObject target;
	public GameObject ball;

}
public class putBallBackEvent : SDD.Events.Event
{
	public GameObject target;
	public GameObject ball;

}

public class CheckMatchBallsEvent : SDD.Events.Event
{
	public GameObject ball;
}
public class AllColorsBallsCurveEvent : SDD.Events.Event
{
	public List<Color> ListColorsCurve;
}
public class GameLevelChangedEvent : SDD.Events.Event
{
	public int eLevel { get; set; }
}
public class FinishCurveEvent : SDD.Events.Event
{
}
public class InstantiateLevelExempleEvent : SDD.Events.Event
{
}
public class DestroyLevelExempleEvent : SDD.Events.Event
{
}
public class DestroyInstanceBallEvent : SDD.Events.Event
{
}
#region Sfx Event
public class PlayerShootEvent : SDD.Events.Event
{
}



#endregion


