using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UNetworkManager : NetworkManager 
{

	public ConnectionManager connectionManager;

	//called when this client connects to a server
	public override void OnClientConnect (NetworkConnection conn)
	{
		Debug.Log("On client connected to server with address["+conn.address+"]");
	}

	//called when this client disconnects to a server
	public override void OnClientDisconnect (NetworkConnection conn)
	{
		Debug.Log("On client disconnected from server with address["+conn.address+"]");
	}

	//called when a new client connects to this server
	public override void OnServerConnect(NetworkConnection conn)
	{
		Debug.Log("On New client connected to this server with address["+conn.address+"]");
	}

	//called when a connected client disconnects from this server
	public override void OnServerDisconnect (NetworkConnection conn)
	{
		Debug.Log("On client disconnected from this server with address["+conn.address+"]");
	}


}
