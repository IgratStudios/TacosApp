using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreen : UIScreen 
{

	public override void Activate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
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

	public void OnBackButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sAdminScreen);
	}
}