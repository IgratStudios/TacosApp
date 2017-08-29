using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : UIScreen 
{

	private string screenTryingToAccessId = string.Empty;


	public override void Activate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		screenTryingToAccessId = string.Empty;
		RestrictedAreaPopUp.OnPasswordValidated += OnPasswordValidated;
		base.Activate (screenChangeCallback);
	}

	public override void UpdateScreen (UIScreenController.ScreenUpdatedEventHandler screenUpdatedCallBack)
	{
		base.UpdateScreen (screenUpdatedCallBack);
	}

	public override void Deactivate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		screenTryingToAccessId = string.Empty;
		RestrictedAreaPopUp.OnPasswordValidated -= OnPasswordValidated;
		base.Deactivate (screenChangeCallback);
	}

	public void OnOrdersButtonPressed()
	{
		screenTryingToAccessId = ScreenIds.sOrdersScreen;
		UIManager.GetInstance().SwitchToScreenWithId(screenTryingToAccessId);
	}

	public void OnMenuEditionButtonPressed()
	{
		screenTryingToAccessId = ScreenIds.sMenuEditionScreen;
		PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sRestrictedSuperAccess,true);

	}

	public void OnCashRegisterButtonPressed()
	{
//		screenTryingToAccessId = ScreenIds.sCashRegisterScreen;
//		PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sRestrictedAccess,true);
	}

	public void OnAdminButtonPressed()
	{
//		screenTryingToAccessId = ScreenIds.sAdminScreen;
//		PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sRestrictedAccess,true);
	}

	private void OnPasswordValidated(bool result)
	{
		if(!string.IsNullOrEmpty(screenTryingToAccessId) && result)
		{
			UIManager.GetInstance().SwitchToScreenWithId(screenTryingToAccessId);
			PopUpsManager.GetInstance().HidePopUpWithId(PopUpIds.sRestrictedSuperAccess);
		}
	}

	public void OnBackButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sPreLoginScreen);
	}

}
