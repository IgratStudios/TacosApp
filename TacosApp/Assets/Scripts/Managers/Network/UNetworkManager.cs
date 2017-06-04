using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UNetworkManager : NetworkManager 
{

	public ConnectionManager connectionManager;
	public bool clientConnectedToServer;
	public int clientsConnectedToThisServer = 0;
	public bool isServer = false;

	public NetworkClient currentNetworkClient;


	public void StartAsHost()
	{
		currentNetworkClient = StartHost();	
		if(currentNetworkClient != null)
		{
			Debug.Log("HOST NC successfully created.");
		}
		else
		{
			Debug.Log("HOST NC couldn't be created.");
		}
	}
		
	public void StartAsClient()
	{
		currentNetworkClient = StartClient();
		if(currentNetworkClient != null)
		{
			Debug.Log("CLIENT NC successfully created.");
		}
		else
		{
			Debug.Log("CLIENT couldn't be created.");

		}
	}

	//called when this client connects to a server
	public override void OnClientConnect (NetworkConnection conn)
	{
		Debug.Log("On client connected to server with address["+conn.address+"]");
		clientConnectedToServer = true;

	}

	//called when this client disconnects from a server
	public override void OnClientDisconnect (NetworkConnection conn)
	{
		Debug.Log("On client disconnected from server with address["+conn.address+"]");
		clientConnectedToServer = false;
	}

	//called when a new client connects to this server
	public override void OnServerConnect(NetworkConnection conn)
	{
		Debug.Log("On New client connected to this server with address["+conn.address+"]");
		clientsConnectedToThisServer++;
	}

	//called when a connected client disconnects from this server
	public override void OnServerDisconnect (NetworkConnection conn)
	{
		Debug.Log("On client disconnected from this server with address["+conn.address+"]");
		clientsConnectedToThisServer--;
	}

	public override void OnClientError (NetworkConnection conn, int errorCode)
	{
		base.OnClientError (conn, errorCode);
		Debug.Log("OnClientError Called ["+conn.connectionId+"]["+errorCode+"]");
	}

	public override void OnClientNotReady (NetworkConnection conn)
	{
		base.OnClientNotReady (conn);
		Debug.Log("OnClientNotReady Called ["+conn.connectionId+"]");
	}

	public override void OnDropConnection (bool success, string extendedInfo)
	{
		base.OnDropConnection (success, extendedInfo);
		Debug.Log("OnDropConnection Called ["+success+"]["+extendedInfo+"]");
	}

	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
	{
		base.OnServerAddPlayer (conn, playerControllerId);
		Debug.Log("OnServerAddPlayer Called ["+conn.connectionId+"]["+playerControllerId+"]");
	}

	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
	{
		base.OnServerAddPlayer (conn, playerControllerId, extraMessageReader);
		Debug.Log("OnServerAddPlayer Called ["+conn.connectionId+"]["+playerControllerId+"]["+extraMessageReader.ReadNetworkIdentity().assetId+"]");
	}

	public override void OnServerError (NetworkConnection conn, int errorCode)
	{
		base.OnServerError (conn, errorCode);
		Debug.Log("OnServerError Called ["+conn.connectionId+"]["+errorCode+"]");
		isServer = false;
	}

	public override void OnServerReady (NetworkConnection conn)
	{
		base.OnServerReady (conn);
		Debug.Log("OnServerReady Called ["+conn.connectionId+"]");
	}

	public override void OnServerRemovePlayer (NetworkConnection conn, PlayerController player)
	{
		base.OnServerRemovePlayer (conn, player);
		Debug.Log("OnServerRemovePlayer Called ["+conn.connectionId+"]["+player.playerControllerId+"]");
		NetworkServer.DestroyPlayersForConnection(conn);
	}

	public override void OnStartClient (NetworkClient client)
	{
		base.OnStartClient (client);
		Debug.Log("OnStartClient Called");
	}

	public override void OnStartHost ()
	{
		base.OnStartHost ();
		Debug.Log("OnStartHost Called");
		isServer = true;
	}

	public override void OnStartServer ()
	{
		base.OnStartServer ();
		Debug.Log("OnStartServer Called");
	}

	public override void OnStopClient ()
	{
		base.OnStopClient ();
		Debug.Log("OnStopClient Called");
	}

	public override void OnStopHost ()
	{
		base.OnStopHost ();
		Debug.Log("OnStopHost Called");
		isServer = false;
	}

	public override void OnStopServer ()
	{
		base.OnStopServer ();
		Debug.Log("OnStopServer Called");
	}
		

}
