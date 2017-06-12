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

	public Color mainColor = new Color (0.8F, 0.8F, 0.8F);
	public Color subColor = new Color (1, 1, 1);
	public Color subBgColor = Color.gray;
	public bool isListening;

	private void OnEnable ()
	{
		list = new ReorderableList (serializedObject, 
			serializedObject.FindProperty ("events"), 
			true, true, true, true);

		if (!isListening) {
			list.onReorderCallback += OnReorder;
			list.onMouseUpCallback += OnMouseUp;
		
			isListening = true;
		}
	}



	public void DrawMoveMethod (SerializedProperty element, EasyEvent.MoveMethod moveMethod)
	{
		//DrawGUIProperty (element, "actorRefType", "", 1);
		DrawGUIProperty (element, "moveType", "", 1);
		DrawGUIProperty (element, "target", "", 1);

		/*
				if (eventInfo.actorRefType == ActorRefType.fromActorList) {
					string[] actors = StoryGetters.GetActorTags (SceneInfo.singletonInstance.actorTags);
					eventInfo.actorIndex = ValidateMaxLength (eventInfo.actorIndex, actors.Length);
					eventInfo.actorIndex = EditorGUI.Popup (curRect, "", eventInfo.actorIndex, actors);
					eventInfo.target = SceneInfo.singletonInstance.actorTags [eventInfo.actorIndex].actorObject.transform;
					NewRow ();
					//DrawGUIProperty (element, "actorRefType", "", 1);
				}
		*/

		if (moveMethod.moveType == MoveType.vector3) {
			NewCol ();
			DrawGUIProperty (element, "curve", "", 1);
			DrawGUIProperty (element, "moveDist", "", 2);
			//DrawGUIProperty (element, "rotDist", "", 2);
		}

		if (moveMethod.moveType == MoveType.target) {
			DrawGUIProperty (element, "targetTo", "", 1);
		}

	}

	public void DrawSetPosMethod (SerializedProperty element, EasyEvent.MoveMethod moveMethod)
	{
		DrawGUIProperty (element, "moveType", "", 1);
		DrawGUIProperty (element, "target", "", 1);

		if (moveMethod.moveType == MoveType.target)
			DrawGUIProperty (element, "targetTo", "", 1);
		else {
			DrawGUIProperty (element, "moveDist", "", 1);
		}
	}

	public void DrawSetActiveMethod (SerializedProperty element, EasyEvent.MoveMethod moveMethod)
	{
		DrawGUIProperty (element, "target", "", 1);
		DrawGUIProperty (element, "visibilityType", "", 1);
	}

	public void DrawInstantiateMethod (SerializedProperty element, EasyEvent.EasyEventInfo eventInfo)
	{
		if (eventInfo.instantiateMethod.spawnTarget == null)
			eventInfo.instantiateMethod.spawnTarget = eventInfo.moveMethod.target;

		DrawGUIProperty (element, "instanTarget", "Target", 3);
		DrawGUIProperty (element, "spawnTarget", "Spawn Position", 3);

		NewCol ();
		GUI.Label (curRect, "Offset");
		NewCol ();

		DrawGUIProperty (element, "offset", "", 3);
	}

	public void DrawEmojiMethod (SerializedProperty element, EasyEvent.EasyEventInfo eventInfo, GameObject prefab)
	{
		//Should probably make a selectable one too!

		eventInfo.instantiateMethod.emojiTarget = prefab;

		if (eventInfo.instantiateMethod.spawnTarget == null)
			eventInfo.instantiateMethod.spawnTarget = eventInfo.moveMethod.target;

		DrawGUIProperty (element, "sprite", "Select Sprite", 3);
		DrawGUIProperty (element, "spawnTarget", "Position Target", 3);

		NewCol ();

		GUI.Label (curRect, "Offset");
		NewCol ();

		DrawGUIProperty (element, "offset", "", 3);
	}

	public void DrawAnimationMethod (SerializedProperty element, EasyEvent.EasyEventInfo eventInfo)
	{
		DrawGUIProperty (element, "animator", "Animator", 3);
		NewCol ();

		if (eventInfo.animationMethod.animParamType != AnimParamType.none)
			DrawGUIProperty (element, "paramName", "Param Name", 3);

		DrawGUIProperty (element, "animParamType", "Param Type", 3);

		if (eventInfo.animationMethod.animParamType == AnimParamType.setInt)
			DrawGUIProperty (element, "intField", "Int", 3);
		if (eventInfo.animationMethod.animParamType == AnimParamType.setFloat)
			DrawGUIProperty (element, "floatField", "Float", 3);
		if (eventInfo.animationMethod.animParamType == AnimParamType.setBool)
			DrawGUIProperty (element, "boolField", "Bool", 3);

		NewCol ();
		DrawGUIProperty (element, "playSpeed", "Play Speed", 3);

	}

	public void DrawJustDialouge (EasyEvent.DialougeMethod dialougeInfo)
	{
		StoryInfo storyInfo = StoryGetters.GetStoryInfo ();
		StoryGetters.GenerateAllIndexes ();

		Dictionary<string,  StoryInfo.Conversation> conDict = StoryGetters.GetConversationDict ();

		//If condict contains GUID do stuff
		if (conDict.ContainsKey (dialougeInfo.conversationGUID)) {
			StoryInfo.Conversation con = conDict [dialougeInfo.conversationGUID];
			dialougeInfo.dialougeIndex = con.linkInfo.dialougeIndex;
			dialougeInfo.conversationIndex = con.linkInfo.conversationIndex;
		} else {
			//Reset values or Pick the first one from the list, make sure to know the type as well! 
		}

		string[] dialouges = StoryGetters.GetStoryBaseNames (storyInfo, dialougeInfo.dialougeType);
		dialougeInfo.dialougeIndex = ValidateMaxLength (dialougeInfo.dialougeIndex, dialouges.Length);
		dialougeInfo.dialougeIndex = EditorGUI.Popup (curRect, "", dialougeInfo.dialougeIndex, dialouges);

		NewRow ();

		StoryInfo.StoryBase dialouge = StoryGetters.GetChatTypeList (storyInfo, dialougeInfo.dialougeType) [dialougeInfo.dialougeIndex];

		string[] conversations = StoryGetters.GetConversation (dialouge);

		dialougeInfo.conversationIndex = ValidateMaxLength (dialougeInfo.conversationIndex, conversations.Length);
		dialougeInfo.conversationIndex = EditorGUI.Popup (curRect, "", dialougeInfo.conversationIndex, conversations);

		StoryInfo.Conversation conversation = dialouge.conversations [dialougeInfo.conversationIndex];
		dialougeInfo.conversationGUID = conversation.GUID; //Set GUID
	}

	public void DrawDialougeMethod (SerializedProperty element, SerializedProperty dialougeProp, EasyEvent.EasyEventInfo eventInfo, EasyEvent.DialougeMethod dialougeInfo, Rect rect)
	{

		StoryInfo storyInfo = StoryGetters.GetStoryInfo ();
		StoryGetters.GenerateAllIndexes ();

		//Dialouge Type
		DrawGUIProperty (dialougeProp, "dialougeType", "", 1);

		if (dialougeInfo.dialougeType != StoryElementType.custom) {

			#region Dialouge 

			Dictionary<string,  StoryInfo.Conversation> conDict = StoryGetters.GetConversationDict ();

			//If condict contains GUID do stuff
			if (conDict.ContainsKey (dialougeInfo.conversationGUID)) {
				StoryInfo.Conversation con = conDict [dialougeInfo.conversationGUID];
				dialougeInfo.dialougeIndex = con.linkInfo.dialougeIndex;
				dialougeInfo.conversationIndex = con.linkInfo.conversationIndex;
			} else {
				//Reset values or Pick the first one from the list, make sure to know the type as well! 
			}

			string[] dialouges = StoryGetters.GetStoryBaseNames (storyInfo, dialougeInfo.dialougeType);
			dialougeInfo.dialougeIndex = ValidateMaxLength (dialougeInfo.dialougeIndex, dialouges.Length);
			dialougeInfo.dialougeIndex = EditorGUI.Popup (curRect, "", dialougeInfo.dialougeIndex, dialouges);

			NewRow ();

			StoryInfo.StoryBase dialouge = StoryGetters.GetChatTypeList (storyInfo, dialougeInfo.dialougeType) [dialougeInfo.dialougeIndex];

			string[] conversations = StoryGetters.GetConversation (dialouge);

			dialougeInfo.conversationIndex = ValidateMaxLength (dialougeInfo.conversationIndex, conversations.Length);
			dialougeInfo.conversationIndex = EditorGUI.Popup (curRect, "", dialougeInfo.conversationIndex, conversations);

			StoryInfo.Conversation conversation = dialouge.conversations [dialougeInfo.conversationIndex];
			dialougeInfo.conversationGUID = conversation.GUID; //Set GUID

			NewCol ();


			if (conversation.pages.Count <= 0)
				return;

			//Check count!

			for (int i = 0; i < conversation.pages.Count; i++) {

				EasyEvent.PageInfo newPage = new EasyEvent.PageInfo ();

				if (eventInfo.pageList.Count < conversation.pages.Count)
					eventInfo.pageList.Add (newPage);

			}

			#endregion

			#region ### Page ###
		
			#region info
			int diff = eventInfo.pageList.Count - conversation.pages.Count;
			for (int i = 0; i < diff; i++) {
				eventInfo.pageList.RemoveAt (0);
			}


			eventInfo.pageIndex = ValidateAboveZero (eventInfo.pageIndex);
			eventInfo.pageIndex = ValidateMaxLength (eventInfo.pageIndex, conversation.pages.Count);

			Rect eventRect = new Rect (startX, curRect.y, rect.width, EditorGUIUtility.singleLineHeight);
			//Event OnStart()

			EditorGUI.DrawRect (eventRect, mainColor);	

			eventInfo.showPageInfo = EditorGUI.Foldout (eventRect, eventInfo.showPageInfo, "Page Info", true);

			if (eventInfo.showPageInfo) {
				NewCol ();
				Rect pageNumberRect = curRect;
				pageNumberRect.x += (width * 3) - 60;
				GUI.Label (pageNumberRect, "Page " + (eventInfo.pageIndex + 1) + "/" + conversation.pages.Count);

				NewCol ();

				//Display page
				StoryInfo.Page page = conversation.pages [eventInfo.pageIndex];
				curRect.width = rect.width;
				curRect.height = EditorGUIUtility.singleLineHeight * 2;
				page.text = GUI.TextField (curRect, page.text);

				NewCol ();

				NewCol ();




				curRect.width = 20;
				curRect.height = 15;
				curRect.x = rect.width - 5;

				if (GUI.Button (curRect, "<"))
					eventInfo.pageIndex--;

				curRect.x += curRect.width;
				if (GUI.Button (curRect, ">"))
					eventInfo.pageIndex++;


			}

			eventInfo.pageIndex = ValidateAboveZero (eventInfo.pageIndex);
			eventInfo.pageIndex = ValidateMaxLength (eventInfo.pageIndex, conversation.pages.Count);

			NewCol (1);

			SerializedProperty pageProp = element.FindPropertyRelative ("pageList").GetArrayElementAtIndex (eventInfo.pageIndex);
			EasyEvent.PageInfo pageInfo = eventInfo.pageList [eventInfo.pageIndex];

			DrawEvents ("eventStartList", pageInfo.eventStartList, eventRect, eventInfo, conversation, rect, element);
			DrawEvents ("eventEndList", pageInfo.eventEndList, eventRect, eventInfo, conversation, rect, element);
			DrawEvents ("eventButtonList", pageInfo.eventButtonList, eventRect, eventInfo, conversation, rect, element);


			return;

			eventRect.y = curRect.y;
			EditorGUI.DrawRect (eventRect, mainColor);
			//eventInfo.pageList [eventInfo.pageIndex].hasButtons = EditorGUI.Foldout (eventRect, eventInfo.pageList [eventInfo.pageIndex].hasButtons, "Page Buttons", true);
			#endregion

			#region buttons

			//Look for it in DIALOUGE
			//DrawGUIProperty (pageProp, "easyButtons", "Response Buttons", 3);
			SerializedProperty buttonsProp = pageProp.FindPropertyRelative ("easyButtons");

			EditorGUI.PropertyField (eventRect, buttonsProp, false);

			NewCol ();

			curRect.width = width;
			//When a button is pressed, it should turn off CHAT!
			if (buttonsProp.isExpanded) {

				for (int i = 0; i < buttonsProp.arraySize; i++) {
					
					EasyEvent.EasyButtons easyButton = pageInfo.easyButtons [i];
					SerializedProperty buttonProp = buttonsProp.GetArrayElementAtIndex (i);
					DrawGUIProperty (buttonsProp.GetArrayElementAtIndex (i), "text", "", 3);
					DrawGUIProperty (buttonsProp.GetArrayElementAtIndex (i), "eventType", "", 1);

					switch (easyButton.eventType) {
					//GoToPage (from within or outside!)
					case EasyEventType.dialouge:
						DrawJustDialouge (easyButton.dialougeMethod);
						break;
					case EasyEventType.unityEvent:
						DrawGUIProperty (buttonsProp.GetArrayElementAtIndex (i), "anEvent", "", 3);
						break;
					case EasyEventType.easyEvent:
						DrawEasyEventMethod (buttonProp.FindPropertyRelative ("easyEventMethod"));
						break;
					}


				}

				NewCol (6);

			}

			#endregion

			NewCol (1);

			#endregion

		}

	
	}

	public void DrawEasyEventMethod (SerializedProperty easyEventMethodProp)
	{
		DrawGUIProperty (easyEventMethodProp, "easyEvent", "Event: ", 3);
		DrawGUIProperty (easyEventMethodProp, "visibility", "Visibility: ", 3);
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		GUI.contentColor = Color.white;
		mainColor = new Color (0.8F, 0.8F, 0.8F);
		subColor = new Color (1, 1, 1);
		subBgColor = Color.gray;

		EasyEvent easyEvent = (EasyEvent)target;

		easyEvent.emojiPrefab = (GameObject)Resources.Load ("Emojis/Emoji") as GameObject;

		serializedObject.Update ();
		list.DoLayoutList ();

		list.drawHeaderCallback = (Rect rect) => {  
			EditorGUI.LabelField (rect, "Events");
		};

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

			//Set foldout
			Rect titleFoldRect = new Rect (rect.x + width, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
			GUIStyle s = new GUIStyle ();

			//Print every second rect
			bool useVisible = true;

			if (useVisible) {
				
				eventInfo.isVisible = EditorGUI.Foldout (titleFoldRect, eventInfo.isVisible, "", true, s);

				if (!eventInfo.isVisible) {
					eventInfo.elementHeight = EditorGUIUtility.singleLineHeight;
					return;
				}

			} else {

				if (!isActive) {
					eventInfo.elementHeight = EditorGUIUtility.singleLineHeight;
					return;
				}

			}

			//On select = OnDown

			if (rect.Overlaps (rect)) {
				eventInfo.elementHeight = EditorGUIUtility.singleLineHeight;
			}

			//Cache everything here!
			SerializedProperty methodProp = element.FindPropertyRelative ("moveMethod");
			SerializedProperty instanProp = element.FindPropertyRelative ("instantiateMethod");
			SerializedProperty animationProp = element.FindPropertyRelative ("animationMethod");
			SerializedProperty dialougeProp = element.FindPropertyRelative ("dialougeMethod");
			SerializedProperty easyEventMethodProp = element.FindPropertyRelative ("easyEventMethod");

			switch (eventInfo.eventType) {

			case EasyEventType.move:
				DrawMoveMethod (methodProp, eventInfo.moveMethod);
				break;

			case EasyEventType.setActive:
				DrawSetActiveMethod (methodProp, eventInfo.moveMethod);
				break;

			case EasyEventType.setPos:
				DrawSetPosMethod (methodProp, eventInfo.moveMethod);
				break;

			case EasyEventType.dialouge:
				DrawDialougeMethod (element, dialougeProp, eventInfo, eventInfo.dialougeMethod, rect);
				break;

			case EasyEventType.instantiate:
				DrawInstantiateMethod (instanProp, eventInfo);
				break;

			case EasyEventType.emoji:
				DrawEmojiMethod (instanProp, eventInfo, easyEvent.emojiPrefab.gameObject);
				break;

			case EasyEventType.unityEvent:
				DrawGUIProperty (element, "unityEvent", "", 3);

				if (eventInfo.unityEvent.GetPersistentEventCount () > 0)
					NewCol ((eventInfo.unityEvent.GetPersistentEventCount ()) * 3);
				else
					NewCol (3);

				NewCol (2);

				break;

			case EasyEventType.log:
				DrawGUIProperty (element, "outputText", "", 3);
				break;

			case EasyEventType.animation:
				DrawAnimationMethod (animationProp, eventInfo);
				break;

			case EasyEventType.loop:
				//Loop Index, 0 = start, or show list of events

				SerializedProperty loopMethod = element.FindPropertyRelative ("loopMethod");

				string[] loopNames = StoryGetters.GetEventNames (easyEvent.events);
				eventInfo.loopMethod.goToIndex = EditorGUI.Popup (curRect, "", eventInfo.loopMethod.goToIndex, loopNames);
				eventInfo.loopMethod.goToIndex = ValidateMaxLength (eventInfo.loopMethod.goToIndex, loopNames.Length);
				//eventInfo.loopMethod.selectedEvent = easyEvent.events [eventInfo.loopMethod.goToIndex];
				NewRow ();
				DrawGUIProperty (loopMethod, "loopType", "", 1);

				if (eventInfo.loopMethod.loopType == LoopType.forever) {

				}

				if (eventInfo.loopMethod.loopType == LoopType.amount) {
					DrawGUIProperty (loopMethod, "loopAmount", "", 1);
				}

				NewCol ();
				break;

			case EasyEventType.easyEvent:
				DrawEasyEventMethod (easyEventMethodProp);
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
			//list.elementHeight = eCol * EditorGUIUtility.singleLineHeight;

		};

		/*
		list.onReorderCallback += OnReorder;
		list.onMouseUpCallback += OnMouseUp;
		*/
		//do a counter for it lol.

		list.elementHeightCallback = (index) => {
			EasyEvent.EasyEventInfo eventInfo = easyEvent.events [index];
			float height = eventInfo.elementHeight;
			return height;
		};

		//Basically if this is active, PING!

		list.onSelectCallback = (ReorderableList l) => {  
			//var prefab = l.serializedProperty.GetArrayElementAtIndex (l.index).FindPropertyRelative ("Prefab").objectReferenceValue as GameObject;
			SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex (l.index);
			SerializedProperty moveProp = element.FindPropertyRelative ("moveMethod");
			EasyEvent.EasyEventInfo eventInfo = easyEvent.events [l.index];

			switch (eventInfo.eventType) {
			case EasyEventType.move:
				if (eventInfo.moveMethod.target != null)
					EditorGUIUtility.PingObject (easyEvent.events [l.index].moveMethod.target.gameObject);
				if (eventInfo.moveMethod.targetTo != null)
					EditorGUIUtility.PingObject (easyEvent.events [l.index].moveMethod.target.gameObject);
				break;
			case EasyEventType.emoji:
				if (eventInfo.instantiateMethod.spawnTarget != null)
					EditorGUIUtility.PingObject (eventInfo.instantiateMethod.spawnTarget.gameObject);
				
				//if (eventInfo.sprite != null)
					//EditorGUIUtility.PingObject (eventInfo.sprite);

				break;
			}
			//Selection.gameObjects = new GameObject[] { theGameObjectIwantToSelect };

			//if (moveProp.)
			//EditorGUIUtility.PingObject (prefab.gameObject);
		};

		if (GUI.changed) {
			EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
		}
		//list.ReleaseKeyboardFocus ();
		serializedObject.ApplyModifiedProperties ();

		//Find where all key positions should be and if they are not there then they are moving!!!

	}

	//Maybe, if you are moving
	//Make all element the same size!

	public void OnMouseUp (ReorderableList list)
	{
		Debug.Log ("mouse up");
	}

	public void OnReorder (ReorderableList list)
	{
		Debug.Log ("reordered");
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
		EditorGUI.PropertyField (newRect, sProp, new GUIContent (propLabel), true);

		for (int i = 0; i < xSize; i++)
			NewRow ();
	}

	public void DrawBackground (float w, float h, Color32 color)
	{
		//Draw a dark rect underneath it?
		Rect r = new Rect (startX, curRect.y, w * width, EditorGUIUtility.singleLineHeight * h);
		GUI.backgroundColor = color;
		GUI.Box (new Rect (startX, curRect.y, w * width, EditorGUIUtility.singleLineHeight * h), "");
	}

	public void DrawBackground (Rect r, Color32 color)
	{
		GUI.backgroundColor = color;
		GUI.Box (r, "");
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

	public void NewCol (int i)
	{
		curRect.y += i * EditorGUIUtility.singleLineHeight;
		curRect.x = startX;
		eCol += i;
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

	public void DrawRect (Color drawColor)
	{
		curRect.y += 1.5F;
		Rect eventRect = new Rect (startX, curRect.y, width * 3, EditorGUIUtility.singleLineHeight);
		Rect eventOutline = eventRect;
		eventOutline.height += 3.0F;
		eventOutline.y -= 1.5F;
		EditorGUI.DrawRect (eventOutline, Color.gray);
		EditorGUI.DrawRect (eventRect, drawColor);
	}

	public void Deselect ()
	{
		GUIUtility.hotControl = 0;
		GUIUtility.keyboardControl = 0;
	}

	public void DrawEvents (string pageEventPropName, List<EasyEvent.PageEvent> pageEvents, Rect eventRect, EasyEvent.EasyEventInfo eventInfo, StoryInfo.Conversation conversation, Rect rect, SerializedProperty element)
	{
		#region Events onStart()

		SerializedProperty pageProp = element.FindPropertyRelative ("pageList").GetArrayElementAtIndex (eventInfo.pageIndex);

		//Page Foldout
		eventRect.y = curRect.y;
		EditorGUI.DrawRect (eventRect, mainColor);
		bool showState = false;
		bool isButtons = false;
		if (pageEventPropName == "eventEndList") {
			showState = eventInfo.pageList [eventInfo.pageIndex].showEnd = EditorGUI.Foldout (eventRect, eventInfo.pageList [eventInfo.pageIndex].showEnd, "OnEnd();", true);
			//showState = eventInfo.pageList [eventInfo.pageIndex].showEnd;
		}
		if (pageEventPropName == "eventStartList") {
			showState = eventInfo.pageList [eventInfo.pageIndex].showStart = EditorGUI.Foldout (eventRect, eventInfo.pageList [eventInfo.pageIndex].showStart, "OnStart();", true);
		}

		if (pageEventPropName == "eventButtonList") {
			isButtons = true;
			showState = eventInfo.pageList [eventInfo.pageIndex].showButtons = EditorGUI.Foldout (eventRect, eventInfo.pageList [eventInfo.pageIndex].showButtons, "Response Buttons", true);
		}

		NewCol (1);

		if (showState) {

			//I need to make a 'start' and an end.
			#region ### Display Events ###

			//Emoji Stuff

			SerializedProperty pageEventsProp = pageProp.FindPropertyRelative (pageEventPropName);


			Rect lastRect = curRect;
			curRect.width = width * 3;
			//curRect.height = EditorGUIUtility.singleLineHeight * 1.25F;

			#region Box height
			//draw background total
			float eventBoxHeight = 0;
			List<float> boxSizes = new List<float> ();

			for (int i = 0; i < pageEvents.Count; i++) {
				float bgHeight = 1; 

				switch (pageEvents [i].eventType) {
				case EasyEventType.emoji:
					bgHeight = 5;
					break;
				case EasyEventType.move:
					bgHeight = 3;
					break;

				case EasyEventType.unityEvent:
					bgHeight = 5;
					break;

				case EasyEventType.dialouge:
					bgHeight = 2;
					break;

				case EasyEventType.easyEvent:
					bgHeight = 3;
					break;

				}

				if (!pageEvents [i].isShow)
					bgHeight = 1;


				if (pageEvents [i].isShow) {

					bgHeight += 2;

				}

				boxSizes.Add (bgHeight);



				eventBoxHeight += bgHeight;


			}

			DrawBackground (3, eventBoxHeight, Color.white);

			//GUI.Box (curRect, "Events");

			curRect = lastRect;

			#endregion

			for (int i = 0; i < pageEvents.Count; i++) {

				#region Display List of events

				SerializedProperty eventProp = pageEventsProp.GetArrayElementAtIndex (i);
				SerializedProperty emojiProp = eventProp.FindPropertyRelative ("emoji");
				SerializedProperty moveProp = eventProp.FindPropertyRelative ("moveMethod");
				SerializedProperty unityEventProp = eventProp.FindPropertyRelative ("unityEventMethod");
				SerializedProperty anEventProp = eventProp.FindPropertyRelative ("anEvent");
				string title = pageEvents [i].eventType.ToString ();


				//Display
				DrawBackground (3, 1, Color.white);
				eventRect.y = curRect.y;
				eventRect.height = EditorGUIUtility.singleLineHeight * 0.9F;

				EditorGUI.DrawRect (new Rect (eventRect.x, eventRect.y, rect.width, boxSizes [i] * EditorGUIUtility.singleLineHeight), subBgColor);

				//eventRect.y += EditorGUIUtility.singleLineHeight * 0.05F;

				EditorGUI.DrawRect (eventRect, subColor);

				if (isButtons) {
					title = pageEvents [i].buttonText;
				}

				//#BUTTONS

				//here;
				pageEvents [i].isShow = EditorGUI.Foldout (eventRect, pageEvents [i].isShow, title, true);

				if (pageEvents [i].isShow) {

					if (isButtons) {
						NewCol (1);
						DrawGUIProperty (eventProp, "buttonText", "", 3);
					}

					NewCol ();
					DrawGUIProperty (eventProp, "eventType", "", 1);


					curRect.width = 20;
					curRect.height = 15;


					Rect bRect = new Rect (curRect);
					bRect.x += width * 2 - (bRect.width * 3);
					if (GUI.Button (bRect, "^") && i != 0) {

						EasyEvent.PageEvent oldPageEvent = pageEvents [i];
						EasyEvent.PageEvent newPageEvent = pageEvents [i - 1];
						pageEvents [i] = newPageEvent;
						pageEvents [i - 1] = oldPageEvent;

						i = 0;
						return;

					}
					bRect.x += bRect.width;
					if (GUI.Button (bRect, "v") && i != pageEvents.Count - 1) {

						EasyEvent.PageEvent oldPageEvent = pageEvents [i];
						EasyEvent.PageEvent newPageEvent = pageEvents [i + 1];
						pageEvents [i] = newPageEvent;
						pageEvents [i + 1] = oldPageEvent;

						i = 0;
						return;

					}
					bRect.x += bRect.width;
					if (GUI.Button (bRect, "X")) {
						pageEvents.RemoveAt (i);
						i = 0;
						return;
					}

					NewCol ();

					curRect.width = width;

					#region Display Specific Events

					switch (pageEvents [i].eventType) {


					case EasyEventType.move:
						EasyEvent.MoveMethod moveMethod = pageEvents [i].moveMethod;
						DrawMoveMethod (moveProp, moveMethod);
						break;

					case EasyEventType.emoji:
						DrawGUIProperty (emojiProp, "playTiming", "Emoji Timing", 3);
						DrawGUIProperty (emojiProp, "sprite", "Sprite", 3);
						DrawGUIProperty (emojiProp, "instantiateTarget", "Prefab", 3);
						DrawGUIProperty (emojiProp, "target", "Target", 3);
						break;

					case EasyEventType.unityEvent:

						DrawGUIProperty (eventProp, "anEvent", "", 3);
						//DrawGUIProperty (unityEventProp, "anEvent", "", 3);
						NewCol (4);
						break;

					case EasyEventType.dialouge:
						DrawJustDialouge (pageEvents [i].dialougeMethod);
						break;

					case EasyEventType.easyEvent:
						DrawEasyEventMethod (eventProp.FindPropertyRelative ("easyEventMethod"));
						break;

					}

					#endregion

					NewCol ();

					DrawGUIProperty (eventProp, "waitType", "", 1);
					DrawGUIProperty (eventProp, "waitTime", "", 1);


				}



				if (i == pageEvents.Count - 1)
					NewCol (1);
				else
					NewCol (1);

				#endregion

			}

			curRect.width = 20;
			curRect.x = rect.width + curRect.width - 5;
			DrawBackground (new Rect (curRect.x - curRect.width, curRect.y, curRect.width * 2, EditorGUIUtility.singleLineHeight), Color.white);

			if (GUI.Button (curRect, "+")) {
				EasyEvent.PageEvent pageEvent = new EasyEvent.PageEvent ();
				pageEvents.Add (pageEvent);
			}

			NewCol ();

			#endregion

		}

		//NewCol (1);

		#endregion
	}

}
