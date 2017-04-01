using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementMoveAC : AnimatedElementController 
{
	public bool autoStartOnEnable = true;
	public bool setStartingPositionAtStart = true;
	public bool setTargetPositionAtStop = true;
	public Vector3 originPosition = Vector3.zero;
	public Vector3 destinyPosition = Vector3.one;
	private Vector3 diff;
	public float duration = 1.0f;
	public float delay = 0.0f;
	private float speed = 1.0f;
	public AnimationCurve curve;
	private bool isPlayingBackwards = false;
	private bool isAnimationFinished = false;

	public bool setOriginPositionOnEnable = false;
	public bool setDestinyPositionAsOriginPositionOffset = false;

	public void SetStartAndEndPositions(Vector3 start, Vector3 destiny)
	{
		originPosition = start;
		destinyPosition = destiny;
	}

	public override void OnEnable ()
	{
		if(setOriginPositionOnEnable)
		{
			originPosition = CachedTransform.position;
		}
		if(setDestinyPositionAsOriginPositionOffset)
		{
			destinyPosition = originPosition + destinyPosition;
		}
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

	public override void ResetAnimation ()
	{
		progress = 0.0f;
	}

	public override void StartAnimation ()
	{
		if(CachedGameObject.activeInHierarchy)
		{
			speed = 1.0f/duration;
			diff = destinyPosition - originPosition;
			isAnimationFinished = false;
			isPlayingBackwards = false;
			if(setStartingPositionAtStart)
			{
				CachedTransform.position = originPosition;
			}
			StartCoroutine("Move");
		}
	}

	public override void StopAnimation ()
	{
		isAnimationFinished = true;
		if(setTargetPositionAtStop)
		{
			CachedTransform.position = destinyPosition;
		}
		StopCoroutine("Move");
		StopCoroutine("MustWaitOtherAnimatedControllers");
	}

	IEnumerator Move()
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

					Vector3 newPosition = originPosition + diff*curve.Evaluate(progress);
					CachedTransform.position = newPosition;

					if(progress >= 1.0f)
					{
						CachedTransform.position = destinyPosition;
						progress = 0.0f;
					}
				}
				break;
			case ELEMENT_ANIMATION_TYPE.PINGPONG:
				{
					if(isPlayingBackwards)
					{
						progress -= Time.deltaTime*speed;
						Vector3 newPosition = originPosition + diff*curve.Evaluate(progress);
						CachedTransform.position = newPosition;

						if(progress <= 0.0f)
						{
							CachedTransform.position = originPosition;
							progress = 0.0f;
							isPlayingBackwards = false;
						}
					}
					else
					{
						progress += Time.deltaTime*speed;
						Vector3 newPosition = originPosition + diff*curve.Evaluate(progress);
						CachedTransform.position = newPosition;

						if(progress >= 1.0f)
						{
							CachedTransform.position = destinyPosition;
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
					Vector3 newPosition = originPosition + diff*curve.Evaluate(progress);
					CachedTransform.position = newPosition;

					if(progress <= 0.0f)
					{
						CachedTransform.position = originPosition;
						progress = 0.0f;
						isPlayingBackwards = false;
						isAnimationFinished = true;
					}
				}
				else
				{
					progress += Time.deltaTime*speed;
					Vector3 newPosition = originPosition + diff*curve.Evaluate(progress);
					CachedTransform.position = newPosition;

					if(progress >= 1.0f)
					{
						CachedTransform.position = destinyPosition;
						progress = 1.0f;
						isPlayingBackwards = true;
					}
				}
				break;
			case ELEMENT_ANIMATION_TYPE.SINGLE:
				{
					progress += Time.deltaTime*speed;

					Vector3 newPosition = originPosition + diff*curve.Evaluate(progress);
					CachedTransform.position = newPosition;

					if(progress >= 1.0f)
					{
						CachedTransform.position = destinyPosition;
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
