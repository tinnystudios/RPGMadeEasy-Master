using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Chat : MonoBehaviour
{
	public int storyElementIndex = 0;
	public int conversationIndex = 0;
	public int pageIndex = 0;
	public string GUID = "";
	public StoryInfo.StoryBase chatInfo;
}

#region Drawer
[CustomEditor (typeof(Chat))]
public class ChatDraw : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		Chat chatScript = (Chat)target;
		Dictionary<string,  StoryInfo.StoryBase> charDict = StoryGetters.GetCharacterDictionary (StoryGetters.GetStoryInfo ().characters);
		//GUID exist, check if the character still exists, else clean up!
		if (chatScript.GUID != "") {
			if (charDict.ContainsKey (chatScript.GUID)) {
				//Character exists
				chatScript.chatInfo = charDict [chatScript.GUID];
				chatScript.storyElementIndex = chatScript.chatInfo.currentIndex;
			} else {
				chatScript.storyElementIndex = 0;
				chatScript.GUID = "";
			}
		}


		string[] chatInfoNames = StoryGetters.GetStoryBaseNames (StoryGetters.GetStoryInfo (), StoryElementType.character);
		chatScript.storyElementIndex = EditorGUILayout.Popup ("Select Character", chatScript.storyElementIndex, chatInfoNames);

		chatScript.chatInfo = StoryGetters.GetStoryInfo ().characters [chatScript.storyElementIndex];
		chatScript.GUID = chatScript.chatInfo.GUID;

	}
}
#endregion
