using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EasyEvent : EventBase
{

	public List<EasyEventInfo> events = new List<EasyEventInfo> ();
	public GameObject emojiPrefab;
	public TriggerType triggerType;

	void Start ()
	{
		if (triggerType == TriggerType.onAwake)
			StartCoroutine (_RunEvents ());
	}

	public void EndEvent ()
	{
		StopAllCoroutines ();
		gameObject.SetActive (false);
	}

	public void StartEvent ()
	{
		StartCoroutine (_RunEvents ());
	}

	#region Event Handler

	IEnumerator _RunEvents ()
	{
		for (int i = 0; i < events.Count; i++) {
			EasyEventInfo myEvent = events [i];
			MoveMethod moveMethod = myEvent.moveMethod;
			DialougeMethod dialougeMethod = myEvent.dialougeMethod;
			AnimationMethod animationMethod = myEvent.animationMethod;
			InstantiateMethod instantiateMethod = myEvent.instantiateMethod;
			EasyEventMethod easyEventMethod = myEvent.easyEventMethod;

			switch (myEvent.eventType) {

			case EasyEventType.setPos:

				//Instant
				if (moveMethod.moveType == MoveType.target) {
					if (moveMethod.target != null && moveMethod.targetTo != null)
						moveMethod.target.position = moveMethod.targetTo.position;
				}

				if (moveMethod.moveType == MoveType.vector3) {
					if (moveMethod.target != null)
						moveMethod.target.transform.position = moveMethod.moveDist;

				}

				//Lerp

				break;

			case EasyEventType.move:

				Vector3 endPos = moveMethod.target.position + moveMethod.moveDist;

				if (myEvent.waitType == WaitType.waitForEvent)
					yield return StartCoroutine (_Move (moveMethod.target, endPos, moveMethod.curve));
				else
					StartCoroutine (_Move (moveMethod.target, endPos, moveMethod.curve));


				break;

			case EasyEventType.log:
				//Wait Function
				Debug.Log (myEvent.outputText);
				break;

			case EasyEventType.dialouge:
				yield return new WaitForFixedUpdate ();

				//Make sure there is a global class!
				Dictionary<string,  StoryInfo.Conversation> conDict = StoryGetters.GetConversationDict ();
				StoryInfo.Conversation conversation = conDict [dialougeMethod.conversationGUID];
		
				ChatManager.singletonInstance.StartDialouge (dialougeMethod.conversationGUID, myEvent.pageList);
		
				if (myEvent.waitType == WaitType.waitForEvent) {
					while (ChatManager.singletonInstance.isChatActive) {
						yield return null;
					}
				}

				//Next thing to do is create a wait for event!

				break;

			case EasyEventType.instantiate:
				Instantiate (instantiateMethod.instanTarget, instantiateMethod.spawnTarget.position + instantiateMethod.offset, Quaternion.identity);
				break;

			case EasyEventType.emoji:
				print ("emoji");
				GameObject emojiGO = Instantiate (instantiateMethod.emojiTarget, instantiateMethod.spawnTarget.position + instantiateMethod.offset, Quaternion.identity) as GameObject;
				emojiGO.GetComponent<EmojiInfo> ().SetSprite (instantiateMethod.sprite);
				break;

			case EasyEventType.unityEvent:
				myEvent.unityEvent.Invoke ();
				break;

			case EasyEventType.animation:
				//myEvent.animator.SetBool

				if (animationMethod.animParamType == AnimParamType.setInt) {
					animationMethod.animator.SetInteger (animationMethod.paramName, animationMethod.intField);
				}

				if (animationMethod.animParamType == AnimParamType.setFloat) {
					animationMethod.animator.SetFloat (animationMethod.paramName, animationMethod.floatField);
				}

				if (animationMethod.animParamType == AnimParamType.setBool) {
					animationMethod.animator.SetBool (animationMethod.paramName, animationMethod.boolField);
				}

				if (animationMethod.animParamType == AnimParamType.setTrigger) {
					animationMethod.animator.SetTrigger (animationMethod.paramName);
				}

				animationMethod.animator.speed = animationMethod.playSpeed;

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

			case EasyEventType.easyEvent:
				if (easyEventMethod.visibility == VisibilityType.on) {
					StartEvent ();
				}
				if (easyEventMethod.visibility == VisibilityType.off) {
					EndEvent ();
				}


				//You can do a 'wait till this easy event' has finished! 
				//So myEvent.EasyEvent.InProgress

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
	public class EasyEventInfo
	{
		public bool isVisible;
		public EasyEventType eventType;

		//Wait
		public WaitType waitType;
		public float waitTime = 0;

		public InstantiateMethod instantiateMethod;
		public MoveMethod moveMethod;
		public LoopMethod loopMethod;
		public AnimationMethod animationMethod;
		public DialougeMethod dialougeMethod;
		public EasyEventMethod easyEventMethod;
		//Debug Log
		public string outputText = "Default output text";
		//GUI
		public float elementHeight;
		public UnityEvent unityEvent;

		public List<Emoji> emojiList = new List<Emoji> ();

		//Have a page list here.
		public bool showPageInfo = false;
		public int pageIndex = 0;
		public List<PageInfo> pageList = new List<PageInfo> ();
	}


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
		public VisibilityType visibilityType;
	}

	[System.Serializable]
	public class InstantiateMethod
	{
		public GameObject instanTarget;
		public GameObject emojiTarget;
		public Vector3 offset = new Vector3 (0, 1.5F, 0);
		public Sprite sprite;
		public Transform spawnTarget;

	}

	[System.Serializable]
	public class DialougeMethod
	{
		//Dialouge
		public StoryElementType dialougeType;
		public int dialougeIndex = 0;
		public int conversationIndex = 0;
		public string conversationGUID = "";
		public StoryInfo.Conversation selConversation;
		public bool hasButtons;

		//Debug Log
		public string outputText = "Default output text";

	}

	[System.Serializable]
	public class AnimationMethod
	{
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
	public class EasyEventMethod
	{
		public EasyEvent easyEvent;
		public VisibilityType visibility;
	}

	[System.Serializable]
	public class PageInfo
	{
		public bool showPage = false;
		public bool hasEvents = false;
		public bool hasButtons = false;
		public bool showEnd = false;
		public bool showStart = false;
		public bool showButtons = false;
		public List<PageEvent> eventEndList = new List<PageEvent> ();
		public List<PageEvent> eventStartList = new List<PageEvent> ();
		public List<PageEvent> eventButtonList = new List<PageEvent> ();
		public List<EasyButtons> easyButtons = new List<EasyButtons> ();

	}

	[System.Serializable]
	public class EasyButtons
	{
		public string text = "default response";
		public UnityEvent anEvent;
		public EasyEventType eventType;
		public DialougeMethod dialougeMethod;
		public EasyEventMethod easyEventMethod;
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
		public bool isShow;
		public WaitType waitType;
		public float waitTime = 0;

		public EasyEventType eventType;
		public UnityEvent anEvent;
		public DialougeMethod dialougeMethod;
		public EasyEventMethod easyEventMethod;
		public string buttonText = "Default Response";

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
	unityEvent,
	easyEvent,
	dialouge
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
	easyEvent
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

public enum TriggerType
{
	none,
	onAwake,
	onTrigger,
	onCollision
}