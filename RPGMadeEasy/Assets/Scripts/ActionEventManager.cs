using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionEventManager : MonoBehaviour
{

	public  Dictionary<string,  StoryInfo.Page> pageDict;
	 
	public List<ActionEvent> actionEvents;

	void Start ()
	{
		pageDict = StoryGetters.GetPagesDictionary ();
		StartCoroutine (_RunEvents ());
	}

	IEnumerator _RunEvents ()
	{
		for (int i = 0; i < actionEvents.Count; i++) {
			ActionEvent actionEvent = actionEvents [i];

			switch (actionEvent.actionEventType) {

			case ActionEventType.setPos:

				//Instant
				if (actionEvent.target != null && actionEvent.targetTo != null)
					actionEvent.target.position = actionEvent.targetTo.position;

				//Lerp

				break;

			case ActionEventType.move:

				Vector3 endPos = actionEvent.target.position + actionEvent.moveDirection;

				if (actionEvent.waitType == WaitType.waitForEvent)
					yield return StartCoroutine (_Move (actionEvent.target, endPos, actionEvent.animationCurve));
				else
					StartCoroutine (_Move (actionEvent.target, endPos, actionEvent.animationCurve));


				break;

			case ActionEventType.debugLog:
				//Wait Function
				Debug.Log (actionEvent.text);
				break;

			case ActionEventType.dialouge:

				//StoryInfo.Page page = actionEvent.dialouge.linkedPage;
				//StoryInfo.PageLinkInfo pageLinkInfo = actionEvent.dialouge.pageLinkInfo;
				//Maybe call chat by pageGUID would solve EVE


				StoryInfo.Page page = pageDict [actionEvent.dialouge.linkedPageGUID];
				ChatManager.singletonInstance.OpenConversation (page.pageLinkInfo.charGUID, page.pageLinkInfo.conversationIndex, page.pageLinkInfo.pageIndex);

				while (ChatManager.singletonInstance.isChatActive) {
					yield return null;
				}

				//Next thing to do is create a wait for event!

				break;

			}

			switch (actionEvent.waitType) {

			case WaitType.waitForSeconds:
				//yield return new WaitForSeconds (actionEvent.waitTime);
				break;

			}

			//If waittime exist, just wait.
			if (actionEvent.waitTime > 0) {
				yield return new WaitForSeconds (actionEvent.waitTime);
			}




		}
	}

	IEnumerator _Move (Transform moveTarget, Vector3 endPos, AnimationCurve curve)
	{

	
		Vector3 startPos = moveTarget.position;

		float dist = Vector3.Distance (startPos, endPos);
		float speed = 6;
		float force = speed / dist;


		for (float i = 0; i < 1f; i += Time.deltaTime * force) {
			moveTarget.position = Vector3.Lerp (startPos, endPos, curve.Evaluate (i));
			yield return null;
		}
	}

	[System.Serializable]
	public class ActionEvent
	{
		public ActionEventType actionEventType;

		//Movement
		public Transform target;
		public AnimationCurve animationCurve = new AnimationCurve ();
		public Vector3 moveDirection;
		//Debug
		public string text = "default log";

		//Wait
		public float waitTime = 0;
		public WaitType waitType;
		public int lastIndex = 0;
		//Emoji

		//Dialouge
		[SerializeField]
		public Dialouge dialouge;

		//SetPosRot
		public Transform targetTo;

		public float myHeight;

		public bool hasButtons;

	}

	[System.Serializable]
	public class Dialouge
	{
		
		public string outputText = "Default text";
		public int dialougeIndex = 0;
		public int conversationIndex = 0;
		public int pageIndex = 0;
		public StoryElementType dialougeType;
		public StoryInfo.Page linkedPage;
		public StoryInfo.PageLinkInfo pageLinkInfo;
		public string linkedPageGUID = "";
	}

}

public enum ActionEventType
{
	move,
	teleport,
	emoji,
	waitForSeconds,
	debugLog,
	dialouge,
	setPos
}

public enum WaitType
{
	none,
	waitForSeconds,
	waitForEvent
}

public enum DialougeTypes
{
	character,
	chapter,
	continueChapter,
	custom
}