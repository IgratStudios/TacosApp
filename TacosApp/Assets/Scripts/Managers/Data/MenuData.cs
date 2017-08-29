using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine.Networking;

[System.Serializable]
public class MenuData : BasicData
{

    public ulong nextCategoryId;
   
    public List<CategoryData> allCategories;
	public ulong nextVarietyId;
	public List<VarietyData> allVarieties;
	public ulong nextDishId;
	public List<DishData> allDishes;
	public ulong nextComboId;
	public List<ComboData> allCombos;

    
	private Dictionary<string,int> categoriesIndexMap = new Dictionary<string, int>();

    private Dictionary<string,int> varietiesIndexMap = new Dictionary<string, int>();

    private Dictionary<string,int> dishesIndexMap = new Dictionary<string, int>();

    private Dictionary<string,int> combosIndexMap = new Dictionary<string, int>();

    public string defaultCategoryOption = "Sin Asignar";
	public string defaultVarietyOption = "Ninguna";


	public override void AfterFirstRead ()
	{
		CreateMaps();
	}

    public bool CompareAndUpdateFromNetwork(NetworkMenu netMenu)
    {
        UnityEngine.Debug.Log("CompareAndUpdateFromNetwork Ver["+version+"] < NetVer["+netMenu.version+"]");
        if (netMenu.version > version)
        {
            version = netMenu.version;
            nextCategoryId = netMenu.nextCategoryId;
            nextVarietyId = netMenu.nextVarietyId;
            nextDishId = netMenu.nextDishId;
            nextComboId = netMenu.nextComboId;

            if (allCategories.Count != netMenu.allCategories.Count)
            {
                allCategories.Clear();
                for (int i = 0; i < netMenu.allCategories.Count; i++)
                {
                    allCategories.Add(netMenu.allCategories[i]);
                }
            }
            else
            {
                for (int i = 0; i < allCategories.Count; i++)
                {
                    allCategories[i].compareAndUpdate(netMenu.allCategories[i]);
                }
            }

            if (allVarieties.Count != netMenu.allVarieties.Count)
            {
                allVarieties.Clear();
                for (int i = 0; i < netMenu.allVarieties.Count; i++)
                {
                    allVarieties.Add(netMenu.allVarieties[i]);
                }
            }
            else
            {
                for (int i = 0; i < allVarieties.Count; i++)
                {
                    allVarieties[i].compareAndUpdate(netMenu.allVarieties[i]);
                }
            }

            if (allDishes.Count != netMenu.allDishes.Count)
            {
                allDishes.Clear();
                for (int i = 0; i < netMenu.allDishes.Count; i++)
                {
                    allDishes.Add(netMenu.allDishes[i]);
                }
            }
            else
            {
                for (int i = 0; i < allDishes.Count; i++)
                {
                    allDishes[i].compareAndUpdate(netMenu.allDishes[i]);
                }
            }

            if (allCombos.Count != netMenu.allCombos.Count)
            {
                allCombos.Clear();
                for (int i = 0; i < netMenu.allCombos.Count; i++)
                {
                    allCombos.Add(netMenu.allCombos[i]);
                }
            }
            else
            {
                for (int i = 0; i < allCombos.Count; i++)
                {
                    allCombos[i].compareAndUpdate(netMenu.allCombos[i]);
                }
            }

            //recreate maps
            CreateMaps();

            netMenu.SetAsNotDirty();



            return true;
        }
       
        return false;
    }

    private void CreateMaps()
	{
        categoriesIndexMap.Clear();
		for(int i = 0; i < allCategories.Count; i++)
		{
			categoriesIndexMap.Add (allCategories[i].id,i);
		}

		varietiesIndexMap.Clear();
        for (int i = 0; i < allVarieties.Count; i++)
		{
			varietiesIndexMap.Add (allVarieties[i].id,i);
		}

		dishesIndexMap.Clear();
        for (int i = 0; i < allDishes.Count; i++)
		{
			dishesIndexMap.Add (allDishes[i].id,i);
		}

		combosIndexMap.Clear();
        for (int i = 0; i < allCombos.Count; i++)
		{
			combosIndexMap.Add (allCombos[i].id,i);
		}

        UnityEngine.Debug.Log("Creating MenuData Maps");
    }

	public override void updateFrom(BasicData readOnlyRemote, bool ignoreVersion = false)
	{
        UnityEngine.Debug.Log("updating from local type");

        base.updateFrom(readOnlyRemote, ignoreVersion);
		MenuData otherData = (MenuData)readOnlyRemote;
		nextCategoryId = otherData.nextCategoryId;
		nextVarietyId = otherData.nextVarietyId;
		nextDishId = otherData.nextDishId;
		nextComboId = otherData.nextComboId;

		if(allCategories.Count != otherData.allCategories.Count)
		{
			allCategories = otherData.allCategories;
		}
		else
		{
			for(int i = 0; i < allCategories.Count; i++)
			{
				allCategories[i].compareAndUpdate(otherData.allCategories[i]);
			}
		}

		if(allVarieties.Count != otherData.allVarieties.Count)
		{
			allVarieties = otherData.allVarieties;
		}
		else
		{
			for(int i = 0; i < allVarieties.Count; i++)
			{
				allVarieties[i].compareAndUpdate(otherData.allVarieties[i]);
			}
		}

		if(allDishes.Count != otherData.allDishes.Count)
		{
			allDishes = otherData.allDishes;
		}
		else
		{
			for(int i = 0; i < allDishes.Count; i++)
			{
				allDishes[i].compareAndUpdate(otherData.allDishes[i]);
			}
		}

		if(allCombos.Count != otherData.allCombos.Count)
		{
			allCombos = otherData.allCombos;
		}
		else
		{
			for(int i = 0; i < allCombos.Count; i++)
			{
				allCombos[i].compareAndUpdate(otherData.allCombos[i]);
			}
		}

		//recreate maps
		CreateMaps();
	}

	public List<string> GetAllCategoriesAsOptions()
	{
		List<string> allCategoryOptions = new List<string>();

		allCategoryOptions.Add(defaultCategoryOption);
		for(int i = 0; i < allCategories.Count; i++)
		{
			allCategoryOptions.Add(allCategories[i].name);
		}

		return allCategoryOptions;
	}

	public CategoryData GetCategoryById(string categoryId)
	{
		int dataIndex = -1;
		if (categoriesIndexMap.TryGetValue (categoryId, out dataIndex)) 
		{
			return allCategories [dataIndex];
		}
		return new CategoryData();
	}

    public CategoryData GetCategoryIdByName(string categoryName)
    {
        for (int i = 0; i < allCategories.Count; i++)
        {
            if (allCategories[i].name == categoryName)
            {
                return allCategories[i];
            }
        }
        return new CategoryData();
    }

    public bool ExistCategoryWithName(string categoryName)
    {
        for (int i = 0; i < allCategories.Count; i++)
        {
            if (allCategories[i].name == categoryName)
            {
                return true;
            }
        }
        return false;
    }

    public VarietyData GetVarietyById(string varietyId)
	{
		int dataIndex = -1;
		if (varietiesIndexMap.TryGetValue (varietyId, out dataIndex)) 
		{
			return allVarieties [dataIndex];
		}
		return new VarietyData();
	}

    public bool ExistVarietyWithName(string varietyName)
    {
        for (int i = 0; i < allVarieties.Count; i++)
        {
            if (allVarieties[i].name == varietyName)
            {
                return true;
            }
        }
        return false;
    }

    public DishData GetDishById(string itemId)
	{
		int dataIndex = -1;
		if (dishesIndexMap.TryGetValue (itemId, out dataIndex)) 
		{
			return allDishes [dataIndex];
		}
		return new DishData();
	}

	public ComboData GetComboById(string comboId)
	{
		int dataIndex = -1;
		if (combosIndexMap.TryGetValue (comboId, out dataIndex)) 
		{
			return allCombos [dataIndex];
		}
		return new ComboData();
	}

    public CategoryData CreateNewCategory(string categoryName, bool isDefaultCreation = false)
	{
		if(!ExistCategoryWithName(categoryName))
        {
            CategoryData newCategory = new CategoryData(0);
            newCategory.id = nextCategoryId.ToString();
            nextCategoryId++;
            newCategory.name = categoryName;
            newCategory.isDirty = !isDefaultCreation;
            version++;
            isDirty = true;
            allCategories.Add(newCategory);
            categoriesIndexMap.Add(newCategory.id, allCategories.Count - 1);
            return newCategory;
        }

        return new CategoryData();
	}

	public VarietyData CreateNewVariety(string varietyName, float price, bool isDefaultCreation = false)
	{
		if(!ExistVarietyWithName(varietyName))
        {
            VarietyData newVariety = new VarietyData(0);
            newVariety.id = nextVarietyId.ToString();
            nextVarietyId++;
            newVariety.price = price;
            newVariety.name = varietyName;
            newVariety.isDirty = !isDefaultCreation;
            version++;
            isDirty = true;
            allVarieties.Add(newVariety);
            varietiesIndexMap.Add(newVariety.id, allVarieties.Count - 1);
            return newVariety;
        }
        return new VarietyData();
		
	}

	public DishData CreateNewDish(string dishName, float price, string description, string categoryId, string[] varietiesIds)
	{
		//check if there is not a variety with this exact name
		for (int i = 0; i < allDishes.Count; i++) 
		{
			if (allDishes [i].name == dishName) 
			{
				return new DishData();
			} 
		}

		DishData newDish = new DishData(0);
		newDish.id = nextDishId.ToString();
		nextDishId++;
		newDish.startingPrice = price;
		newDish.name = dishName;
		newDish.description = description;
		newDish.categoryId = categoryId;
		newDish.varietyIds = varietiesIds;
		newDish.isDirty = true;
		version++;
        isDirty = true;
        allDishes.Add(newDish);
        dishesIndexMap.Add(newDish.id, allDishes.Count - 1);
        return newDish;
	}

	public ComboData CreateNewCombo(string comboName, float price, string specificDescription, string[] dishesIds)
	{
		//check if there is not a variety with this exact name
		for (int i = 0; i < allCombos.Count; i++) 
		{
			if (allCombos [i].name == comboName) 
			{
				return new ComboData();
			} 
		}

		ComboData newCombo = new ComboData(0);
		newCombo.id = nextComboId.ToString();
		nextComboId++;
		newCombo.price = price;
		newCombo.name = comboName;
		newCombo.specificDescription = specificDescription;
		newCombo.dishesIds = dishesIds;
		newCombo.isDirty = true;
		version++;
        isDirty = true;
        allCombos.Add(newCombo);
        combosIndexMap.Add(newCombo.id, allCombos.Count - 1);
        return newCombo;
	}

	public bool NeedSyncronization()
	{
		for (int i = 0; i < allCategories.Count; i++) 
		{
			if(allCategories [i].isDirty) 
			{
				return true;
			} 
		}
		for (int i = 0; i < allVarieties.Count; i++) 
		{
			if(allVarieties [i].isDirty) 
			{
				return true;
			} 
		}
		for (int i = 0; i < allDishes.Count; i++) 
		{
			if(allDishes [i].isDirty) 
			{
				return true;
			} 
		}
		for (int i = 0; i < allCombos.Count; i++) 
		{
			if(allCombos [i].isDirty) 
			{
				return true;
			} 
		}
		return false;
	}

    public void SetAsNotDirty()
    {
        isDirty = false;
        for (int i = 0; i < allCategories.Count; i++)
        {
            allCategories[i].SetAsNotDirty();
        }
        for (int i = 0; i < allVarieties.Count; i++)
        {
            allVarieties[i].SetAsNotDirty();
        }
        for (int i = 0; i < allDishes.Count; i++)
        {
            allDishes[i].SetAsNotDirty();
        }
        for (int i = 0; i < allCombos.Count; i++)
        {
            allCombos[i].SetAsNotDirty();
        }
    }


}
