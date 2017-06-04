using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class UNetworkDiscovery :  NetworkDiscovery
{
	private bool isSearchingOtherServers = false;
	public ConnectionManager connectionManager;

	public ConnectionConfig connectionConfiguration;

	public NetworkClient networkClient;
	public bool isConfirmedClient = false;
	public bool isConfirmedServer = false;

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
			return isConfirmedClient && !isSearchingOtherServers;
		}
	}

	public bool IsServer
	{
		get
		{
			return isConfirmedServer && !isSearchingOtherServers;
		}
	}

	public void Init()
	{
		bool canInit = Initialize();
		connectionManager.networkManager.maxConnections = 10;
		if(!canInit)
		{
			Debug.LogWarning("Couldnt start network discovery. =(");
		}
	}

	public void StartServerSearching()
	{
		//start by waiting to see if there are other servers already running
		isSearchingOtherServers = true;
	//	broadcastPort = 0;
		NetworkServer.Reset();

		if(StartAsClient())
		{
			Debug.Log("Started as Client. isClientFlag["+isClient+"] isConfirmedClientFlag["+isConfirmedClient+"]");
		}
		else
		{
			Debug.LogWarning("The port is already occupied, this means that either there is a client waiting for a server or another program is using this port");
		}

	}

	public void StopSearchingForServersAndBecomeHost()
	{
		Debug.Log("Stop searching for servers and become a Host." +
			"Searching?["+isSearchingOtherServers+"] " +
			"client?["+isClient+"] confirmedClient["+isConfirmedClient+"]" +
			"server["+isServer+"] confirmedServer["+isConfirmedServer+"]");
		if(isSearchingOtherServers)
		{
			StopBroadcast();
			isSearchingOtherServers = false;
			broadcastData = broadcastPort+":"+connectionManager.clientId;


			NetworkServer.Reset();
			connectionManager.networkManager.StopClient();
			connectionManager.networkManager.StopHost();
			connectionManager.networkManager.StopMatchMaker();
			Network.Disconnect();

			isConfirmedServer = StartAsServer();
			networkClient = connectionManager.networkManager.StartHost();	
			if(networkClient != null)
			{
				Debug.Log("Network Client["+networkClient.isConnected+"]");
			}
			else
			{
				Debug.Log("Network Client is null something went bad");
			}

		}
		else if(!isClient)
		{
			broadcastData = broadcastPort+":"+connectionManager.clientId;


			NetworkServer.Reset();
			connectionManager.networkManager.StopClient();
			connectionManager.networkManager.StopHost();
			connectionManager.networkManager.StopMatchMaker();
			Network.Disconnect();

			isConfirmedServer = StartAsServer();
			networkClient = connectionManager.networkManager.StartHost();	
			if(networkClient != null)
			{
				Debug.Log("Network Client["+networkClient.isConnected+"]");
			}
			else
			{
				Debug.Log("Network Client is null something went bad");
			}
		}
		else
		{
			Debug.Log("This is already a Client?["+isConfirmedClient+"] and the server is ["+networkClient.serverIp+"]:["+networkClient.serverPort+"]");
		}
	}

	public override void OnReceivedBroadcast(string fromAddress, string data)
	{
		string ipAddress = string.Empty;
		string[] addressDatas = fromAddress.Split(':');
		for(int i = 0; i < addressDatas.Length; i++)
		{
			Debug.Log("["+i+"]:["+addressDatas[i]+"]");
		}


		ipAddress = addressDatas[addressDatas.Length-1];


		string[] datas = data.Split(':');
		if(datas.Length > 1)
		{
			string msg = "["+ipAddress+"] port["+datas[0]+"]=>["+datas[1]+"]";
			Debug.Log("SearchingServer["+isSearchingOtherServers+"] Broadcast Received:"+msg);	
			connectionManager.AddBroadcastedMessage(msg);
		
			if(isSearchingOtherServers)
			{
				Debug.Log("ClientId["+connectionManager.clientId+"]");
				if(data.Contains(connectionManager.clientId))
				{
					connectionManager.networkManager.networkAddress = ipAddress;
					connectionManager.networkManager.networkPort = Convert.ToInt32(datas[0]);
					if(isSearchingOtherServers)
					{
						isConfirmedClient = true;
						connectionManager.networkManager.serverBindAddress = ipAddress;
						connectionManager.networkManager.serverBindToIP = true;
						networkClient = connectionManager.networkManager.StartClient();
						if(networkClient != null)
						{
							Debug.Log("Network Client["+networkClient.isConnected+"] ["+networkClient.serverIp+":"+networkClient.serverPort+"]");
						}
						else
						{
							Debug.Log("Network client is null, something went bad.");
						}
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
				isSearchingOtherServers = false;
			}
		}
	}

}
