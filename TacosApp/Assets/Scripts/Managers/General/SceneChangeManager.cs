using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : Manager<SceneChangeManager> 
{

	public string startingSceneId = "Bootstrap";
	public string mainSceneId = "MainScene";
	public LoadSceneMode sceneChangeMode = LoadSceneMode.Single;

	protected override void Awake ()
	{
		base.Awake ();
		if(isThisManagerValid)
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
	}

	public override void StartManager()
	{
		base.StartManager();

		if(SceneManager.GetActiveScene().name == startingSceneId)
		{
			SceneManager.LoadScene(mainSceneId,sceneChangeMode);
		}
	}

	private void OnSceneLoaded(Scene scene,LoadSceneMode mode)
	{
		if(alreadystarted)
		{
			if(scene.name == startingSceneId)
			{
				SceneManager.LoadScene(mainSceneId,sceneChangeMode);
			}
		}
	}

}
