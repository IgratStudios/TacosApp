using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectionComboDishSlot : PoolableObject 
{
	public Toggle toggle;
	public Text label;
    public DishData dishData;

    public delegate void SelectionChanged(DishData dData,bool isSelected);


    public SelectionChanged OnDishSelectionChanged;


    public override void OnSpawn(bool isFirstTime)
    {
        CachedGameObject.ForceActivateRecursively(true);
        toggle.isOn = false;
    }

    public override void OnDespawn()
    {
        dishData = new DishData();
        label.text = string.Empty;
        toggle.isOn = false;
        CachedGameObject.ForceActivateRecursively(false);
    }

    public void SetDishData(DishData data)
    {
        dishData = data;
        label.text = dishData.name;
    }

    public void ForceSelection()
    {
        toggle.isOn = true;
    }

    public void OnToggleChanged(bool newValue)
    {
        if (dishData.version != -1)
        {
            if (OnDishSelectionChanged != null)
            {
                OnDishSelectionChanged(dishData,toggle.isOn);
            }
        }
    }


}
