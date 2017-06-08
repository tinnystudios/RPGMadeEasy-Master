using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EasyEvent : EventBase
{

	public List<EasyEventInfo> events = new List<EasyEventInfo> ();
	public GameObject emojiPrefab;

	void Start ()
	{
		StartCoroutine (_RunEvents ());
	}

	#region Event Handler

	IEnumerator _RunEvents ()
	{
		for (int i = 0; i < events.Count; i++) {
			EasyEventInfo myEvent = events [i];

			switch (myEvent.eventType) {

			case EasyEventType.setPos:

				//Instant
				if (myEvent.moveType == MoveType.target) {
					if (myEvent.target != null && myEvent.targetTo != null)
						myEvent.target.position = myEvent.targetTo.position;
				}

				if (myEvent.moveType == MoveType.vector3) {
					if (myEvent.target != null)
						myEvent.target.transform.position = myEvent.moveDist;
						
				}

				//Lerp

				break;

			case EasyEventType.move:

				Vector3 endPos = myEvent.moveMethod.target.position + myEvent.moveMethod.moveDist;

				if (myEvent.waitType == WaitType.waitForEvent)
					yield return StartCoroutine (_Move (myEvent.moveMethod.target, endPos, myEvent.moveMethod.curve));
				else
					StartCoroutine (_Move (myEvent.moveMethod.target, endPos, myEvent.moveMethod.curve));


				break;

			case EasyEventType.log:
				//Wait Function
				Debug.Log (myEvent.outputText);
				break;

			case EasyEventType.dialouge:

				//Make sure there is a global class!
				Dictionary<string,  StoryInfo.Conversation> conDict = StoryGetters.GetConversationDict ();
				//null check
				StoryInfo.Conversation conversation = conDict [myEvent.conversationGUID];
				//ChatManager.singletonInstance.OpenConversation (conversation.linkInfo.dialougeGUID, conversation.linkInfo.conversationIndex, 0);


				ChatManager.singletonInstance.StartDialouge (myEvent.conversationGUID, myEvent.pageList);

				while (ChatManager.singletonInstance.isChatActive) {
					yield return null;
				}

				//Next thing to do is create a wait for event!

				break;

			case EasyEventType.instantiate:
				Instantiate (myEvent.instantiateTarget, myEvent.spawnTarget.position + myEvent.offset, Quaternion.identity);
				break;

			case EasyEventType.emoji:
				print ("emoji");
				GameObject emojiGO = Instantiate (myEvent.instantiateTarget, myEvent.spawnTarget.position + myEvent.offset, Quaternion.identity) as GameObject;
				emojiGO.GetComponent<EmojiInfo> ().SetSprite (myEvent.sprite);
				break;

			case EasyEventType.unityEvent:
				myEvent.unityEvent.Invoke ();
				break;

			case EasyEventType.animation:
				//myEvent.animator.SetBool


				if (myEvent.animParamType == AnimParamType.setInt) {
					myEvent.animator.SetInteger (myEvent.paramName, myEvent.intField);
				}

				if (myEvent.animParamType == AnimParamType.setFloat) {
					myEvent.animator.SetFloat (myEvent.paramName, myEvent.floatField);
				}

				if (myEvent.animParamType == AnimParamType.setBool) {
					myEvent.animator.SetBool (myEvent.paramName, myEvent.boolField);
				}

				if (myEvent.animParamType == AnimParamType.setTrigger) {
					myEvent.animator.SetTrigger (myEvent.paramName);
				}

				myEvent.animator.speed = myEvent.playSpeed;

				break;


			case EasyEventType.loop:

				if (myEvent.loopMethod.loopType == LoopType.amount) {
					myEvent.loopMethod.loopCounted++;
				}

				//If you can still loop, loop.
				if (myEvent.loopMethod.loopCounted <= myEvent.loopMethod.loopAmount)
					i = myEvent.loopMethod.goToIndex - 1;

				if (myEvent.loopMethod.loopType == LoopType.forever) {
					i = myEvent.loopMethod.goToIndex - 1;
				}

				break;

			}

			//If waittime exist, just wait.
			if (myEvent.waitTime > 0) {
				yield return new WaitForSeconds (myEvent.waitTime);
			}


		}
	}


	#endregion

	#region Event Info Class

	[System.Serializable]
	public class MoveMethod
	{
		public Transform target;
		public Transform targetTo;
		public AnimationCurve curve;
		public Vector3 moveDist;
		public Vector3 rotDist;
		public MoveType moveType;
		public ActorRefType actorRefType;
		public int actorIndex = 0;
		public Vector3 offset = new Vector3 (0, 1.5F, 0);
	}

	[System.Serializable]
	public class EasyEventInfo
	{
		public bool isVisible;
		public EasyEventType eventType;

		//Wait
		public WaitType waitType;
		public float waitTime = 0;

		public GameObject instantiateTarget;

		public MoveMethod moveMethod;
		public LoopMethod loopMethod;

		//Moving:
		public Transform target;
		public Transform targetTo;
		public AnimationCurve curve;
		public Vector3 moveDist;
		public Vector3 rotDist;
		public MoveType moveType;
		public ActorRefType actorRefType;
		public int actorIndex = 0;
		public Vector3 offset = new Vector3 (0, 1.5F, 0);

		//Set Visiblity
		public VisibilityType visiblityType;

		//Dialouge
		public StoryElementType dialougeType;
		public int dialougeIndex = 0;
		public int conversationIndex = 0;
		public string conversationGUID = "";
		public StoryInfo.Conversation selConversation;
		public bool hasButtons;
		public List<EasyButtons> easyButtons;
		public int pageIndex = 0;

		//Debug Log
		public string outputText = "Default output text";

		//GUI
		public float elementHeight;

		//Emoji
		public Sprite sprite;
		public Transform spawnTarget;

		//UnityEvent
		public UnityEvent unityEvent;

		//Animation
		public Animator animator;
		public string paramName = "DefaultName";
		public AnimParamType animParamType = AnimParamType.setTrigger;
		public int intField = 0;
		public float floatField = 0;
		public bool triggerField = false;
		public bool boolField = false;
		public AnimPlayMode animPlayMode = AnimPlayMode.play;
		public float playSpeed = 1;

		public List<Emoji> emojiList = new List<Emoji> ();
		public List<PageInfo> pageList = new List<PageInfo> ();
		public bool showPageInfo = false;

		public float yPos;
	}

	[System.Serializable]
	public class LoopMethod
	{
		public EasyEventInfo selectedEvent;
		public LoopType loopType;
		public int goToIndex = 0;
		public int loopAmount = 0;
		public int loopCounted = 0;
	}

	[System.Serializable]
	public class PageInfo
	{
		public bool showPage = false;
		public bool hasEvents = false;
		public bool hasButtons = false;
		public bool showEnd = false;
		public bool showStart = false;
		public List<PageEvent> eventEndList = new List<PageEvent> ();
		public List<PageEvent> eventStartList = new List<PageEvent> ();
	}

	[System.Serializable]
	public class UnityEventMethod
	{
		public UnityEvent anEvent;
	}

	[System.Serializable]
	public class PageEvent
	{
		public Emoji emoji;
		public MoveMethod moveMethod;
		public UnityEventMethod unityEventMethod;
		public PageEventType pageEventType = new PageEventType ();
		public bool isShow;
		public WaitType waitType;
		public float waitTime = 0;
	}

	[System.Serializable]
	public class Emoji
	{
		public Sprite sprite;
		public GameObject instantiateTarget;
		public GameObject target;
		public float waitTime = 0;
		public Vector3 offset = new Vector3 (0, 1.5F, 0);
		public PlayTiming playTiming;
	}

	[System.Serializable]
	public class EasyButtons
	{
		public string text = "default";
		public UnityEvent anEvent;
	}

	#endregion



}

public enum LoopType
{
	forever,
	amount,
	loopIf
}

public enum LoopTarget
{
	loopTo,
	start
}

public enum PageEventType
{
	move,
	emoji,
	animation,
	setActive,
	unityEvent
}

public enum EasyEventType
{
	move,
	setPos,
	setActive,
	emoji,
	waitForSeconds,
	dialouge,
	instantiate,
	unityEvent,
	animation,
	log,
	loop,
}

public enum VisibilityType
{
	off,
	on
}

public enum MoveType
{
	vector3,
	target
}

public enum ActorRefType
{
	manual,
	fromActorList
}

public enum AnimParamType
{
	none,
	setFloat,
	setInt,
	setBool,
	setTrigger
}

public enum AnimPlayMode
{
	none,
	stop,
	pause,
	play
}

public enum PlayTiming
{
	onStart,
	onEnd
}