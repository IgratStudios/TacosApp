using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class ClientProfileManager : LocalDataManager<ClientData>
{
	public static ClientProfileManager GetProfileManager()
	{
		return GetCastedInstance<ClientProfileManager>();
	}

	public string defaultPassword = "demo";

	protected override void fillDefaultData ()
	{
		base.fillDefaultData ();
		currentData.password = defaultPassword;
	}


	public bool IsPasswordValid(string testPwd)
	{
		if(currentData != null)
		{
			return currentData.password == testPwd;
		}
		return false;
	}

}