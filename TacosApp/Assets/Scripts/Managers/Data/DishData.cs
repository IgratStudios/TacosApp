using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Data;
using System;

[System.Serializable]
public struct DishData 
{
    public string id;
    public int version;
    public bool isDirty;
    public string name;
	public string description;
	public float startingPrice;
	public string categoryId;
	public string[] varietyIds;


    public DishData(int defaultVersion = -1)
    {
        id = string.Empty;
        version = defaultVersion;
        isDirty = true;
        name = string.Empty;
        startingPrice = 0;
        categoryId = string.Empty;
        description = string.Empty;
        varietyIds = new string[0];
    }

    public void updateFrom (DishData readOnlyRemote, bool ignoreVersion = false)
	{
        //Si se ignora la version entonces no la cambiamos
        if (ignoreVersion)
        {
            version = readOnlyRemote.version;
        }
        Copy(readOnlyRemote);
		isDirty = false;
	}

	public void Copy(DishData other)
	{
        this.id = other.id;
		this.version = other.version;
		this.name = other.name;
		this.description = other.description;
		this.startingPrice = other.startingPrice;
		this.categoryId = other.categoryId;
		this.varietyIds = other.varietyIds;
	}

    public bool compareAndUpdate(DishData remote, bool ignoreVersion = false)
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
		if (startingPrice > 0) 
		{
			return "+" + startingPrice.ToString ("F2");
		} 
		else if (startingPrice < 0) 
		{
			return "-" + Mathf.Abs (startingPrice).ToString ("F2");
		} 
		else 
		{
			return "0.00";
		}
	}

	public bool HasVariety(string varietyId)
	{
		for(int i = 0; i < varietyIds.Length; i++)
		{
			if(varietyIds[i] == varietyId)
			{
				return true;
			}
		}
		return false;
	}

	public void AddVariety(string varietyId)
	{
		if(!HasVariety(varietyId))
		{
            Array.Resize<string>(ref varietyIds, varietyIds.Length + 1);

            varietyIds[varietyIds.Length - 1] = varietyId;

			version++;
			isDirty = true;
		}
	}

	public void RemoveVariety(string varietyId)
	{
		for(int i = 0; i < varietyIds.Length; i++)
		{
			if(varietyIds[i] == varietyId)
			{
                Array.Resize<string>(ref varietyIds, varietyIds.Length - 1);
                version++;
				isDirty = true;
				break;
			}
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
		description = newDescription;
		version++;
		isDirty = true;
	}

	public void SetNewStartingPrice(float newPrice)
	{
		startingPrice = newPrice;
		version++;
		isDirty = true;
	}

	public void SetNewCategory(string newCategoryId)
	{
		categoryId = newCategoryId;
		version++;
		isDirty = true;
	}

	public void SetNewVarieties(string[] newVarietiesIds)
	{
		varietyIds = newVarietiesIds;
		version++;
		isDirty = true;
	}

    public bool IsDataValid()
    {
        bool isPriceValid = startingPrice > 0.0f;
        bool isNameValid = !string.IsNullOrEmpty(name);
        bool isDescriptionValid = !string.IsNullOrEmpty(description);
        bool isCategoryValid = !string.IsNullOrEmpty(categoryId);

        return isPriceValid && isNameValid && isDescriptionValid && isCategoryValid;

    }

    public void SetAsNotDirty()
    {
        isDirty = false;
    }
}
