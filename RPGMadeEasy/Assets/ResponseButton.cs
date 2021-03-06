﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ResponseButton : MonoBehaviour
{
	public EasyEvent.PageEvent pageEvent;
	public Text text;
	public UnityEvent myEvent;
	public ChatManager chatManager;
	public StoryInfo.ButtonInfo buttonInfo;
	public ResponseButton responseButton;

	void Awake ()
	{
		Button btn = GetComponent<Button> ();
		btn.onClick.AddListener (ButtonPressed);

	}

	public void ButtonPressed ()
	{
		ChatManager.singletonInstance.ButtonPressed (pageEvent);
		EventSystem.current.SetSelectedGameObject (null);
		//myEvent.Invoke ();

		return;
		switch (buttonInfo.buttonType) {

		case ButtonType.none:

			//Continue casually

			break;

		case ButtonType.goTo:
			//Get CharID from page
		
			chatManager.CloseConversation (true);
			chatManager.OpenConversation (buttonInfo.charGUID, buttonInfo.conversationIndex, buttonInfo.pageIndex);

			//Go directly to page.
			break;

		case ButtonType.close:
			chatManager.CloseConversation ();
			break;

		}


	}
}
