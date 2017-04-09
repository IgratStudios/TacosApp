using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionManager : Manager<ConnectionManager> 
{
	public UNetworkManager networkManager;
	public UNetworkDiscovery networkDiscovery;
	public string clientId = "TacosClientApp";

}
