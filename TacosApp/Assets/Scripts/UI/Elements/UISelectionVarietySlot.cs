using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectionVarietySlot : PoolableObject 
{
	public Toggle toggle;
	public Text label;
	public Text price;
	public VarietyData varietyData;

	public delegate void SelectionChanged(VarietyData vData,bool isSelected);
	

	public SelectionChanged OnVarietySelectionChanged;
	


	public override void OnSpawn (bool isFirstTime)
	{
		CachedGameObject.ForceActivateRecursively(true);
		toggle.isOn = false;
	}

	public override void OnDespawn ()
	{
        varietyData = new VarietyData();
		label.text = string.Empty;
		price.text = string.Empty;
		toggle.isOn = false;
		CachedGameObject.ForceActivateRecursively(false);
	}

	public void SetVarietyData(VarietyData data)
	{
		varietyData = data;
		label.text = varietyData.name;
		price.text = varietyData.PriceToString();
	}

	public void ForceSelection()
	{
		toggle.isOn = true;
	}

	public void OnToggleChanged(bool newValue)
	{
		if(varietyData.version != -1)
        {
            if (OnVarietySelectionChanged != null)
            {
                OnVarietySelectionChanged(varietyData,toggle.isOn);
            }
        }
	}

}

