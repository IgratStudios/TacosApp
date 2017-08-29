using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectionDishSlot : PoolableObject 
{
    public delegate void Select(DishData dData);
    public Select OnDishSelected;

	public Text dishName;
	public Text dishCategory;
	public Text dishDescription;
	public Text dishPrice;
	public Text dishVarietiesCounter;
	private DishData currentDishData;

	public override void OnSpawn (bool isFirstTime)
	{
        currentDishData = new DishData();
		CachedGameObject.ForceActivateRecursively(true);
	}

	public override void OnDespawn ()
	{
        currentDishData = new DishData();
		CachedGameObject.ForceActivateRecursively(false);
	}

	public void SetDishData(DishData dData)
	{
		currentDishData.Copy(dData);
		dishName.text = currentDishData.name;
		dishDescription.text = currentDishData.description;
		dishCategory.text = MenuDataManager.GetMenuDataManager().GetCategoryNameById(currentDishData.categoryId);
		dishPrice.text = currentDishData.PriceToString();
		dishVarietiesCounter.text = currentDishData.varietyIds.Length.ToString();
	}

    public void OnDishSelectButtonPressed()
    {
        if (OnDishSelected != null)
        {
            OnDishSelected(currentDishData);
        }
    }
}

