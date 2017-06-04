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
		PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sConnecting,true);
		ConnectionManager.GetInstance().TryToStart(false,OnConnectionDone);
	}

	public void OnKitchenButtonPressed()
	{
		screenTryingToAccessId = ScreenIds.sKitchenScreen;
		PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sRestrictedAccess,true);

	}

	public void OnCashRegisterButtonPressed()
	{
		screenTryingToAccessId = ScreenIds.sCashRegisterScreen;
		PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sRestrictedAccess,true);
	}

	public void OnAdminButtonPressed()
	{
		screenTryingToAccessId = ScreenIds.sAdminScreen;
		PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sRestrictedAccess,true);
	}

	private void OnPasswordValidated(bool result)
	{
		if(!string.IsNullOrEmpty(screenTryingToAccessId) && result)
		{
			PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sConnecting,true);
			ConnectionManager.GetInstance().TryToStart(true,OnConnectionDone);
		}
	}

	private void OnConnectionDone()
	{
		UIManager.GetInstance().SwitchToScreenWithId(screenTryingToAccessId);
		PopUpsManager.GetInstance().HidePopUpWithId(PopUpIds.sConnecting);
	}
}
