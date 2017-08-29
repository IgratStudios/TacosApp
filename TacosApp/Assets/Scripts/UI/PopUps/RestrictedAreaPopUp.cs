using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RestrictedAreaPopUp : UIScreen 
{
	public delegate void PasswordValidated(bool result);
	public static PasswordValidated OnPasswordValidated;

	public bool isSuperAccess = false;
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
		EventSystem.current.SetSelectedGameObject(passwordField.gameObject, null);
		passwordField.OnPointerClick(new PointerEventData(EventSystem.current));
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
		bool result = false;
		if (isSuperAccess) 
		{
			result = ClientProfileManager.GetProfileManager ().IsSuperPasswordValid (passwordField.text);
		} 
		else 
		{
			result = ClientProfileManager.GetProfileManager ().IsPasswordValid (passwordField.text);
		}
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