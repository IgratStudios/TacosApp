using UnityEngine;
using System.Collections;

/// <summary>
/// User interface screen. Base class for any screen controller script that must work with the UIManager system.
/// </summary>
public class UIScreen : CachedMonoBehaviour
{
	/// <summary>
	/// The must show debug info flag.
	/// </summary>
	public bool _mustShowDebugInfo = true;

	/// <summary>
	/// Determines whether this instance GameObject screen is active in hierarchy.
	/// </summary>
	/// <returns><c>true</c> if this screen GO is active in hierarchy; otherwise, <c>false</c>.</returns>
	public bool IsScreenGOActiveInHierarchy()
	{
		return CachedGameObject.activeInHierarchy;
	}

	/// <summary>
	/// Calls the specified screenChangeCallback.
	/// </summary>
	/// <param name="screenChangeCallback">Screen change callback.</param>
	public virtual void Activate(UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		if(screenChangeCallback != null && Application.isPlaying)
		{
			screenChangeCallback(true);
		}
	}

	/// <summary>
	/// Calls the specified screenChangeCallback.
	/// </summary>
	/// <param name="screenChangeCallback">Screen change callback.</param>
	public virtual void Deactivate(UIScreenController.ScreenChangedEventHandler screenChangeCallback)
	{
		if(screenChangeCallback != null && Application.isPlaying)
		{
			screenChangeCallback(false);
		}
	}

	/// <summary>
	/// Calls the specified screenUpdatedCallBack.
	/// </summary>
	/// <param name="screenUpdatedCallBack">Screen updated call back.</param>
	public virtual void UpdateScreen(UIScreenController.ScreenUpdatedEventHandler screenUpdatedCallBack)
	{
		if(screenUpdatedCallBack != null && Application.isPlaying)
		{
			screenUpdatedCallBack();
		}
	}
}
	
