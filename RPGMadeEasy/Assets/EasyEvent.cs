using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyEvent : MonoBehaviour
{

	public List<EasyEventInfo> events = new List<EasyEventInfo> ();

	[System.Serializable]
	public class EasyEventInfo
	{
		public EasyEventType eventType;

		//Wait
		public WaitType waitType;
		public float waitTime = 0;

		//Moving:
		public Transform target;
		public Transform targetTo;
		public AnimationCurve curve;
		public Vector3 moveDist;
		public Vector3 rotDist;
		public MoveType moveType;

		//Set Visiblity
		public VisibilityType visiblityType;

		//Dialouge
		public StoryElementType dialougeType;
		public int dialougeIndex = 0;
		public int conversationIndex = 0;
		public string conversationGUID = "";

		//Debug Log
		public string outputText = "Default output text";

		//GUI
		public float elementHeight;
	}



}

public enum EasyEventType
{
	move,
	setPos,
	setActive,
	emoji,
	waitForSeconds,
	dialouge,
	log,
}

public enum VisibilityType
{
	off,
	on
}

public enum MoveType
{
	vector3,
	target
}
