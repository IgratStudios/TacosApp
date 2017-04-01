using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimatedElementController : CachedMonoBehaviour 
{
	public enum ELEMENT_ANIMATION_TYPE
	{
		SINGLE,
		LOOP,
		PINGPONG,
		PINGPONGONCE
	}

	public ELEMENT_ANIMATION_TYPE animationType = ELEMENT_ANIMATION_TYPE.SINGLE;
	private bool isAnimating = false;
	protected float progress = 0.0f;
	public abstract void OnEnable();
	public abstract void OnDisable();
	public abstract void StartAnimation();
	public abstract void StopAnimation();
	public abstract void ResetAnimation();

	public RelatedAC[] relatedAnimatedControllers;

	public bool IsAnimating
	{
		get
		{
			return isAnimating;
		}
	}

	public void SwitchAnimation(bool enable)
	{
		isAnimating = enable;
		if(enable)
		{
			ResetAnimation();
			StartAnimation();
		}
		else
		{
			StopAnimation();
			ResetAnimation();
		}
	}

	public float GetAnimationProgress()
	{
		return progress;
	}

	protected IEnumerator MustWaitOtherAnimatedControllers()
	{
		bool mustWait = MustWaitOtherACs();
		while(mustWait)
		{
			yield return 0;
			mustWait = MustWaitOtherACs();
		}
	}

	private bool MustWaitOtherACs()
	{
		bool result = false;
		for(int i = 0; i < relatedAnimatedControllers.Length ; i++)
		{
			result |= !relatedAnimatedControllers[i].IsRelatedControllerReady();
		}
		return result;
	}

}

[System.Serializable]
public class RelatedAC
{
	public AnimatedElementController relatedController;
	public float advanceNeeded = 0.0f;

	public bool IsRelatedControllerReady()
	{
		if(relatedController != null)
		{
			return relatedController.GetAnimationProgress() >= advanceNeeded;		
		}
		return true;
	}
}
