using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBase : MonoBehaviour
{
	public IEnumerator _Move (Transform moveTarget, Vector3 endPos, AnimationCurve curve)
	{
		Vector3 startPos = moveTarget.position;

		float dist = Vector3.Distance (startPos, endPos);
		float speed = 6;
		float force = speed / dist;


		for (float i = 0; i < 1f; i += Time.deltaTime * force) {
			moveTarget.position = Vector3.Lerp (startPos, endPos, curve.Evaluate (i));
			yield return null;
		}

		moveTarget.position = Vector3.Lerp (startPos, endPos, curve.Evaluate (1));


		//moveTarget.position = endPos;
	}
}
