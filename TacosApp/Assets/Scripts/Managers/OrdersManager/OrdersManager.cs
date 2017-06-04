using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OrdersManager : Manager<OrdersManager> 
{

	public delegate void OrderAdded(NetworkOrder netOrder);
	public static OrderAdded OnOrderAdded;
	public delegate void OrderRemoved(NetworkOrder netOrder);
	public static OrderRemoved OnOrderRemoved;

	public List<NetworkOrder> allActiveOrders = new List<NetworkOrder>();

	public override void StartManager ()
	{
		base.StartManager ();


	}

	public static void AddSpawnedOrder(NetworkOrder netOrder)
	{
		if(_cachedInstance != null)
		{
			_cachedInstance.AddNetOrder(netOrder);
		}
	}

	public void AddNetOrder(NetworkOrder netOrder)
	{
		allActiveOrders.Add(netOrder);
		if(OnOrderAdded != null)
		{
			OnOrderAdded(netOrder);
		}
	}

	public void RemoveNetOrder(NetworkOrder netOrder)
	{
		if(allActiveOrders.Contains(netOrder))
		{
			allActiveOrders.Remove(netOrder);
			if(OnOrderRemoved != null)
			{
				OnOrderRemoved(netOrder);
			}
		}
	}

	public void CreateNewOrder()
	{
		NetworkOrder.OrderData orderData = new NetworkOrder.OrderData();
		orderData.menuItemId = Random.Range(0,10).ToString();
		orderData.quantity = (uint)Random.Range(1,4);

		ConnectionManager.GetInstance().SpawnNetOrder("NETO_"+allActiveOrders.Count,
			ConnectionManager.GetInstance().GetLocalDataSyncerEntityType()+
			"_"+ConnectionManager.GetInstance().GetLocalDataSyncerNetId(),
			orderData
		);

	}


}
