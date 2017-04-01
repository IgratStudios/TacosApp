using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementScaleAC : AnimatedElementController 
{
	public bool autoStartOnEnable = true;
	public bool setStartingScaleAtStart = true;

	public bool setStartingScaleAtStop = false;
	public bool setTargetScaleAtStop = true;

	public Vector3 startScale = Vector3.zero;
	public Vector3 targetScale = Vector3.one;
	private Vector3 diff;
	public float duration = 1.0f;
	public float delay = 0.0f;
	private float speed = 1.0f;
	public AnimationCurve curve;
	private bool isPlayingBackwards = false;
	private bool isAnimationFinished = false;

	public override void OnEnable ()
	{
		if(autoStartOnEnable)
		{
			SwitchAnimation(true);
		}
	}

	public override void OnDisable ()
	{
		if(IsAnimating)
		{
			SwitchAnimation(false);
		}
	}

	public void SetStartingScale()
	{
		CachedTransform.localScale = startScale;
	}

	public void SetEndingScale()
	{
		CachedTransform.localScale = targetScale;
	}

	public override void ResetAnimation ()
	{
		progress = 0.0f;
	}

	public override void StartAnimation ()
	{
		if(CachedGameObject.activeInHierarchy)
		{
			speed = 1.0f/duration;
			diff = targetScale - startScale;
			isAnimationFinished = false;
			isPlayingBackwards = false;
			if(setStartingScaleAtStart)
			{
				CachedTransform.localScale = startScale;
			}
			StartCoroutine("ScaleUp");
		}
	}

	public override void StopAnimation ()
	{
		isAnimationFinished = true;
		if(setStartingScaleAtStop)
		{
			CachedTransform.localScale = startScale;
		}
		else if(setTargetScaleAtStop)
		{
			CachedTransform.localScale = targetScale;
		}
		StopCoroutine("ScaleUp");
		StopCoroutine("MustWaitOtherAnimatedControllers");
	}

	IEnumerator ScaleUp()
	{
		//HACK to avoid first time playing on autostart
		yield return 0;

		yield return StartCoroutine("MustWaitOtherAnimatedControllers");

		if(delay > 0.0f)
		{
			yield return new WaitForSeconds(delay);
		}
		while(!isAnimationFinished)
		{
			switch(animationType)
			{
			case ELEMENT_ANIMATION_TYPE.LOOP:
				{
					progress += Time.deltaTime*speed;

					Vector3 newScale = startScale + diff*curve.Evaluate(progress);
					CachedTransform.localScale = newScale;

					if(progress >= 1.0f)
					{
						CachedTransform.localScale = targetScale;
						progress = 0.0f;
					}
				}
				break;
			case ELEMENT_ANIMATION_TYPE.PINGPONG:
				{
					if(isPlayingBackwards)
					{
						progress -= Time.deltaTime*speed;
						Vector3 newScale = startScale + diff*curve.Evaluate(progress);
						CachedTransform.localScale = newScale;

						if(progress <= 0.0f)
						{
							CachedTransform.localScale = startScale;
							progress = 0.0f;
							isPlayingBackwards = false;
						}
					}
					else
					{
						progress += Time.deltaTime*speed;
						Vector3 newScale = startScale + diff*curve.Evaluate(progress);
						CachedTransform.localScale = newScale;

						if(progress >= 1.0f)
						{
							CachedTransform.localScale = targetScale;
							progress = 1.0f;
							isPlayingBackwards = true;
						}
					}
				}
				break;
			case ELEMENT_ANIMATION_TYPE.PINGPONGONCE:
				if(isPlayingBackwards)
				{
					progress -= Time.deltaTime*speed;
					Vector3 newScale = startScale + diff*curve.Evaluate(progress);
					CachedTransform.localScale = newScale;

					if(progress <= 0.0f)
					{
						CachedTransform.localScale = startScale;
						progress = 0.0f;
						isPlayingBackwards = false;
						isAnimationFinished = true;
					}
				}
				else
				{
					progress += Time.deltaTime*speed;
					Vector3 newScale = startScale + diff*curve.Evaluate(progress);
					CachedTransform.localScale = newScale;

					if(progress >= 1.0f)
					{
						CachedTransform.localScale = targetScale;
						progress = 1.0f;
						isPlayingBackwards = true;
					}
				}
				break;
			case ELEMENT_ANIMATION_TYPE.SINGLE:
				{
					progress += Time.deltaTime*speed;

					Vector3 newScale = startScale + diff*curve.Evaluate(progress);
					CachedTransform.localScale = newScale;

					if(progress >= 1.0f)
					{
						CachedTransform.localScale = targetScale;
						progress = 1.0f;
						isAnimationFinished = true;
					}
				}
				break;
			}
			yield return 0;
		}
		SwitchAnimation(false);
	}

}
