using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInfo : MonoBehaviour
{
	public List<ActorTags> actorTags;
	public static SceneInfo _instance;

	[System.Serializable]
	public class ActorTags
	{
		public string name = "defaultName";
		public GameObject actorObject;
	}

	public static SceneInfo singletonInstance {
		get { 
			if (_instance == null)
				_instance = GameObject.FindObjectOfType<SceneInfo> ();
			return _instance;
		}
	}
}
