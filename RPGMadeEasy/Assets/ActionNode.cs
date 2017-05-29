using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionNode : MonoBehaviour
{
	public NodeActionType nodeActionType;
	public StoryElementType dialougeType;
	public List<ConversationEvent> conversationEvents = new List<ConversationEvent> ();
	public List<UnityChatEvent> pageEvents = new List<UnityChatEvent> ();

	[HideInInspector]
	public int dialougeIndex = 0, conversationIndex = 0, pageIndex = 0;

	public List<SplinePath> splinePaths;

	[System.Serializable]
	public class ConversationEvent
	{
		public List<UnityChatEvent> pageEvents = new List<UnityChatEvent> ();
	}

	[System.Serializable]
	public class UnityChatEvent
	{
		public UnityEventDynamic onStart;
		public UnityEventDynamic onEnd;
	}

	[System.Serializable]
	public class UnityEventDynamic : UnityEvent
	{
		
	}


}
