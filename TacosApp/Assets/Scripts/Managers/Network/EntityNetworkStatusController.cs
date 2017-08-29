using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EntityNetworkStatusController : NetworkBehaviour 
{

	[System.Serializable]
	public class StatusData
	{
		public ENTITY_TYPE entityType;
	}

	public NetworkIdentity netIdentity;

	[SyncVar] public StatusData statusData;


	public void Awake()
	{
		DontDestroyOnLoad(this);
		Debug.LogWarning("ENSC id["+netIdentity.netId+"] spawned isLocal["+isLocalPlayer+"] isClient["+isClient+"] isServer["+isClient+"]");
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		Debug.Log("On ENSC id["+netIdentity.netId+"] CLIENT started");
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		Debug.Log("On ENSC id["+netIdentity.netId+"] SERVER started");
	}

	public override void OnStartLocalPlayer ()
	{
		base.OnStartLocalPlayer ();
		Debug.Log("On ENSC id["+netIdentity.netId+"] LOCALPLAYER started local["+isLocalPlayer+"] client["+isClient+"] server["+isServer+"]");
		if(isLocalPlayer)
		{
			Debug.Log("Updating like["+(isServer?"HOST":"CLIENT")+"]");
			if(isServer)
			{
				UpdateStatusOnServer(ENTITY_TYPE.HOST);
			}
			else if(isClient)
			{
				UpdateStatusOnServer(ENTITY_TYPE.CLIENT);
			}


		}

	}

	void UpdateStatusOnServer(ENTITY_TYPE entityType)
	{
		Debug.LogWarning("On ENSC id["+netIdentity.netId+"] updating to["+entityType+"]");
		statusData.entityType = entityType;
	}



}
