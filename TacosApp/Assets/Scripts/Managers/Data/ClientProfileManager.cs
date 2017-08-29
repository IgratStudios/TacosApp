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
	public string superPassword = "demo";

	protected override void fillDefaultData ()
	{
		currentData.password = defaultPassword;
		currentData.superPassword = superPassword;
	}


	public bool IsPasswordValid(string testPwd)
	{
		if(currentData != null)
		{
			return currentData.password == testPwd;
		}
		return false;
	}

	public bool IsSuperPasswordValid(string testPwd)
	{
		if(currentData != null)
		{
			return currentData.superPassword == testPwd;
		}
		return false;
	}

}