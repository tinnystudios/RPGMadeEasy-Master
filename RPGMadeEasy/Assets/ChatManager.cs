using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
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
	public Chat chat;
	public Coroutine coroutineConversation;
	public Coroutine textCoroutine;
	public bool isChatActive;
	public float speedText = 0.01F;

	void Awake ()
	{
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

				/*
				string[] outputArray = outputText.Split ('#');
				string tagName = outputArray [1];

				if (tagNameDict.ContainsKey (tagName)) {
					print (tagNameDict [tagName]);

					//Remove the end
					outputText = "";
					outputText += outputArray [0];
					outputText += tagNameDict [tagName];
					outputText += outputArray [2];

				}
				*/

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
