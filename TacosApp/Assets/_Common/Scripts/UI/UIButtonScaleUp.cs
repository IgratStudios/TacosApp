using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// User interface element scale up on press and scale down on unpress.
/// </summary>
public class UIButtonScaleUp : EventTrigger
{
	public Vector3 targetScale = new Vector3(1.1f, 1.1f, 1.1f);
	private Vector3 startingScale = new Vector3(-1.0f, -1.0f, -1.0f);
	private RectTransform rectTransform;
	public float scaleUpDuration = 0.1f;
	private float scaleUpSpeed = 10.0f;
	private bool isScalingUp = false;
	private float scalePercent = 0.0f;

	public Graphic colorTarget;
	private Color unpressedColor = Color.white;
	public Color pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.0f);

	private EventTrigger eventTrigger;

	void OnEnable()
	{
		scalePercent = 0.0f;
		if(rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		if(startingScale == new Vector3(-1.0f, -1.0f, -1.0f))
		{
			startingScale = rectTransform.localScale;
		}
		scaleUpSpeed = 1.0f/scaleUpDuration;
		if(colorTarget != null)
		{
			if(unpressedColor == Color.white)
			{
				unpressedColor = colorTarget.color;
			}
			colorTarget.color = unpressedColor;
		}
	}

	void OnDisable()
	{
		scalePercent = 0.0f;
		isScalingUp = false;
	}

	public override void OnPointerClick( PointerEventData data )
	{
		rectTransform.localScale = startingScale;
		isScalingUp = false;
		if(colorTarget != null)
		{
			colorTarget.color = unpressedColor;
		}
		base.OnPointerClick(data);
	}

	public override void OnPointerUp( PointerEventData data )
	{
		isScalingUp = false;
		if(colorTarget != null)
		{
			colorTarget.color = unpressedColor;
		}
		base.OnPointerUp(data);
	}

	public override void OnPointerDown( PointerEventData data )
	{
		isScalingUp = true;
		if(colorTarget != null)
		{
			colorTarget.color = (unpressedColor - pressedColor);
		}
		base.OnPointerDown(data);
	}

	void Update()
	{
		if(rectTransform != null)
		{
			if(isScalingUp)
			{
				if(scalePercent < 1.0f)
				{
					scalePercent += Time.deltaTime*scaleUpSpeed;
					rectTransform.localScale = Vector3.Lerp(startingScale,targetScale,scalePercent);
					if(scalePercent >= 1.0f)
					{
						scalePercent = 1.0f;
						rectTransform.localScale = targetScale;
					}
				}
			}
			else
			{
				if(scalePercent > 0.0f)
				{
					scalePercent -= Time.deltaTime*scaleUpSpeed;
					rectTransform.localScale = Vector3.Lerp(startingScale,targetScale,scalePercent);
					if(scalePercent <= 0.0f)
					{
						scalePercent = 0.0f;
						rectTransform.localScale = startingScale;
					}
				}
			}
		}
	}

}
