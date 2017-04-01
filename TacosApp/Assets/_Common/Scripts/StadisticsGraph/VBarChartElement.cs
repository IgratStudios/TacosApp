using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VBarChartElement : PoolableObject 
{
	public Color leftColor;
	public Color rightColor;

	public float leftSamples;
	public float rightSamples;

	public float leftDataPercent;//value between 0-1
	public float rightDataPercent;//value between 0-1

	uint minRange;
	uint maxRange;
	public bool addLesserThanSymbol = false;
	public bool addBiggerThanSymbol = false;

	public Image leftBar;
	public Image rightBar;

	public Text dataLabel;

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
		Color leftColor,
		Color rightColor,
		float samplesLeft,
		float samplesRight,
		float totalSamples,
		uint rangeInit,
		uint rangeEnd,
		bool addLesserThanSymbol,
		bool addBiggerThanSymbol)
	{
		this.leftColor = leftColor;
		leftBar.color = this.leftColor;
		this.rightColor = rightColor;
		rightBar.color = this.rightColor;

		minRange = rangeInit;
		maxRange = rangeEnd;

		this.addLesserThanSymbol = addLesserThanSymbol;
		this.addBiggerThanSymbol = addBiggerThanSymbol;

		SetSamples(samplesLeft,samplesRight,totalSamples);
		UpdateRangeLabel();
	}

	public void SetSamples(float leftSamples,float rightSamples,float totalSamples)
	{
		this.leftSamples = leftSamples;
		if(totalSamples > 0)
		{
			leftDataPercent = leftSamples/totalSamples;
		}
		else
		{
			leftDataPercent = 0.0f;
		}

		this.rightSamples = rightSamples;
		if(totalSamples > 0)
		{
			rightDataPercent = rightSamples/totalSamples;
		}
		else
		{
			rightDataPercent = 0.0f;
		}

		UpdateBars();
	}

	public void UpdateBars()
	{
		leftBar.fillAmount = leftDataPercent;
		rightBar.fillAmount = rightDataPercent;
	}

	public void UpdateRangeLabel()
	{
		if(addLesserThanSymbol)
		{
			dataLabel.text = "<"+maxRange;
		}
		else if(addBiggerThanSymbol)
		{
			dataLabel.text = ">"+minRange;
		}
		else
		{
			dataLabel.text = minRange+" - "+maxRange;
		}
	}

	public void Reset()
	{
		leftColor = Color.white;
		leftBar.color = Color.white;
		rightColor = Color.white;
		rightBar.color = Color.white;

		minRange = 0;
		maxRange = 1;

		leftSamples = 0.0f;
		leftDataPercent = 0.0f;

		rightSamples = 0.0f;
		rightDataPercent = 0.0f;

		addLesserThanSymbol = false;
		addBiggerThanSymbol = false;
	}

	public void Despawn()
	{
		ObjectManager.Despawn(this);
	}
}
