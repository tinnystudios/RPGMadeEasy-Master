using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryGetters
{

	public static List<StoryInfo.StoryBase> GetChatTypeList (StoryInfo storyInfo, StoryElementType elementType)
	{
		if (elementType == StoryElementType.character)
			return storyInfo.characters;
		else
			return storyInfo.chapters;
	}

	public static StoryInfo.StoryBase GetStoryBasedObject (StoryInfo storyInfo, StoryElementType eType, int index)
	{
		StoryInfo.StoryBase storyBase = new StoryInfo.StoryBase ();

		if (eType == StoryElementType.chapter)
			storyBase = storyInfo.chapters [index];

		if (eType == StoryElementType.character)
			storyBase = storyInfo.characters [index];

		return storyBase;
	}

	public static string[] GetStoryBaseNames (StoryInfo storyInfo, StoryElementType elementType)
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

	public static string[] GetConversation (StoryInfo.StoryBase baseAsset)
	{
		List<StoryInfo.Conversation> list = baseAsset.conversations;

		List<string> strings = new List<string> ();

		for (int i = 0; i < list.Count; i++) {
			strings.Add (i.ToString () + ":" + list [i].name);
		}

		return strings.ToArray ();
	}

	public static string[] GetPage (StoryInfo.Conversation baseConversation)
	{
		List<StoryInfo.Page> list = baseConversation.pages;

		List<string> strings = new List<string> ();

		for (int i = 0; i < list.Count; i++) {
			strings.Add (i.ToString () + ":" + list [i].name);
		}

		return strings.ToArray ();
	}

	public static string[] GetCharacterImagesNames (StoryInfo.StoryBase storyBase)
	{
		List<StoryInfo.CharacterImage> list = storyBase.characterImages;

		List<string> strings = new List<string> ();

		for (int i = 0; i < list.Count; i++) {
			strings.Add (i.ToString () + ":" + list [i].name);
		}

		strings.Add ("Custom");

		return strings.ToArray ();
	}

	public static StoryInfo GetStoryInfo ()
	{
		return Resources.Load ("Stories/Story") as StoryInfo;	
	}


	public static  Dictionary<string,  StoryInfo.StoryBase> GetCharacterDictionary (List<StoryInfo.StoryBase> list)
	{
		Dictionary<string,  StoryInfo.StoryBase> charDict = new Dictionary<string, StoryInfo.StoryBase> ();
		int charCount = 0;

		foreach (StoryInfo.StoryBase storyBase in list) {
			storyBase.currentIndex = charCount;
			charDict.Add (storyBase.GUID, storyBase);
			charCount++;
		}

		return charDict;
	}

	public static  Dictionary<string,  StoryInfo.Page> GetPagesDictionary ()
	{
		List<StoryInfo.StoryBase> list = new List<StoryInfo.StoryBase> (GetStoryInfo ().characters);
		List<StoryInfo.StoryBase> list2 = GetStoryInfo ().chapters;

		Dictionary<string,  StoryInfo.Page> pageDict = new Dictionary<string, StoryInfo.Page> ();

		int charCount = 0;

		foreach (StoryInfo.StoryBase storyBase in list) {
			storyBase.currentIndex = charCount;

			int cIndex = 0;
			foreach (StoryInfo.Conversation convo in storyBase.conversations) {
			
				int pIndex = 0;
				foreach (StoryInfo.Page page in convo.pages) {

					page.pageLinkInfo.charGUID = storyBase.GUID;
					page.pageLinkInfo.conversationIndex = cIndex;
					page.pageLinkInfo.elementIndex = charCount;
					page.pageLinkInfo.pageIndex = pIndex;

					pageDict.Add (page.GUID, page);
					pIndex++;
				}

				cIndex++;
			}
			charCount++;
		}

		charCount = 0;
		foreach (StoryInfo.StoryBase storyBase in list2) {
			storyBase.currentIndex = charCount;

			int cIndex = 0;
			foreach (StoryInfo.Conversation convo in storyBase.conversations) {

				int pIndex = 0;
				foreach (StoryInfo.Page page in convo.pages) {

					page.pageLinkInfo.charGUID = storyBase.GUID;
					page.pageLinkInfo.conversationIndex = cIndex;
					page.pageLinkInfo.elementIndex = charCount;
					page.pageLinkInfo.pageIndex = pIndex;

					pageDict.Add (page.GUID, page);
					pIndex++;
				}

				cIndex++;
			}
			charCount++;
		}

		return pageDict;
	}

	public static  Dictionary<string,  StoryInfo.StoryBase> GetStoryElementDictionary (StoryInfo storyInfo)
	{

		Dictionary<string,  StoryInfo.StoryBase> elementDict = new Dictionary<string, StoryInfo.StoryBase> ();
		int charCount = 0;

		foreach (StoryInfo.StoryBase storyBase in storyInfo.characters) {
			storyBase.currentIndex = charCount;
			elementDict.Add (storyBase.GUID, storyBase);
			charCount++;
		}

		charCount = 0;

		foreach (StoryInfo.StoryBase storyBase in storyInfo.chapters) {
			storyBase.currentIndex = charCount;
			elementDict.Add (storyBase.GUID, storyBase);
			charCount++;
		}

		return elementDict;
	}

	public static Dictionary<string,string> GetTagNames ()
	{
		Dictionary<string,string> tagNameDict = new Dictionary<string,string> ();
		//Tag name dict
		tagNameDict.Clear ();

		foreach (StoryInfo.TagName tag in GetStoryInfo().tags)
			tagNameDict.Add (tag.tagName, tag.tagOutput);

		return tagNameDict;
	}

	public static string GetStringFromTag (Dictionary<string,string> tagNameDict, string tagName)
	{
		string name = "";

		if (tagNameDict.ContainsKey (tagName)) {
			name = tagNameDict [tagName];
		}

		return name;
	}

	public static string GetStringAndAppendFromTag (Dictionary<string,string> tagNameDict, string output)
	{
		//No # means no tags
		if (!output.Contains ("#")) {
			return output;
		}

		string outputText = "";
		string[] outputArray = output.Split ('#');
		string tagName = outputArray [1];

		string name = "";

		if (tagNameDict.ContainsKey (tagName)) {
			name = tagNameDict [tagName];
		}
			

		if (tagNameDict.ContainsKey (tagName)) {

			//Remove the end
			outputText = "";
			outputText += outputArray [0];
			outputText += name;
			outputText += outputArray [2];

		}

		return outputText;

	}

}
