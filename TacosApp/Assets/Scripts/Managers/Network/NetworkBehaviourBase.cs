using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkBehaviourBase : NetworkBehaviour 
{
	public NetworkIdentity netIdentity;

	public virtual void Awake()
	{
		DontDestroyOnLoad(this);
	}

    public void ForceNetworkUpdate()
    {
        //this forces the update on all clients for this OBJ
        SetDirtyBit(0xFFFFFFFF);
    }

}