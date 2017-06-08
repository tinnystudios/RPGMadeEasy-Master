using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiInfo : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	public void SetSprite (Sprite newSprite)
	{
		spriteRenderer.sprite = newSprite;
	}
}
