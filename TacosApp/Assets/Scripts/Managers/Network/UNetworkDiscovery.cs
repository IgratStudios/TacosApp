using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UNetworkDiscovery :  NetworkDiscovery
{
	private bool isSearchingOtherServers = true;
	public ConnectionManager connectionManager;

	public int maxNumberOfConnections = 10;

	public ConnectionConfig connectionConfiguration;

	public NetworkClient networkClient;


	public bool IsWaitingForServers
	{
		get
		{
			return isSearchingOtherServers;
		}
	}

	public bool IsClient
	{
		get
		{
			return isClient && !isSearchingOtherServers;
		}
	}

	public bool IsServer
	{
		get
		{
			return isServer && !isSearchingOtherServers;
		}
	}

	public void Init()
	{
		bool canInit = Initialize();
		if(canInit)
		{
			//start by waiting to see if there are other servers already running
			isSearchingOtherServers = true;
			StartAsClient();
			Debug.Log("Started as Client. isClientFlag["+isClient+"]");
		}
	}

	public void StopSearchingForServersAndBecomeHost()
	{
		if(isSearchingOtherServers)
		{
			StopBroadcast();
			isSearchingOtherServers = false;
			isServer = true;
			broadcastData = connectionManager.clientId;
			networkClient = connectionManager.networkManager.StartHost(connectionConfiguration,maxNumberOfConnections);	
			StartAsServer();
		}
		else
		{
			Debug.Log("This is already a Client?["+isClient+"] and the server is ["+networkClient.serverIp+"]:["+networkClient.serverPort+"]");
		}
	}


	public override void OnReceivedBroadcast(string fromAddress, string data)
	{
		if(data == connectionManager.clientId)
		{
			connectionManager.networkManager.networkAddress = fromAddress;
			if(isSearchingOtherServers)
			{
				isSearchingOtherServers = false;
				isClient = true;
				networkClient = connectionManager.networkManager.StartClient();	
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

}
