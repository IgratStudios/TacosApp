using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EntityNetworkStatusController : NetworkBehaviour 
{
	public enum ENTITY_TYPE
	{
		NONE,
		HOST,
		CLIENT
	}

	[System.Serializable]
	public class StatusData
	{
		public ENTITY_TYPE entityType;
	}

	[SyncVar] public StatusData statusData;


	public void Awake()
	{
		statusData.entityType = ENTITY_TYPE.NONE;
		if(isLocalPlayer)
		{
			//check if this must be marked as the host or as a client
			SetAsServer();

			if(statusData.entityType == ENTITY_TYPE.NONE)
			{
				CmdUpdateStatusOnServer(ENTITY_TYPE.CLIENT);
			}
		}
	}

	[Server] public void SetAsServer()
	{
		statusData.entityType = ENTITY_TYPE.HOST;
	}

	[Command] void CmdUpdateStatusOnServer(ENTITY_TYPE entityType)
	{
		statusData.entityType = entityType;
	}

}
