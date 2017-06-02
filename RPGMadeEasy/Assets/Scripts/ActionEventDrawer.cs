using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor (typeof(ActionEventManager))]
public class ActionEventDrawer : Editor
{

	private ReorderableList list;
	public Rect currentRect;

	public Vector2 rectPos;

	private void OnEnable ()
	{
		list = new ReorderableList (serializedObject, 
			serializedObject.FindProperty ("actionEvents"), 
			true, true, true, true);
	}

	public override void OnInspectorGUI ()
	{

		ActionEventManager aEvent = (ActionEventManager)target;

		serializedObject.Update ();
		//SerializedProperty sProp = serializedObject.FindProperty ("actionEvents");
		list.DoLayoutList ();


		list.drawElementCallback = 
			(Rect rect, int index, bool isActive, bool isFocused) => {



			float width = rect.width / 3;

			var element = list.serializedProperty.GetArrayElementAtIndex (index);
			rect.y += 2;
		
			float height = EditorGUIUtility.singleLineHeight * 3f;

			EditorGUI.PropertyField (
				new Rect (rect.x, rect.y, width, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative ("actionEventType"), GUIContent.none);

			ActionEventManager.ActionEvent actionEvent = aEvent.actionEvents [index];
			actionEvent.myHeight = height;

			List<Rect> rectItemCol = new List<Rect> ();

			currentRect = new Rect (rect.x, rect.y, width, EditorGUIUtility.singleLineHeight);


			for (int i = 0; i < 10; i++) {
				rectItemCol.Add (new Rect (rect.x, rect.y + (EditorGUIUtility.singleLineHeight * i), width, EditorGUIUtility.singleLineHeight));
			}

			Rect[] rectItemArrayCol = rectItemCol.ToArray ();

			Rect itemRect = new Rect (rect.x, rect.y, width, EditorGUIUtility.singleLineHeight);
			Rect itemRect2 = new Rect (rect.x, rect.y + (EditorGUIUtility.singleLineHeight), width, EditorGUIUtility.singleLineHeight);

			int waitPosIndex = 2;

			//Create the position and height of even rect
			//wHEN YOU DRAWguipProp, increase the X!!
			switch (actionEvent.actionEventType) {

			case ActionEventType.setPos:
				actionEvent.myHeight = 2 * EditorGUIUtility.singleLineHeight;
				rectItemArrayCol [0].x += width;
				DrawGUIProperty (rectItemArrayCol [0], element, "target", "");
				rectItemArrayCol [0].x += width;
				DrawGUIProperty (rectItemArrayCol [0], element, "targetTo", "");
				waitPosIndex = 1;
				break;

			case ActionEventType.move:

				itemRect.x += width;
				DrawGUIProperty (itemRect, element, "target", "");
				itemRect.x += width;

				DrawGUIProperty (itemRect, element, "animationCurve", "");

				rectItemArrayCol [1].width = rect.width;
				DrawGUIProperty (rectItemArrayCol [1], element, "moveDirection", "");

				break;

			case ActionEventType.debugLog:

				itemRect.x += width;
				DrawGUIProperty (itemRect, element, "text", "");	
				itemRect.x += width;
				DrawGUIProperty (rectItemCol [1], element, "waitType", "");		
				rectItemArrayCol [1].x += width;
				DrawGUIProperty (rectItemArrayCol [1], element, "waitTime", "");
				break;

			case ActionEventType.waitForSeconds:
				rectItemArrayCol [0].x += width;
				DrawGUIProperty (rectItemArrayCol [0], element, "waitTime", "");
				break;

			case ActionEventType.dialouge:

				SerializedProperty dProp = element.FindPropertyRelative ("dialouge");
				SerializedProperty dTypeProp = dProp.FindPropertyRelative ("dialougeType");

				rectItemArrayCol [0].x += width;
				EditorGUI.PropertyField (rectItemArrayCol [0], dTypeProp, new GUIContent (""));
				waitPosIndex = 2;

				if (actionEvent.dialouge.dialougeType == StoryElementType.custom) {
					actionEvent.myHeight = 5 * EditorGUIUtility.singleLineHeight;

					rectItemArrayCol [1].width = rect.width;
					rectItemArrayCol [1].height = EditorGUIUtility.singleLineHeight * 3;
					actionEvent.dialouge.outputText = EditorGUI.TextArea (rectItemArrayCol [1], actionEvent.dialouge.outputText);
					waitPosIndex = 4;
				}


				if (actionEvent.dialouge.dialougeType != StoryElementType.custom) {

					actionEvent.myHeight = 3 * EditorGUIUtility.singleLineHeight;

					//if switched

					//Has a page

					//For some reason, when I chaneg this page location, it changes the file


					//Get the page from pageGUID

					//Check for pages
				
					/*
					if (actionEvent.dialouge.pageLinkInfo != null) {
						actionEvent.dialouge.dialougeIndex = actionEvent.dialouge.pageLinkInfo.elementIndex;
						actionEvent.dialouge.conversationIndex = actionEvent.dialouge.pageLinkInfo.conversationIndex;
						actionEvent.dialouge.pageIndex = actionEvent.dialouge.pageLinkInfo.pageIndex;
					}
					*/

					//GUID is the best

					#region Dialouge
					StoryInfo storyInfo = StoryGetters.GetStoryInfo ();

					//Set up GUID stuff.

					Dictionary<string,  StoryInfo.Page> pageDict = StoryGetters.GetPagesDictionary ();

					if (pageDict.ContainsKey (actionEvent.dialouge.linkedPageGUID)) {


						StoryInfo.Page pageInfo = pageDict [actionEvent.dialouge.linkedPageGUID];

						actionEvent.dialouge.dialougeIndex = pageInfo.pageLinkInfo.elementIndex;
						actionEvent.dialouge.conversationIndex = pageInfo.pageLinkInfo.conversationIndex;
						actionEvent.dialouge.pageIndex = pageInfo.pageLinkInfo.pageIndex;


					}

					string[] dialouges = StoryGetters.GetStoryBaseNames (storyInfo, actionEvent.dialouge.dialougeType);

					actionEvent.dialouge.dialougeIndex = ValidateMaxLength (actionEvent.dialouge.dialougeIndex, dialouges.Length);
					rectItemArrayCol [0].x += width;
					actionEvent.dialouge.dialougeIndex = EditorGUI.Popup (rectItemArrayCol [0], "", actionEvent.dialouge.dialougeIndex, dialouges);

					StoryInfo.StoryBase dialouge = StoryGetters.GetChatTypeList (storyInfo, actionEvent.dialouge.dialougeType) [actionEvent.dialouge.dialougeIndex];

					string[] conversations = StoryGetters.GetConversation (dialouge);

					//rectItemArrayCol [1].x += width;

					actionEvent.dialouge.conversationIndex = ValidateMaxLength (actionEvent.dialouge.conversationIndex, conversations.Length);
					actionEvent.dialouge.conversationIndex = EditorGUI.Popup (rectItemArrayCol [1], "", actionEvent.dialouge.conversationIndex, conversations);

					if (GUI.changed) {
						Debug.Log (actionEvent.dialouge.conversationIndex);
						//Make page null
						actionEvent.dialouge.linkedPageGUID = ""; //Or page 0 of this convo
					}

					StoryInfo.Conversation conversation = dialouge.conversations [actionEvent.dialouge.conversationIndex];

					//The key here is to make sure that after you've changed the conversation
					//It then finds the conversation

					//Show conversations




					#endregion

					#region General Page

					rectItemArrayCol [1].x += width;
					rectItemArrayCol [1].width = width * 2;
					//There are no pages
					if (conversation.pages.Count <= 0)
						return;
					string[] pages = StoryGetters.GetPage (conversation);

					actionEvent.dialouge.pageIndex = EditorGUI.Popup (rectItemArrayCol [1], "", actionEvent.dialouge.pageIndex, pages);
					actionEvent.dialouge.pageIndex = ValidateMaxLength (actionEvent.dialouge.pageIndex, conversation.pages.Count);

					StoryInfo.Page page = conversation.pages [actionEvent.dialouge.pageIndex];
				

					if (page != null) {


						//linkedPageGUID
						actionEvent.dialouge.linkedPageGUID = page.GUID;

						//actionEvent.dialouge.pageLinkInfo = page.pageLinkInfo;

						//Maybe I have to use a GUID style.
						//Break linkage first
						//actionEvent.dialouge.linkedPage = page;
						//actionEvent.dialouge.linkedPage = page;
					} else {
						Debug.Log ("null");
					}

					#endregion



					//Display Buttons
					currentRect = rectItemArrayCol [1];



					//GUI.Label (currentRect, "testText");


					waitPosIndex = 6;

				}


				actionEvent.lastIndex = index;

				break;

			}

			actionEvent.myHeight = (waitPosIndex + 2) * EditorGUIUtility.singleLineHeight;

			DrawGUIProperty (rectItemCol [waitPosIndex], element, "waitType", "");		
			rectItemArrayCol [waitPosIndex].x += width;

			if (actionEvent.waitType != WaitType.none)
				DrawGUIProperty (rectItemArrayCol [waitPosIndex], element, "waitTime", "");

		};

		if (GUI.changed) {
			EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
		}

		serializedObject.ApplyModifiedProperties ();

		list.elementHeightCallback = (index) => {
			float height = EditorGUIUtility.singleLineHeight * 3;

			ActionEventManager.ActionEvent actionEvent = aEvent.actionEvents [index];

			/*
			if (actionEvent.actionEventType == ActionEventType.dialouge) {
				if (actionEvent.dialouge.dialougeType == StoryElementType.custom)
					height = EditorGUIUtility.singleLineHeight * 5;
				else
					height = EditorGUIUtility.singleLineHeight * 3;
			}
			*/

			height = actionEvent.myHeight;

			return height;
		};

		return;

		for (int i = 0; i < aEvent.actionEvents.Count; i++) {
			ActionEventManager.ActionEvent actionEvent = aEvent.actionEvents [i];
			GUILayout.Label ("Event" + i);
			actionEvent.actionEventType = (ActionEventType)EditorGUILayout.EnumPopup (actionEvent.actionEventType);

			switch (actionEvent.actionEventType) {

			case ActionEventType.move:

				actionEvent.target = (Transform)EditorGUILayout.ObjectField (actionEvent.target, typeof(Transform));
				actionEvent.moveDirection = EditorGUILayout.Vector3Field ("", actionEvent.moveDirection);
				actionEvent.animationCurve = EditorGUILayout.CurveField (actionEvent.animationCurve);
				break;

			case ActionEventType.debugLog:
				actionEvent.text = EditorGUILayout.TextField ("Text: ", actionEvent.text);
				break;

			case ActionEventType.dialouge:

				break;

			}

			actionEvent.waitType = (WaitType)EditorGUILayout.EnumPopup ("Wait Type", actionEvent.waitType);
			actionEvent.waitTime = EditorGUILayout.FloatField ("Wait Time", actionEvent.waitTime);

			GUILayout.Space (10);

		}

		if (GUILayout.Button ("Add Event")) {
			ActionEventManager.ActionEvent newEvent = new ActionEventManager.ActionEvent ();
			aEvent.actionEvents.Add (newEvent);
		}

	}

	public void DrawGUIProperty (Rect rect, SerializedProperty mainProp, string propName, string propLabel)
	{
		SerializedProperty sProp = mainProp.FindPropertyRelative (propName);
		EditorGUI.PropertyField (rect, sProp, new GUIContent (propLabel));
	}

	public void DrawNewLine (SerializedProperty mainProp, string propName, string propLabel)
	{
		currentRect.x = 0;
		currentRect.y += EditorGUIUtility.singleLineHeight;
		SerializedProperty sProp = mainProp.FindPropertyRelative (propName);
		EditorGUI.PropertyField (currentRect, sProp, new GUIContent (propLabel));
	}

	public void DrawProp (SerializedProperty mainProp, string propName, string propLabel)
	{
		currentRect.x += currentRect.width;
		SerializedProperty sProp = mainProp.FindPropertyRelative (propName);
		EditorGUI.PropertyField (currentRect, sProp, new GUIContent (propLabel));
	}

	public void DrawLabelLine (string name)
	{
		currentRect.x = 0;
		currentRect.y += EditorGUIUtility.singleLineHeight;
		GUI.Label (currentRect, name);
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
