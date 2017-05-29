using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

public class StoryEditor : EditorWindow
{
	public string pathToStory = "Assets/RpgMadeEasy/Resources/Stories/";
	public StoryInfo storyInfo;
	public GUISkin editorSkin;
	public StoryElementType storyElementType;

	public string inputName = "Enter Name";
	public string inputConversation = "Enter Conversation";
	public string inputPage = "Enter Page";

	public string styleSmallButton = "smallButton";
	public string styleButtonHeader = "buttonHeader";
	public string styleButtonHeaderB = "buttonHeaderB";
	public float smallButtonWidth = 0.5F;
	public float xSmallButtonWidth = 0.25F;
	public int storyElementIndex = 0;

	public StoryInfo.StoryBase selectedAsset;
	public float padLeft = 40;
	public float padTop = 20;
	public Vector2 scrollPos;
	Vector2 scrollPosImages;
	public string stringUnfolded = "▼";
	public string stringFolded = "►";
	public string stringFold = "";
	public string inputImageName = "";

	private Color conversationColor = Color.white;
	private  Color pageColor = Color.white;
	private  Color normalColor = Color.white;

	public static EditorWindow window;
	public EditorColors editorColorInfo;

	public Dictionary<string,  StoryInfo.StoryBase> charDict = new Dictionary<string, StoryInfo.StoryBase> ();
	public Dictionary<string,  StoryInfo.Page> pageDict = new Dictionary<string, StoryInfo.Page> ();
	public Dictionary<string,  string> tagNameDict = new Dictionary<string, string> ();

	public SerializedObject storyObject;

	float texture2DSize = 80;
	public Texture2D inputImage;

	public GUISkin defaultSkin;

	[MenuItem ("RPGMadeEasy/StoryEditor")]
	static void  Init ()
	{
		if (window == null)
			window = EditorWindow.GetWindow (typeof(StoryEditor));



		Debug.Log ("Init");

	}

	public string GetFoldedString (bool b)
	{
		if (b)
			return stringUnfolded + " ";
		else
			return stringFolded + " ";
	}

	void OnEnable ()
	{

		storyInfo = StoryGetters.GetStoryInfo ();
		DictionarySetup ();

	}

	//Use boolean from 'storyinfo' not the character ?
	void OnGUI ()
	{
		
		if (editorColorInfo != null)
			editorColorInfo = AssetDatabase.LoadAssetAtPath (pathToStory + "EditorColors.asset", typeof(EditorColors)) as EditorColors;

		if (editorColorInfo == null) {
			EditorColors asset = ScriptableObject.CreateInstance<EditorColors> ();
			AssetDatabase.CreateAsset (asset, pathToStory + "EditorColors.asset");
			AssetDatabase.SaveAssets ();
			editorColorInfo = asset;
		}


		if (storyObject == null) {
			if (storyInfo != null)
				storyObject = new SerializedObject (storyInfo);
		}

		//storyObject.Update ();



		SerializedObject sObject = new SerializedObject (editorColorInfo);
		sObject.Update ();
		SerializedProperty sProp = sObject.FindProperty ("colorList");
		EditorGUILayout.PropertyField (sProp, true);
		sObject.ApplyModifiedProperties ();

		conversationColor = Color.white;
		pageColor = Color.white;
		normalColor = Color.white;

		//Make a color field class


		scrollPos =	EditorGUILayout.BeginScrollView (scrollPos);

		smallButtonWidth = 0.15F * Screen.width;
		xSmallButtonWidth = 0.075F * Screen.width;
		StoryFileValidation ();

		if (storyInfo == null)
			return;


		SerializedObject storyInfoObject = new SerializedObject (storyInfo);
		storyInfoObject.Update ();

		GetEditorSkin ();

		MakeTitleLabel ("Story Creation");

		//========= STORY ELEMENT =============//
		if (GUILayout.Button (GetFoldedString (storyInfo.isStoryElement) + "Story ELement", editorSkin.GetStyle (styleButtonHeaderB)))
			storyInfo.isStoryElement = !storyInfo.isStoryElement;

		if (storyInfo.isStoryElement)
			DisplayStoryElement ();

		#region ### Check for nulls ###

		AssetValidation ();

		#endregion


		//========= CHARACTER INFO / ACTOR =============//

		MakeTitleLabel (selectedAsset.name);



		if (GUILayout.Button (GetFoldedString (storyInfo.isInfo) + "Character Info", editorSkin.GetStyle (styleButtonHeaderB)))
			storyInfo.isInfo = !storyInfo.isInfo;

		if (storyInfo.isInfo) {
			DisplayElementInfo ();
			//display character images

			//Make a character image

			GUILayout.BeginHorizontal (EditorStyles.helpBox);
			GUILayout.BeginVertical (EditorStyles.helpBox);

			GUI.skin = defaultSkin;
			inputImage = (Texture2D)EditorGUILayout.ObjectField (inputImage, typeof(Texture2D), true, GUILayout.Width (texture2DSize), GUILayout.Height (texture2DSize));
			GUI.skin = editorSkin;
			inputImageName = EditorGUILayout.TextField ("", inputImageName, GUILayout.Width (texture2DSize));

			if (GUILayout.Button ("New", GUILayout.Width (texture2DSize))) {
				StoryInfo.CharacterImage characterImage = new StoryInfo.CharacterImage ();
				characterImage.image = inputImage;
				characterImage.name = inputImageName;
				selectedAsset.characterImages.Add (characterImage);	
			}

			GUILayout.EndVertical ();

			GUILayout.Space (padLeft);

			GUILayout.BeginHorizontal (EditorStyles.helpBox);
			scrollPosImages = EditorGUILayout.BeginScrollView (scrollPosImages, GUILayout.Width (Screen.width - 150), GUILayout.Height (140));
			GUILayout.BeginHorizontal ();

			for (int i = 0; i < selectedAsset.characterImages.Count; i++) {

				selectedAsset.characterImages [i].index = i;

				GUILayout.BeginVertical ();
				GUI.skin = defaultSkin;


				selectedAsset.characterImages [i].image = (Texture2D)EditorGUILayout.ObjectField (selectedAsset.characterImages [i].image, typeof(Texture2D), true, GUILayout.Width (texture2DSize), GUILayout.Height (texture2DSize));
				//get type


				GUI.skin = editorSkin;
				selectedAsset.characterImages [i].name = EditorGUILayout.TextField ("", selectedAsset.characterImages [i].name, GUILayout.Width (texture2DSize));
				if (GUILayout.Button ("X", GUILayout.Width (texture2DSize))) {
					selectedAsset.characterImages.RemoveAt (i);
					GUILayout.EndVertical ();
					i = -1;
				}
				GUILayout.EndVertical ();
			

			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			GUILayout.EndHorizontal ();
		
			EditorGUILayout.EndScrollView ();

			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();


		}

		//========= CONVERSATIONS =============//
		if (GUILayout.Button (GetFoldedString (storyInfo.isConversation) + "Conversations", editorSkin.GetStyle (styleButtonHeaderB)))
			storyInfo.isConversation = !storyInfo.isConversation;

		if (storyInfo.isConversation)
			DisplayConversation ();

		EditorGUILayout.EndScrollView ();

		if (GUILayout.Button ("Check Dictionary")) {
			Debug.Log ("Page Count: " + pageDict.Count);
			Debug.Log ("Characters/Chapters Count: " + charDict.Count);
		}

		//Have it called everytime you create something new! 
		if (GUI.changed) {
			DictionarySetup ();
		}

		storyInfoObject.ApplyModifiedProperties ();
		//storyObject.ApplyModifiedProperties ();

	}

	public void MakeTitleLabel (string text)
	{
		GUILayout.BeginHorizontal (EditorStyles.helpBox);
		int lastSize = GUI.skin.label.fontSize;
		GUI.skin.label.fontSize = 15;
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUILayout.Label (text);
		GUI.skin.label.fontSize = lastSize;
		GUILayout.EndHorizontal ();
	}

	//Creation & Selection
	public void DisplayStoryElement ()
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Space (padLeft / 2);
		GUILayout.BeginVertical (EditorStyles.helpBox);
		storyElementType = (StoryElementType)EditorGUILayout.EnumPopup ("Select Story Element", storyElementType);

		string[] selectElementStrings = GetStoryBaseNames (storyElementType);
		storyElementIndex = EditorGUILayout.Popup ("Select Character", storyElementIndex, selectElementStrings);


		GUILayout.BeginHorizontal (EditorStyles.helpBox);
		inputName = EditorGUILayout.TextField ("Name: ", inputName);

		if (GUILayout.Button ("New", GUILayout.Width (smallButtonWidth))) {
			NewStoryElement (storyElementType);
		}
		GUILayout.EndHorizontal ();

		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
		GUILayout.Space (padTop);


	}

	//Info, name stat etc
	public void DisplayElementInfo ()
	{
		GUILayout.BeginHorizontal ();
		GUILayout.Space (padLeft / 2);
		GUILayout.BeginVertical (EditorStyles.helpBox);

		/*
		string[] selectElementStrings = GetStoryBaseNames (storyElementType);
		storyElementIndex = EditorGUILayout.Popup ("Select Character", storyElementIndex, selectElementStrings);
		selectedAsset = GetChatTypeList (storyElementType) [storyElementIndex];
*/

		GUILayout.BeginHorizontal ();

		selectedAsset.name = EditorGUILayout.TextField ("Name: ", selectedAsset.name);

		if (GUILayout.Button ("X", GUILayout.Width (xSmallButtonWidth))) {
			GetChatTypeList (storyElementType).RemoveAt (storyElementIndex);
			storyElementIndex--;
			Repaint ();
			return;
		}
		GUILayout.EndHorizontal ();

		GUILayout.EndVertical ();
	


		GUILayout.EndHorizontal ();
		GUILayout.Space (padTop);
	}

	public List<StoryInfo.StoryBase> GetChatTypeList (StoryElementType elementType)
	{
		if (storyElementType == StoryElementType.character)
			return storyInfo.characters;
		else
			return storyInfo.chapters;
	}



	public void DisplayConversation ()
	{


		GUI.backgroundColor = editorColorInfo.colorList.conversationColor;

		GUILayout.BeginHorizontal ();

		GUILayout.Space (padLeft / 2);

		GUILayout.BeginVertical (EditorStyles.helpBox);

		List<StoryInfo.Conversation> conversations = selectedAsset.conversations;

		for (int i = 0; i < conversations.Count; i++) {

			if (GUILayout.Button (i.ToString () + ": " + conversations [i].name, editorSkin.GetStyle (styleButtonHeader)))
				conversations [i].isActive = !conversations [i].isActive;

			GUILayout.BeginHorizontal ();
			GUILayout.Space (padLeft);
			GUILayout.BeginVertical ();

			if (conversations [i].isActive) {

				GUILayout.BeginHorizontal (EditorStyles.helpBox);
				conversations [i].name = EditorGUILayout.TextField ("Name: ", conversations [i].name);
				GUILayout.EndHorizontal ();


				if (GUILayout.Button (GetFoldedString (conversations [i].isDisplayPages) + "Pages (" + conversations [i].pages.Count + ")", editorSkin.GetStyle (styleButtonHeaderB)))
					conversations [i].isDisplayPages = !conversations [i].isDisplayPages;

				if (conversations [i].isDisplayPages) {
					//For each pages display
					DisplayPages (i);


				}

			}

			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();
		}

		GUILayout.BeginHorizontal (EditorStyles.helpBox);
		inputConversation = EditorGUILayout.TextField ("Name: ", inputConversation);
		if (GUILayout.Button ("New", GUILayout.Width (smallButtonWidth))) {
			NewConversation ();
		}
		GUILayout.EndHorizontal ();


		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
	}

	public void DisplayPages (int convoIndex)
	{

		GUI.backgroundColor = editorColorInfo.colorList.pageColor;

		GUILayout.BeginHorizontal ();

		if (selectedAsset.conversations [convoIndex].pages.Count > 0)
			GUILayout.Space (padLeft);
		
		GUILayout.BeginVertical (EditorStyles.helpBox);

		for (int i = 0; i < selectedAsset.conversations [convoIndex].pages.Count; i++) {
			
			StoryInfo.Page page = selectedAsset.conversations [convoIndex].pages [i];


			page.name = page.text;
			if (page.text.Length > 50) {
				page.name = page.text.Substring (0, 50) + "...";
			}

			//#new

			float pageButtonWidth = Screen.width - (xSmallButtonWidth * 2) - (padLeft * 4.8F);

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (i.ToString () + ": " + page.name, editorSkin.GetStyle (styleButtonHeader), GUILayout.Width (pageButtonWidth)))
				page.isActive = !page.isActive;

			#region b
			if (GUILayout.Button ("^", GUILayout.Width (xSmallButtonWidth))) {

				StoryInfo.Page newPage = selectedAsset.conversations [convoIndex].pages [i - 1];
				selectedAsset.conversations [convoIndex].pages [i - 1] = page;
				selectedAsset.conversations [convoIndex].pages [i] = newPage;

				DictionarySetup ();

				i = 0;

				return;
			}

			if (GUILayout.Button ("v", GUILayout.Width (xSmallButtonWidth))) {

				StoryInfo.Page newPage = selectedAsset.conversations [convoIndex].pages [i + 1];
				selectedAsset.conversations [convoIndex].pages [i + 1] = page;
				selectedAsset.conversations [convoIndex].pages [i] = newPage;

				DictionarySetup (); //assign the info linkage at the start of every page/convo cycle!

				i = 0;

				return;
			}
		
			#endregion



			GUILayout.EndHorizontal ();
			if (page.isActive) {


				//Is dynamic
				DisplayButtonInfo (page.dynamicButtonInfo);

				//page.name = EditorGUILayout.TextField ("Name: ", page.name);

				//Texture2D is a serialized property....




				//GUILayout.Button ("Output Text", editorSkin.GetStyle (styleButtonHeaderB));
				page.speakerName = EditorGUILayout.TextField ("Speaker: ", page.speakerName);
				//Show text box



				GUILayout.BeginHorizontal (EditorStyles.helpBox);


				GUILayout.BeginVertical ();

				#region ### Character Image ###


				//Instead of using GUID, I'm directly linking to character image file. //I don't want too many GUID n dicts lol...

				GUI.skin = defaultSkin;

				if (selectedAsset.characterImages.Count <= 0)
					page.isCustomImage = true;

				if (page.isCustomImage) {
					//make sure index is correct
					page.characterImageIndex = selectedAsset.characterImages.Count;
					page.outputCharacterImage.image = (Texture2D)EditorGUILayout.ObjectField (page.outputCharacterImage.image, typeof(Texture2D), true, GUILayout.Width (texture2DSize), GUILayout.Height (texture2DSize));

				} else {
//					Debug.Log (page.characterImage.name);
					page.characterImageIndex = page.characterImage.index;
					page.characterImage.image = (Texture2D)EditorGUILayout.ObjectField (selectedAsset.characterImages [page.characterImageIndex].image, typeof(Texture2D), true, GUILayout.Width (texture2DSize), GUILayout.Height (texture2DSize));
				}




				string[] characterImages = StoryGetters.GetCharacterImagesNames (selectedAsset);
				page.characterImageIndex = EditorGUILayout.Popup ("", page.characterImageIndex, characterImages, GUILayout.Width (texture2DSize));
			
				if (page.characterImageIndex != selectedAsset.characterImages.Count) {
					page.characterImage = selectedAsset.characterImages [page.characterImageIndex];
					page.isCustomImage = false;
				} else {
					page.isCustomImage = true;
				}
				GUI.skin = editorSkin;
				#endregion

				GUILayout.EndVertical ();

				page.text = GUILayout.TextArea (page.text, GUILayout.MinHeight (90), GUILayout.Width (Screen.width - 180));

				GUILayout.FlexibleSpace ();

				GUILayout.EndHorizontal ();



				GUILayout.BeginVertical (EditorStyles.helpBox);

				//Responses
				if (GUILayout.Button ("▼ Response Buttons", editorSkin.GetStyle (styleButtonHeaderB)))
					page.isShowButtons = !page.isShowButtons;

				//Show buttons
				if (page.isShowButtons) {
					DisplayEventButtons (convoIndex, i);
				}

				GUILayout.EndVertical ();


			}

		}

		GUILayout.BeginHorizontal (EditorStyles.helpBox);
		inputPage = EditorGUILayout.TextField ("Name: ", inputPage);
		if (GUILayout.Button ("New", GUILayout.Width (smallButtonWidth))) {
			NewPage (convoIndex);
		}
		GUILayout.EndHorizontal ();

		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();


		if (GUI.changed) {
			//Save
			//EditorUtility.SetDirty (storyInfo);
		}

	}

	//What I need to do with the buttons is, after it finds the 'page', locate the file. (the page will remember the, conversation, the conversation will remember the character)
	public void DisplayEventButtons (int convoIndex, int pageIndex)
	{
		
		List<StoryInfo.ButtonInfo> buttonInfos = selectedAsset.conversations [convoIndex].pages [pageIndex].buttonInfos;

		GUILayout.BeginHorizontal ();
		GUILayout.Space (padLeft);
		GUILayout.BeginVertical ();
		//Give each button an indent
		for (int i = 0; i < buttonInfos.Count; i++) {

			GUILayout.BeginVertical (EditorStyles.helpBox);

			if (GUILayout.Button ("▼ " + buttonInfos [i].responseText, editorSkin.GetStyle (styleButtonHeaderB))) {
				buttonInfos [i].isActive = !buttonInfos [i].isActive;
			}

			GUILayout.BeginHorizontal ();

			if (buttonInfos [i].isActive) {

				GUILayout.BeginVertical (EditorStyles.helpBox);

				//Display them
				buttonInfos [i].responseText = EditorGUILayout.TextField ("Response: ", buttonInfos [i].responseText);

				DisplayButtonInfo (buttonInfos [i]);


				GUILayout.EndVertical ();

			}

			GUILayout.EndVertical ();

			GUILayout.EndHorizontal ();

		}

		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		//inputPage = EditorGUILayout.TextField ("Name: ", inputPage);
		if (GUILayout.Button ("New", GUILayout.Width (smallButtonWidth))) {
			StoryInfo.ButtonInfo buttonInfo = new StoryInfo.ButtonInfo ();
			buttonInfos.Add (buttonInfo);
		}
		GUILayout.EndHorizontal ();

		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();



	}

	public void DisplayButtonInfo (StoryInfo.ButtonInfo buttonInfo)
	{
		buttonInfo.buttonType = (ButtonType)EditorGUILayout.EnumPopup ("Action", buttonInfo.buttonType);

		if (buttonInfo.buttonType == ButtonType.goTo) {

			//if (GUI.changed) {
			if (buttonInfo.pageGUID != "unassigned") {
				//Check if the pageGUID exists

				//Present issue, when you attempt to change buttoninfos need to reassign GUID.
				//Right, I'm checking if p
				if (pageDict.ContainsKey (buttonInfo.pageGUID)) {
					//Debug.Log ("exists");
					StoryInfo.Page pageInfo = pageDict [buttonInfo.pageGUID];


					//	Debug.Log (pageInfo.pageLinkInfo.charGUID);

					//What I should do is go into StoryInfo, Add a page link info inside button info so you can just go buttoninfo.pagelinkinfo = page.pagelinkinfo
					buttonInfo.storyElementType = pageInfo.pageLinkInfo.storyElementType;
					buttonInfo.elementIndex = pageInfo.pageLinkInfo.elementIndex;
					buttonInfo.conversationIndex = pageInfo.pageLinkInfo.conversationIndex;
					buttonInfo.pageIndex = pageInfo.pageLinkInfo.pageIndex;

					//buttonInfo.charGUID = pageInfo.pageLinkInfo.charGUID;

					pageInfo.pageLinkInfo.pageGUID = buttonInfo.pageGUID;
			

				}

				//} 
			}


			buttonInfo.storyElementType = (StoryElementType)EditorGUILayout.EnumPopup ("Story Element", buttonInfo.storyElementType);

			StoryElementType pickedStoryType = buttonInfo.storyElementType;

			//0 > 1
			if (buttonInfo.elementIndex > GetStoryBaseNames (buttonInfo.storyElementType).Length - 1)
				buttonInfo.elementIndex = 0;

			int lastElement = buttonInfo.elementIndex;

			buttonInfo.elementIndex = EditorGUILayout.Popup ("Select Character", buttonInfo.elementIndex, GetStoryBaseNames (buttonInfo.storyElementType));

			if (GUI.changed) {
				if (buttonInfo.elementIndex != lastElement) {
					buttonInfo.charGUID = "unassigned";
					buttonInfo.pageGUID = "unassigned";
				}
			}

			StoryInfo.StoryBase pickedElement = GetStoryBasedObject (pickedStoryType, buttonInfo.elementIndex);

			if (buttonInfo.conversationIndex > pickedElement.conversations.Count - 1)
				buttonInfo.conversationIndex = 0;

			buttonInfo.conversationIndex = EditorGUILayout.Popup ("Select Conversation", buttonInfo.conversationIndex, GetConversation (pickedElement));

			//Making sure a conversation exist
			if (pickedElement.conversations.Count > 0) {

				StoryInfo.Conversation pickedConversation = pickedElement.conversations [buttonInfo.conversationIndex];

				if (buttonInfo.pageIndex > pickedConversation.pages.Count - 1)
					buttonInfo.pageIndex = 0;

			
				buttonInfo.pageIndex = EditorGUILayout.Popup ("Select Page", buttonInfo.pageIndex, GetPage (pickedConversation));

				StoryInfo.Page pickedPage = pickedConversation.pages [buttonInfo.pageIndex];

				buttonInfo.pageGUID = pickedPage.GUID;
				buttonInfo.charGUID = pickedElement.GUID;

				if (GUI.changed)
					Debug.Log ("Assigned pageGUID: " + buttonInfo.pageGUID);

			} else {
				buttonInfo.pageGUID = "unassigned";
			}



			//check if button inf




		}
	}

	public void NewStoryElement (StoryElementType elementType)
	{

		StoryInfo.StoryBase storyElement = new StoryInfo.StoryBase ();
		storyElement.name = inputName;

		if (elementType == StoryElementType.character) {
			storyInfo.characters.Add (storyElement);
		}

		if (elementType == StoryElementType.chapter) {
			storyInfo.chapters.Add (storyElement);
		}

		storyElement.GUID = GetGUID ();

		storyElementIndex = GetChatTypeList (storyElementType).Count - 1;

	
		AssetValidation ();
		NewConversation ();

	}


	public void NewConversation ()
	{
		StoryInfo.Conversation conversation = new StoryInfo.Conversation ();
		conversation.name = inputConversation;
		selectedAsset.conversations.Add (conversation);
		conversation.GUID = GetGUID ();
	}

	public void NewPage (int convoIndex)
	{
		StoryInfo.Page page = new StoryInfo.Page ();
		page.name = inputPage;
		selectedAsset.conversations [convoIndex].pages.Add (page);
		page.GUID = GetGUID ();
	}

	public StoryInfo.StoryBase GetStoryBasedObject (StoryElementType eType, int index)
	{
		StoryInfo.StoryBase storyBase = new StoryInfo.StoryBase ();

		if (eType == StoryElementType.chapter)
			storyBase = storyInfo.chapters [index];

		if (eType == StoryElementType.character)
			storyBase = storyInfo.characters [index];

		return storyBase;
	}

	public string[] GetStoryBaseNames (StoryElementType elementType)
	{
		List<StoryInfo.StoryBase> list = new List<StoryInfo.StoryBase> ();

		if (elementType == StoryElementType.character)
			list = storyInfo.characters;

		if (elementType == StoryElementType.chapter)
			list = storyInfo.chapters;
		
		List<string> strings = new List<string> ();

		for (int i = 0; i < list.Count; i++) {
			strings.Add (i.ToString () + ":" + list [i].name);
		}

		return strings.ToArray ();
	}

	public string[] GetConversation (StoryInfo.StoryBase baseAsset)
	{
		List<StoryInfo.Conversation> list = baseAsset.conversations;

		List<string> strings = new List<string> ();

		for (int i = 0; i < list.Count; i++) {
			strings.Add (i.ToString () + ":" + list [i].name);
		}

		return strings.ToArray ();
	}

	public string[] GetPage (StoryInfo.Conversation baseConversation)
	{
		List<StoryInfo.Page> list = baseConversation.pages;

		List<string> strings = new List<string> ();

		for (int i = 0; i < list.Count; i++) {
			strings.Add (i.ToString () + ":" + list [i].name);
		}

		return strings.ToArray ();
	}

	public void GetEditorSkin ()
	{
		if (defaultSkin == null)
			defaultSkin = GUI.skin;
		if (editorSkin == null)
			editorSkin = (GUISkin)(AssetDatabase.LoadAssetAtPath ("Assets/GUISkins/GUISkin.guiskin", typeof(GUISkin)));
		GUI.skin = editorSkin;
	}



	public void AssetValidation ()
	{
		if (storyElementIndex <= 0)
			storyElementIndex = 0;

		if (storyInfo.characters.Count == 0) {
			inputName = "Template Character";
			NewStoryElement (StoryElementType.character);
		}

		if (storyInfo.chapters.Count == 0) {
			inputName = "Template Chapter";
			NewStoryElement (StoryElementType.chapter);
		}

		//Generate a get lists later.
		if (storyElementType == StoryElementType.character) {
			if (storyElementIndex > storyInfo.characters.Count - 1)
				storyElementIndex = storyInfo.characters.Count;
		}

		if (storyElementType == StoryElementType.chapter) {
			if (storyElementIndex > storyInfo.chapters.Count - 1)
				storyElementIndex = storyInfo.chapters.Count;
		}

		selectedAsset = GetChatTypeList (storyElementType) [storyElementIndex];

	}

	//Maybe everytime something is created, it lists itself into the dictionary?

	//Add page, //Add character
	public void AddToDictionary ()
	{

	}

	//Current this dictionary set up clears everything and readd it. (It's dirty but it temporarily works.
	public void DictionarySetup ()
	{

		charDict.Clear ();
		pageDict.Clear ();
		if (storyInfo.characters == null)
			Debug.Log ("null");
		GenerateDictionaryFromList (storyInfo.characters, StoryElementType.character);
		GenerateDictionaryFromList (storyInfo.chapters, StoryElementType.chapter);

	}

	public void GenerateDictionaryFromList (List<StoryInfo.StoryBase> list, StoryElementType sType)
	{
		int charCount = 0;
		foreach (StoryInfo.StoryBase storyBase in list) {
			int convoCount = 0;
			storyBase.currentIndex = charCount;
			foreach (StoryInfo.Conversation conversation in storyBase.conversations) {
				int pageCount = 0;
				conversation.currentIndex = convoCount;
				conversation.parentIndex = charCount;
				foreach (StoryInfo.Page page in conversation.pages) {
					page.currentIndex = pageCount;
					page.parentIndex = convoCount;






					page.pageLinkInfo.elementIndex = charCount;

					page.pageLinkInfo.storyElementType = sType;

					page.pageLinkInfo.conversationIndex = convoCount;
					page.pageLinkInfo.pageIndex = pageCount;
					page.pageLinkInfo.charGUID = storyBase.GUID;
					page.pageLinkInfo.pageGUID = page.GUID;
					pageDict.Add (page.GUID, page);
					pageCount++;
				}
				convoCount++;
			}
			charDict.Add (storyBase.GUID, storyBase);
			charCount++;
		}
	}



	public void StoryFileValidation ()
	{
		if (storyInfo == null)
			storyInfo = AssetDatabase.LoadAssetAtPath (pathToStory + "Story.asset", typeof(StoryInfo)) as StoryInfo;
		if (storyInfo == null)
			Create ();
	}

	public StoryInfo  Create ()
	{
		StoryInfo asset = ScriptableObject.CreateInstance<StoryInfo> ();
		AssetDatabase.CreateAsset (asset, pathToStory + "Story.asset");
		AssetDatabase.SaveAssets ();
		return asset;
	}

	public void Deselect ()
	{
		GUIUtility.hotControl = 0;
		GUIUtility.keyboardControl = 0;
	}


	public void CopyGUIStyle ()
	{
		GUIStyle buttonStyle = GUI.skin.GetStyle ("Button");
		GUIStyle titleStyle = GUI.skin.GetStyle (styleSmallButton);

		titleStyle.normal = buttonStyle.normal;
		titleStyle.hover = buttonStyle.hover;
		titleStyle.active = buttonStyle.active;
		titleStyle.focused = buttonStyle.focused;
		titleStyle.onNormal = buttonStyle.onNormal;
		titleStyle.onHover = buttonStyle.onHover;
		titleStyle.border = buttonStyle.border;
		titleStyle.margin = buttonStyle.margin;
		titleStyle.padding = buttonStyle.padding;
		titleStyle.overflow = buttonStyle.overflow;
		titleStyle.font = buttonStyle.font;
		titleStyle.fontSize = buttonStyle.fontSize;
		titleStyle.fontStyle = buttonStyle.fontStyle;
		titleStyle.alignment = buttonStyle.alignment;
		titleStyle.wordWrap = buttonStyle.wordWrap;
		titleStyle.richText = buttonStyle.richText;
		titleStyle.clipping = buttonStyle.clipping;
		titleStyle.imagePosition = buttonStyle.imagePosition;
		titleStyle.contentOffset = buttonStyle.contentOffset;
		titleStyle.fixedWidth = buttonStyle.fixedWidth;
		titleStyle.fixedHeight = buttonStyle.fixedHeight;
		titleStyle.stretchWidth = buttonStyle.stretchWidth;
		titleStyle.stretchHeight = buttonStyle.stretchHeight;
	}

	public string GetGUID ()
	{
		string[] myStrings = { "a", "b", "c", "d", "e", "f" };
		string creationCount = storyInfo.creationCount.ToString ();
		string alpha = myStrings [storyInfo.currentAlpha];

		string newGUID = alpha + creationCount; 
	
		storyInfo.creationCount++;
		int maxInt = 2147483647; //Max integar

		if (storyInfo.creationCount >= maxInt) {
			storyInfo.creationCount = 0;
			storyInfo.currentAlpha++;
		}

		//Make new one
		return newGUID;
	}

}

public enum NameTypes
{
	chapter,
	character,
	conversation,
	page

}

//A dictionary of pages
//Search key "For mother"
//pageDict[i].contains 