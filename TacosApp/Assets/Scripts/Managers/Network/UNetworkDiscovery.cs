using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UNetworkDiscovery :  NetworkDiscovery
{
	public bool searchingOtherServers = true;
	public float secondstoWaitForOtherServers = 5.0f;
	public ConnectionManager connectionManager;

	public void Init()
	{
		//start by waiting to see if there are other servers already running
		searchingOtherServers = true;
		Invoke("StopSearchingForServers",secondstoWaitForOtherServers);
	}


	public override void OnReceivedBroadcast(string fromAddress, string data)
	{
		if(data == connectionManager.clientId)
		{
			connectionManager.networkManager.networkAddress = fromAddress;
			if(searchingOtherServers)
			{
				CancelInvoke("StopSearchingForServers");
				searchingOtherServers = false;
				connectionManager.networkManager.StartClient();	
			}
			else
			{
				//ignore??
			}
		}
		else
		{
			Debug.Log("Received["+data+"] from["+fromAddress+"]");
		}
	}


	void StopSearchingForServers()
	{
		searchingOtherServers = false;
		connectionManager.networkManager.StartServer();	
	}

}
