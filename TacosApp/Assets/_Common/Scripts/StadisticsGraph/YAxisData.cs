using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YAxisData : PoolableObject 
{
	public bool addPlusSymbol = false;
	public uint dataValue;
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

	public void InitWithData(uint dataValue,bool addPlusSymbol = false)
	{
		this.addPlusSymbol = addPlusSymbol;
		this.dataValue = dataValue;
		UpdateLabel();
	}

	public void Reset()
	{
		dataValue = 0;
		addPlusSymbol = false;
	}

	public void UpdateLabel()
	{
		if(addPlusSymbol)
		{
			dataLabel.text = dataValue+"+";
		}
		else
		{
			dataLabel.text = dataValue.ToString();
		}
	}
}
