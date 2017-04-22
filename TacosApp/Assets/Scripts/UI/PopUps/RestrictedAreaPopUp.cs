using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestrictedAreaPopUp : UIScreen 
{
	public delegate void PasswordValidated(bool result);
	public static PasswordValidated OnPasswordValidated;

	private UIScreenManager screenManager;

	public InputField passwordField;
	public GameObject invalidPassword;

	public override void Activate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		if(screenManager == null)
		{
			screenManager = ForceGet<UIScreenManager>();
		}
		SwitchInvalidPassword(false);
		ResetPasswordField();
		base.Activate (screenChangeCallback);
	}

	public override void UpdateScreen (UIScreenController.ScreenUpdatedEventHandler screenUpdatedCallBack)
	{
		base.UpdateScreen (screenUpdatedCallBack);
	}

	public override void Deactivate (UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		ResetPasswordField();
		base.Deactivate (screenChangeCallback);
	}

	public void OnCloseButtonPressed()
	{
		ResetPasswordField();
		PopUpsManager.GetInstance().HidePopUpWithId(screenManager._uniqueScreenId);
	}

	public void OnPasswordEntered()
	{
		bool result = ClientProfileManager.GetProfileManager().IsPasswordValid(passwordField.text);
		if(result)
		{
			//close pop up
			PopUpsManager.GetInstance().HidePopUpWithId(screenManager._uniqueScreenId);
		}
		else
		{
			SwitchInvalidPassword(true);
			ResetPasswordField();
		}
		if(OnPasswordValidated != null)
		{
			OnPasswordValidated(result);
		}
	}

	public void OnInputEntered()
	{
		if(passwordField.text.Length > 0)
		{
			SwitchInvalidPassword(false);
		}
	}

	private void ResetPasswordField()
	{
		passwordField.text = string.Empty;
	}

	private void SwitchInvalidPassword(bool enable)
	{
		invalidPassword.SetActive(enable);
	}

}