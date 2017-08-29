using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CHART_DATA_TYPE
{
	PIE_CHART,
	BAR_CHART,
	ONLY_LABELS
}

public class ChartData : PoolableObject 
{
	
	public CHART_DATA_TYPE dataType;
	public string statName;
	public Color dataColor;
	public float dataPercent;//value between 0-1
	public float samples;
	private float accumulatedData;
	public float offset = 4;

	public Image graphicElement;
	public ChartInfoLabel label;

	public override void OnSpawn (bool isFirstTime)
	{
		CachedGameObject.SetActive(true);
		Reset();
	}

	public override void OnDespawn ()
	{
		CachedGameObject.SetActive(false);
	}

	public void InitWithData(CHART_DATA_TYPE dataType, 
		string statName, 
		Color color,
		float samples,
		float totalSamples,
		ChartInfoLabel label)
	{
		this.dataType = dataType;
		this.statName = statName;
		dataColor = color;
		graphicElement.color = dataColor;
		this.label = label;
		SetSamples(samples,totalSamples);

	}

	public void UpdateGraph(float accumulatedChartData)
	{
		accumulatedData = accumulatedChartData;
		UpdateGraphicElement();
		if(label != null)
		{
			switch(dataType)
			{
			case CHART_DATA_TYPE.PIE_CHART:
				label.InitWithData(statName,dataColor,GetDataLabel());
				break;
			case CHART_DATA_TYPE.BAR_CHART:
				label.InitWithData(statName,dataColor,GetDataLabel());
				break;
			case CHART_DATA_TYPE.ONLY_LABELS:
				label.InitWithData(statName,dataColor,GetDataLabel(),GetSamples(true));
				break;
			}

		}

	}

	//Dhin tadaak dhin tadaak aa jaa ud ke saraat Pairon se bedi zaraa khol

	public void Reset()
	{
		this.statName = string.Empty;
		dataColor = Color.white;
		accumulatedData = 0;
		samples = 0.0f;
		dataPercent = 0.0f;
	}

	public string GetPercent()
	{
		return string.Format("{0:###.##}%",(dataPercent*100.0f));
	}

	public string GetSamples(bool addFormatDots)
	{
		if(addFormatDots)
		{
			return string.Format(" ....... {0}",samples);
		}
		else
		{
			return string.Format("{0}",samples);
		}
	}

	public void SetSamples(float samples,float totalSamples)
	{
		this.samples = samples;
		if(totalSamples > 0)
		{
			dataPercent = samples/totalSamples;
		}
		else
		{
			dataPercent = 0.0f;
		}
	}

	public void UpdateGraphicElement()
	{
		switch(dataType)
		{
		case CHART_DATA_TYPE.PIE_CHART:
			{
				graphicElement.fillOrigin = 1;
				graphicElement.fillMethod = Image.FillMethod.Radial360;
				graphicElement.fillClockwise = false;

				graphicElement.transform.SetLocalRotationZ(360*accumulatedData);

				if(dataPercent > 0.0f)
				{
					graphicElement.fillAmount = dataPercent;
					float rotationAngle = ( 360.0f*(accumulatedData + dataPercent*0.5f) )*Mathf.Deg2Rad;

					float angleX = Mathf.Cos( rotationAngle )*Mathf.Rad2Deg;
					float angleY = Mathf.Sin( rotationAngle )*Mathf.Rad2Deg;

					Vector2 dir = new Vector2(angleX,angleY);
					dir.Normalize();

					//			Debug.Log("["+name+"] " +
					//				"RotAngle["+rotationAngle+"]" +
					//				"MoveDirection["+angleX+","+angleY+"] " +
					//				"elemRotation["+graphicElement.transform.eulerAngles.z+"] " +
					//				"NormalizedDir["+dir+"]");

					dir *= offset;

					graphicElement.rectTransform.anchoredPosition = dir;
					graphicElement.rectTransform.SetLocalPositionXY(dir.x,dir.y);
				}
				else
				{
					graphicElement.fillAmount = 0.0f;
				}
				break;
			}
		case CHART_DATA_TYPE.BAR_CHART:
			graphicElement.fillAmount = dataPercent;
			graphicElement.fillOrigin = 0;
			graphicElement.fillMethod = Image.FillMethod.Vertical;
			break;
		case CHART_DATA_TYPE.ONLY_LABELS:
			{
				graphicElement.color = new Color(0,0,0,0);
				break;
			}
		default:break;
		}

	}

	public string GetDataLabel()
	{
		switch(dataType)
		{
		case CHART_DATA_TYPE.PIE_CHART:
			if(dataPercent > 0.0f)
			{
				return string.Format("{0} {1:######} - {2:###.##}%",statName,samples,dataPercent*100.0f);
			}
			else
			{
				return string.Format("{0} {1:######} - 0%",statName,samples);
			}

		case CHART_DATA_TYPE.ONLY_LABELS:
			return string.Format("{0}",statName);
		case CHART_DATA_TYPE.BAR_CHART:
			return string.Format("{0}",statName);
			default:
			if(dataPercent > 0.0f)
			{
				return string.Format("{0} {1:######} - {2:###.##}%",statName,samples,dataPercent*100.0f);
			}
			else
			{
				return string.Format("{0} {1:######} - 0%",statName,samples);
			}
		}
	}
		
}
