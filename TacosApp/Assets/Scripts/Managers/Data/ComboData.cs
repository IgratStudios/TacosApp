using UnityEngine;
using Data;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;


[System.Serializable]
public struct ComboData 
{
    public string id;
    public int version;
    public bool isDirty;
    public string name;
	public float price;
	public string specificDescription;
	public string[] dishesIds;

    public ComboData(int defaultVersion = -1)
    {
        id = string.Empty;
        version = defaultVersion;
        isDirty = true;
        name = string.Empty;
        price = 0;
        specificDescription = string.Empty;
        dishesIds = new string[0];
    }

    public void updateFrom(ComboData readOnlyRemote, bool ignoreVersion = false)
    {
        //Si se ignora la version entonces no la cambiamos
        if (ignoreVersion)
        {
            version = readOnlyRemote.version;
        }
        Copy(readOnlyRemote);
        isDirty = false;
    }

    public void Copy(ComboData other)
	{
        this.id = other.id;
        this.version = other.version;
        this.name = other.name;
		this.specificDescription = other.specificDescription;
		this.price = other.price;
		this.dishesIds = other.dishesIds;
	}

    public bool compareAndUpdate(ComboData remote, bool ignoreVersion = false)
    {
        if (remote.version > version || ignoreVersion)
        {
            updateFrom(remote, ignoreVersion);
            return true;
        }

        return false;
    }


    public void AfterFirstRead()
    {

    }

    public string PriceToString()
	{
		if (price > 0) 
		{
			return "+" + price.ToString ("F2");
		} 
		else if (price < 0) 
		{
			return "-" + Mathf.Abs (price).ToString ("F2");
		} 
		else 
		{
			return "0.00";
		}
	}

	public string GetDescription()
	{
		if (string.IsNullOrEmpty (specificDescription)) 
		{
			string description = "";
			for (int i = 0; i < dishesIds.Length; i++) 
			{
				DishData dish = MenuDataManager.GetMenuDataManager ().GetDishById (dishesIds[i]);
				if(dish.version != -1)
				{
					description += dish.description.TrimEnd (new char[] { '.' });
				}
				if (i < dishesIds.Length - 1) 
				{
					description += " + ";
				} 
				else
				{
					description += ".";
				}
			}
			return description;
		}
		else
		{
			return specificDescription;
		}
	}

	public void SetNewName(string newName)
	{
		name = newName;
		version++;
		isDirty = true;
	}

	public void SetNewDescription(string newDescription)
	{
		specificDescription = newDescription;
		version++;
		isDirty = true;
	}

	public void SetNewStartingPrice(float newPrice)
	{
		price = newPrice;
		version++;
		isDirty = true;
	}

	public void SetNewDishes(string[] newDishesIds)
	{
		dishesIds = newDishesIds;
		version++;
		isDirty = true;
	}

    public bool HasDish(string dishId)
    {
        for (int i = 0; i < dishesIds.Length; i++)
        {
            if (dishesIds[i] == dishId)
            {
                return true;
            }
        }
        return false;
    }

    public void AddDish(string dishId)
    {
        if (!HasDish(dishId))
        {
           
            Array.Resize<string>(ref dishesIds, dishesIds.Length + 1);
            dishesIds[dishesIds.Length - 1] = dishId;
            version++;
            isDirty = true;
        }
    }

    public void RemoveDish(string dishId)
    {
        for (int i = 0; i < dishesIds.Length; i++)
        {
            if (dishesIds[i] == dishId)
            {
                Array.Resize<string>(ref dishesIds, dishesIds.Length - 1);
                version++;
                isDirty = true;
                break;
            }
        }

    }


    public bool IsDataValid()
    {
        bool isPriceValid = price > 0.0f;
        bool isNameValid = !string.IsNullOrEmpty(name);
        bool isDescriptionValid = !string.IsNullOrEmpty(GetDescription());
        bool isDishesListValid = dishesIds.Length > 0;

        return isPriceValid && isNameValid && isDescriptionValid && isDishesListValid;

    }

    public void SetAsNotDirty()
    {
        isDirty = false;
    }
}