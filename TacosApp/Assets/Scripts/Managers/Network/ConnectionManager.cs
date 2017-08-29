using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;


public enum NETWORK_PREFAB_TYPE
{
	NONE,
	MENU,
	ORDER
	//add others here
}

[System.Serializable]
public class NetworkPrefabData
{
	public NETWORK_PREFAB_TYPE prefabType;
	public GameObject prefab;
}

public class ConnectionManager : Manager<ConnectionManager> 
{
	public delegate void ConnectionDone();

	public UNetworkManager networkManager;
	//public UNetworkDiscovery networkDiscovery;


	public string clientId = "TacosClientApp";

	public List<NetworkPrefabData> allNetPrefabDatas = new List<NetworkPrefabData>();
	private Dictionary<NETWORK_PREFAB_TYPE,GameObject> netPrefabsMap = new Dictionary<NETWORK_PREFAB_TYPE, GameObject>();

	private bool isWaitingHost = false;

	public List<NetworkDataSyncer> allDataSyncers = new List<NetworkDataSyncer>();
	private NetworkDataSyncer thisClientDataSyncer;

	public bool showStatusGUI;

	public override void StartManager ()
	{
        if (alreadystarted)
            return;    
        
//		networkDiscovery.Init();
		CreatePrefabsMap();
		base.StartManager ();
	}

    
    private void CreatePrefabsMap()
    {
        Debug.Log("Creating network prefabs map.[" + allNetPrefabDatas.Count + "] network prefabs founded!");

        for (int i = 0; i < allNetPrefabDatas.Count; i++)
        {
            netPrefabsMap.Add(allNetPrefabDatas[i].prefabType, allNetPrefabDatas[i].prefab);
            RegisterNetworkPrefab(allNetPrefabDatas[i].prefab);
        }
    }

    public bool TryGetNetPrefab(NETWORK_PREFAB_TYPE prefabType,out GameObject prefab)
	{
		return netPrefabsMap.TryGetValue(prefabType,out prefab);
	}
		

	public void TryToStart(bool tryToBeHost,ConnectionDone callback)
	{
        //Check if there is already a host
        bool isThisAClient = networkManager.clientConnectedToServer;

        Debug.Log("Trying to start as a Host?["+tryToBeHost+"]. There is already a Host["+isThisAClient+"].");
		if(isThisAClient)
		{
			if(callback != null)
			{
				callback();
			}
		}
		else if(tryToBeHost)
		{
			//networkDiscovery.StopSearchingForServersAndBecomeHost();
			networkManager.StartAsHost();
			if(callback != null)
			{
				callback();
			}
		}
		else
		{
			//networkDiscovery.StartServerSearching();
			networkManager.StartAsClient();
			//keep waiting for a host to appear
			StartCoroutine("WaitForHost",callback);
		}
	}

	IEnumerator WaitForHost(ConnectionDone callback)
	{
		isWaitingHost = true;
		while(!networkManager.IsClientConnected())
		{
			yield return 0;
		}
		Debug.Log("A Wild Host Appears!");
		isWaitingHost = false;
		if(callback != null)
		{
			callback();
		}
	}

	public bool IsClient()
	{
        return networkManager.IsClientConnected();
	}

	public bool IsServer()
	{
        return networkManager.isServer;
	}

	public void AddNetworkDataSyncer(NetworkDataSyncer playerDataSyncer,bool isThisclientDataSyncer)
	{
		Debug.Log("Adding Network Data Syncer from id["+playerDataSyncer.netIdentity.netId+"] ["+playerDataSyncer.statusData.entityType+"]");
		allDataSyncers.Add(playerDataSyncer);
		if(isThisclientDataSyncer)
		{
			thisClientDataSyncer = playerDataSyncer;
		}
	}

	public ENTITY_TYPE GetLocalDataSyncerEntityType()
	{
		if(thisClientDataSyncer != null)
		{
			return thisClientDataSyncer.statusData.entityType;
		}
		else
		{
			for(int i = 0; i < allDataSyncers.Count; i++)
			{
				if(allDataSyncers[i].isLocalPlayer)
				{
					return allDataSyncers[i].statusData.entityType;
				}
			}
			return ENTITY_TYPE.NONE;
		}
	}

	public uint GetLocalDataSyncerNetId()
	{
		for(int i = 0; i < allDataSyncers.Count; i++)
		{
			if(allDataSyncers[i].isLocalPlayer)
			{
				return allDataSyncers[i].netId.Value;
			}
		}
		return uint.MinValue;
	}

	public string GetLocalDataSyncerUniqueId()
	{
		for(int i = 0; i < allDataSyncers.Count; i++)
		{
			if(allDataSyncers[i].isLocalPlayer)
			{
				return allDataSyncers[i].UniqueId;
			}
		}
		return string.Empty;
	}

    public void RegisterNetworkPrefab(GameObject prefabToRegister)
    {
        NetworkIdentity netId = prefabToRegister.GetComponent<NetworkIdentity>();
        if (netId != null)
        {
            if (_mustShowDebugInfo)
            {
                Debug.Log("Registering Prefab [" + prefabToRegister.name + "]");
            }
            ClientScene.RegisterPrefab(prefabToRegister);

        }
    }

    public void SpawnNetOrder(string uid, string userName, NetworkOrder.OrderData orderData)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Spawn New NetOrder ["+uid+"]["+userName+"]");
		}

		if(thisClientDataSyncer != null)
		{
			thisClientDataSyncer.CmdSpawnNetOrderOnServer(NETWORK_PREFAB_TYPE.ORDER,uid,userName,orderData);
		}
		else
		{
			for(int i = 0; i < allDataSyncers.Count; i++)
			{
				if(allDataSyncers[i].statusData.entityType == ENTITY_TYPE.HOST)
				{
					allDataSyncers[i].CmdSpawnNetOrderOnServer(NETWORK_PREFAB_TYPE.ORDER,uid,userName,orderData);
					break;
				}
			}
		}
	}

	public void SpawnNetMenu(string uid, string userName)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Spawn New NetMenu ["+uid+"]["+userName+"]");
		}

		if(thisClientDataSyncer != null)
		{
			thisClientDataSyncer.CmdCreateMenuOnServer(NETWORK_PREFAB_TYPE.MENU,uid,userName);
		}
		else
		{
			for(int i = 0; i < allDataSyncers.Count; i++)
			{
				if(allDataSyncers[i].statusData.entityType == ENTITY_TYPE.HOST)
				{
					allDataSyncers[i].CmdCreateMenuOnServer(NETWORK_PREFAB_TYPE.MENU,uid,userName);
					break;
				}
			}
		}
	}




	string[] messageListened = new string[3];
	private int messageCounter;
	public void AddBroadcastedMessage(string message)
	{
		if(showStatusGUI)
		{
			int index = messageCounter%3;
		//	Debug.Log("Index["+index+"]");
			messageListened[index] = "["+messageCounter+"]=>["+message+"]";
			messageCounter++;
		}
	}

	GUIStyle style = new GUIStyle();
	void OnGUI()
	{
		if(showStatusGUI)
		{
			style.normal.textColor = Color.black;

            GUILayout.Label("Network status:[" + SceneManager.GetActiveScene().name + "]", style);
            GUILayout.Label("Network Server/Client Active:" + networkManager.isNetworkActive, style);
            GUILayout.Label("Waiting host:" + isWaitingHost + " at port[" + networkManager.networkPort + "]", style);
            GUILayout.Label("Client:" + IsClient(), style);
            GUILayout.Label("Server:" + IsServer(), style);
            GUILayout.Label("Server Port:" + networkManager.networkPort, style);
            GUILayout.Label("Messages:", style);
            GUILayout.Label(messageListened[0], style);
            GUILayout.Label("-----------------------------------", style);
            GUILayout.Label(messageListened[1], style);
            GUILayout.Label("-----------------------------------", style);
            GUILayout.Label(messageListened[2], style);
            if (IsClient())
            {
                GUILayout.Label("Client Connected to server [" + networkManager.IsClientConnected() + "][" + networkManager.networkAddress + ":" + networkManager.networkPort + "] ", style);
            }
            if (IsServer())
            {
                GUILayout.Label("Clients Connected to this host [" + networkManager.clientsConnectedToThisServer + "/" + networkManager.maxConnections + "] ActivePlayersGO[" + networkManager.numPlayers + "]", style);
            }
            if (thisClientDataSyncer != null)
            {
                GUILayout.Label("ClientDataSyncer statusData.EntityType [" + thisClientDataSyncer.statusData.entityType + "]", style);
            }

        }
	}

}
