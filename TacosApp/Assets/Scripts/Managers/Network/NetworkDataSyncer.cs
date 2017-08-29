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

	private string uID = string.Empty;

	public string UniqueId
	{
		get{ return uID;}
	}

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
        uID = netIdentity.netId.ToString();
        Debug.Log("On NDS id["+netIdentity.netId+"] CLIENT started ["+statusData.entityType+"]");
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
        uID = netIdentity.netId.ToString();
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

			//Add here other calls needed when a new client gets connected
			uID = netIdentity.netId.ToString();
			//spawn menu that syncs over the network
			ConnectionManager.GetInstance().SpawnNetMenu("NETMENU_"+uID,uID);

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
			GameObject newNetOrderGO = 	Instantiate(netOrderPrefab,Vector3.zero, Quaternion.identity);
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



	[Command]
	public void CmdCreateMenuOnServer(NETWORK_PREFAB_TYPE prefabType,string uid, string userName)
	{
		GameObject netMenuPrefab = null;

		if(ConnectionManager.GetInstance().TryGetNetPrefab(prefabType, out netMenuPrefab))
		{
			Debug.Log("Spawning prefab for ["+prefabType+"], PrefabFounded["+(netMenuPrefab != null)+"]");
			GameObject newNetMenuGO = Instantiate(netMenuPrefab,Vector3.zero, Quaternion.identity);
            if (newNetMenuGO != null)
            {
                NetworkMenu netMenu = newNetMenuGO.GetComponent<NetworkMenu>();
                if (netMenu != null)
                {
                    netMenu.uid = uid;
                    netMenu.userName = userName;
                    newNetMenuGO.name = "Menu(" + uid + "_" + userName + ")";

                    netMenu.SetMenuData(MenuDataManager.GetMenuDataManager().GetCurrentMenuData());

                    NetworkServer.Spawn(newNetMenuGO);
                }
            }
            else
            {
                Debug.LogError("Could not create new ["+prefabType+"] instance.");
            }
		}
		else
		{
			Debug.LogWarning("No Prefab founded for["+prefabType+"]");
		}


	}




}
