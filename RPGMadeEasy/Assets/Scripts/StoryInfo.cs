using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StoryInfo : ScriptableObject
{
	//GUID Generator
	public int creationCount = 0;
	public int currentAlpha = 0;

	public List<StoryBase> characters = new List<StoryBase> ();
	public List<StoryBase> chapters = new List<StoryBase> ();

	public bool isConversation;
	public bool isInfo;
	public bool isStoryElement = true;

	[SerializeField]
	public Dictionary <string,StoryBase> storyDictionary;
	//Helps find a character
	public List<TagName> tags = new List<TagName> ();

	[System.Serializable]
	public class CharacterImage
	{
		public Texture2D image;
		public string name;
		public int index = 0;
	}

	[System.Serializable]
	public class TagName
	{
		public string tagName = "";
		public string tagOutput = "";
	}

	[System.Serializable]
	public class Conversation : BasicInfo
	{
		public bool isDisplayPages;
		public List<Page> pages = new List<Page> ();
		public LinkInfo linkInfo = new LinkInfo ();
	}

	[System.Serializable]
	public class LinkInfo
	{
		public string dialougeGUID = "";
		public int dialougeIndex;
		public int conversationIndex;
	}

	[System.Serializable]
	public class Page : BasicInfo
	{
		public string text = "This the default text for a page";
		public bool isShowButtons;
		public List<ButtonInfo> buttonInfos = new List<ButtonInfo> ();
		public PageLinkInfo pageLinkInfo = new PageLinkInfo ();
		public ButtonInfo dynamicButtonInfo = new ButtonInfo ();
		public string speakerName = "Default speaker";
		//This page lets you jumps to another page.
		public CharacterImage characterImage;
		public CharacterImage outputCharacterImage;
		public int characterImageIndex = 0;
		public bool isCustomImage = false;
	}

	[System.Serializable]
	public class StoryBase : BasicInfo
	{
		public List<Conversation> conversations = new List<Conversation> ();
		public bool isConversation;
		public bool isInfo;
		public bool isStoryElement;
		public List<CharacterImage> characterImages = new List<CharacterImage> ();
	}

	[System.Serializable]
	public class ButtonInfo : PageLinkInfo
	{
		public ButtonType buttonType;
		public string responseText = "This is the default response";
		public bool isActive;
		//public PageLinkInfo pageLinkInfo;

	}


	[System.Serializable]
	public class PageLinkInfo
	{
		//PAGE NEEDS A PAGE LINK INFO, DONE!!!
		//Index? //Unique ID?
		public StoryElementType storyElementType;
		public int elementIndex = 0;
		public int conversationIndex = 0;
		public int pageIndex = 0;
		public string pageGUID = "unassigned";
		public string charGUID = "unassigned";
	}



	[System.Serializable]
	public class BasicInfo
	{
		public string name = "default";
		public string GUID = "default";
		public bool isActive = false;
		public string stringFold = "";
		public int parentIndex = 0;
		public int currentIndex = 0;
		public PlayMethod playMethod;
	}

}

public enum PlayMethod
{
	linear,
	nonLinear,
	jumpTo,
	close
}

public enum ButtonType
{
	none,
	goTo,
	close
}

public enum StoryElementType
{
	character,
	chapter,
	custom
}

//Next thing to do is to create unique ids
//Create a dictionary on load based on current data each time
//Add to dictionary
//References will be done via UniqueId.
