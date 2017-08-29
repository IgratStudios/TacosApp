using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreLogin : UIScreen 
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
		screenTryingToAccessId = ScreenIds.sMenuScreen;
		if(!ConnectionManager.GetInstance().IsServer() && !ConnectionManager.GetInstance().IsClient())
		{
			PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sConnecting,true);
			ConnectionManager.GetInstance().TryToStart(false,OnConnectionDone);
		}
		else
		{
			UIManager.GetInstance().SwitchToScreenWithId(screenTryingToAccessId);
		}

	}

	public void OnMainAdminButtonPressed()
	{
		screenTryingToAccessId = ScreenIds.sMainMenuScreen;
		if(ConnectionManager.GetInstance().IsServer())
		{
			UIManager.GetInstance().SwitchToScreenWithId(screenTryingToAccessId);
		}
		else if(!ConnectionManager.GetInstance().IsClient())
		{
			PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sRestrictedAccess, true);
		}
	}

    public void OnSecondaryAdminButtonPressed()
    {
        screenTryingToAccessId = ScreenIds.sMainMenuScreen;
        if (!ConnectionManager.GetInstance().IsServer() && !ConnectionManager.GetInstance().IsClient())
        {
            PopUpsManager.GetInstance().ShowPopUpWithId(PopUpIds.sConnecting, true);
            ConnectionManager.GetInstance().TryToStart(false, OnConnectionDone);
        }
        else
        {
            UIManager.GetInstance().SwitchToScreenWithId(screenTryingToAccessId);
        }
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