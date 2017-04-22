using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TemplatePopUp : UIScreen 
{
	private UIScreenManager screenManager;


	public override void Activate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		if(screenManager == null)
		{
			screenManager = ForceGet<UIScreenManager>();
		}

		base.Activate (screenChangeCallback);
	}

	public override void UpdateScreen (UIScreenController.ScreenUpdatedEventHandler screenUpdatedCallBack)
	{
		base.UpdateScreen (screenUpdatedCallBack);
	}

	public override void Deactivate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		base.Deactivate (screenChangeCallback);
	}

	public void OnCloseButtonPressed()
	{
		PopUpsManager.GetInstance().HidePopUpWithId(screenManager._uniqueScreenId);
	}
		
}