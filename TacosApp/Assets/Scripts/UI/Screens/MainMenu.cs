using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : UIScreen 
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

	public void OnOrdersButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sOrdersScreen);
	}

	public void OnKitchenButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sKitchenScreen);
	}

	public void OnAdminButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sAdminScreen);
	}

}
