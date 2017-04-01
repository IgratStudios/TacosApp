using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class AppManager : Manager<AppManager> 
{
	public string startingSceneId = "BootstrapScene";
	public string configurationSceneId = "ConfigurationScene";
	public string mainSceneId = "MainScene";
	public string jingleId = "IS_Jingle";
	public string mainMenuMusicId = "MainMenu";
	public float timeToWaitAfterGameMusicToLoadMainMenu = 3.0f;
	public string introScreenId = "Intro";
	public string mainMenuScreenId = "MainMenu";

	public int framesBetweenEachManagerInitialization = 1;
	public Manager[] mainManagers;
	public Manager[] secundaryManagers;

	private EventSystem uiInputSystem;
	private bool mustSwitchOffSFXAfterJingle = false;
	private bool waitingToPlayJingle = false;

	private bool alreadyPresentedTheMain = false;
	private bool sessionValidatedForTheFirstTime = false;

	private bool initializationFinished = false;

	protected override void Awake()
	{
		base.Awake();
		if(isThisManagerValid)
		{
			Debug.LogWarning("AWAKE OM META GAME MANAGER");
			SessionManager.OnSessionValidated += OnSessionValidated;
			StartIntroManagers();
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
	}

	private void OnSessionValidated()
	{
		//we are only interested on the first time
		SessionManager.OnSessionValidated -= OnSessionValidated;
		sessionValidatedForTheFirstTime = true;
		if(_mustShowDebugInfo)
		{
			Debug.Log("Session Validated for the first time in this session.");
		}
	}

	private void PlayJingle()
	{
		if(!waitingToPlayJingle)
		{
			StartCoroutine("PlayJingleWhenAble");
		}
	}

	IEnumerator PlayJingleWhenAble()
	{
		waitingToPlayJingle = true;
		while(AudioManager.GetInstance() == null)
		{
			yield return 0;
		}

		mustSwitchOffSFXAfterJingle = !PlayerPreferences.GetSettingStatus(PlayerPreferences.SETTINGS_TYPE.SFX);
		if(mustSwitchOffSFXAfterJingle)
		{
			AudioManager.GetInstance().SwitchCategory("SFX",true);
		}
		AudioManager.GetInstance().RegisterToItemStopped(jingleId,OnJingleFinished);
		AudioObject jingle = AudioManager.GetInstance().Play(jingleId);
		while(jingle == null)
		{
			yield return 0;
			jingle = AudioManager.GetInstance().Play(jingleId);
		}
		if(jingle != null)
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("Playing awesome Jingle  at ["+Time.realtimeSinceStartup+" with duration ["+( jingle.currentClip.clip != null ? jingle.currentClip.clip.length.ToString() : "NO CLIP")+"]");
			}
		}
		waitingToPlayJingle = false;
	}

	private void OnJingleFinished(string audioId)
	{
		if(audioId == jingleId)
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("Jingle finished, changing to MainScene at ["+Time.realtimeSinceStartup+"]");
			}
			AudioManager.GetInstance().UnregisterToItemStopped(jingleId,OnJingleFinished);
			if(mustSwitchOffSFXAfterJingle)
			{
				mustSwitchOffSFXAfterJingle = false;
				AudioManager.GetInstance().SwitchCategory("SFX",false);
			}

			if(AudioManager.GetInstance().IsCategoryEnabled("MUSIC"))
			{
				PlayMainMenuMusic(true);
			}
			else
			{
				Invoke("ChangeToConfigurationScene",timeToWaitAfterGameMusicToLoadMainMenu);
			}
		}
	}

	private void PlayMainMenuMusic(bool forTheFirstTime = false)
	{
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("Playing MainMenu Music! at ["+Time.realtimeSinceStartup+"]");
		}
		if(forTheFirstTime)
		{
			AudioManager.GetInstance().RegisterToItemStart(mainMenuMusicId,MainMenuMusicStartedForTheFirstTime);
		}
		AudioManager.GetInstance().Play(mainMenuMusicId);

	}

	public void PlayMainMenuMusic()
	{
		AudioManager.GetInstance().Play(mainMenuMusicId);
	}

	void MainMenuMusicStartedForTheFirstTime(string audioId)
	{
		if(audioId == mainMenuMusicId)
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("MainMenu Music started  at ["+Time.realtimeSinceStartup+"]");
			}
			AudioManager.GetInstance().UnregisterToItemStart(mainMenuMusicId,MainMenuMusicStartedForTheFirstTime);
			Invoke("ChangeToConfigurationScene",timeToWaitAfterGameMusicToLoadMainMenu);
			//ChangeToMainScene();
		}
	}

	void ChangeToConfigurationScene()
	{
		UIManager.GetInstance().SwitchScreenById(startingSceneId,false);
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("Start loading ["+configurationSceneId+"] Async! at ["+Time.realtimeSinceStartup+"]");
		}
		SceneManager.LoadSceneAsync(configurationSceneId,LoadSceneMode.Single);
	}

	void ChangeToMainScene()
	{
		UIManager.GetInstance().SwitchScreenById(startingSceneId,false);
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("Start loading ["+mainSceneId+"] Async! at ["+Time.realtimeSinceStartup+"]");
		}
		SceneManager.LoadSceneAsync(mainSceneId,LoadSceneMode.Single);
	}

	void PresentGame()
	{
		if(!alreadyPresentedTheMain)
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("Presenting Game");
			}
			StartSecundaryManagers();
			PresentMainMenu();
			alreadyPresentedTheMain = true;
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("SCENE LOADED ON METAGAMEMANAGER["+scene.name+"]");
		}
		if(initializationFinished)
		{
			if(scene.name == configurationSceneId)
			{
				ChangeToMainScene();
			}

			if(scene.name == mainSceneId)
			{
				PresentGame();
			}
		}
	}

	void StartIntroManagers()
	{
		if(mainManagers != null)
		{
			StartCoroutine("IntroManagersInitialization",framesBetweenEachManagerInitialization);
		}
	}

	IEnumerator IntroManagersInitialization(int framesBetweenEachInitialization)
	{
		int i = 0;
		int j = 0;
		for(i = 0; i < mainManagers.Length; i++)
		{
			for( j = 0; j < framesBetweenEachInitialization; j++ )
			{
				yield return 0;
			}
			mainManagers[i].StartManager();
		}
		if(_mustShowDebugInfo)
		{
			Debug.Log("Starting Managers Initialized!");
		}
		initializationFinished = true;
		if(SceneManager.GetActiveScene().name != mainSceneId)
		{
			PlayJingle();
		}
		else
		{
			AudioManager.GetInstance().Play(mainMenuMusicId);
			while(!sessionValidatedForTheFirstTime)
			{
				yield return 0;
			}
			PresentGame();
		}

	}

	void StartSecundaryManagers()
	{
		if(secundaryManagers != null)
		{
			StartCoroutine("SecundaryManagersInitialization",framesBetweenEachManagerInitialization);
		}
	}

	IEnumerator SecundaryManagersInitialization(int framesBetweenEachInitialization)
	{
		int i = 0;
		int j = 0;
		for(i = 0; i < secundaryManagers.Length; i++)
		{
			secundaryManagers[i].StartManager();
			for( j = 0; j < framesBetweenEachInitialization; j++ )
			{
				yield return 0;
			}
		}
		if(_mustShowDebugInfo)
		{
			Debug.Log("Secundary Managers Initialized!");
		}
		if(uiInputSystem == null)
		{
			uiInputSystem = FindObjectOfType<EventSystem>();
		}
	}

	public void PresentMainMenu()
	{
		UIManager.GetInstance().SwitchToScreenWithId(mainMenuScreenId);
	}

	public void EnableUIInput()
	{
		if(uiInputSystem != null)
		{
			uiInputSystem.enabled = true;	
		}
	}

	public void DisableUIInput()
	{
		if(uiInputSystem != null)
		{
			uiInputSystem.enabled = false;	
		}
	}

	public static string GetMainMenuScreenId()
	{
		if(_cachedInstance != null)
		{
			return _cachedInstance.mainMenuScreenId;
		}
		return string.Empty;
	}

}