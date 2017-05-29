using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorColors : ScriptableObject
{

	public ColorList colorList = new ColorList ();

	[System.Serializable]
	public class ColorList
	{
		public  Color normalColor = Color.white;
		public Color conversationColor = Color.white;
		public  Color pageColor = Color.white;

	}

}
