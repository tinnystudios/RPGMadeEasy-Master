using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(ActionNode))]
public class ActionNodeDrawer : Editor
{

	public ActionNode actionNode;

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		actionNode = (ActionNode)target;

		if (actionNode.nodeActionType == NodeActionType.animation) {
			DisplayAnimation ();
		}

		if (actionNode.nodeActionType == NodeActionType.chat) {
			DisplayChat ();
		}

	}

	public string splinePathName = "splinePath";

	public void DisplayAnimation ()
	{

		if (GUI.changed) {
			actionNode.splinePaths.Clear ();
			int count = 0;

			foreach (Transform s in actionNode.transform) {
				if (s.GetComponent<SplinePath> ()) {
					actionNode.splinePaths.Add (s.GetComponent<SplinePath> ());
					s.name = splinePathName + count;
				}
				count++;
			}
		}

		if (GUILayout.Button ("Add Spline")) {

			GameObject spline = new GameObject (splinePathName + actionNode.splinePaths.Count);
			SplinePath splinePath = spline.AddComponent<SplinePath> ();
			spline.transform.SetParent (actionNode.transform);
			actionNode.splinePaths.Add (splinePath);

		}


		//Go through a loop and show all of it

		for (int i = 0; i < actionNode.splinePaths.Count; i++) {
			GUILayout.Label (actionNode.splinePaths [i].name);

			if (GUILayout.Button ("Add Path")) {
				GameObject splineNode = new GameObject ();
				splineNode.name = "node"; //+currentCount, I need a node
				splineNode.transform.SetParent (actionNode.splinePaths [i].transform);
			}

		}

		//Check path for path 0



		//Play an emoji
		//Select an emoji to play or none

		//Have "Paths List"
		//Create a 'path'

		//Path: 
		////targetObject/targetObjects
		//The targetObjects group will move in this path. for example A to B.


		//Conditions
		//Is this an end piece?

	}

	public void DisplayChat ()
	{

		StoryInfo storyInfo = StoryGetters.GetStoryInfo ();

		#region Get ### Dialouges ###
		string[] dialouges = StoryGetters.GetStoryBaseNames (storyInfo, actionNode.dialougeType);

		actionNode.dialougeIndex = ValidateMaxLength (actionNode.dialougeIndex, dialouges.Length);

		actionNode.dialougeIndex = EditorGUILayout.Popup ("Dialouge", actionNode.dialougeIndex, dialouges);
		#endregion

		StoryInfo.StoryBase dialouge = StoryGetters.GetChatTypeList (storyInfo, actionNode.dialougeType) [actionNode.dialougeIndex];

		#region ### Get Conversations ###
		string[] conversations = StoryGetters.GetConversation (dialouge);

		actionNode.conversationIndex = ValidateMaxLength (actionNode.conversationIndex, conversations.Length);

		actionNode.conversationIndex = EditorGUILayout.Popup ("Conversation", actionNode.conversationIndex, conversations);
		#endregion

		StoryInfo.Conversation conversation = dialouge.conversations [actionNode.conversationIndex];

		#region ### Display General Pages From Editor ###

		//There are no pages
		if (conversation.pages.Count <= 0)
			return;

		actionNode.pageIndex = ValidateMaxLength (actionNode.pageIndex, conversation.pages.Count);
		StoryInfo.Page page = conversation.pages [actionNode.pageIndex];

		//Display
		GUILayout.Label ("Page: " + (actionNode.pageIndex + 1) + "/" + conversation.pages.Count);
		float iconWidth = 80;
		float smallTextFieldHeight = 18;
		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();
		page.speakerName = EditorGUILayout.TextField (page.speakerName, GUILayout.Width (iconWidth), GUILayout.Height (smallTextFieldHeight));

		page.outputCharacterImage.image = (Texture2D)EditorGUILayout.ObjectField (page.outputCharacterImage.image, typeof(Texture2D), true, GUILayout.Width (iconWidth), GUILayout.Height (iconWidth));

		string[] characterImages = StoryGetters.GetCharacterImagesNames (dialouge);
		page.characterImageIndex = EditorGUILayout.Popup ("", page.characterImageIndex, characterImages, GUILayout.Width (iconWidth), GUILayout.Height (smallTextFieldHeight));

		//page image 

		if (page.characterImageIndex < dialouge.characterImages.Count) {
			page.outputCharacterImage = new StoryInfo.CharacterImage ();
			page.outputCharacterImage.image = dialouge.characterImages [page.characterImageIndex].image;
			page.characterImage = dialouge.characterImages [page.characterImageIndex];

		} else {
			//Set unique ID
		}

		//Save changes?

		GUILayout.EndVertical ();

		page.text = GUILayout.TextArea (page.text, GUILayout.MinHeight (iconWidth + smallTextFieldHeight * 2), GUILayout.Width (Screen.width - (iconWidth + 50)));
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();

		#region Page Toggle 
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("<")) {
			if (actionNode.pageIndex > 0)
				actionNode.pageIndex--;
		}

		if (GUILayout.Button (">")) {

			if (actionNode.pageIndex < conversation.pages.Count - 1)
				actionNode.pageIndex++;

		}
		GUILayout.EndHorizontal ();
		#endregion

		#endregion

		#region Set Up Buttons

		while (actionNode.pageEvents.Count < conversation.pages.Count) {
			actionNode.pageEvents.Add (new ActionNode.UnityChatEvent ());
		}

		while (actionNode.pageEvents.Count > conversation.pages.Count) {
			actionNode.pageEvents.RemoveAt (0);
		}

		/*
		for (int i = 0; i < dialouge.conversations.Count; i++) {
			if (actionNode.conversationEvents.Count < dialouge.conversations.Count) {
				actionNode.conversationEvents.Add (new ActionNode.ConversationEvent ());
			}
			//Check for pages
			while (actionNode.conversationEvents [i + 1].pageEvents.Count < dialouge.conversations [i + 1].pages.Count) {
				actionNode.conversationEvents [i + 1].pageEvents.Add (new ActionNode.UnityChatEvent ());
			}
		}

		Debug.Log (actionNode.conversationEvents.Count);

		//Remove it when it's higher


		/*
		while (actionNode.pageChatEvents.Count < conversation.pages.Count) {
			actionNode.pageChatEvents.Add
		}
*/


		#endregion
	}

	public int ValidateMaxLength (int a, int b)
	{
		if (a >= b) {
			a = b - 1;
		}
		return a;
	}

	public int ValidateAboveZero (int a)
	{
		if (a <= 0)
			a = 0;
		return a;
	}
}

public enum NodeActionType
{
	animation,
	chat,
	isCustomChat
}