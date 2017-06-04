using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkOrder : NetworkBehaviourBase 
{
	private static OrdersManager ordersManager;

	[System.Serializable]
	public class OrderData
	{
		public uint quantity;
		public string menuItemId;
	}

	[SyncVar] public string uid;
	[SyncVar] public string userName;
	[SyncVar] public OrderData orderData;


	public override void OnStartClient ()
	{
		base.OnStartClient ();
		Debug.Log("NetOrder Start on Client ["+gameObject.name+"] ["+isLocalPlayer+"]");

		if(ordersManager == null)
		{
			ordersManager = OrdersManager.GetInstance();
		}

		if(ordersManager != null)
		{
			ordersManager.AddNetOrder(this);
		}

	}


}
