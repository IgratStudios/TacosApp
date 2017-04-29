using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class ConnectionManager : Manager<ConnectionManager> 
{
	public delegate void ConnectionDone();


	public UNetworkManager networkManager;
	public UNetworkDiscovery networkDiscovery;
	public string clientId = "TacosClientApp";

	public override void StartManager ()
	{
		networkDiscovery.Init();

		base.StartManager ();
	}


	public void TryToStart(bool tryToBeHost,ConnectionDone callback)
	{
		//Check if there is already a host
		bool isThisAClient = !networkDiscovery.IsClient;
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
			networkDiscovery.StopSearchingForServersAndBecomeHost();
			if(callback != null)
			{
				callback();
			}
		}
		else
		{
			//keep waiting for a host to appear
			StartCoroutine("WaitForHost",callback);
		}
	}

	IEnumerator WaitForHost(ConnectionDone callback)
	{
		while(!networkDiscovery.IsClient)
		{
			yield return 0;
		}
		Debug.Log("A Wild Host Appears!");
		if(callback != null)
		{
			callback();
		}
	}

	public bool IsClient()
	{
		return networkDiscovery.IsClient;
	}

	public bool IsServer()
	{
		return networkDiscovery.IsServer;
	}

}
