using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrdersScreen : UIScreen 
{
	public UIOrderSlot orderSlotPrefab;
	public Transform ordersHolder;

	public List<UIOrderSlot> allOrderSlots = new List<UIOrderSlot>();

	public override void Activate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		OrdersManager.OnOrderAdded += OnOrderAdded;
		OrdersManager.OnOrderRemoved += OnOrderRemoved;
		base.Activate (screenChangeCallback);
	}

	public override void UpdateScreen (UIScreenController.ScreenUpdatedEventHandler screenUpdatedCallBack)
	{
		base.UpdateScreen (screenUpdatedCallBack);
	}

	public override void Deactivate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		OrdersManager.OnOrderAdded -= OnOrderAdded;
		OrdersManager.OnOrderRemoved -= OnOrderRemoved;
		base.Deactivate (screenChangeCallback);
	}
		
	public void OnBackButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sMainMenuScreen);
	}

	public void OnCreateOrderPressed()
	{
		OrdersManager.GetInstance().CreateNewOrder();
	}

	public void OnOrderAdded(NetworkOrder order)
	{
		
		UIOrderSlot orderSlot = Instantiate<UIOrderSlot>(orderSlotPrefab);
		if(orderSlot != null)
		{
			orderSlot.CachedTransform.SetParent(ordersHolder,false);
			orderSlot.CachedRectTransform.ResetToFillParent();
			Vector3 tempPos = orderSlot.CachedRectTransform.localPosition;
			tempPos.z = 0;
			orderSlot.CachedRectTransform.localPosition = tempPos;
			orderSlot.Init(order);
			allOrderSlots.Add(orderSlot);
		}
	}

	public void OnOrderRemoved(NetworkOrder order)
	{
		for(int i = 0; i < allOrderSlots.Count; i++)
		{
			if(allOrderSlots[i].relatedOrder.uid == order.uid)
			{
				allOrderSlots[i].ClearSlot();
				Destroy(allOrderSlots[i]);
				allOrderSlots.RemoveAt(i);
				break;
			}
		}
	}
}
