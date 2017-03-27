using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Pop ups manager that works with the UIManager system.
/// </summary>
public class PopUpsManager : Manager<PopUpsManager> 
{
	/// <summary>
	/// Pop up status changed event handler. Sends whether this pop up changed to active or inactive.
	/// </summary>
	public delegate void PopUpStatusChangedEventHandler(string popupId,bool enabled);
	/// <summary>
	/// The on pop up status changed.
	/// </summary>
	public static PopUpStatusChangedEventHandler OnPopUpStatusChanged;

	public delegate void AnyPopUpActiveChangedEventHandler(bool anyPopUpActive);

	public static AnyPopUpActiveChangedEventHandler OnAnyPopUpActiveChanged;

	public static bool _isAnyPopUpActive = false;

	public PopUpsDisponibilityProvider _disponibilityProvider;

	/// <summary>
	/// The minimum camera depth used for all pop ups, it will be override by the max depth in the UIScreens.
	/// </summary>
	public float _minCameraDepth = 50.0f;
	/// <summary>
	/// The current camera depth the pop ups have.
	/// </summary>
	private float _currentCameraDepth = 0.0f;
	/// <summary>
	/// The just disable on pop up deactivation instead of destroying it.
	/// </summary>
	public bool _justDisableOnPopUpDeactivation = true;
	/// <summary>
	/// All pop up controllers.
	/// </summary>
	public List<UIScreenController>	_allPopUpControllers;
	/// <summary>
	/// All pop up backgrounds controllers.
	/// </summary>
	public List<UIScreenController>	_allPopUpBackgroundsControllers;
	/// <summary>
	/// The fast access pop up controllers including backgrounds.
	/// </summary>
	private Dictionary<string,UIScreenController> _fastAccessPopUpControllers;
	/// <summary>
	/// The canvas scaler reference resolution to use when creatin a new pop up.
	/// </summary>
	public Vector2 	_canvasScalerReferenceResolution = new Vector2(800,600);
	/// <summary>
	/// The canvas scaler mode.
	/// </summary>
	public CanvasScaler.ScaleMode _canvasScalerMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
	/// <summary>
	/// The canvas scaler screen match mode.
	/// </summary>
	public CanvasScaler.ScreenMatchMode _canvasScalerScreenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
	/// <summary>
	/// The canvas match mode range.
	/// </summary>
	[Range(0,1)]
	public float _canvasMatchModeRange = 0.5f;
	/// <summary>
	/// The canvas scaler pixels per unit.
	/// </summary>
	public int _canvasScalerPixelsPerUnit = 100;
	/// <summary>
	/// The popups position number of columns.
	/// </summary>
	public int	_popupsPositionNumberOfColumns = 3;
	/// <summary>
	/// The add help frame to newly created pop ups.
	/// </summary>
	public bool _addHelpFrameToCreatedPopUps = true;
	/// <summary>
	/// The pop up help frame prefab.
	/// </summary>
	public GameObject	_popUpHelpFramePrefab;
	/// <summary>
	/// The pop up bkg help frame prefab.
	/// </summary>
	public GameObject	_popUpBkgHelpFramePrefab;
	/// <summary>
	/// The pop up screen separation offset.
	/// </summary>
	public Vector2	_screenSeparation = new Vector2(19,11);//screen width with max aspect ratio(19),screen height with max aspect ratio(11)
	/// <summary>
	/// The system layer.
	/// </summary>
	public LayerMask	_systemLayer = 0;

	public List<string> _popUpQueue;

	public List<string> _screenProhibitedForPopUps = new List<string>();

	/// <summary>
	/// Awake this instance and register the instance in this gameObject.
	/// </summary>
	protected override void Awake ()
	{
		base.Awake();
		_popUpQueue = new List<string>();
		if(_disponibilityProvider == null)
		{
			_disponibilityProvider = GetComponent<PopUpsDisponibilityProvider>();
		}
		RegisterControllers();
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	public override void StartManager()
	{
		if(alreadystarted)
			return;
		base.StartManager();

		_minCameraDepth = Mathf.Max(_minCameraDepth,UIManager.GetBiggestCameraDepth());
		SetCameraDepth(_minCameraDepth);
		SwitchAllPopUps(false);
	}

	void CheckIfAnyPopUpActiveChanged()
	{
		if(!Application.isPlaying)
			return;

		bool currentStatus = false;
		for(int i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(_allPopUpControllers[i]._isActive)
			{
				currentStatus = true;
				break;
			}
		}

		if(_isAnyPopUpActive != currentStatus)
		{
			_isAnyPopUpActive = currentStatus;
			if(OnAnyPopUpActiveChanged != null)
			{
				OnAnyPopUpActiveChanged(_isAnyPopUpActive);
			}
		}
	}

	/// <summary>
	/// Sets the camera depth.
	/// </summary>
	/// <param name="newDepth">New depth.</param>
	void SetCameraDepth(float newDepth)
	{
		if(newDepth >= _minCameraDepth && newDepth <= 100)
		{
			_currentCameraDepth = newDepth;
		}
	}

	/// <summary>
	/// Updates the camera depth.
	/// </summary>
	void UpdateCameraDepth()
	{
		float minCurrentCameraDepth = _minCameraDepth;
		for(int  i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(_allPopUpControllers[i]._isActive)
			{
				float depth = _allPopUpControllers[i].GetCameraDepth();
				if(depth > minCurrentCameraDepth)
				{
					minCurrentCameraDepth = depth;
				}
			}
		}
		for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			if(_allPopUpBackgroundsControllers[i]._isActive)
			{
				float depth = _allPopUpBackgroundsControllers[i].GetCameraDepth();
				if(depth > minCurrentCameraDepth)
				{
					minCurrentCameraDepth = depth;
				}
			}
		}

		SetCameraDepth(minCurrentCameraDepth);
	}
		
	/// <summary>
	/// Registers the controllers.
	/// </summary>
	void RegisterControllers()
	{
		_fastAccessPopUpControllers = new Dictionary<string, UIScreenController>();
		for(int  i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(!_fastAccessPopUpControllers.ContainsKey(_allPopUpControllers[i]._uiUniqueId))
			{
				_fastAccessPopUpControllers.Add(_allPopUpControllers[i]._uiUniqueId,_allPopUpControllers[i]);
			}
			else
			{
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Theres already a UIPopUpScreen with Id["+_allPopUpControllers[i]._uiUniqueId+"]");
				}
			}
		}
		for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			if(!_fastAccessPopUpControllers.ContainsKey(_allPopUpBackgroundsControllers[i]._uiUniqueId))
			{
				_fastAccessPopUpControllers.Add(_allPopUpBackgroundsControllers[i]._uiUniqueId,_allPopUpBackgroundsControllers[i]);
			}
			else
			{
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Theres already a UIPopUpScreen Background with Id["+_allPopUpBackgroundsControllers[i]._uiUniqueId+"]");
				}
			}
		}
	}
		
	/// <summary>
	/// Gets the biggest camera depth.
	/// </summary>
	/// <returns>The biggest camera depth.</returns>
	public static float GetBiggestCameraDepth()
	{
		//Min value for camera depth in Unity
		float biggestCameraDepth = -100;
		if(_cachedInstance != null)
		{
			for(int i = 0; i < _cachedInstance._allPopUpControllers.Count; i++)
			{
				float tempDepth = _cachedInstance._allPopUpControllers[i].GetCameraDepth();
				if(tempDepth > biggestCameraDepth)
				{
					tempDepth = biggestCameraDepth;
				}
			}
		}
		return biggestCameraDepth;
	}

	private bool IsPopUpWithIdSpecial(string popUpId)
	{
		if(_fastAccessPopUpControllers != null)
		{
			UIScreenController popUpController;
			if(_fastAccessPopUpControllers.TryGetValue(popUpId,out popUpController))
			{
				return popUpController._isSpecialPopUp;
			}
		}
		else
		{
			int popUpIndex = -1;
			for(int  i = 0; i < _allPopUpControllers.Count; i++)
			{
				if(popUpId == _allPopUpControllers[i]._uiUniqueId)	
				{
					popUpIndex = i;
					break;
				}
			}
			if(popUpIndex >= 0)
			{
				return _allPopUpControllers[popUpIndex]._isSpecialPopUp;
			}
		}
		return false;
	}

	public void QueueOrShowPopUpWithId(string popUpId)
	{
		bool canShow = true;
		if(_disponibilityProvider != null)
		{
			canShow = _disponibilityProvider.CanShowNonSpecialPopUps() || IsPopUpWithIdSpecial(popUpId);
		}
			
		//second check if there is any pop up active
		//if it is not, show this pop up
		if(!_isAnyPopUpActive && canShow)
		{
			ShowPopUpWithId(popUpId);
		}
		//else, add to queue if it is not already active
		else if(!_popUpQueue.Contains(popUpId))
		{
			if(!IsPopUpActive(popUpId))
			{
				_popUpQueue.Add(popUpId);
			}
		}
	}

	public bool IsPopUpWithIdAlreadyQueued(string popUpId)
	{
		for(int i = 0; _popUpQueue.Count > 0; i++)
		{
			if(_popUpQueue[i] == popUpId)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Shows the pop up with identifier.
	/// </summary>
	/// <param name="popUpId">Pop up identifier.</param>
	/// <param name="closeOtherPopUps">If set to <c>true</c> close other pop ups.</param>
	public void ShowPopUpWithId(string popUpId,bool closeOtherPopUps = false)
	{
		StartCoroutine(ShowPopUpIfAble(popUpId,closeOtherPopUps));
	}

	private IEnumerator ShowPopUpIfAble(string popUpId,bool closeOtherPopUps)
	{
		while(!IsPopUpAllowed())
		{
			yield return 0;
			if(_mustShowDebugInfo)
			{
				Debug.Log("Wait for blocking screen before showing popup with id["+popUpId+"]");
			}
		}

		if(_fastAccessPopUpControllers != null)
		{
			UIScreenController popUpController;
			if(_fastAccessPopUpControllers.TryGetValue(popUpId,out popUpController))
			{
				SwitchPopUpBackgroundsForPopUp(popUpController,true);
				//activates pop up
				SetCameraDepth(_currentCameraDepth + 1);
				popUpController.SetCameraDepth(_currentCameraDepth);
				if(popUpController.Switch(true))
				{
					if(OnPopUpStatusChanged != null)
					{
						OnPopUpStatusChanged(popUpController._uiUniqueId,true);
					}
					CheckIfAnyPopUpActiveChanged();
				}
				if(closeOtherPopUps)
				{
					HideAllBut(popUpId);
					HideAllBackgroundsUnrelated(popUpController);

				}
			}
		}
		else
		{
			int indexToSwitchOn = -1;
			for(int  i = 0; i < _allPopUpControllers.Count; i++)
			{
				if(popUpId == _allPopUpControllers[i]._uiUniqueId)	
				{
					indexToSwitchOn = i;
					break;
				}
			}
			if(indexToSwitchOn >= 0)
			{
				SwitchPopUpBackgroundsForPopUp(_allPopUpControllers[indexToSwitchOn],true);
				//activates pop up
				SetCameraDepth(_currentCameraDepth + 1);
				_allPopUpControllers[indexToSwitchOn].SetCameraDepth(_currentCameraDepth);
				if(_allPopUpControllers[indexToSwitchOn].Switch(true))
				{
					if(OnPopUpStatusChanged != null)
					{
						OnPopUpStatusChanged(_allPopUpControllers[indexToSwitchOn]._uiUniqueId,true);
					}
					CheckIfAnyPopUpActiveChanged();
				}
			}
		}
	}

	private bool IsPopUpAllowed()
	{
		for(int i = 0; i < _screenProhibitedForPopUps.Count; i++)
		{
			if(UIManager.IsScreenWithIdActive(_screenProhibitedForPopUps[i]))
			{
				return false;
			}
		}
		return true;
	}

	private void HideAllBut(string popUpId)
	{
		for(int  i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(popUpId != _allPopUpControllers[i]._uiUniqueId)	
			{
				_allPopUpControllers[i].Switch(false, _justDisableOnPopUpDeactivation || !Application.isPlaying);
				SwitchPopUpBackgroundsForPopUp(_allPopUpControllers[i],false);
			}
		}
	}

	private void HideAllBackgroundsUnrelated(UIScreenController popUp)
	{
		for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			//if it is unrelated
			if(!popUp.HaveComplementScreenWithId(_allPopUpBackgroundsControllers[i]._uiUniqueId))
			{
				_allPopUpBackgroundsControllers[i].Switch(false, _justDisableOnPopUpDeactivation || !Application.isPlaying);
			}
		}
	}

	/// <summary>
	/// Hides the pop up with identifier.
	/// </summary>
	/// <param name="popUpId">Pop up identifier.</param>
	public void HidePopUpWithId(string popUpId)
	{
		if(_fastAccessPopUpControllers != null)
		{
			UIScreenController popUpController;
			if(_fastAccessPopUpControllers.TryGetValue(popUpId,out popUpController))
			{
				SwitchPopUpBackgroundsForPopUp(popUpController,false);
				//Deactivates pop up
				if(popUpController.Switch(false,_justDisableOnPopUpDeactivation))
				{
					if(OnPopUpStatusChanged != null)
					{
						OnPopUpStatusChanged(popUpController._uiUniqueId,false);
					}
					UpdateCameraDepth();
					CheckIfAnyPopUpActiveChanged();
				}
			}
		}
		else
		{
			int indexToSwitchOn = -1;
			for(int  i = 0; i < _allPopUpControllers.Count; i++)
			{
				if(popUpId == _allPopUpControllers[i]._uiUniqueId)	
				{
					indexToSwitchOn = i;
					break;
				}
			}
			if(indexToSwitchOn >= 0)
			{
				SwitchPopUpBackgroundsForPopUp(_allPopUpControllers[indexToSwitchOn],false);
				//deactivates pop up
				if(_allPopUpControllers[indexToSwitchOn].Switch(false,_justDisableOnPopUpDeactivation))
				{
					if(OnPopUpStatusChanged != null)
					{
						OnPopUpStatusChanged(_allPopUpControllers[indexToSwitchOn]._uiUniqueId,false);
					}
					UpdateCameraDepth();
					CheckIfAnyPopUpActiveChanged();
				}
			}
		}
	}

	/// <summary>
	/// Switchs the pop up backgrounds for pop up. A background can't be disable if a nother pop up is using it.
	/// </summary>
	/// <param name="controller">Controller.</param>
	/// <param name="enable">If set to <c>true</c> enable.</param>
	private void SwitchPopUpBackgroundsForPopUp(UIScreenController controller,bool enable)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Switching ["+controller._complementScreenIds.Count+"] PopUpBkgs for["+controller._uiUniqueId+"] to ["+enable+"]. Fast["+(_fastAccessPopUpControllers != null)+"]");
		}
		if(_fastAccessPopUpControllers != null)
		{
			for(int  i = 0; i < controller._complementScreenIds.Count; i++)
			{
				UIScreenController backgroundController;
				if(_fastAccessPopUpControllers.TryGetValue(controller._complementScreenIds[i],out backgroundController))
				{
					if(_mustShowDebugInfo)
					{
						Debug.Log("Try Switching PopUpBkg ["+backgroundController._uiUniqueId+"] to ["+enable+"]");
					}
					if(enable)
					{
						SetCameraDepth(_currentCameraDepth + 1);
						backgroundController.SetCameraDepth(_currentCameraDepth);
						backgroundController.Switch(true,_justDisableOnPopUpDeactivation || !Application.isPlaying);	
						if(!backgroundController.HaveComplementScreenWithId(controller._uiUniqueId))
						{
							backgroundController._complementScreenIds.Add(controller._uiUniqueId);
						}
					}
					else
					{
						if(backgroundController.HaveComplementScreenWithId(controller._uiUniqueId))
						{
							backgroundController._complementScreenIds.Remove(controller._uiUniqueId);
							if(_mustShowDebugInfo)
							{
								Debug.Log("After Removed["+backgroundController._complementScreenIds.Count+"]");
							}
						}
						if(backgroundController._complementScreenIds.Count == 0)
						{
							if(_mustShowDebugInfo)
							{
								Debug.Log("Effectively switching background controller to false");
							}
							if(backgroundController.Switch(false,_justDisableOnPopUpDeactivation || !Application.isPlaying))
							{	
								UpdateCameraDepth();
							}
						}
					}
				}
				else
				{
					if(_mustShowDebugInfo)
					{
						Debug.LogWarning("PopUpBkg ["+controller._complementScreenIds[i]+"] Not Founded");
					}
				}
			}
		}
		else
		{
			//Activates/deactivates all related backgrounds
			for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
			{
				if(controller.HaveComplementScreenWithId(_allPopUpBackgroundsControllers[i]._uiUniqueId))
				{
					if(_mustShowDebugInfo)
					{
						Debug.Log("Try Switching PopUpBkg ["+_allPopUpBackgroundsControllers[i]._uiUniqueId+"] to ["+enable+"]");
					}
					if(enable)
					{
						SetCameraDepth(_currentCameraDepth + 1);
						_allPopUpBackgroundsControllers[i].SetCameraDepth(_currentCameraDepth);
						_allPopUpBackgroundsControllers[i].Switch(true,_justDisableOnPopUpDeactivation || !Application.isPlaying);	
						if(!_allPopUpBackgroundsControllers[i].HaveComplementScreenWithId(controller._uiUniqueId))
						{
							_allPopUpBackgroundsControllers[i]._complementScreenIds.Add(controller._uiUniqueId);
						}
					}
					else
					{
						if(_allPopUpBackgroundsControllers[i].HaveComplementScreenWithId(controller._uiUniqueId))
						{
							_allPopUpBackgroundsControllers[i]._complementScreenIds.Remove(controller._uiUniqueId);
						}
						if(_allPopUpBackgroundsControllers[i]._complementScreenIds.Count == 0)
						{
							_allPopUpBackgroundsControllers[i].Switch(false,_justDisableOnPopUpDeactivation || !Application.isPlaying);	
							UpdateCameraDepth();
						}
					}
				}				
			}
		}
	}

	/// <summary>
	/// Updates the pop up by identifier.
	/// </summary>
	/// <param name="popUpId">Pop up identifier.</param>
	public void UpdatePopUpById(string popUpId)
	{
		UIScreenController controller;
		if(_fastAccessPopUpControllers.TryGetValue(popUpId,out controller))
		{
			controller.UpdateScreen();
			//Updates all related backgrounds
			for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
			{
				if(controller.HaveComplementScreenWithId(_allPopUpBackgroundsControllers[i]._uiUniqueId))
				{
					_allPopUpBackgroundsControllers[i].UpdateScreen();
				}
			}
		}
	}

	/// <summary>
	/// Switchs all pop ups.
	/// </summary>
	/// <param name="enable">If set to <c>true</c> enable.</param>
	public void SwitchAllPopUps(bool enable)
	{
		for(int  i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(_allPopUpControllers[i].Switch(enable, _justDisableOnPopUpDeactivation || !Application.isPlaying))
			{
				if(OnPopUpStatusChanged != null)
				{
					OnPopUpStatusChanged(_allPopUpControllers[i]._uiUniqueId,enable);
				}
			}
		}

		bool isQueuedBackground = false;
		UIScreenController queuedPopUp = null;
		if(_popUpQueue.Count > 0)
		{
			_fastAccessPopUpControllers.TryGetValue(_popUpQueue[0],out queuedPopUp);
			_popUpQueue.RemoveAt(0);
		}

		for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			if(queuedPopUp != null)
			{
				isQueuedBackground = queuedPopUp.HaveComplementScreenWithId(_allPopUpBackgroundsControllers[i]._uiUniqueId);
			}
			if(_allPopUpBackgroundsControllers[i].Switch(enable || (!enable && isQueuedBackground), _justDisableOnPopUpDeactivation || !Application.isPlaying))
			{
				if(OnPopUpStatusChanged != null)
				{
					OnPopUpStatusChanged(_allPopUpBackgroundsControllers[i]._uiUniqueId,enable);
				}
			}
		}
		if(queuedPopUp != null && !enable)
		{
			ShowPopUpWithId(queuedPopUp._uiUniqueId);
		}
		CheckIfAnyPopUpActiveChanged();
	}

	/// <summary>
	/// Updates all active pop ups.
	/// </summary>
	public void UpdateAllActivePopUps()
	{
		for(int  i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(_allPopUpControllers[i]._isActive)
			{
				UpdatePopUpById(_allPopUpControllers[i]._uiUniqueId);
			}
		}
	}

	/// <summary>
	/// Gets the UIScreen on the pop up with identifier.
	/// </summary>
	/// <returns>The user interface screen pop up for identifier.</returns>
	/// <param name="popUpId">Pop up identifier.</param>
	public UIScreen GetUIScreenPopUpForId(string popUpId)
	{
		UIScreenController controller;
		if(_fastAccessPopUpControllers.TryGetValue(popUpId,out controller))
		{
			return controller.GetCurrentUIScreen();
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("UIControllerNotFound by Id["+popUpId+"]");
			}
			return null;
		}
	}

	/// <summary>
	/// Gets the UIScreen on the pop up with identifier.
	/// </summary>
	/// <returns>The user interface screen pop up for identifier.</returns>
	/// <param name="popUpId">Pop up identifier.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T GetUIScreenPopUpForId<T>(string popUpId) where T : UIScreen
	{
		UIScreenController controller;
		if(_fastAccessPopUpControllers.TryGetValue(popUpId,out controller))
		{
			return controller.GetCurrentUIScreen<T>();
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("UIControllerNotFound by Id["+popUpId+"]");
			}
			return null;
		}
	}

	/// <summary>
	/// Gets the pop up position.
	/// </summary>
	/// <returns>The pop up position.</returns>
	/// <param name="popUpId">Pop up identifier.</param>
	public Vector3 GetPopUpPosition(string popUpId)
	{
		bool founded = false;
		Vector3 screenWorldPosition = Vector3.zero;
		for(int i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(_allPopUpControllers[i]._uiUniqueId == popUpId)
			{
				screenWorldPosition = _allPopUpControllers[i]._uiScreenPosition;
				founded = true;
				break;
			}
		}
		if(!founded)
		{
			screenWorldPosition = CalculateNewPopUpPositionInWorld(_allPopUpControllers.Count);
		}
		return screenWorldPosition;
	}

	/// <summary>
	/// Gets the pop up bkg position.
	/// </summary>
	/// <returns>The pop up bkg position.</returns>
	/// <param name="popUpBkgId">Pop up bkg identifier.</param>
	public Vector3 GetPopUpBkgPosition(string popUpBkgId)
	{
		bool founded = false;
		Vector3 screenWorldPosition = Vector3.zero;
		for(int i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			if(_allPopUpBackgroundsControllers[i]._uiUniqueId == popUpBkgId)
			{
				screenWorldPosition = _allPopUpBackgroundsControllers[i]._uiScreenPosition;
				founded = true;
				break;
			}
		}
		if(!founded)
		{
			screenWorldPosition = CalculateNewPopUpBkgPositionInWorld(_allPopUpControllers.Count);
		}
		return screenWorldPosition;
	}

	/// <summary>
	/// Resets all PopUps positions.
	/// </summary>
	public void ResetAllPositions()
	{
		for(int i = 0; i < _allPopUpControllers.Count; i++)
		{
			_allPopUpControllers[i].ResetPosition(CalculateNewPopUpPositionInWorld(i));
		}
		for(int i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			_allPopUpBackgroundsControllers[i].ResetPosition(CalculateNewPopUpBkgPositionInWorld(i));
		}
	}

	/// <summary>
	/// Registers to change event for pop up with identifier.
	/// </summary>
	/// <param name="popUpId">Pop up identifier.</param>
	/// <param name="listener">Listener.</param>
	public void RegisterToChangeEventForPopUpWithId(string popUpId,UIScreenController.ScreenChangedEventHandler listener)
	{
		UIScreenController controller;
		if(_fastAccessPopUpControllers.TryGetValue(popUpId,out controller))
		{
			controller.OnScreenStatusChanged += listener;
		}
	}

	/// <summary>
	/// Unregisters to change event for pop up with identifier.
	/// </summary>
	/// <param name="popUpId">Pop up identifier.</param>
	/// <param name="listener">Listener.</param>
	public void UnregisterToChangeEventForPopUpWithId(string popUpId,UIScreenController.ScreenChangedEventHandler listener)
	{
		UIScreenController controller;
		if(_fastAccessPopUpControllers.TryGetValue(popUpId,out controller))
		{
			controller.OnScreenStatusChanged -= listener;
		}
	}

	/// <summary>
	/// Registers to update event for pop up with identifier.
	/// </summary>
	/// <param name="popUpId">Pop up identifier.</param>
	/// <param name="listener">Listener.</param>
	public void RegisterToUpdateEventForPopUpWithId(string popUpId,UIScreenController.ScreenUpdatedEventHandler listener)
	{
		UIScreenController controller;
		if(_fastAccessPopUpControllers.TryGetValue(popUpId,out controller))
		{
			controller.OnScreenUpdated += listener;
		}
	}

	/// <summary>
	/// Unregisters to update event for pop up with identifier.
	/// </summary>
	/// <param name="popUpId">Pop up identifier.</param>
	/// <param name="listener">Listener.</param>
	public void UnregisterToUpdateEventForPopUpWithId(string popUpId,UIScreenController.ScreenUpdatedEventHandler listener)
	{
		UIScreenController controller;
		if(_fastAccessPopUpControllers.TryGetValue(popUpId,out controller))
		{
			controller.OnScreenUpdated -= listener;
		}
	}

	/// <summary>
	/// Determines whether the matched pop up is active.
	/// </summary>
	/// <returns><c>true</c> if this instance matching pop up is active; otherwise, <c>false</c>.</returns>
	/// <param name="popUpId">Pop up identifier.</param>
	public bool IsPopUpActive(string popUpId)
	{
		UIScreenController controller;
		if(_fastAccessPopUpControllers.TryGetValue(popUpId,out controller))
		{
			return controller._isActive;
		}
		return false;
	}

	/// <summary>
	/// Switchs the pop up by identifier.
	/// </summary>
	/// <param name="uniqueId">Unique identifier.</param>
	/// <param name="enable">If set to <c>true</c> enable.</param>
	public void SwitchById(string popUpId,bool enable,bool removeFromQueueIfFirst = false)
	{
		for(int  i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(_allPopUpControllers[i]._uiUniqueId == popUpId)
			{
				_allPopUpControllers[i].Switch(enable, _justDisableOnPopUpDeactivation || !Application.isPlaying);
			}
		}
		for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			if(_allPopUpBackgroundsControllers[i]._uiUniqueId == popUpId)
			{
				_allPopUpBackgroundsControllers[i].Switch(enable, _justDisableOnPopUpDeactivation || !Application.isPlaying);
			}
		}
		if(_popUpQueue.Count > 0 && removeFromQueueIfFirst)
		{
			//if it was the next pop up in the queue, remove it
			if(_popUpQueue[0] == popUpId)
			{
				_popUpQueue.RemoveAt(0);
			}
		}
		CheckIfAnyPopUpActiveChanged();
	}

	/// <summary>
	/// Switchs on the pop up with the passed id and switchs off the others.
	/// </summary>
	/// <param name="uniqueId">Unique identifier.</param>
	public void SwitchSolo(string uniqueId)
	{
		for(int  i = 0; i < _allPopUpControllers.Count; i++)
		{
			_allPopUpControllers[i].Switch(_allPopUpControllers[i]._uiUniqueId == uniqueId, _justDisableOnPopUpDeactivation || !Application.isPlaying);
		}
		for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			_allPopUpBackgroundsControllers[i].Switch(_allPopUpBackgroundsControllers[i]._uiUniqueId == uniqueId, _justDisableOnPopUpDeactivation || !Application.isPlaying);
		}
	}

	#region EDITOR HELPING FUNCTIONS
	//EDITOR HELPING FUNCTIONS
	/// <summary>
	/// Calculates the new pop up position in world.
	/// </summary>
	/// <returns>The new pop up position in world.</returns>
	/// <param name="screenIndex">Screen index.</param>
	Vector3 CalculateNewPopUpPositionInWorld(int screenIndex)
	{
		Vector3 finalPosition = Vector3.zero;
		int xCoord = screenIndex%_popupsPositionNumberOfColumns;
		int yCoord = screenIndex/_popupsPositionNumberOfColumns;
		finalPosition = new Vector3(-1*_screenSeparation.x*(xCoord+1), _screenSeparation.y*yCoord, 0);
		return finalPosition;
	}

	/// <summary>
	/// Calculates the new pop up bkg position in world.
	/// </summary>
	/// <returns>The new pop up bkg position in world.</returns>
	/// <param name="screenIndex">Screen index.</param>
	Vector3 CalculateNewPopUpBkgPositionInWorld(int screenIndex)
	{
		Vector3 finalPosition = Vector3.zero;
		int xCoord = screenIndex%_popupsPositionNumberOfColumns;
		int yCoord = screenIndex/_popupsPositionNumberOfColumns;
		finalPosition = new Vector3(-1*_screenSeparation.x*(xCoord+1), -1*_screenSeparation.y*yCoord, 0);
		return finalPosition;
	}

	/// <summary>
	/// Gets an unique identifier.
	/// </summary>
	/// <returns>The unique identifier.</returns>
	/// <param name="proposedIdComplement">Proposed identifier complement.</param>
	/// <param name="isPopUp">If set to <c>true</c> is pop up.</param>
	private string GetUniqueId(int proposedIdComplement,bool isPopUp = true)
	{
		string id = (isPopUp ? "UIPOP_":"UIPOPBG_")+proposedIdComplement;
		for(int i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(id == _allPopUpControllers[i]._uiUniqueId)
			{
				int newProposal = proposedIdComplement+1;
				id = GetUniqueId(newProposal,isPopUp);
			}
		}
		for(int i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			if(id == _allPopUpBackgroundsControllers[i]._uiUniqueId)
			{
				int newProposal = proposedIdComplement+1;
				id = GetUniqueId(newProposal,isPopUp);
			}
		}
		return id;
	}

	/// <summary>
	/// Creates a new UIScreenManager along with its controller for a pop up.
	/// </summary>
	/// <returns>The new user interface pop up screen manager.</returns>
	/// <param name="newPopUpId">New pop up identifier.</param>
	public GameObject CreateNewUIPopUpScreenManager(string newPopUpId = "")
	{
		int systemLayerValue = UIManager.GetUISystemLayer();
		if(systemLayerValue == 0)
		{
			systemLayerValue = _systemLayer.value;
		}
		if(_mustShowDebugInfo)
		{
			Debug.Log("Creating new UIPopUp with Layer["+LayerMask.LayerToName(systemLayerValue)+"]["+systemLayerValue+"]");
		}
		GameObject go = new GameObject((newPopUpId == "" ? GetUniqueId(_allPopUpControllers.Count) : newPopUpId));
		go.layer = systemLayerValue;
		Camera cam = go.AddComponent<Camera>();
		if(cam != null)
		{
			cam.orthographic = true;
			cam.orthographicSize = 5;
			cam.hdr = false;
			cam.useOcclusionCulling = true;
			cam.clearFlags = CameraClearFlags.Depth;
			cam.cullingMask = 1 << systemLayerValue;
			cam.farClipPlane = 200.0f;
			//add child Canvas
			GameObject canvasGO = new GameObject("Canvas");
			canvasGO.transform.SetParent(go.transform);
			canvasGO.layer = systemLayerValue;
			canvasGO.transform.localPosition = Vector3.zero;
			Canvas canvas = canvasGO.AddComponent<Canvas>();
			if(canvas != null)
			{
				canvas.renderMode = RenderMode.ScreenSpaceCamera;
				canvas.worldCamera = cam;
				canvas.planeDistance = 100;
				CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
				if(scaler != null)
				{
					scaler.uiScaleMode =_canvasScalerMode;
					scaler.screenMatchMode = _canvasScalerScreenMatchMode;
					scaler.matchWidthOrHeight = _canvasMatchModeRange;
					scaler.referenceResolution = _canvasScalerReferenceResolution;
					scaler.referencePixelsPerUnit = _canvasScalerPixelsPerUnit;
				}

				GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();
				if(raycaster != null)
				{
					raycaster.ignoreReversedGraphics = true;
					raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
				}

				UIScreenManager screenManager = go.AddComponent<UIScreenManager>();
				if(screenManager != null)
				{
					screenManager._isPopUp = true;
					Vector3 newPosition = CalculateNewPopUpPositionInWorld(_allPopUpControllers.Count);
					//create controller for this screenManager
					UIScreenController controller = new UIScreenController(go.name,null,screenManager,newPosition);
					_allPopUpControllers.Add(controller);
				}

				if(_addHelpFrameToCreatedPopUps && _popUpHelpFramePrefab != null)
				{
					GameObject helpFrame = GameObject.Instantiate(_popUpHelpFramePrefab);
					helpFrame.transform.SetParent(canvasGO.transform);
					RectTransform rect = helpFrame.GetComponent<RectTransform>();
					if(rect != null)
					{
						rect.anchorMin = Vector2.zero;
						rect.anchorMax = Vector2.one;
						rect.offsetMin = Vector2.zero;
						rect.offsetMax = Vector2.zero;
					}
					Text textId = helpFrame.GetComponentInChildren<Text>();
					if(textId != null)
					{
						textId.text = go.name;
					}
				}
			}
		}
		return go;
	}

	/// <summary>
	/// Creates a new UIScreenManager along with its controller for a pop up background.
	/// </summary>
	/// <returns>The new user interface pop up BKG screen manager.</returns>
	/// <param name="newPopUpBkgId">New pop up bkg identifier.</param>
	public GameObject CreateNewUIPopUpBKGScreenManager(string newPopUpBkgId = "")
	{
		int systemLayerValue = UIManager.GetUISystemLayer();
		if(systemLayerValue == 0)
		{
			systemLayerValue = _systemLayer.value;
		}
		if(_mustShowDebugInfo)
		{
			Debug.Log("Creating new UIPopUpBG");
		}
		GameObject go = new GameObject((newPopUpBkgId == "" ? GetUniqueId(_allPopUpBackgroundsControllers.Count,false) : newPopUpBkgId));
		go.layer = systemLayerValue;
		Camera cam = go.AddComponent<Camera>();
		if(cam != null)
		{
			cam.orthographic = true;
			cam.orthographicSize = 5;
			cam.hdr = false;
			cam.useOcclusionCulling = true;
			cam.clearFlags = CameraClearFlags.Depth;
			cam.cullingMask = 1 << systemLayerValue;
			cam.farClipPlane = 200.0f;
			//add child Canvas
			GameObject canvasGO = new GameObject("Canvas");
			canvasGO.transform.SetParent(go.transform);
			canvasGO.layer = systemLayerValue;
			canvasGO.transform.localPosition = Vector3.zero;
			Canvas canvas = canvasGO.AddComponent<Canvas>();
			if(canvas != null)
			{
				canvas.renderMode = RenderMode.ScreenSpaceCamera;
				canvas.worldCamera = cam;
				canvas.planeDistance = 100;
				CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
				if(scaler != null)
				{
					scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
					scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
					scaler.referenceResolution = _canvasScalerReferenceResolution;
					scaler.matchWidthOrHeight = 0.5f;
					scaler.referencePixelsPerUnit = 100;
				}

				GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();
				if(raycaster != null)
				{
					raycaster.ignoreReversedGraphics = true;
					raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
				}

				UIScreenManager screenManager = go.AddComponent<UIScreenManager>();
				if(screenManager != null)
				{
					screenManager._isPopUp = true;
					Vector3 newPosition = CalculateNewPopUpBkgPositionInWorld(_allPopUpControllers.Count);
					//create controller for this screenManager
					UIScreenController controller = new UIScreenController(go.name,null,screenManager,newPosition);
					_allPopUpBackgroundsControllers.Add(controller);
				}

				if(_addHelpFrameToCreatedPopUps && _popUpBkgHelpFramePrefab != null)
				{
					GameObject helpFrame = GameObject.Instantiate(_popUpBkgHelpFramePrefab);
					helpFrame.transform.SetParent(canvasGO.transform);
					RectTransform rect = helpFrame.GetComponent<RectTransform>();
					if(rect != null)
					{
						rect.anchorMin = Vector2.zero;
						rect.anchorMax = Vector2.one;
						rect.offsetMin = Vector2.zero;
						rect.offsetMax = Vector2.zero;
					}
					Text textId = helpFrame.GetComponentInChildren<Text>();
					if(textId != null)
					{
						textId.text = go.name;
					}
				}
			}
		}
		return go;
	}
		
	/// <summary>
	/// Gets an unique pop up identifier from a suggestion.
	/// </summary>
	/// <returns>The unique pop up identifier suggestion.</returns>
	/// <param name="currentId">Current identifier.</param>
	private string GetUniquePopUpIdFrom(string currentId)
	{
		for(int i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(currentId == _allPopUpControllers[i]._uiUniqueId)
			{
				currentId = GetUniqueId(_allPopUpControllers.Count);
			}
		}
		return currentId;
	}

	/// <summary>
	/// Gets an unique pop up bkg identifier from a suggestion.
	/// </summary>
	/// <returns>The unique pop up bkg identifier suggestion.</returns>
	/// <param name="currentId">Current identifier.</param>
	private string GetUniquePopUpBkgIdFrom(string currentId)
	{
		for(int i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			if(currentId == _allPopUpBackgroundsControllers[i]._uiUniqueId)
			{
				currentId = GetUniqueId(_allPopUpBackgroundsControllers.Count,false);
			}
		}
		return currentId;
	}

	/// <summary>
	/// Creates a new controller for a UIScreenManager pop up in scene.
	/// </summary>
	/// <returns>The new user interface pop up screen controller from scene object.</returns>
	/// <param name="screenManager">Screen manager.</param>
	public UIScreenManager CreateNewUIPopUpScreenControllerFromSceneObject(UIScreenManager screenManager)
	{
		bool alreadyExist = false;	
		for(int i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(_allPopUpControllers[i]._uiUniqueId == screenManager._uniqueScreenId)
			{
				alreadyExist = true;
				break;
			}
		}
		if(_mustShowDebugInfo)
		{
			Debug.Log("Creating new UIPopUpScreenController. AlreadyExist?["+alreadyExist+"]");
		}
		if(!alreadyExist)
		{
			string id = GetUniquePopUpIdFrom(screenManager._uniqueScreenId);
			Vector3 newPosition = CalculateNewPopUpPositionInWorld(_allPopUpControllers.Count);
			//create controller for this screenManager
			UIScreenController controller = new UIScreenController(id,null,screenManager,newPosition);
			_allPopUpControllers.Add(controller);
			screenManager._isPopUp = true;
			return screenManager;
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("A Controller with Id["+screenManager._uniqueScreenId+"] already exist! It cannot be created in this manager.");
			}
			return null;
		}
	}

	/// Creates a new controller for a UIScreenManager pop up background in scene.
	public UIScreenManager CreateNewUIPopUpScreenBkgControllerFromSceneObject(UIScreenManager screenManager)
	{
		bool alreadyExist = false;	
		for(int i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			if(_allPopUpBackgroundsControllers[i]._uiUniqueId == screenManager._uniqueScreenId)
			{
				alreadyExist = true;
				break;
			}
		}
		if(_mustShowDebugInfo)
		{
			Debug.Log("Creating new UIPopUpBkgScreenController. AlreadyExist?["+alreadyExist+"]");
		}
		if(!alreadyExist)
		{
			string id = GetUniquePopUpBkgIdFrom(screenManager._uniqueScreenId);
			Vector3 newPosition = CalculateNewPopUpBkgPositionInWorld(_allPopUpBackgroundsControllers.Count);
			//create controller for this screenManager
			UIScreenController controller = new UIScreenController(id,null,screenManager,newPosition);
			_allPopUpBackgroundsControllers.Add(controller);
			screenManager._isPopUp = true;
			return screenManager;
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("A Controller with Id["+screenManager._uniqueScreenId+"] already exist! It cannot be created in this manager.");
			}
			return null;
		}
	}

	/// <summary>
	/// Gets the lowest sibling index of all the pop ups.
	/// </summary>
	/// <returns>The last pop up sibling index.</returns>
	public int GetLastPopUpSiblingIndex()
	{
		int lastSiblingIndex = CachedTransform.GetSiblingIndex();
		for(int i = 0; i < _allPopUpControllers.Count; i++)
		{
			if(_allPopUpControllers[i]._uiScreenObject != null)
			{
				int newSiblingIndex = _allPopUpControllers[i]._uiScreenObject.CachedTransform.GetSiblingIndex();
				if(newSiblingIndex > lastSiblingIndex)
				{
					lastSiblingIndex = newSiblingIndex;
				}
			}
		}
		return lastSiblingIndex;
	}

	/// <summary>
	/// Gets the lowest sibling index of all the pop up backgrounds.
	/// </summary>
	/// <returns>The last pop up bkg sibling index.</returns>
	public int GetLastPopUpBkgSiblingIndex()
	{
		int lastSiblingIndex = CachedTransform.GetSiblingIndex();
		for(int i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			if(_allPopUpBackgroundsControllers[i]._uiScreenObject != null)
			{
				int newSiblingIndex = _allPopUpBackgroundsControllers[i]._uiScreenObject.CachedTransform.GetSiblingIndex();
				if(newSiblingIndex > lastSiblingIndex)
				{
					lastSiblingIndex = newSiblingIndex;
				}
			}
		}
		return lastSiblingIndex;
	}

	/// <summary>
	/// Removes the pop up passed from the PopUpsManager.
	/// </summary>
	/// <param name="popUpToRemove">Pop up to remove.</param>
	/// <param name="mustDestroyGameObject">If set to <c>true</c> must destroy game object.</param>
	public void RemoveUIPopUpScreenManager(UIScreenManager popUpToRemove,bool mustDestroyGameObject = true)
	{
		if(popUpToRemove != null)
		{
			int indexToRemove = -1;
			for(int  i = 0; i < _allPopUpControllers.Count; i++)
			{
				bool hasSameGO = _allPopUpControllers[i]._uiScreenObject == popUpToRemove;
				bool hasSameId = _allPopUpControllers[i]._uiUniqueId == popUpToRemove._uniqueScreenId;
				if(hasSameGO || hasSameId)
				{
					if(_mustShowDebugInfo)
					{
						Debug.Log("Removing UIPopUpScreenManager["+popUpToRemove.name+"] with Id["+popUpToRemove._uniqueScreenId+"] by Object?["+hasSameGO+"] by Id?["+hasSameId+"]");
					}
					indexToRemove = i;
					break;
				}
			}
			if(indexToRemove >= 0)
			{
				_allPopUpControllers.RemoveAt(indexToRemove);
				if(mustDestroyGameObject)
				{
					DestroyImmediate(popUpToRemove.gameObject);
				}
				ResetAllPositions();
			}
		}
	}

	/// <summary>
	/// Removes the pop up background passed from the PopUpsManager.
	/// </summary>
	/// <param name="popUpBkgToRemove">Pop up bkg to remove.</param>
	/// <param name="mustDestroyGameObject">If set to <c>true</c> must destroy game object.</param>
	public void RemoveUIPopUpBkgScreenManager(UIScreenManager popUpBkgToRemove,bool mustDestroyGameObject = true)
	{
		if(popUpBkgToRemove != null)
		{
			int indexToRemove = -1;
			for(int  i = 0; i < _allPopUpControllers.Count; i++)
			{
				bool hasSameGO = _allPopUpControllers[i]._uiScreenObject == popUpBkgToRemove;
				bool hasSameId = _allPopUpControllers[i]._uiUniqueId == popUpBkgToRemove._uniqueScreenId;
				if(hasSameGO || hasSameId)
				{
					if(_mustShowDebugInfo)
					{
						Debug.Log("Removing UIPopUpBkgScreenManager["+popUpBkgToRemove.name+"] with Id["+popUpBkgToRemove._uniqueScreenId+"] by Object?["+hasSameGO+"] by Id?["+hasSameId+"]");
					}
					indexToRemove = i;
					break;
				}
			}
			if(indexToRemove >= 0)
			{
				_allPopUpControllers.RemoveAt(indexToRemove);
				if(mustDestroyGameObject)
				{
					DestroyImmediate(popUpBkgToRemove.gameObject);
				}
				ResetAllPositions();
			}
		}
	}

	/// <summary>
	/// Removes the user interface pop up screen manager controller.
	/// </summary>
	/// <param name="uniqueId">Unique identifier.</param>
	/// <param name="justSetAsNull">If set to <c>true</c> just set as null and do not remove.</param>
	public void RemoveUIPopUpScreenManagerController(string uniqueId, bool justSetAsNull)
	{
		int indexToRemove = -1;
		for(int  i = 0; i < _allPopUpControllers.Count; i++)
		{
			bool hasSameId = _allPopUpControllers[i]._uiUniqueId == uniqueId;
			if(hasSameId)
			{
				if(_mustShowDebugInfo)
				{
					Debug.Log("Removing UIPopUpScreenManagerController with Id["+uniqueId+"]");
				}
				indexToRemove = i;
				break;
			}
		}
		if(indexToRemove >= 0)
		{
			if(justSetAsNull)
			{
				_allPopUpControllers[indexToRemove]._uiScreenObject = null;
			}
			else
			{
				_allPopUpControllers.RemoveAt(indexToRemove);
				ResetAllPositions();
			}
		}
	}

	/// <summary>
	/// Removes the user interface pop up bkg screen manager controller.
	/// </summary>
	/// <param name="uniqueId">Unique identifier.</param>
	/// <param name="justSetAsNull">If set to <c>true</c> just set as null and do not remove.</param>
	public void RemoveUIPopUpBkgScreenManagerController(string uniqueId, bool justSetAsNull)
	{
		int indexToRemove = -1;
		if(!justSetAsNull)
		{
			for(int  i = 0; i < _allPopUpControllers.Count; i++)
			{
				_allPopUpControllers[i].RemoveComplementId(uniqueId);
			}
		}
		for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			bool hasSameId = _allPopUpBackgroundsControllers[i]._uiUniqueId == uniqueId;
			if(hasSameId)
			{
				if(_mustShowDebugInfo)
				{
					Debug.Log("Removing UIPopUpBkgScreenManagerController with Id["+uniqueId+"]");
				}
				indexToRemove = i;
				break;
			}
		}
		if(indexToRemove >= 0)
		{
			if(justSetAsNull)
			{
				_allPopUpBackgroundsControllers[indexToRemove]._uiScreenObject = null;
			}
			else
			{
				_allPopUpBackgroundsControllers.RemoveAt(indexToRemove);
				ResetAllPositions();
			}
		}
	}

	/// <summary>
	/// Creates the new empty pop up controller alone.
	/// </summary>
	public void CreateNewEmptyPopUpControllerAlone()
	{
		//create controller for this screenManager
		Vector3 newPosition = CalculateNewPopUpPositionInWorld(_allPopUpControllers.Count);
		UIScreenController controller = new UIScreenController(GetUniqueId(_allPopUpControllers.Count,true),null,null,newPosition);
		_allPopUpControllers.Add(controller);
	}

	/// <summary>
	/// Creates the new empty pop up background controller alone.
	/// </summary>
	public void CreateNewEmptyPopUpBackgroundControllerAlone()
	{
		//create controller for this screenManager
		Vector3 newPosition = CalculateNewPopUpBkgPositionInWorld(_allPopUpBackgroundsControllers.Count);
		UIScreenController controller = new UIScreenController(GetUniqueId(_allPopUpBackgroundsControllers.Count,false),null,null,newPosition);
		_allPopUpBackgroundsControllers.Add(controller);
	}


	/// <summary>
	/// Reset this instance.
	/// </summary>
	public void Reset()
	{
		if(_allPopUpControllers != null)
		{
			while(_allPopUpControllers.Count > 0)
			{
				if(_allPopUpControllers[0]._uiScreenObject != null)
				{
					if(Application.isPlaying)
					{
						Destroy(_allPopUpControllers[0]._uiScreenObject.gameObject);
					}
					else
					{
						DestroyImmediate(_allPopUpControllers[0]._uiScreenObject.gameObject);
					}
				}
				_allPopUpControllers.RemoveAt(0);
			}
			_allPopUpControllers.Clear();
		}
		if(_allPopUpBackgroundsControllers != null)
		{
			while(_allPopUpBackgroundsControllers.Count > 0)
			{
				if(_allPopUpBackgroundsControllers[0]._uiScreenObject != null)
				{
					if(Application.isPlaying)
					{
						Destroy(_allPopUpBackgroundsControllers[0]._uiScreenObject.gameObject);
					}
					else
					{
						DestroyImmediate(_allPopUpBackgroundsControllers[0]._uiScreenObject.gameObject);
					}
				}
				_allPopUpBackgroundsControllers.RemoveAt(0);
			}
			_allPopUpBackgroundsControllers.Clear();
			ResetAllPositions();
		}
	}

	/// <summary>
	/// Despawns all pop ups on scene.
	/// </summary>
	public void DespawnAllPopUpsOnScene()
	{
		for(int  i = 0; i < _allPopUpControllers.Count; i++)
		{
			_allPopUpControllers[i].Switch(false);
		}
	}
	/// <summary>
	/// Despawns all pop up backgrounds on scene.
	/// </summary>
	public void DespawnAllPopUpBkgsOnScene()
	{
		for(int  i = 0; i < _allPopUpBackgroundsControllers.Count; i++)
		{
			_allPopUpBackgroundsControllers[i].Switch(false);
		}
	}


	#endregion


}
