﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : EventBase
{

	public static ChatManager _instance;

	public GameObject displayChat;
	public Text textChat;
	public Text textSpeaker;
	public Transform buttonGroup;
	public List<ResponseButton> buttons;

	public StoryInfo storyInfo;
	public Dictionary<string,  StoryInfo.StoryBase> charDict = new Dictionary<string, StoryInfo.StoryBase> ();
	public Dictionary<string,  StoryInfo.Page> pageDict = new Dictionary<string, StoryInfo.Page> ();
	public Dictionary<string,  StoryInfo.Conversation> conDict = new Dictionary<string, StoryInfo.Conversation> ();

	public Chat chat;
	public Coroutine coroutineConversation;
	public Coroutine textCoroutine;
	public bool isChatActive;
	public float speedText = 0.01F;

	void Awake ()
	{

		conDict = StoryGetters.GetConversationDict ();

		//	charDict = StoryGetters.GetCharacterDictionary (storyInfo.characters);
		charDict = StoryGetters.GetStoryElementDictionary (storyInfo);

		CloseConversation ();

	}

	void Update ()
	{
		//When a character opens a conversation
		if (Input.GetKeyDown (KeyCode.C))
			OpenConversation (chat.GUID, chat.conversationIndex, chat.pageIndex);

		//Get chapters GUID
		if (Input.GetKeyDown (KeyCode.S))
			OpenConversation (storyInfo.chapters [0].GUID, 0, 0);
	}


	//Open conversation with emoji & buttons

	public void StartDialouge (string conGUID, List<EasyEvent.PageInfo> pageEvents)
	{
	
		//Check page event count;
		StartCoroutine (_StartDialouge (conGUID, pageEvents));
	}

	IEnumerator _StartDialouge (string conGUID, List<EasyEvent.PageInfo> pageEvents)
	{
		isChatActive = true;
		int pIndex = 0;


		StoryInfo.Conversation conversation = conDict [conGUID];

		while (pIndex < conversation.pages.Count) {
			StoryInfo.Page page = conversation.pages [pIndex];
			EasyEvent.PageInfo pageEvent = pageEvents [pIndex];

			//Check for the type of 'action'

			//if (myEmoji.playTiming == PlayTiming.onStart)
			//yield return StartCoroutine (_PlayEmoji (myEmoji));

			#region Events
			PlayEvent (pageEvent.eventStartList);
			#endregion

			#region Reset values logic
			displayChat.SetActive (true);	
			textChat.text = "";
			HideButtons ();
			#endregion

			#region ### Output Text Logic ### 

			Dictionary<string,string> tagNameDict = StoryGetters.GetTagNames ();

			string outputText = StoryGetters.GetStringAndAppendFromTag (tagNameDict, page.text);
			string speakerText = StoryGetters.GetStringAndAppendFromTag (tagNameDict, page.speakerName);
			textSpeaker.text = speakerText;

			if (outputText.Contains ("#")) {
				outputText = StoryGetters.GetStringAndAppendFromTag (tagNameDict, page.text);
			}

			textCoroutine = StartCoroutine (PrintText (outputText));
			#endregion

			#region ### Space to complete text ###
			while (textChat.text != outputText) {

				if (Input.GetKeyDown (KeyCode.Space))
					break;

				yield return null;
			}


			//Wait until the frame ends
			yield return new WaitForEndOfFrame ();

			StopCoroutine (textCoroutine);
			textChat.text = outputText;

			#endregion

			#region ### Closing Dialouge Logic ###
			bool canSkip = true;

			//Buttons
			#region Buttons

			//Has buttons
			if (pageEvent.easyButtons.Count > 0) {
				for (int i = 0; i < pageEvent.easyButtons.Count; i++) {
					buttons [i].gameObject.SetActive (true);
					buttons [i].text.text = pageEvent.easyButtons [i].text;
					buttons [i].myEvent = pageEvent.easyButtons [i].anEvent;
				}

			}

			#endregion

			/* (this uses the WindowEditor ones)
			//If there are buttons, you cannot skip
			if (page.buttonInfos.Count > 0) {
				canSkip = false;
			}
			*/


			#endregion

			#region ### Space to close dialouge ###
			while (true) {
				if (Input.GetKeyDown (KeyCode.Space) && canSkip) {
					break;
				}
				yield return null;
			}
			#endregion




			#region ### Emoji ###

			//Check For events
			PlayEvent (pageEvent.eventEndList);






			/*
			//Maybe I should have a 'boolean' for it.
			if (pageEvents [pIndex].emoji.sprite != null) {
				//Spawn emoji
				EasyEvent.Emoji myEmoji = pageEvents [pIndex].emoji;
				GameObject emojiGO = Instantiate (myEmoji.instantiateTarget, myEmoji.target.transform.position + myEmoji.offset, Quaternion.identity) as GameObject;
				emojiGO.GetComponent<EmojiInfo> ().SetSprite (myEmoji.sprite);
			}
			*/

			#endregion




			pIndex++;
			yield return null;
		}

		CloseConversation ();
		isChatActive = false;
	}

	public void PlayEvent (List<EasyEvent.PageEvent> pageEvents)
	{
		StartCoroutine (_PlayEvent (pageEvents));
	}

	IEnumerator _PlayEvent (List<EasyEvent.PageEvent> pageEvents)
	{
		for (int i = 0; i < pageEvents.Count; i++) {

			EasyEvent.PageEvent myEvent = pageEvents [i];

			switch (pageEvents [i].pageEventType) {

			case PageEventType.move:

				Vector3 endPos = myEvent.moveMethod.target.position + myEvent.moveMethod.moveDist;

				if (myEvent.waitType == WaitType.waitForEvent)
					yield return StartCoroutine (_Move (myEvent.moveMethod.target, endPos, myEvent.moveMethod.curve));
				else
					StartCoroutine (_Move (myEvent.moveMethod.target, endPos, myEvent.moveMethod.curve));

				break;

			case PageEventType.emoji:
				StartCoroutine (_PlayEmoji (pageEvents [i].emoji));
				break;

			}

			yield return new WaitForSeconds (pageEvents [i].waitTime);

		}
	}

	public void PlayEmoji (EasyEvent.Emoji myEmoji)
	{
		//if (myEmoji.playTiming != PlayTiming.none) {
		GameObject emojiGO = Instantiate (myEmoji.instantiateTarget, myEmoji.target.transform.position + myEmoji.offset, Quaternion.identity) as GameObject;
		emojiGO.GetComponent<EmojiInfo> ().SetSprite (myEmoji.sprite);
		//}
	}

	IEnumerator _PlayEmoji (EasyEvent.Emoji myEmoji)
	{

		//if (myEmoji.playTiming != PlayTiming.none) {
		GameObject emojiGO = Instantiate (myEmoji.instantiateTarget, myEmoji.target.transform.position + myEmoji.offset, Quaternion.identity) as GameObject;
		emojiGO.GetComponent<EmojiInfo> ().SetSprite (myEmoji.sprite);
		yield return new WaitForSeconds (myEmoji.waitTime);
		//}

	}

	public void OpenConversation (string charGUID, int cIndex, int pIndex)
	{
		if (!isChatActive)
			coroutineConversation = StartCoroutine (_OpenConversation (charGUID, cIndex, pIndex));
	}

	//Need to be able to start a conversation with just 'conversation'

	IEnumerator _OpenConversation (string charGUID, int cIndex, int pIndex)
	{
		isChatActive = true;

		if (charDict.ContainsKey (charGUID)) {
			
			displayChat.SetActive (true);	
			StoryInfo.StoryBase storyBase = charDict [charGUID];
			StoryInfo.Conversation conversation = storyBase.conversations [cIndex];

			//Page
			while (pIndex < conversation.pages.Count) {
				
				StoryInfo.Page page = storyBase.conversations [cIndex].pages [pIndex];
				textChat.text = "";
				HideButtons ();


				Dictionary<string,string> tagNameDict = StoryGetters.GetTagNames ();
				string outputText = StoryGetters.GetStringAndAppendFromTag (tagNameDict, page.text);
				string speakerText = StoryGetters.GetStringAndAppendFromTag (tagNameDict, page.speakerName);
				textSpeaker.text = speakerText;
				if (outputText.Contains ("#")) {
					outputText = StoryGetters.GetStringAndAppendFromTag (tagNameDict, page.text);
				}

				textCoroutine = StartCoroutine (PrintText (outputText));

				while (textChat.text != outputText) {
				
					if (Input.GetKeyDown (KeyCode.Space))
						break;
				
					yield return null;
				}

				StopCoroutine (textCoroutine);
				textChat.text = outputText;

				//Check for buttons
				for (int i = 0; i < page.buttonInfos.Count; i++) {
					buttons [i].buttonInfo = page.buttonInfos [i];
					buttons [i].text.text = page.buttonInfos [i].responseText;
					buttons [i].chatManager = GetComponent<ChatManager> ();
					buttons [i].gameObject.SetActive (true);
				}

				yield return new WaitForEndOfFrame ();
			
				bool canSkip = true;

				if (page.buttonInfos.Count > 0) {
					canSkip = false;
				}

				while (true) {
					if (Input.GetKeyDown (KeyCode.Space) && canSkip) {
						break;
					}
					yield return null;
				}

				//Maybe there will be a vertification
				if (page.dynamicButtonInfo.buttonType == ButtonType.close) {
					CloseConversation ();
				}

				if (page.dynamicButtonInfo.buttonType == ButtonType.goTo) {
					StoryInfo.ButtonInfo buttonInfo = page.dynamicButtonInfo;
					CloseConversation (true);
					StartCoroutine (DelayOpenConversation (buttonInfo.charGUID, buttonInfo.conversationIndex, buttonInfo.pageIndex));
				}

				//Linear method, page will add
				if (page.dynamicButtonInfo.buttonType == ButtonType.none) {
					pIndex++;
				}

				yield return null;
			}

		}

		CloseConversation ();
		isChatActive = false;
	}

	IEnumerator DelayOpenConversation (string charGUID, int cIndex, int pIndex, float waitTime)
	{
		yield return new WaitForSeconds (waitTime);
		OpenConversation (charGUID, cIndex, pIndex);
	}

	IEnumerator DelayOpenConversation (string charGUID, int cIndex, int pIndex)
	{
		yield return new WaitForEndOfFrame ();
		OpenConversation (charGUID, cIndex, pIndex);
	}

	public void CloseConversation (bool displayState)
	{
		isChatActive = false;
		StopChatCoroutine ();
		displayChat.SetActive (displayState);	
		Clear ();
	}

	public void CloseConversation ()
	{
		StopChatCoroutine ();
		displayChat.SetActive (false);	
		Clear ();
	}

	public void Clear ()
	{
		isChatActive = false;
	}

	public void StopChatCoroutine ()
	{
		if (textCoroutine != null)
			StopCoroutine (textCoroutine);
		if (coroutineConversation != null)
			StopCoroutine (coroutineConversation);
	}


	public IEnumerator PrintText (string pageText)
	{
		while (true) {
			textChat.text += pageText [0].ToString ();
			pageText = pageText.Substring (1);

			if (pageText.Length <= 0)
				break;

			yield return new WaitForSeconds (speedText);
		}
	}

	public void HideButtons ()
	{
		for (int i = 0; i < buttons.Count; i++) {
			buttons [i].gameObject.SetActive (false);
		}
	}

	public StoryInfo _StoryInfo {
		get { 
			return Resources.Load ("Stories/Story") as StoryInfo;
		}
	}

	void OnValidate ()
	{
		storyInfo = _StoryInfo;

		buttons.Clear ();
		foreach (Transform t in buttonGroup) {
			buttons.Add (t.GetComponent<ResponseButton> ());
		}

	}

	public static ChatManager singletonInstance {
		get { 
			if (_instance == null)
				_instance = GameObject.FindObjectOfType<ChatManager> ();
			return _instance;
		}
	}

}
