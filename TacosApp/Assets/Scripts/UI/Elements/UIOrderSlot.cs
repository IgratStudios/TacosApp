using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOrderSlot : CachedMonoBehaviour 
{
	public NetworkOrder relatedOrder;
	public Text orderUID;
	public Text orderUserName;
	public Text orderMenuItemId;
	public Text orderQuantity;

	public void Init(NetworkOrder netOrder)
	{
		relatedOrder = netOrder;
		orderUID.text = netOrder.uid;
		orderUserName.text = netOrder.userName;
		orderMenuItemId.text = netOrder.orderData.menuItemId;
		orderQuantity.text = netOrder.orderData.quantity.ToString();
	}

	public void ClearSlot()
	{
		relatedOrder = null;
		orderUID.text = string.Empty;
		orderUserName.text = string.Empty;
		orderMenuItemId.text = string.Empty;
		orderQuantity.text = string.Empty;
	}

}
