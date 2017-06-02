using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor (typeof(EasyEvent))]
public class EasyEventDrawer : Editor
{
	private ReorderableList list;

	public Rect curRect;

	public float width;
	public float startX;

	public int eCol;
	public int eRow;

	private void OnEnable ()
	{
		list = new ReorderableList (serializedObject, 
			serializedObject.FindProperty ("events"), 
			true, true, true, true);
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		EasyEvent easyEvent = (EasyEvent)target;

		serializedObject.Update ();
		list.DoLayoutList ();
		list.drawElementCallback = 
			(Rect rect, int index, bool isActive, bool isFocused) => {

			var element = list.serializedProperty.GetArrayElementAtIndex (index);

			EasyEvent.EasyEventInfo eventInfo = easyEvent.events [index];
		
			#region Set Base Values
			curRect = new Rect (rect.x, rect.y, rect.width / 3, EditorGUIUtility.singleLineHeight);
			width = curRect.width;
			startX = rect.x;
			eCol = 1;
			eRow = 0;
			#endregion

			DrawGUIProperty (element, "eventType", "", 1);
			NewCol ();

			switch (eventInfo.eventType) {

			case EasyEventType.move:
				DrawGUIProperty (element, "moveType", "", 1);
				DrawGUIProperty (element, "target", "", 1);

				if (eventInfo.moveType == MoveType.vector3) {
					DrawGUIProperty (element, "curve", "", 1);

					DrawGUIProperty (element, "moveDist", "Move", 2);

					DrawGUIProperty (element, "rotDist", "Rotate", 2);
				}

				if (eventInfo.moveType == MoveType.target) {
					DrawGUIProperty (element, "targetTo", "", 1);
				}

				break;

			case EasyEventType.setActive:
				DrawGUIProperty (element, "target", "", 1);
				DrawGUIProperty (element, "visiblityType", "", 1);
				break;

			case EasyEventType.setPos:
				DrawGUIProperty (element, "target", "", 1);
				DrawGUIProperty (element, "targetTo", "", 1);
				break;

			case EasyEventType.dialouge:

				StoryInfo storyInfo = StoryGetters.GetStoryInfo ();
				Dictionary<string,  StoryInfo.Conversation> conDict = StoryGetters.GetConversationDict ();
				//If condict contains GUID do stuff

				//Dialouge Type
				DrawGUIProperty (element, "dialougeType", "", 1);

				string[] dialouges = StoryGetters.GetStoryBaseNames (storyInfo, eventInfo.dialougeType);
				eventInfo.dialougeIndex = ValidateMaxLength (eventInfo.dialougeIndex, dialouges.Length);
				eventInfo.dialougeIndex = EditorGUI.Popup (curRect, "", eventInfo.dialougeIndex, dialouges);

				NewRow ();

				StoryInfo.StoryBase dialouge = StoryGetters.GetChatTypeList (storyInfo, eventInfo.dialougeType) [eventInfo.dialougeIndex];

				string[] conversations = StoryGetters.GetConversation (dialouge);
				eventInfo.conversationIndex = ValidateMaxLength (eventInfo.conversationIndex, conversations.Length);
				eventInfo.conversationIndex = EditorGUI.Popup (curRect, "", eventInfo.conversationIndex, conversations);

				NewRow ();

				break;

			case EasyEventType.log:
				DrawGUIProperty (element, "outputText", "", 1);
				break;
			}

			#region Wait Section

			//Add if a 'collum' isn't going to be generated
			if (eRow > 0)
				NewCol ();

			DrawGUIProperty (element, "waitType", "", 1);
			DrawGUIProperty (element, "waitTime", "", 1);

			NewCol ();

			#endregion

			//Set element height
			eventInfo.elementHeight = eCol * EditorGUIUtility.singleLineHeight;

		};

		list.elementHeightCallback = (index) => {
			EasyEvent.EasyEventInfo eventInfo = easyEvent.events [index];

			float height = eventInfo.elementHeight;
			return height;
		};

		if (GUI.changed) {
			EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
		}

		serializedObject.ApplyModifiedProperties ();
	}

	public void DrawGUIProperty (SerializedProperty mainProp, string propName, string propLabel, int xSize)
	{
		
		if (eRow >= 3 || eRow + xSize > 3) {
			NewCol ();
		}
		//When you make, that's when you make new col

		SerializedProperty sProp = mainProp.FindPropertyRelative (propName);
		Rect newRect = new Rect (curRect);
		newRect.width = xSize * width;
		EditorGUI.PropertyField (newRect, sProp, new GUIContent (propLabel));

		for (int i = 0; i < xSize; i++)
			NewRow ();
	}

	public void NewItem (int xSize)
	{
		if (eRow >= 3) {
			NewCol ();
		}

		for (int i = 0; i < xSize; i++)
			NewRow ();
	}

	public void NewCol ()
	{
		curRect.y += 1 * EditorGUIUtility.singleLineHeight;
		curRect.x = startX;
		eCol += 1;
		eRow = 0;
	}

	public void NewRow ()
	{
		curRect.x += width;
		eRow += 1;
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
