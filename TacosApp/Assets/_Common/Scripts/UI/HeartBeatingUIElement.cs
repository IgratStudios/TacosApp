using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBeatingUIElement : MonoBehaviour 
{
	public enum HEARTBEAT_TYPE
	{
		SOFT,
		HARD,
		THREE_STEPPED,
		THREE_STEPPED_WITH_FADE
	}

	private bool mustAnimate = true;
	public RectTransform heart;
	public HEARTBEAT_TYPE heartBeatType = HEARTBEAT_TYPE.SOFT;
	public float beatsPerSecond = 1;
	public Vector3 beatStartScale = Vector3.one;
	public Vector3 beatFinalScale = Vector3.one;
	public float hardHeartBeatSteps = 3;
	public float waitBetweenHardHeartBeatSteps = 0.1f;
	private float hardSpeed = 1.0f;
	public float waitAtBeginningOf3Stepped = 0.5f;
	public float waitAtMiddleOf3Stepped = 0.25f;
	private uint threeStepStage = 0;
	public float fadeDuration = 0.5f;
	private float fadeSpeed = 1.0f;

	private bool isBeatingUp = true;
	private float beatingPercent = 0.0f;

	void OnEnable()
	{
		ResetHeartBeat();
		StartCoroutine("DoHeartBeat");
	}

	void OnDisable()
	{
		StopCoroutine("DoHeartBeat");
		ResetHeartBeat();
	}

	private void ResetHeartBeat()
	{
		heart.localScale = beatStartScale;
		hardSpeed = 1/hardHeartBeatSteps;
		fadeSpeed = 1/fadeDuration;
		beatingPercent = 0.0f;
		isBeatingUp = true;
		threeStepStage = 0;
	}

	public void SwitchAnimation(bool enable)
	{
		mustAnimate = enable;
	}

	IEnumerator DoHeartBeat()
	{
		while(true)
		{
			if(mustAnimate)
			{
				switch(heartBeatType)
				{
				case HEARTBEAT_TYPE.SOFT:
					{
						beatingPercent += Time.deltaTime*beatsPerSecond*2;
						Vector3 newScale = heart.localScale;
						if(isBeatingUp)
						{
							newScale = Vector3.Lerp(beatStartScale, beatFinalScale, beatingPercent);
						}
						else
						{
							newScale = Vector3.Lerp(beatFinalScale, beatStartScale, beatingPercent);
						}

						heart.localScale = newScale;
						if(beatingPercent >= 1.0f)
						{
							isBeatingUp = !isBeatingUp;
							beatingPercent = 0.0f;
						}
						yield return 0;
					}
					break;
				case HEARTBEAT_TYPE.HARD:
					{
						beatingPercent += hardSpeed;
						Vector3 newScale = heart.localScale;
						if(isBeatingUp)
						{
							newScale = Vector3.Lerp(beatStartScale, beatFinalScale, beatingPercent);
						}
						else
						{
							newScale = Vector3.Lerp(beatFinalScale, beatStartScale, beatingPercent);
						}

						heart.localScale = newScale;
						if(beatingPercent >= 1.0f)
						{
							isBeatingUp = !isBeatingUp;
							beatingPercent = 0.0f;
						}
						yield return new WaitForSeconds(waitBetweenHardHeartBeatSteps);
					}
					break;
				case HEARTBEAT_TYPE.THREE_STEPPED:
					{
						if(threeStepStage == 0)//is at beginning
						{
							yield return new WaitForSeconds(waitAtBeginningOf3Stepped);
							threeStepStage++;
							beatingPercent = 0.5f;
							Vector3 newScale = heart.localScale;
							newScale = Vector3.Lerp(beatStartScale, beatFinalScale, beatingPercent);
							heart.localScale = newScale;
						}
						if(threeStepStage == 1)//is at middle
						{
							yield return new WaitForSeconds(waitAtMiddleOf3Stepped);
							threeStepStage++;
							beatingPercent = 1.0f;
							Vector3 newScale = heart.localScale;
							newScale = Vector3.Lerp(beatStartScale, beatFinalScale, beatingPercent);
							heart.localScale = newScale;
						}
						else //is at end
						{
							yield return new WaitForSeconds(waitAtMiddleOf3Stepped);
							threeStepStage = 0;
							beatingPercent = 0.0f;
							Vector3 newScale = heart.localScale;
							newScale = Vector3.Lerp(beatStartScale, beatFinalScale, beatingPercent);
							heart.localScale = newScale;
						}
					}
					break;
				case HEARTBEAT_TYPE.THREE_STEPPED_WITH_FADE:
					{
						if(threeStepStage == 0)//is at beginning
						{
							yield return new WaitForSeconds(waitAtBeginningOf3Stepped);
							threeStepStage++;
							heart.localScale = Vector3.Lerp(beatStartScale, beatFinalScale, 0.5f);
						}
						if(threeStepStage == 1)//is at middle
						{
							yield return new WaitForSeconds(waitAtMiddleOf3Stepped);
							threeStepStage++;
							heart.localScale = beatFinalScale;
							beatingPercent = 0.0f;
						}
						else //is at end, do fade
						{
							beatingPercent += Time.deltaTime*fadeSpeed;
							Vector3 newScale = Vector3.Lerp(beatFinalScale, beatStartScale, beatingPercent);
							heart.localScale = newScale;
							if(beatingPercent >= 1.0f)
							{
								beatingPercent = 0.0f;
								threeStepStage = 0;
							}
							yield return 0;
						}
					}
					break;
				}
			}
			yield return 0;
		}
	}
}
