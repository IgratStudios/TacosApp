using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class MenuEditionScreen : UIScreen 
{

	public string selectDishSlotId = "SelectDishSlot";
	public string selectVarietySlotId = "SelectVarietySlot";
	public string selectionComboSlotId = "SelectionComboSlot";
	public string selectionComboDishSlotId = "SelectionComboDishSlot";

	public enum SCREEN_STATE
	{
		NONE,
		ACTION_SLECTION,
		NEW_DISH,
		SELECT_DISH,
		NEW_COMBO,
		SELECT_COMBO
	}

	public SCREEN_STATE startingState = SCREEN_STATE.ACTION_SLECTION;
	public SCREEN_STATE currentState = SCREEN_STATE.NONE;

	public GameObject startingMenuHolder;
	public GameObject dishEditionHolder;
	public InputField dishName;
	public InputField dishDescription;
	public InputField dishPrice;
	public Dropdown dishCategoryDropdown;
	public GameObject dishCategoryDropdownTemplate;
	public Transform varietiesHolder;
	public List<UISelectionVarietySlot> selectionVarietySlots = new List<UISelectionVarietySlot>();
	private Dictionary<string,int> selectionVarietiesMap = new Dictionary<string, int>();

	public GameObject dishSelectionHolder;
    public GameObject noDishesYetHolder;
    public Transform dishesHolder;
	public List<UISelectionDishSlot> selectionDishSlots = new List<UISelectionDishSlot>();

	public GameObject comboSelectionHolder;
    public GameObject noCombosYetHolder;
	public Transform combosHolder;
	public List<UISelectionComboSlot> selectionComboSlots = new List<UISelectionComboSlot>();

	public GameObject comboEditionHolder;
	public InputField comboName;
	public Text comboDescription;
	public InputField comboPrice;
	public Transform comboDishesHolder;
	public List<UISelectionComboDishSlot> selectionComboDishSlots = new List<UISelectionComboDishSlot>();
    private Dictionary<string, int> selectionDishesMap = new Dictionary<string, int>();

    public GameObject newCategoryHolder;
	public InputField newCategoryName;
	public GameObject newVarietyHolder;
	public InputField newVarietyName;
	public InputField newVarietyPrice;
	public GameObject newComboDescriptionHolder;
	public InputField newComboDescription;

	private DishData currentDish;//this will be a temporary copy, this way we can discard changes easily
	private ComboData currentCombo;//this will be a temporary copy, this way we can discard changes easily

	public override void Activate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		base.Activate (screenChangeCallback);
        currentDish = new DishData();
		currentCombo = new ComboData();
		ChangeScreenToState(startingState);
		SwitchNewCategory(false);
		SwitchNewVariety(false);
		SwitchNewComboDescription(false);
	}

	private void ChangeScreenToState(SCREEN_STATE newState)
	{
		currentState = newState;
		switch(currentState)
		{
		case SCREEN_STATE.NONE:
			SwitchDishEdition(false);
			SwitchComboEdition(false);
			SwitchDishSelection(false);
			SwitchComboSelection(false);
			SwitchDishOrComboSelection(false);
			break;
		case SCREEN_STATE.ACTION_SLECTION:
			SwitchDishEdition(false);
			SwitchComboEdition(false);
			SwitchDishSelection(false);
			SwitchComboSelection(false);
			SwitchDishOrComboSelection(true);
			break;
		case SCREEN_STATE.NEW_DISH:
			SwitchDishEdition(true);
            SwitchDishSelection(false);
            SwitchDishOrComboSelection(false);
			break;
		case SCREEN_STATE.SELECT_DISH:
			SwitchDishSelection(true);
			SwitchDishOrComboSelection(false);
			break;
		case SCREEN_STATE.NEW_COMBO:
			SwitchComboEdition(true);
            SwitchComboSelection(false);
            SwitchDishOrComboSelection(false);
			break;
		case SCREEN_STATE.SELECT_COMBO:
			SwitchComboSelection(true);
			SwitchDishOrComboSelection(false);
			break;
		}
	}

	public void SwitchDishOrComboSelection(bool enable)
	{
		startingMenuHolder.ForceActivateRecursively(enable);
	}

    #region DISHE's
    public void SwitchNewCategory(bool enable)
    {
        if (enable)
        {
            newCategoryHolder.ForceActivateRecursively(enable);
            newCategoryName.text = string.Empty;
            EventSystem.current.SetSelectedGameObject(newCategoryName.gameObject, null);
            newCategoryName.OnPointerClick(new PointerEventData(EventSystem.current));
        }
        else
        {
            newCategoryHolder.ForceActivateRecursively(enable);
        }
    }

    public void OnSubmitNewCategoryButtonPressed()
    {
      //  Debug.Log("Submitting new category["+newCategoryName.text+"]. Exist?["+ MenuDataManager.GetMenuDataManager().ExistCategoryWithName(newCategoryName.text) + "]");
        if (!string.IsNullOrEmpty(newCategoryName.text) && !MenuDataManager.GetMenuDataManager().ExistCategoryWithName(newCategoryName.text))
        {
            CategoryData newCategory = MenuDataManager.GetMenuDataManager().CreateNewCategory(newCategoryName.text);
            if (newCategory.version != -1)
            {
                //add to dropdown menu
                dishCategoryDropdown.AddOptions(new List<string>() { newCategory.name });
                int optionIndex = dishCategoryDropdown.GetOptionIndex(newCategory.name);
                if (optionIndex >= 0)
                {
                    dishCategoryDropdown.value = optionIndex;
                }
                else
                {
                    dishCategoryDropdown.value = 0;
                }
                dishCategoryDropdown.RefreshShownValue();
              

                SwitchNewCategory(false);
            }
        }
    }

    public void SwitchNewVariety(bool enable)
    {
        if (enable)
        {
            newVarietyHolder.ForceActivateRecursively(enable);
            newVarietyName.text = string.Empty;
            newVarietyPrice.text = "0.00";
            EventSystem.current.SetSelectedGameObject(newVarietyName.gameObject, null);
            newVarietyName.OnPointerClick(new PointerEventData(EventSystem.current));
        }
        else
        {
            newVarietyHolder.ForceActivateRecursively(enable);
        }
    }

    public void OnSubmitNewVarietyButtonPressed()
    {
        if (!string.IsNullOrEmpty(newVarietyName.text) && !MenuDataManager.GetMenuDataManager().ExistVarietyWithName(newVarietyName.text))
        {
            float newPrice = 0.0f;
            if (float.TryParse(newVarietyPrice.text, out newPrice))
            {
                VarietyData newVariety = MenuDataManager.GetMenuDataManager().CreateNewVariety(newVarietyName.text, newPrice);
                if (newVariety.version != -1)
                {
                    UISelectionVarietySlot slot = ObjectManager.SpawnLike<UISelectionVarietySlot>(selectVarietySlotId);
                    if (slot != null)
                    {
                        slot.CachedTransform.SetParent(varietiesHolder);
                        slot.CachedTransform.localScale = Vector3.one;
                        slot.CachedTransform.SetLocalPositionZ(0.0f);
                        slot.SetVarietyData(newVariety);
                        //add listeners
                        slot.OnVarietySelectionChanged += OnVarietySelectionChanged;
                        //add to list and map
                        selectionVarietySlots.Add(slot);
                        selectionVarietiesMap.Add(newVariety.id, selectionVarietySlots.Count - 1);
                        SwitchNewVariety(false);
                    }
                }
            }
        }
    }

    public void OnDishNameEditFinished(InputField nameInputField)
    {
        if (currentDish.version != -1)
        {
            currentDish.SetNewName(nameInputField.text);
            dishName.text = currentDish.name;
        }
    }

    public void OnDishDescriptionEditFinished(InputField descriptionInputField)
    {
        if (currentDish.version != -1)
        {
            currentDish.SetNewDescription(descriptionInputField.text);
            dishDescription.text = currentDish.description;
        }
    }

    public void OnDishPriceEditFinished(InputField priceInputField)
    {
        if (currentDish.version != -1)
        {
            float newPrice = 0.0f;
            if (float.TryParse(priceInputField.text, out newPrice))
            {
                currentDish.SetNewStartingPrice(newPrice);
            }
            dishPrice.text = currentDish.PriceToString();
        }
        
    }

    public void SwitchDishEdition(bool enable)
	{
		if(enable)
		{
			dishName.onEndEdit.AddListener(delegate { OnDishNameEditFinished (dishName); });
			dishDescription.onEndEdit.AddListener(delegate { OnDishDescriptionEditFinished (dishDescription); });
			dishPrice.onEndEdit.AddListener(delegate { OnDishPriceEditFinished (dishPrice); });

			dishEditionHolder.ForceActivateRecursively(enable);
			FillCategoryDropdown();
			FillVarietiesScrollview();
			if(!string.IsNullOrEmpty(currentDish.id))//dish edition setup
			{
				dishName.text = currentDish.name;
				dishDescription.text = currentDish.description;
				dishPrice.text = currentDish.PriceToString();
                
				CategoryData dishCategory = MenuDataManager.GetMenuDataManager().GetCategoryById(currentDish.categoryId);
				if(dishCategory.version != -1)
				{
					int optionIndex = dishCategoryDropdown.GetOptionIndex(dishCategory.name);
					if(optionIndex >= 0)
					{
						dishCategoryDropdown.value = optionIndex;
					}
					else
					{
						dishCategoryDropdown.value = 0;
					}
					dishCategoryDropdown.RefreshShownValue();
				}
				SelectDishVarieties(currentDish);

			}
			else//new dish setup
			{
				currentDish = new DishData(0);
				dishName.text = "Nombre del platillo.";
				dishDescription.text = "Descripción del platillo para el cliente.";
				dishPrice.text = "0.00";
				dishCategoryDropdown.value = 0;
				dishCategoryDropdown.RefreshShownValue();
			}
		}
		else
		{
			dishName.onEndEdit.RemoveAllListeners();
			dishDescription.onEndEdit.RemoveAllListeners();
			dishPrice.onEndEdit.RemoveAllListeners();

			currentDish = new DishData();
			ClearCategoryDropdown();
			ClearVarietiesScrollView();
			dishEditionHolder.ForceActivateRecursively(enable);
		}
	}

	public void SwitchDishSelection(bool enable)
	{
		if(enable)
		{
            dishSelectionHolder.ForceActivateRecursively(enable);
			List<DishData> allDishes = MenuDataManager.GetMenuDataManager().GetAllDishes();
			if(allDishes != null)
			{
                noDishesYetHolder.ForceActivateRecursively(allDishes.Count == 0);

                for (int i = 0; i < allDishes.Count; i++ )
				{
					UISelectionDishSlot slot = ObjectManager.SpawnLike<UISelectionDishSlot>(selectDishSlotId);
					if(slot != null)
					{
						slot.CachedTransform.SetParent(dishesHolder);
						slot.CachedTransform.localScale = Vector3.one;
                        slot.CachedTransform.SetLocalPositionZ(0.0f);
                        slot.SetDishData(allDishes[i]);
						//add listeners
						slot.OnDishSelected += OnDishSelected;
						selectionDishSlots.Add(slot);
					}
				}
			}
		}
		else
		{
			while(selectionDishSlots.Count > 0)
			{
				//remove listeners
				selectionDishSlots[0].OnDishSelected -= OnDishSelected;
				selectionDishSlots[0].Despawn();
				selectionDishSlots.RemoveAt(0);
			}
            dishSelectionHolder.ForceActivateRecursively(enable);

        }
	}

	private void FillCategoryDropdown()
	{
		dishCategoryDropdownTemplate.SetActive(false);
		List<string> allCategoryOptions =  MenuDataManager.GetMenuDataManager().GetAllCategoriesAsOptions();
		if(allCategoryOptions != null)
		{
			dishCategoryDropdown.ClearOptions();
			dishCategoryDropdown.AddOptions(allCategoryOptions);
		}
	}

	private void ClearCategoryDropdown()
	{
		dishCategoryDropdown.ClearOptions();
	}

	private void FillVarietiesScrollview()
	{
        List <VarietyData> allVarieties = MenuDataManager.GetMenuDataManager().GetAllVarieties();
		if(allVarieties != null)
		{
			for(int i = 0; i < allVarieties.Count; i++ )
			{
				UISelectionVarietySlot slot = ObjectManager.SpawnLike<UISelectionVarietySlot>(selectVarietySlotId);
				if(slot != null)
				{
					slot.CachedTransform.SetParent(varietiesHolder);
					slot.CachedTransform.localScale = Vector3.one;
                    slot.CachedTransform.SetLocalPositionZ(0.0f);
                    slot.SetVarietyData(allVarieties[i]);
					//add listeners
					slot.OnVarietySelectionChanged += OnVarietySelectionChanged;
					//add to list and map
					selectionVarietySlots.Add(slot);
					selectionVarietiesMap.Add(allVarieties[i].id,selectionVarietySlots.Count-1);
				}
			}
		}
	}

	private void SelectDishVarieties(DishData dishData)
	{
		for(int i = 0; i < dishData.varietyIds.Length; i++)
		{
			int slotIndex = -1;
			if(selectionVarietiesMap.TryGetValue(dishData.varietyIds[i], out slotIndex))
			{
				selectionVarietySlots[slotIndex].ForceSelection();
			}
		}
	}

	private void ClearVarietiesScrollView()
	{
		while(selectionVarietySlots.Count > 0)
		{
			//remove listeners
			selectionVarietySlots[0].OnVarietySelectionChanged -= OnVarietySelectionChanged;
			selectionVarietySlots[0].Despawn();
			selectionVarietySlots.RemoveAt(0);
		}
		selectionVarietiesMap.Clear();
	}

	private void OnDishSelected(DishData dData)
	{
        currentDish.Copy(dData);
        ChangeScreenToState(SCREEN_STATE.NEW_DISH);
    }

    private void OnComboDishSelectionChanged(DishData dData,bool isSelected)
    {
        if (currentCombo.version != -1)
        {
            if (isSelected)
            {
                currentCombo.AddDish(dData.id);
            }
            else
            {
                if (currentCombo.HasDish(dData.id))
                {
                    currentCombo.RemoveDish(dData.id);
                }
            }
            //update combo description text
            comboDescription.text = currentCombo.GetDescription();
        }
    }

	private void OnVarietySelectionChanged(VarietyData vData, bool isSelected)
	{
        Debug.Log("OnVariety selection changed["+isSelected+"] [" + vData.name + "] DishVersion[" + currentDish.version + "]");
        if (currentDish.version != -1)
		{
            if (isSelected)
            {
                currentDish.AddVariety(vData.id);
            }
            else
            {
                if (currentDish.HasVariety(vData.id))
                {
                    currentDish.RemoveVariety(vData.id);
                }
            }
		}
	}

    #endregion

    #region COMBO's
    public void SwitchComboEdition(bool enable)
    {
        if (enable)
        {
            comboName.onEndEdit.AddListener(delegate { OnComboNameEditFinished(comboName); });
           
            comboPrice.onEndEdit.AddListener(delegate { OnComboPriceEditFinished(comboPrice); });

            comboEditionHolder.ForceActivateRecursively(enable);
           
            FillComobDishesScrollview();
            if (!string.IsNullOrEmpty(currentCombo.id))//combo edition setup
            {
                comboName.text = currentCombo.name;
                comboDescription.text = currentCombo.GetDescription();
                comboPrice.text = currentCombo.PriceToString();

                SelectComboDishes(currentCombo);
            }
            else//new combo setup
            {
                currentCombo = new ComboData(0);
                comboName.text = "Nombre del combo.";
                comboDescription.text = "Descripción del combo para el cliente, se genera automático con los platillos seleccionados, o escribe una propia.";
                comboPrice.text = "0.00";
            }
        }
        else
        {
            comboName.onEndEdit.RemoveAllListeners();
            
            comboPrice.onEndEdit.RemoveAllListeners();
           
            currentCombo = new ComboData();
            ClearComboDishesScrollView();
            comboEditionHolder.ForceActivateRecursively(enable);
        }
    }

    public void SwitchComboSelection(bool enable)
    {
        if (enable)
        {
            comboSelectionHolder.ForceActivateRecursively(enable);
            List<ComboData> allCombos = MenuDataManager.GetMenuDataManager().GetAllCombos();
            if (allCombos != null)
            {
                noCombosYetHolder.ForceActivateRecursively(allCombos.Count == 0);
                for (int i = 0; i < allCombos.Count; i++)
                {
                    UISelectionComboSlot slot = ObjectManager.SpawnLike<UISelectionComboSlot>(selectionComboSlotId);
                    if (slot != null)
                    {
                        slot.CachedTransform.SetParent(combosHolder);
                        slot.CachedTransform.localScale = Vector3.one;
                        slot.CachedTransform.SetLocalPositionZ(0.0f);
                        slot.SetComboData(allCombos[i]);
                        //add listeners
                        slot.OnComboSelected += OnComboSelected;
                        selectionComboSlots.Add(slot);
                    }
                }
            }
        }
        else
        {
            while (selectionComboSlots.Count > 0)
            {
                //remove listeners
                selectionComboSlots[0].OnComboSelected -= OnComboSelected;
                selectionComboSlots[0].Despawn();
                selectionComboSlots.RemoveAt(0);
            }
            comboSelectionHolder.ForceActivateRecursively(enable);
        }
    }

    private void FillComobDishesScrollview()
    {
        List<DishData> allDishes = MenuDataManager.GetMenuDataManager().GetAllDishes();
        if (allDishes != null)
        {
            for (int i = 0; i < allDishes.Count; i++)
            {
                UISelectionComboDishSlot slot = ObjectManager.SpawnLike<UISelectionComboDishSlot>(selectionComboDishSlotId);
                if (slot != null)
                {
                    slot.CachedTransform.SetParent(comboDishesHolder);
                    slot.CachedTransform.localScale = Vector3.one;
                    slot.CachedTransform.SetLocalPositionZ(0.0f);
                    slot.SetDishData(allDishes[i]);
                    //add listeners
                    slot.OnDishSelectionChanged += OnComboDishSelectionChanged;
                  
                    selectionComboDishSlots.Add(slot);
                    selectionDishesMap.Add(allDishes[i].id, selectionComboDishSlots.Count - 1);
                }
            }
        }
    }

    private void SelectComboDishes(ComboData comboData)
    {
        for (int i = 0; i < comboData.dishesIds.Length; i++)
        {
            int slotIndex = -1;
            if (selectionDishesMap.TryGetValue(comboData.dishesIds[i], out slotIndex))
            {
                selectionComboDishSlots[slotIndex].ForceSelection();
            }
        }
    }

    private void ClearComboDishesScrollView()
    {
        while (selectionComboDishSlots.Count > 0)
        {
            //remove listeners
            selectionComboDishSlots[0].OnDishSelectionChanged -= OnComboDishSelectionChanged;
            selectionComboDishSlots[0].Despawn();
            selectionComboDishSlots.RemoveAt(0);
        }
        selectionDishesMap.Clear();
    }

    public void SwitchNewComboDescription(bool enable)
    {
        if (enable)
        {
            newComboDescriptionHolder.ForceActivateRecursively(enable);
            newComboDescription.text = currentCombo.GetDescription();
            EventSystem.current.SetSelectedGameObject(newComboDescription.gameObject, null);
            newComboDescription.OnPointerClick(new PointerEventData(EventSystem.current));
        }
        else
        {
            newComboDescriptionHolder.ForceActivateRecursively(enable);
        }
    }

    public void OnSubmitNewComboDescriptionButtonPressed()
    {
        if (currentCombo.version != -1)
        {
            if (!string.IsNullOrEmpty(newComboDescription.text))
            {
                currentCombo.SetNewDescription(newComboDescription.text);
                comboDescription.text = currentCombo.GetDescription();
                SwitchNewComboDescription(false);
            }
        }
    }

    public void OnResetComboDescriptionButtonPressed()
    {
        if (currentCombo.version != -1)
        {
            if (!string.IsNullOrEmpty(newComboDescription.text))
            {
                currentCombo.SetNewDescription(string.Empty);
                newComboDescription.text = currentCombo.GetDescription();
                comboDescription.text = currentCombo.GetDescription();
            }
        }
    }

    private void OnComboSelected(ComboData cData)
    {
        currentCombo.Copy(cData);
        ChangeScreenToState(SCREEN_STATE.NEW_COMBO);
    }

    public void OnComboNameEditFinished(InputField nameInputField)
	{
		currentCombo.SetNewName(nameInputField.text);
	}

    public void OnComboPriceEditFinished(InputField priceInputField)
    {
        float newPrice = 0.0f;
        if (float.TryParse(priceInputField.text, out newPrice))
        {
            currentCombo.SetNewStartingPrice(newPrice);
        }
    }
	#endregion

	public override void UpdateScreen (UIScreenController.ScreenUpdatedEventHandler screenUpdatedCallBack)
	{
		base.UpdateScreen (screenUpdatedCallBack);
	}

	public override void Deactivate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		base.Deactivate (screenChangeCallback);
        ChangeScreenToState(SCREEN_STATE.NONE);
		currentDish = new DishData();
		currentCombo = new ComboData();
	}

	public void OnNewDishButtonPressed()
	{
		ChangeScreenToState(SCREEN_STATE.NEW_DISH);
	}

	public void OnModifyDishButtonPressed()
	{
		ChangeScreenToState(SCREEN_STATE.SELECT_DISH);
	}

	public void OnNewComboButtonPressed()
	{
		ChangeScreenToState(SCREEN_STATE.NEW_COMBO);
	}

	public void OnModifyComboButtonPressed()
	{
		ChangeScreenToState(SCREEN_STATE.SELECT_COMBO);
	}

	public void OnSubmitChangesButtonPressed()
	{
        switch (currentState)
        {
            case SCREEN_STATE.NEW_COMBO:
                //validate combo data
                if (currentCombo.IsDataValid())
                {
                    //update in client data and in network
                    MenuDataManager.GetMenuDataManager().CreateOrUpdateCombo(currentCombo);
                    OnBackButtonPressed();
                }
                else
                {
                    //TODO: show warning pop up
                }
                break;
            case SCREEN_STATE.NEW_DISH:
                if (currentDish.version != -1)
                {
                    //update dish from current copy
                    //category
                    string categoryName = dishCategoryDropdown.options[dishCategoryDropdown.value].text;
                    string dishCategoryId = MenuDataManager.GetMenuDataManager().GetCategoryIdByName(categoryName);
                    currentDish.SetNewCategory(dishCategoryId);
                    //validate dish data
                    if (currentDish.IsDataValid())
                    {
                        //update in client data and in network
                        MenuDataManager.GetMenuDataManager().CreateOrUpdateDish(currentDish);
                        OnBackButtonPressed();
                    }
                    else
                    {
                        //TODO: show warning pop up
                    }
                }
                break;
        }
	}

	public void OnBackButtonPressed()
	{
		switch(currentState)
		{
		case SCREEN_STATE.NEW_DISH:
		case SCREEN_STATE.SELECT_DISH:
		case SCREEN_STATE.NEW_COMBO:
		case SCREEN_STATE.SELECT_COMBO:
			ChangeScreenToState(SCREEN_STATE.ACTION_SLECTION);
			break;
		default:
			UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sMainMenuScreen);
			break;
		}

	}
}