using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChartInfoLabel : PoolableObject 
{
	public string id;
	public Color dataColor;

	public Image graphicElement;
	public Text label;
	public Text labelAux;

	public override void OnSpawn (bool isFirstTime)
	{
		CachedGameObject.SetActive(true);
		Reset();
	}

	public override void OnDespawn ()
	{
		CachedGameObject.SetActive(false);
	}

	public void Reset()
	{
		this.id = string.Empty;
		dataColor = Color.white;
		label.text = string.Empty;
	}

	public void InitWithData(string id, Color color, string labelText)
	{
		this.id = id;
		dataColor = color;
		label.text = labelText;
		graphicElement.color = dataColor;
	}

	public void InitWithData(string id, Color color, string labelText,string labelTextAux)
	{
		this.id = id;
		dataColor = color;
		label.text = labelText;
		labelAux.text = labelTextAux;
		graphicElement.color = dataColor;
	}
		
}
