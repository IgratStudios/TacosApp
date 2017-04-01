using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShowDebugManager : Manager<ShowDebugManager> 
{
	private bool checkDone = false;
	private float timeToCheckForLife = 3.0f;

	public AudioListener listener;

	protected override void Awake()
	{
		base.Awake();
		if(isThisManagerValid)
		{
			if(_mustPersistSceneChange)
			{
				DontDestroyOnLoad(gameObject);
			}

			SetShowDebugOnChildrenManagers();

			if(listener != null)
			{
				//check if there is other listener in the scene
				AudioListener[] listeners = FindObjectsOfType<AudioListener>();
				if(listeners.Length > 1)
				{
					listener.enabled = false;
				}
			}
		}
	}

	void Update () 
	{
		if(!checkDone)
		{
			timeToCheckForLife -= Time.deltaTime;
			if(timeToCheckForLife <= 0)
			{
				timeToCheckForLife = 0.0f;
				checkDone = true;
				if(transform.childCount == 0)
				{
					Destroy(gameObject);
				}
			}
		}
	}
		
	private void SetShowDebugOnChildrenManagers()
	{
		List<Manager> managers = new List<Manager>();

		GetManagersInChildren(managers,transform);

		for(int i = 0; i < managers.Count; i++)
		{
			managers[i]._mustShowDebugInfo = _mustShowDebugInfo;
		}
	}

	private void GetManagersInChildren(List<Manager> list, Transform t)
	{
		if(t.childCount > 0)
		{
			for(int i = 0; i < t.childCount; i++)
			{
				GetManagersInChildren(list, t.GetChild(i));
			}
		}
		else
		{
			Manager manager = t.GetComponent<Manager>();
			if(manager != null)
			{
				list.Add(manager);
			}
		}
	}

}
