using UnityEngine;
using System.Collections;

public class PlayerPreferences : Manager<PlayerPreferences>
{
	public enum SETTINGS_TYPE
	{
		MUSIC,
		SFX,
		NOTIFICATIONS,
		FACEBOOK,
		ADS,
		TUTORIAL
	}

	[System.Serializable]
	public struct Setting
	{
		public SETTINGS_TYPE settingType;
		public bool isEnabled;

		public void Save()
		{
			PlayerPrefs.SetInt(settingType.ToString(),(isEnabled?1:0));
		}

		public void Load()
		{
			isEnabled = PlayerPrefs.GetInt(settingType.ToString(),(isEnabled?1:0)) == 1;
		}
	}

	private AudioManager audioManager;
//	private LocalNotificationManager localNotificationsManager;


	public Setting musicSetting;
	public Setting sfxSetting;
	public Setting tutorialSetting;
	public Setting notificationsSetting;
	public Setting facebookSetting;
	public Setting adsSetting;

	public void SaveSettings()
	{
		musicSetting.Save();
		sfxSetting.Save();
		tutorialSetting.Save();
		notificationsSetting.Save();
		facebookSetting.Save();
		adsSetting.Save();
	}

	public void LoadSettings()
	{
		musicSetting.Load();
		sfxSetting.Load();
		tutorialSetting.Load();
		notificationsSetting.Load();
		//re-check notifications
		updateNotificationStatusFromManager();
		facebookSetting.Load();
		adsSetting.Load();
	}

	void updateNotificationStatusFromManager()
	{
		bool notificationsActive = false;//localNotificationsManager.isAnyNotificationTypeAvailable();
		if(Application.isEditor)
		{
			notificationsActive = notificationsSetting.isEnabled;
			SaveSettings();
		}
		if(notificationsSetting.isEnabled != notificationsActive)
		{
			notificationsSetting.isEnabled = notificationsActive;
			SaveSettings();
		}
	}

	void SwitchNotifications(bool enable)
	{
		if(!enable && notificationsSetting.isEnabled)
		{
			notificationsSetting.isEnabled = false;
			SaveSettings();
		}
		if(enable && !notificationsSetting.isEnabled)
		{
			//localNotificationsManager.registerNotificationTypes(true,true,true);
			if(Application.isEditor)
			{
				notificationsSetting.isEnabled = true;
				SaveSettings();
			}
			else
			{
				updateNotificationStatusFromManager();
			}
		}
		//else do nothing

	}

	public override void StartManager()
	{
		if(alreadystarted)
			return;
		base.StartManager();

		if(audioManager == null)
		{
			audioManager = AudioManager.GetInstance();
		}

//		if(localNotificationsManager == null)
//		{
//			localNotificationsManager = (LocalNotificationManager)LocalNotificationManager.GetInstance();
//		}

		LoadSettings();
		UpdateAudioManager();
	}

	void UpdateAudioManager()
	{
		audioManager.SwitchCategory("MUSIC",musicSetting.isEnabled);
		audioManager.SwitchCategory("SFX",sfxSetting.isEnabled);
		audioManager.SwitchCategory("LOOP_SFX",sfxSetting.isEnabled);
	}

	public void ChangeSetting(SETTINGS_TYPE sType)
	{
		switch(sType)
		{
		case SETTINGS_TYPE.MUSIC:
			musicSetting.isEnabled = !musicSetting.isEnabled;
			UpdateAudioManager();
			break;
		case SETTINGS_TYPE.SFX:
			sfxSetting.isEnabled = !sfxSetting.isEnabled;
			UpdateAudioManager();
			break;
		case SETTINGS_TYPE.NOTIFICATIONS:
			SwitchNotifications(!notificationsSetting.isEnabled);
			break;
		case SETTINGS_TYPE.FACEBOOK:
			//TODO:
			break;
		case SETTINGS_TYPE.ADS:
			adsSetting.isEnabled = !adsSetting.isEnabled;
			break;
		case SETTINGS_TYPE.TUTORIAL:
			tutorialSetting.isEnabled = !tutorialSetting.isEnabled;
			break;
		}
		SaveSettings();
		//update screen
		UIManager.GetInstance().UpdateAllActiveScreens();
	}

	public bool GetSettingOfTypeStatus(SETTINGS_TYPE sType)
	{
		switch(sType)
		{
		case SETTINGS_TYPE.MUSIC:
			return musicSetting.isEnabled;
		case SETTINGS_TYPE.SFX:
			return sfxSetting.isEnabled;
		case SETTINGS_TYPE.NOTIFICATIONS:
			return notificationsSetting.isEnabled;
		case SETTINGS_TYPE.FACEBOOK:
			return facebookSetting.isEnabled;
		case SETTINGS_TYPE.ADS:
			return adsSetting.isEnabled;
		case SETTINGS_TYPE.TUTORIAL:
			return tutorialSetting.isEnabled;
		default:
			return false;
		}
	}

	public static bool GetSettingStatus(SETTINGS_TYPE sType)
	{
		if(_cachedInstance != null)
		{
			return _cachedInstance.GetSettingOfTypeStatus(sType);
		}
		return false;
	}

	public static void ChangeSettingStatus(SETTINGS_TYPE sType)
	{
		if(_cachedInstance != null)
		{
			_cachedInstance.ChangeSetting(sType);
		}
	}
		
}