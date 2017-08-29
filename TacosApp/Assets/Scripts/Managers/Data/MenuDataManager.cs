using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Data;

public class MenuDataManager : LocalDataManager<MenuData>
{
	public delegate void MenuUpdated();
	public static MenuUpdated OnMenuUpdated;

	public static MenuDataManager GetMenuDataManager()
	{
		return GetCastedInstance<MenuDataManager>();
	}

	public string[] defaultCategories;
	public VarietyData[] defaultVarieties;

	public NetworkMenu currentNetworkMenu;

   

	protected override void fillDefaultData ()
	{
		currentData.nextCategoryId = ulong.MinValue;
		currentData.nextVarietyId = ulong.MinValue;
		currentData.nextDishId = ulong.MinValue;
		currentData.nextComboId = ulong.MinValue;
        currentData.allCategories = new List<CategoryData>();
       
		for(int i = 0; i < defaultCategories.Length; i++)
		{
			currentData.CreateNewCategory(defaultCategories[i],true);
		}
        currentData.allVarieties = new List<VarietyData>();
		for(int i = 0; i < defaultVarieties.Length; i++)
		{
			currentData.CreateNewVariety(defaultVarieties[i].name,defaultVarieties[i].price,true);
		}
        currentData.allDishes = new List<DishData>();
        currentData.allCombos = new List<ComboData>();
        currentData.isDirty = false;
	}
		
	public MenuData GetCurrentMenuData()
	{
		return currentData;
	}

	public void AddRelatedNetworkMenuOrSyncIfNeeded(NetworkMenu netMenu)
	{
        Debug.LogWarning("A new client joined["+netMenu.userName+"] and version["+ netMenu .version+ "] I am ["+ ConnectionManager.GetInstance().GetLocalDataSyncerUniqueId() + "]");
        Debug.LogWarning("Do I have a current NetMenu?["+(currentNetworkMenu != null)+"]");
		if(currentNetworkMenu == null)
		{
            if (ConnectionManager.GetInstance().GetLocalDataSyncerUniqueId().Equals(netMenu.userName))
            {
                Debug.LogWarning("Assigning this net menu to this client");
                currentNetworkMenu = netMenu;
            }
		}
		else
		{
            Debug.LogWarning("My net menu version is["+currentNetworkMenu.version+"] other netMenuVersion is["+netMenu.version+"]");
            if (currentNetworkMenu.version < netMenu.version)
            {
                currentNetworkMenu.compareAndUpdate(netMenu);
            }
        }
	}

	public void UpdateNetworkMenu()
	{
		if(currentNetworkMenu != null)
		{
			currentNetworkMenu.compareAndUpdate(currentData);			
		}
	}

	public CategoryData GetCategoryById(string categoryId)
	{
		if (currentData != null) 
		{
			//create a copy
			CategoryData findedCategory = currentData.GetCategoryById (categoryId);
			if(findedCategory.version != -1)
			{
				CategoryData clonedData = new CategoryData();
				clonedData.Copy(findedCategory);
				return clonedData;
			}
		}
		return new CategoryData();
	}

    public bool ExistCategoryWithName(string categoryName)
    {
        if (currentData != null)
        {
            return currentData.ExistCategoryWithName(categoryName);
        }
        return false;
    }

    public string GetCategoryNameById(string categoryId)
	{
		if (currentData != null) 
		{
			//create a copy
			CategoryData findedCategory = currentData.GetCategoryById (categoryId);
			if(findedCategory.version != -1)
			{
				return findedCategory.name;
			}
		}
		return GetDefaultCategoryOption();
	}

    public string GetCategoryIdByName(string categoryName)
    {
        if (currentData != null)
        {
            //create a copy
            CategoryData findedCategory = currentData.GetCategoryIdByName(categoryName);
            if (findedCategory.version != -1)
            {
                return findedCategory.id;
            }
        }
        return string.Empty;
    }

    public VarietyData GetVarietyById(string varietyId)
	{
		if (currentData != null) 
		{
			//create a copy
			VarietyData findedVariety = currentData.GetVarietyById (varietyId);
			if(findedVariety.version != -1)
			{
				VarietyData clonedData = new VarietyData();
				clonedData.Copy(findedVariety);
				return clonedData;
			}
		}
		return new VarietyData();
	}

    public bool ExistVarietyWithName(string varietyName)
    {
        if (currentData != null)
        {
            return currentData.ExistVarietyWithName(varietyName);
        }
        return false;
    }

    public DishData GetDishById(string dishId)
	{
		if (currentData != null) 
		{
			//create a copy
			DishData findedDish = currentData.GetDishById (dishId);
			if(findedDish.version != -1)
            {
				DishData clonedData = new DishData();
				clonedData.Copy(findedDish);
				return clonedData;
			}
		}
		return new DishData();
	}

	public ComboData GetComboById(string comboId)
	{
		if (currentData != null) 
		{
			//create a copy
			ComboData findedCombo = currentData.GetComboById (comboId);
			if(findedCombo.version != -1)
            {
				ComboData clonedData = new ComboData();
				clonedData.Copy(findedCombo);
				return clonedData;
			}
		}
		return new ComboData();
	}
		
	public List<CategoryData> GetAllCategories()
	{
		if(currentData != null)
		{
			return currentData.allCategories;
		}
		return null;
	}

	public List<string> GetAllCategoriesAsOptions()
	{
		List<string> allCategoryOptions = new List<string>();

		if(currentData != null)
		{
			return currentData.GetAllCategoriesAsOptions();

		}
		return allCategoryOptions;
	}

	public string GetDefaultCategoryOption()
	{
		if(currentData != null)
		{
			return currentData.defaultCategoryOption;
		}
		return string.Empty;
	}

	public List<VarietyData> GetAllVarieties()
	{
		if(currentData != null)
		{
			return currentData.allVarieties;
		}
		return null;
	}

	public List<DishData> GetAllDishes()
	{
		if(currentData != null)
		{
			return currentData.allDishes;
		}
		return null;
	}

	public List<ComboData> GetAllCombos()
	{
		if(currentData != null)
		{
			return currentData.allCombos;
		}
		return null;
	}
		
	public bool NeedSyncronization()
	{
		if(currentData != null)
		{
			return currentData.NeedSyncronization();
		}
		return false;
	}

	public void SyncMenuDataFromRemoteVersion()
	{
		if(currentNetworkMenu != null && currentData != null)
		{
			if(currentData.CompareAndUpdateFromNetwork(currentNetworkMenu))
			{
				if(OnMenuUpdated != null)
				{
					OnMenuUpdated();
				}
			}
		}
	}

	public void CheckAndSyncIfNeeded()
	{
		if(NeedSyncronization())
		{
			if(currentNetworkMenu != null && currentData != null)
			{
				if(currentNetworkMenu.compareAndUpdate(currentData))
				{
                    Debug.Log("Setting current data as not dirty.");
                    currentData.SetAsNotDirty();
					currentNetworkMenu.ForceNetworkUpdate();
                    saveLocalData();
				}
			}
		}
	}

	public CategoryData CreateNewCategory(string categoryName, bool syncIfNeeded = true)
	{
		if(currentData != null)
		{
			CategoryData newCategory = currentData.CreateNewCategory(categoryName);
			if(newCategory.version != -1)
			{
                if (syncIfNeeded)
                {
                    CheckAndSyncIfNeeded();
                }
				return newCategory;
			}
		}
		return new CategoryData();
	}

	public VarietyData CreateNewVariety(string varietyName, float price, bool syncIfNeeded = true)
	{
		if(currentData != null)
		{
			VarietyData newVariety = currentData.CreateNewVariety(varietyName,price);
			if(newVariety.version != -1)
            {
                if (syncIfNeeded)
                {
                    CheckAndSyncIfNeeded();
                }
                return newVariety;
			}
		}
		return new VarietyData();
	}

    public void CreateOrUpdateDish(DishData dData)
    {
        if (currentData != null)
        {
            //search if a dish with this id already exist
            if (string.IsNullOrEmpty(dData.id))//then it is a new dish
            {
                CreateNewDish(dData.name, dData.startingPrice, dData.description, dData.categoryId, dData.varietyIds);
            }
            else//is modifying an existing dish
            {
                DishData original = currentData.GetDishById(dData.id);
                if (original.version != -1)
                {
                    original.compareAndUpdate(dData);
                    CheckAndSyncIfNeeded();
                }
            }
        }
    }

	public DishData CreateNewDish(string dishName, float price, string description, string categoryId, string[] varietiesIds)
	{
		if(currentData != null)
		{
			DishData newDish = currentData.CreateNewDish(dishName,price,description,categoryId,varietiesIds);
			if(newDish.version != -1)
            {
				CheckAndSyncIfNeeded();
				return newDish;
			}
		}
		return new DishData();
	}

    public void CreateOrUpdateCombo(ComboData cData)
    {
        if (currentData != null)
        {
            //search if a dish with this id already exist
            if (string.IsNullOrEmpty(cData.id))//then it is a new dish
            {
                CreateNewCombo(cData.name,cData.price,cData.specificDescription,cData.dishesIds);
            }
            else//is modifying an existing dish
            {
                ComboData original = currentData.GetComboById(cData.id);
                if (original.version != -1)
                {
                    original.compareAndUpdate(cData);
                    CheckAndSyncIfNeeded();
                }
            }
        }
    }

    public ComboData CreateNewCombo(string comboName, float price, string specificDescription, string[] dishesIds)
	{
		if(currentData != null)
		{
			ComboData newCombo = currentData.CreateNewCombo(comboName, price, specificDescription, dishesIds);
			if(newCombo.version != -1)
            {
				CheckAndSyncIfNeeded();
				return newCombo;
			}
		}
		return new ComboData();
	}

}