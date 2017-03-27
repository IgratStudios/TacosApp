using UnityEngine;
using System.Collections;

public class ManagersHolder : MonoBehaviour 
{
	private bool checkDone = false;
	private float timeToCheckForLife = 3.0f;

	public AudioListener listener;

	void Awake()
	{
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
}
