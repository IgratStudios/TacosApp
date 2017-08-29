using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMenu : NetworkBehaviourBase 
{
    public class SyncListCategory : SyncListStruct<CategoryData>
    {

    }
    public class SyncListVariety : SyncListStruct<VarietyData>
    {
    }
    public class SyncListDish : SyncListStruct<DishData>
    {
    }
    public class SyncListCombo : SyncListStruct<ComboData>
    {
    }

    [SyncVar] public string uid;
	[SyncVar] public string userName;
    [SyncVar]public int version;
    [SyncVar] public ulong nextCategoryId;
    [SyncVar] public SyncListCategory allCategories;
    [SyncVar] public ulong nextVarietyId;
    [SyncVar] public SyncListVariety allVarieties;
    [SyncVar] public ulong nextDishId;
    [SyncVar] public SyncListDish allDishes;
    [SyncVar] public ulong nextComboId;
    [SyncVar] public SyncListCombo allCombos;

    public override void Awake()
    {
        base.Awake();
        allCategories = new SyncListCategory();
        allVarieties = new SyncListVariety();
        allDishes = new SyncListDish();
        allCombos = new SyncListCombo();
    }

    public override void OnStartClient ()
	{
		base.OnStartClient ();
		Debug.Log("NetworkMenu Start on Client ["+gameObject.name+"] ["+isLocalPlayer+"]");

		if(MenuDataManager.GetInstance() != null)
		{
			MenuDataManager.GetMenuDataManager().AddRelatedNetworkMenuOrSyncIfNeeded(this);
		}

	}

	public void SetMenuData(MenuData menuData)
	{
        version = menuData.version;

        nextCategoryId = menuData.nextCategoryId;
        allCategories.Clear();
        for (int i = 0; i < menuData.allCategories.Count; i++)
        {
            allCategories.Add(menuData.allCategories[i]);
        }

        nextVarietyId = menuData.nextVarietyId;
        for (int i = 0; i < menuData.allVarieties.Count; i++)
        {
            allVarieties.Add(menuData.allVarieties[i]);
        }

        nextDishId = menuData.nextDishId;
        for (int i = 0; i < menuData.allDishes.Count; i++)
        {
            allDishes.Add(menuData.allDishes[i]);
        }

        nextComboId = menuData.nextComboId;
        for (int i = 0; i < menuData.allCombos.Count; i++)
        {
            allCombos.Add(menuData.allCombos[i]);
        }

    }

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		//now check update with MenuDataManager
		if(MenuDataManager.GetInstance() != null && isLocalPlayer)
		{
			MenuDataManager.GetMenuDataManager().SyncMenuDataFromRemoteVersion();
		}


	}

    public bool compareAndUpdate(MenuData menuData)
    {
        Debug.Log("CompareAndUpdateFromLocalData NetVer[" + version + "] < LocalVer[" + menuData.version + "]");
        if (menuData.version > version)
        {
            version = menuData.version;

            nextCategoryId = menuData.nextCategoryId;
            allCategories.Clear();
            for (int i = 0; i < menuData.allCategories.Count; i++)
            {
                allCategories.Add(menuData.allCategories[i]);
            }

            nextVarietyId = menuData.nextVarietyId;
            allVarieties.Clear();
            for (int i = 0; i < menuData.allVarieties.Count; i++)
            {
                allVarieties.Add(menuData.allVarieties[i]);
            }

            nextDishId = menuData.nextDishId;
            allDishes.Clear();
            for (int i = 0; i < menuData.allDishes.Count; i++)
            {
                allDishes.Add(menuData.allDishes[i]);
            }

            nextComboId = menuData.nextComboId;
            allCombos.Clear();
            for (int i = 0; i < menuData.allCombos.Count; i++)
            {
                allCombos.Add(menuData.allCombos[i]);
            }
            SetAsNotDirty();
            return true;
        }
        return false;
    }

    public bool compareAndUpdate(NetworkMenu netMenu)
    {
        Debug.Log("CompareAndUpdateFromNetworkData ThisNetVer[" + version + "] < OtherNetVer[" + netMenu.version + "]");
        if (netMenu.version > version)
        {
            version = netMenu.version;

            nextCategoryId = netMenu.nextCategoryId;
            allCategories.Clear();
            for (int i = 0; i < netMenu.allCategories.Count; i++)
            {
                allCategories.Add(netMenu.allCategories[i]);
            }

            nextVarietyId = netMenu.nextVarietyId;
            for (int i = 0; i < netMenu.allVarieties.Count; i++)
            {
                allVarieties.Add(netMenu.allVarieties[i]);
            }

            nextDishId = netMenu.nextDishId;
            for (int i = 0; i < netMenu.allDishes.Count; i++)
            {
                allDishes.Add(netMenu.allDishes[i]);
            }

            nextComboId = netMenu.nextComboId;
            for (int i = 0; i < netMenu.allCombos.Count; i++)
            {
                allCombos.Add(netMenu.allCombos[i]);
            }
            return true;
        }
        return false;
    }

    public void SetAsNotDirty()
    {
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
