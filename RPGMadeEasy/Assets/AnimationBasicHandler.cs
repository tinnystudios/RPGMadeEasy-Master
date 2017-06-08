using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBasicHandler : MonoBehaviour
{
	public Animator anim;

	public void Close ()
	{
		gameObject.SetActive (false);
	}

}
  