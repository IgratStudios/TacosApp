using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum ENTITY_TYPE
{
	NONE,
	HOST,
	CLIENT
}


public class NetworkDataSyncer : NetworkBehaviourBase 
{

	[System.Serializable]
	public class StatusData
	{
        [SyncVar]
        public ENTITY_TYPE entityType;
	}

	[SyncVar] public StatusData statusData;

	public override void OnStartClient()
	{
		base.OnStartClient();
		Debug.Log("On NDS id["+netIdentity.netId+"] CLIENT started ["+statusData.entityType+"]");
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		Debug.Log("On NDS id["+netIdentity.netId+"] SERVER started["+statusData.entityType+"]");
    }

	public override void OnStartLocalPlayer ()
	{
		base.OnStartLocalPlayer ();

		Debug.Log("On NDS id["+netIdentity.netId+"] LOCALPLAYER started local["+isLocalPlayer+"] client["+isClient+"] server["+isServer+"] CurrentType["+statusData.entityType+"] ");
		ConnectionManager.GetInstance().AddNetworkDataSyncer(this,isLocalPlayer);
		if(isLocalPlayer)
		{
            if (isServer)
            {
                statusData.entityType = ENTITY_TYPE.HOST;
            }
            else if (isClient)
            {
                CmdUpdateUtatusOnServer(ENTITY_TYPE.CLIENT);
            }
        }
	}


    [Command]
    void CmdUpdateUtatusOnServer(ENTITY_TYPE entityType)
    {
        Debug.Log("CMD_Updating status on server for[" + netIdentity.netId + "] to[" + entityType + "]");
        statusData.entityType = entityType;
        ForceNetworkUpdate();
    }

    [Command]
	public void CmdSpawnNetOrderOnServer(NETWORK_PREFAB_TYPE prefabType,string uid, string userName, NetworkOrder.OrderData orderData)
	{
		GameObject netOrderPrefab = null;

		if(ConnectionManager.GetInstance().TryGetNetPrefab(prefabType, out netOrderPrefab))
		{
			Debug.Log("Spawning prefab for ["+prefabType+"]");
			GameObject newNetOrderGO = 	(GameObject)Instantiate(netOrderPrefab,Vector3.zero, Quaternion.identity);
			NetworkOrder netOrder = newNetOrderGO.GetComponent<NetworkOrder>();
			if(netOrder != null)
			{
				netOrder.uid = uid;
				netOrder.userName = userName;
				newNetOrderGO.name = "Order("+uid+"_"+userName+")";
				netOrder.orderData.menuItemId = Random.Range(0,10).ToString();
				netOrder.orderData.quantity = (uint)Random.Range(1,4);
				netOrder.name = netOrder.userName;

				NetworkServer.Spawn(newNetOrderGO);
			}
		}
		else
		{
			Debug.LogWarning("No Prefab founded for["+prefabType+"]");
		}

	}




}
