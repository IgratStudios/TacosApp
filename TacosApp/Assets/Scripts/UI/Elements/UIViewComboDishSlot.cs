using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIViewComboDishSlot : PoolableObject 
{
	
	public Text label;
	public override void OnSpawn (bool isFirstTime)
	{
        CachedGameObject.ForceActivateRecursively(true);
    }

	public override void OnDespawn ()
	{
        CachedGameObject.ForceActivateRecursively(false);
    }

    public void SetDishName(string dishName)
    {
        label.text = dishName;
    }
}
