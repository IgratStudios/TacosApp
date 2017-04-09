using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminScreen : UIScreen 
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
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sMainMenuScreen);
	}

	public void OnInventoryButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sInventoryScreen);
	}

	public void OnMenuButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sMenuScreen);
	}

	public void OnTablesButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sTablesScreen);
	}

	public void OnStatisticsButtonPressed()
	{
		UIManager.GetInstance().SwitchToScreenWithId(ScreenIds.sStatsScreen);
	}

	public void OnKitchenConfigButtonPressed()
	{
		
	}

	public void OnPasswordButtonPressed()
	{
		
	}


}
