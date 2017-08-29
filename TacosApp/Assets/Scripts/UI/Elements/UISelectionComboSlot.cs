using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectionComboSlot : PoolableObject 
{
	public string viewComboDishSlotId = "ViewComboDishSlot";

    public delegate void Selected(ComboData cData);
    public Selected OnComboSelected;

    public Text comboName;
	public Text comboDescription;
	public Text comboPrice;
	public Transform dishesHolder;
	public List<UIViewComboDishSlot> viewComboDishSlots = new List<UIViewComboDishSlot>();
    private ComboData currentComboData;


    public override void OnSpawn(bool isFirstTime)
    {
        CachedGameObject.ForceActivateRecursively(true);
    }

    public override void OnDespawn()
    {
        currentComboData = new ComboData();
        ClearDishes();
        CachedGameObject.ForceActivateRecursively(false);
    }

    public void SetComboData(ComboData cData)
    {
        currentComboData = cData;
        comboName.text = currentComboData.name;
        comboDescription.text = currentComboData.GetDescription();
        comboPrice.text = currentComboData.PriceToString();

        for (int i = 0; i < currentComboData.dishesIds.Length; i++)
        {
            DishData dish = MenuDataManager.GetMenuDataManager().GetDishById(currentComboData.dishesIds[i]);
            if (dish.version != -1)
            {
                UIViewComboDishSlot slot = ObjectManager.SpawnLike<UIViewComboDishSlot>(viewComboDishSlotId);
                if (slot != null)
                {
                    slot.CachedTransform.SetParent(dishesHolder);
                    slot.CachedTransform.localScale = Vector3.one;
                    slot.CachedTransform.SetLocalPositionZ(0);
                    slot.SetDishName(dish.name);

                    viewComboDishSlots.Add(slot);
                }
            }
        }
    }

    private void ClearDishes()
    {
        while (viewComboDishSlots.Count > 0)
        {
            viewComboDishSlots[0].Despawn();
            viewComboDishSlots.RemoveAt(0);
        }
    }

    public void OnComboSelectButtonPressed()
    {
        if (OnComboSelected != null)
        {
            OnComboSelected(currentComboData);
        }
    }
}

