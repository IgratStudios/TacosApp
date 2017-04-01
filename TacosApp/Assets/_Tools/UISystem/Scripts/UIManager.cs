using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// User interface screen controller.
/// </summary>
[System.Serializable]
public class UIScreenController
{
	/// <summary>
	/// Screen changed event handler.It informs whether the screen is active or inactive.
	/// </summary>
	public delegate void ScreenChangedEventHandler(bool enabled);
	/// <summary>
	/// The on screen status changed.
	/// </summary>
	public ScreenChangedEventHandler OnScreenStatusChanged;
	/// <summary>
	/// Screen updated event handler.
	/// </summary>
	public delegate void ScreenUpdatedEventHandler();
	/// <summary>
	/// The on screen updated.
	/// </summary>
	public ScreenUpdatedEventHandler OnScreenUpdated;

	/// <summary>
	/// The user interface unique identifier.
	/// </summary>
	public string _uiUniqueId;
	/// <summary>
	/// The do not destroy on deactivation flag.
	/// </summary>
	public bool _doNotDestroyOnDeactivation = false;
	/// <summary>
	/// The is starting screen flag.
	/// </summary>
	public bool _isStartingScreen;
	/// <summary>
	/// The is active screen status flag.
	/// </summary>
	public bool _isActive;
	/// <summary>
	/// The is special pop up if it must skip the disponibility provider.
	/// </summary>
	public bool _isSpecialPopUp;
	/// <summary>
	/// The user interface screen prefab.
	/// </summary>
	public UIScreenManager _uiScreenPrefab;
	/// <summary>
	/// The current user interface screen object.
	/// </summary>
	public UIScreenManager	_uiScreenObject;
	/// <summary>
	/// The user interface screen position.
	/// </summary>
	public Vector3	_uiScreenPosition;
	/// <summary>
	/// The complement screen identifiers.
	/// </summary>
	public List<string> _complementScreenIds = new List<string>();
	/// <summary>
	/// Updates the complement identifiers.
	/// </summary>
	/// <param name="oldId">Old identifier.</param>
	/// <param name="newId">New identifier.</param>
	public void UpdateComplementIds(string oldId, string newId)
	{
		if(_complementScreenIds != null)
		{
			for(int i = 0; i < _complementScreenIds.Count; i++)
			{
				if(_complementScreenIds[i] == oldId)
				{
					_complementScreenIds[i] = newId;
				}
			}
		}
	}
	/// <summary>
	/// Removes the complement identifier.
	/// </summary>
	/// <param name="idToRemove">Identifier to remove.</param>
	public void RemoveComplementId(string idToRemove)
	{
		if(_complementScreenIds != null)
		{
			for(int i = 0; i < _complementScreenIds.Count; )
			{
				if(_complementScreenIds[i] == idToRemove)
				{
					Debug.Log("Removing ["+idToRemove+"] from ["+_uiUniqueId+"]");
					_complementScreenIds.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}
	}
	/// <summary>
	/// Switch the specified screen.
	/// </summary>
	/// <param name="enable">If set to <c>true</c> enable.</param>
	/// <param name="doNotDestroy">If set to <c>true</c> do not destroy on deactivation.</param>
	public bool Switch(bool enable,bool doNotDestroy = false,bool forceDestroy = false)
	{
		//Debug.Log("Switching ["+_uiUniqueId+"] to ["+enable+"] GralNotDestroy["+doNotDestroy+"] PartiNotDestroy["+_doNotDestroyOnDeactivation+"]");
		bool result = false;
		if(enable)
		{
			if(!_isActive)
			{
				//Debug.Log("UIScreenObj["+(_uiScreenObject == null)+"] uiPrefab["+(_uiScreenPrefab != null)+"]");
				if(_uiScreenObject == null && _uiScreenPrefab != null)
				{
					if(Application.isPlaying)
					{
						//Debug.LogWarning("Setting screen object to new instantiate");
						// Create uiScreen
						_uiScreenObject = GameObject.Instantiate<UIScreenManager>(_uiScreenPrefab);
					}
				}
				if(_uiScreenObject != null)
				{
					// Init uiScreen
					_uiScreenObject.SetPosition(_uiScreenPosition);
					_uiScreenObject.Init(OnScreenStatusChanged);
					result = true;
				}
			}
			else
			{
				//just update
				if(_uiScreenObject != null)
				{
					_uiScreenObject.UpdateScreen(OnScreenUpdated);
				}
			}
		}
		else
		{
			if(_uiScreenObject != null)
			{
				if(_uiScreenObject._mustShowDebugInfo)
				{
					Debug.Log("Switch OFF ["+_uiUniqueId+"] UIScreenObj["+(_uiScreenObject != null)+"] uiPrefab["+(_uiScreenPrefab != null)+"] destroy["+(!doNotDestroy)+"] forceDestroy["+forceDestroy+"]");
				}
				if(_isActive)
				{
					//Deactivate uiScreen
					_uiScreenObject.Deactivate(OnScreenStatusChanged);
				}

				if(_uiScreenPrefab != null && (!doNotDestroy || forceDestroy))
				{
					//Destroy uiScreen
					if(Application.isPlaying)
					{
						if(!_doNotDestroyOnDeactivation || forceDestroy)
						{
							GameObject.Destroy(_uiScreenObject.CachedGameObject);
							//Debug.LogWarning("Setting screen object to null");
							_uiScreenObject = null;
						}
					}
					else
					{
						GameObject.DestroyImmediate(_uiScreenObject.CachedGameObject);
						//Debug.LogWarning("Setting screen object to null");
						_uiScreenObject = null;
					}
				}
				result = _isActive;
			}
		}
		_isActive = enable;
		return result;
	}

	/// <summary>
	/// Updates the screen.
	/// </summary>
	public void UpdateScreen()
	{
		if(_isActive && _uiScreenObject != null)
		{
			_uiScreenObject.UpdateScreen(OnScreenUpdated);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UIScreenController"/> class.
	/// </summary>
	/// <param name="uniqueId">Unique identifier.</param>
	/// <param name="prefab">Prefab.</param>
	/// <param name="currentObject">Current object.</param>
	/// <param name="position">Position.</param>
	public UIScreenController(string uniqueId,UIScreenManager prefab,UIScreenManager currentObject,Vector3 position)
	{
		_uiUniqueId = uniqueId;
		_uiScreenPrefab = prefab;
		Debug.LogWarning("Setting screen object to ["+currentObject+"]");
		_uiScreenObject = currentObject;
		_uiScreenPosition = position;
		_isActive =  _uiScreenObject != null;
		if(_isActive)
		{
			_uiScreenObject._uniqueScreenId = _uiUniqueId;
			_uiScreenObject.SetPosition(_uiScreenPosition);
		}
	}

	/// <summary>
	/// Resets the position to a new one.
	/// </summary>
	/// <param name="newPositionInWorld">New position in world.</param>
	public void ResetPosition(Vector3 newPositionInWorld)
	{
		_uiScreenPosition = newPositionInWorld;
		if(_isActive)
		{
			_uiScreenObject.SetPosition(_uiScreenPosition);
		}
	}
	/// <summary>
	/// Gets the current user interface screen.
	/// </summary>
	/// <returns>The current user interface screen.</returns>
	public UIScreen GetCurrentUIScreen()
	{
		if(_uiScreenObject != null)
		{
			return _uiScreenObject.GetUIScreenInstance();
		}
		return null;
	}

	/// <summary>
	/// Gets the current user interface screen casting it to the T type passed.
	/// </summary>
	/// <returns>The current user interface screen.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T GetCurrentUIScreen<T>() where T : UIScreen
	{
		if(_uiScreenObject != null)
		{
			return _uiScreenObject.GetUIScreenInstance<T>();
		}
		return null;
	}
	/// <summary>
	/// Haves the complement screen with identifier.
	/// </summary>
	/// <returns><c>true</c>, if complement screen with identifier was had, <c>false</c> otherwise.</returns>
	/// <param name="otherScreenId">Other screen identifier.</param>
	public bool HaveComplementScreenWithId(string otherScreenId)
	{
		if(_complementScreenIds != null)
		{
			for(int i = 0; i < _complementScreenIds.Count; i++)
			{
				if(_complementScreenIds[i] == otherScreenId)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Gets the camera depth.
	/// </summary>
	/// <returns>The camera depth.</returns>
	public float GetCameraDepth()
	{
		if(_uiScreenObject != null)
		{
			return _uiScreenObject._screenCameraDepth;
		}
		else if(_uiScreenPrefab != null)
		{
			return _uiScreenPrefab._screenCameraDepth;
		}
		else
		{
			return -100;
		}
	}

	/// <summary>
	/// Sets the camera depth.
	/// </summary>
	/// <param name="newDepth">New depth.</param>
	public void SetCameraDepth(float newDepth)
	{
		if(_uiScreenPrefab != null)
		{
			_uiScreenPrefab._screenCameraDepth = newDepth;
		}
		if(_uiScreenObject != null)
		{
			_uiScreenObject.UpdateCameraDepth(newDepth);
		}
	}
}

/// <summary>
/// User interface manager.
/// </summary>
public class UIManager : Manager<UIManager> 
{
	/// <summary>
	/// Screen status changed event handler.
	/// </summary>
	public delegate void ScreenStatusChangedEventHandler(string screenId,bool enabled);
	/// <summary>
	/// The on screen status changed.
	/// </summary>
	public static ScreenStatusChangedEventHandler OnScreenStatusChanged;
	/// <summary>
	/// The just disable on screen deactivation flag, 
	/// if true the Gameobjects of this screen will not be destroyed after swithing it off.
	/// </summary>
	public bool 					_justDisableOnScreenDeactivation = true;
	/// <summary>
	/// All screen controllers.
	/// </summary>
	public List<UIScreenController>	_allScreenControllers;
	/// <summary>
	/// The fast access ditionary with the screen controllers.
	/// </summary>
	private Dictionary<string,UIScreenController> _fastAccessScreenControllers;
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
	/// The canvas scaler reference resolution used when creating new screens.
	/// </summary>
	public Vector2 	_canvasScalerReferenceResolution = new Vector2(800,600);
	/// <summary>
	/// The canvas scaler pixels per unit.
	/// </summary>
	public int _canvasScalerPixelsPerUnit = 100;

	/// <summary>
	/// The screen position number of columns.
	/// </summary>
	public int	_screenPositionNumberOfColumns = 3;
	/// <summary>
	/// The add help frame to created screens flag.
	/// </summary>
	public bool _addHelpFrameToCreatedScreens = true;
	/// <summary>
	/// The help frame prefab.
	/// </summary>
	public GameObject	_helpFramePrefab;
	/// <summary>
	/// The screen separation used to set screen positions.
	/// </summary>
	public Vector2	_screenSeparation = new Vector2(19,11);//screen width with max aspect ratio(19),screen height with max aspect ratio(11)
	/// <summary>
	/// The Unity's layer this system will work with.
	/// </summary>
	public LayerMask	_systemLayer = 0;
	/// <summary>
	/// The current screen identifier.
	/// </summary>
	private string _currentScreenId = string.Empty;

	private string _lastScreenId = string.Empty;

	/// <summary>
	/// Gets the current screen identifier.
	/// </summary>
	/// <value>The current screen identifier.</value>
	public string CurrentScreenId
	{
		get
		{
			return _currentScreenId;
		}
	}

	public string LastScreenId
	{
		get
		{
			return _lastScreenId;
		}
	}

	public static int GetUISystemLayer()
	{
		if(_cachedInstance != null)
		{
			return _cachedInstance._systemLayer.value;
		}
		else
		{
			//try to find the object
			UIManager instance = GameObject.FindObjectOfType<UIManager>();
			if(instance != null)
			{
				return instance._systemLayer.value;
			}
			else
			{
				return 0;
			}
		}
	}
		
	/// <summary>
	/// Awake this instance and register the instance in this gameObject.
	/// </summary>
	public override void StartManager()
	{
		base.StartManager();
		if(isThisManagerValid)
		{
			RegisterControllers();
			SwitchToScreenWithIndex(GetStartingScreenIndex());
		}
	}

	void OnDestroy()
	{
		if(Application.isPlaying)
		{
			SwitchAllScreens(false,true);
		}
	}

	/// <summary>
	/// Registers the controllers for fast access.
	/// </summary>
	void RegisterControllers()
	{
		_fastAccessScreenControllers = new Dictionary<string, UIScreenController>();
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			if(!_fastAccessScreenControllers.ContainsKey(_allScreenControllers[i]._uiUniqueId))
			{
				_fastAccessScreenControllers.Add(_allScreenControllers[i]._uiUniqueId,_allScreenControllers[i]);
			}
			else
			{
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Theres already a UIScreen with Id["+_allScreenControllers[i]._uiUniqueId+"]");
				}
			}
		}
	}

	/// <summary>
	/// Gets the index of the starting screen.
	/// </summary>
	/// <returns>The starting screen index.</returns>
	int GetStartingScreenIndex()
	{
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			if(_allScreenControllers[i]._isStartingScreen)
			{
				return i;
			}
		}
		return 0;
	}

	/// <summary>
	/// Get the biggest camera depth used by the screens registered.
	/// </summary>
	/// <returns>The biggest camera depth.</returns>
	public static float GetBiggestCameraDepth()
	{
		//Min value for camera depth in Unity
		float biggestCameraDepth = -100;
		if(_cachedInstance != null)
		{
			for(int i = 0; i < _cachedInstance._allScreenControllers.Count; i++)
			{
				float tempDepth = _cachedInstance._allScreenControllers[i].GetCameraDepth();
				if(tempDepth > biggestCameraDepth)
				{
					tempDepth = biggestCameraDepth;
				}
			}
		}
		return biggestCameraDepth;
	}

	/// <summary>
	/// Switchs to screen with the index passed.
	/// </summary>
	/// <param name="screenIndex">Screen index.</param>
	public void SwitchToScreenWithIndex(int screenIndex,bool registerLast = true)
	{
		string tempCurrent = _currentScreenId;
		if(screenIndex >= 0 && screenIndex < _allScreenControllers.Count)
		{
			//activates screen
			if(_allScreenControllers[screenIndex].Switch(true,_justDisableOnScreenDeactivation || !Application.isPlaying))
			{
				_currentScreenId = _allScreenControllers[screenIndex]._uiUniqueId;
				if(OnScreenStatusChanged != null && Application.isPlaying)
				{
					OnScreenStatusChanged(_allScreenControllers[screenIndex]._uiUniqueId,true);
				}
			}
			//deactivates all others
			for(int  i = 0; i < _allScreenControllers.Count; i++)
			{
				if(screenIndex != i)
				{
					if(_allScreenControllers[screenIndex].HaveComplementScreenWithId(_allScreenControllers[i]._uiUniqueId))
					{
						if(_allScreenControllers[i].Switch(true,_justDisableOnScreenDeactivation || !Application.isPlaying))
						{
							if(OnScreenStatusChanged != null && Application.isPlaying)
							{
								OnScreenStatusChanged(_allScreenControllers[i]._uiUniqueId,true);
							}
						}
							
					}
					else
					{
						if(_allScreenControllers[i].Switch(false,_justDisableOnScreenDeactivation || !Application.isPlaying))
						{
							if(_allScreenControllers[i]._uiScreenObject._mustRegisterForBackOperations)
							{
								_lastScreenId = tempCurrent;
							}
							if(OnScreenStatusChanged != null && Application.isPlaying)
							{
								OnScreenStatusChanged(_allScreenControllers[screenIndex]._uiUniqueId,false);
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Switchs to screen with identifier.
	/// </summary>
	/// <param name="screenId">Screen identifier.</param>
	public void SwitchToScreenWithId(string screenId)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Switching To Screen With Id["+screenId+"]");
		}
		int indexToSwitchOn = -1;
		string tempCurrent = _currentScreenId;
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			if(screenId == _allScreenControllers[i]._uiUniqueId)	
			{
				indexToSwitchOn = i;
				break;
			}
		}
		if(indexToSwitchOn >= 0)
		{
			//activates screen
			if(_allScreenControllers[indexToSwitchOn].Switch(true,_justDisableOnScreenDeactivation || !Application.isPlaying))
			{
				_currentScreenId = _allScreenControllers[indexToSwitchOn]._uiUniqueId;
				if(OnScreenStatusChanged != null && Application.isPlaying)
				{
					OnScreenStatusChanged(_allScreenControllers[indexToSwitchOn]._uiUniqueId,true);
				}
			}
			//deactivates all others
			for(int  i = 0; i < _allScreenControllers.Count; i++)
			{
				if(indexToSwitchOn != i)
				{
					if(_allScreenControllers[indexToSwitchOn].HaveComplementScreenWithId(_allScreenControllers[i]._uiUniqueId))
					{
						if(_allScreenControllers[i].Switch(true,_justDisableOnScreenDeactivation || !Application.isPlaying))
						{
							if(OnScreenStatusChanged != null && Application.isPlaying)
							{
								OnScreenStatusChanged(_allScreenControllers[i]._uiUniqueId,true);
							}
						}
					}
					else
					{
						if(_allScreenControllers[i].Switch(false,_justDisableOnScreenDeactivation || !Application.isPlaying))
						{
							if(_allScreenControllers[i]._uiScreenObject != null)
							{
								if(_allScreenControllers[i]._uiScreenObject._mustRegisterForBackOperations)
								{
									_lastScreenId = tempCurrent;
								}
								if(OnScreenStatusChanged != null && Application.isPlaying)
								{
									OnScreenStatusChanged(_allScreenControllers[i]._uiUniqueId,false);
								}
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Switchs the screen by identifier.
	/// </summary>
	/// <param name="screenId">Screen identifier.</param>
	/// <param name="enable">If set to <c>true</c> enable.</param>
	public void SwitchScreenById(string screenId,bool enable)
	{
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			if(screenId == _allScreenControllers[i]._uiUniqueId)
			{
				if(_allScreenControllers[i].Switch(enable, _justDisableOnScreenDeactivation || !Application.isPlaying))
				{
					if(OnScreenStatusChanged != null && Application.isPlaying)
					{
						OnScreenStatusChanged(_allScreenControllers[i]._uiUniqueId,enable);
					}
				}
				break;
			}
		}
	}

	/// <summary>
	/// Updates the screen by identifier.
	/// </summary>
	/// <param name="screenId">Screen identifier.</param>
	public void UpdateScreenById(string screenId)
	{
		UIScreenController controller;
		if(_fastAccessScreenControllers.TryGetValue(screenId,out controller))
		{
			controller.UpdateScreen();
		}
	}

	/// <summary>
	/// Switchs the screen with index.
	/// </summary>
	/// <param name="screenIndex">Screen index.</param>
	/// <param name="enable">If set to <c>true</c> enable.</param>
	public void SwitchScreenByIndex(int screenIndex,bool enable)
	{
		if(screenIndex >= 0 && screenIndex < _allScreenControllers.Count)
		{
			if(_allScreenControllers[screenIndex].Switch(enable, _justDisableOnScreenDeactivation || !Application.isPlaying))
			{
				if(OnScreenStatusChanged != null && Application.isPlaying)
				{
					OnScreenStatusChanged(_allScreenControllers[screenIndex]._uiUniqueId,enable);
				}
			}	
		}
	}

	/// <summary>
	/// Updates the screen by index.
	/// </summary>
	/// <param name="screenIndex">Screen index.</param>
	public void UpdateScreenByIndex(int screenIndex)
	{
		if(screenIndex >= 0 && screenIndex < _allScreenControllers.Count)
		{
			_allScreenControllers[screenIndex].UpdateScreen();
		}
	}

	/// <summary>
	/// Switchs all screens.
	/// </summary>
	/// <param name="enable">If set to <c>true</c> enable.</param>
	public void SwitchAllScreens(bool enable,bool forceDestroy = false)
	{
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			if(_allScreenControllers[i].Switch(enable,  _justDisableOnScreenDeactivation || !Application.isPlaying ,forceDestroy))
			{
				if(OnScreenStatusChanged != null && Application.isPlaying)
				{
					OnScreenStatusChanged(_allScreenControllers[i]._uiUniqueId,enable);
				}
			}
		}
	}

	/// <summary>
	/// Updates all active screens.
	/// </summary>
	public void UpdateAllActiveScreens()
	{
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			if(_allScreenControllers[i]._isActive)
			{
				_allScreenControllers[i].UpdateScreen();
			}
		}
	}

	/// <summary>
	/// Gets the UIScreen for the screen with identifier.
	/// </summary>
	/// <returns>The user interface screen for identifier.</returns>
	/// <param name="screenId">Screen identifier.</param>
	public UIScreen GetUIScreenForId(string screenId)
	{
		UIScreenController controller;
		if(_fastAccessScreenControllers.TryGetValue(screenId,out controller))
		{
			return controller.GetCurrentUIScreen();
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("UIControllerNotFound by Id["+screenId+"]");
			}
			return null;
		}
	}

	/// <summary>
	/// Gets the UI controller for identifier.
	/// </summary>
	/// <returns>The user interface controller for identifier.</returns>
	/// <param name="screenId">Screen identifier.</param>
	public UIScreenController GetUIControllerForId(string screenId)
	{
		UIScreenController controller;
		if(_fastAccessScreenControllers.TryGetValue(screenId,out controller))
		{
			return controller;
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("UIControllerNotFound by Id["+screenId+"]");
			}
			return null;
		}
	}

	/// <summary>
	/// Determines whether this instance is screen active the specified screenId.
	/// </summary>
	/// <returns><c>true</c> if this instance is screen active the specified screenId; otherwise, <c>false</c>.</returns>
	/// <param name="screenId">Screen identifier.</param>
	public bool IsScreenActive(string screenId)
	{
		UIScreenController controller;
		if(_fastAccessScreenControllers.TryGetValue(screenId,out controller))
		{
			return controller._isActive;
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("UIControllerNotFound by Id["+screenId+"]");
			}
			return false;
		}
	}

	public static bool IsScreenWithIdActive(string screenId)
	{
		if(_cachedInstance != null)
		{
			return _cachedInstance.IsScreenActive(screenId);
		}
		return false;
	}

	/// <summary>
	/// Gets the UIScreen casted to type T for the screen with identifier.
	/// </summary>
	/// <returns>The user interface screen for identifier.</returns>
	/// <param name="screenId">Screen identifier.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T GetUIScreenForId<T>(string screenId) where T : UIScreen
	{
		UIScreenController controller;
		if(_fastAccessScreenControllers.TryGetValue(screenId,out controller))
		{
			return controller.GetCurrentUIScreen<T>();
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("UIControllerNotFound by Id["+screenId+"]");
			}
			return null;
		}
	}

	/// <summary>
	/// Gets the position of this screen.
	/// </summary>
	/// <returns>The position.</returns>
	/// <param name="uniqueScreenId">Unique screen identifier.</param>
	public Vector3 GetPosition(string uniqueScreenId)
	{
		bool founded = false;
		Vector3 screenWorldPosition = Vector3.zero;
		for(int i = 0; i < _allScreenControllers.Count; i++)
		{
			if(_allScreenControllers[i]._uiUniqueId == uniqueScreenId)
			{
				screenWorldPosition = _allScreenControllers[i]._uiScreenPosition;
				founded = true;
				break;
			}
		}
		if(!founded)
		{
			screenWorldPosition = CalculateNewPositionInWorld(_allScreenControllers.Count);
		}
		return screenWorldPosition;
	}

	/// <summary>
	/// Resets all screen positions.
	/// </summary>
	public void ResetAllPositions()
	{
		for(int i = 0; i < _allScreenControllers.Count; i++)
		{
			_allScreenControllers[i].ResetPosition(CalculateNewPositionInWorld(i));
		}
	}

	/// <summary>
	/// Registers to status change event for screen with identifier.
	/// </summary>
	/// <param name="screenId">Screen identifier.</param>
	/// <param name="listener">Listener.</param>
	public void RegisterToChangeEventForScreenWithId(string screenId,UIScreenController.ScreenChangedEventHandler listener,bool sendCurrentstate = true)
	{
		if(listener != null)
		{
			UIScreenController controller;
			if(_fastAccessScreenControllers.TryGetValue(screenId,out controller))
			{
				controller.OnScreenStatusChanged += listener;
				if(sendCurrentstate)
				{
					listener(controller._isActive);
				}
			}
		}
	}

	/// <summary>
	/// Unregisters to status change event for screen with identifier.
	/// </summary>
	/// <param name="screenId">Screen identifier.</param>
	/// <param name="listener">Listener.</param>
	public void UnregisterToChangeEventForScreenWithId(string screenId,UIScreenController.ScreenChangedEventHandler listener)
	{
		UIScreenController controller;
		if(_fastAccessScreenControllers.TryGetValue(screenId,out controller))
		{
			controller.OnScreenStatusChanged -= listener;
		}
	}

	/// <summary>
	/// Registers to update event for screen with identifier.
	/// </summary>
	/// <param name="screenId">Screen identifier.</param>
	/// <param name="listener">Listener.</param>
	public void RegisterToUpdateEventForScreenWithId(string screenId,UIScreenController.ScreenUpdatedEventHandler listener)
	{
		UIScreenController controller;
		if(_fastAccessScreenControllers.TryGetValue(screenId,out controller))
		{
			controller.OnScreenUpdated += listener;
		}
	}

	/// <summary>
	/// Unregisters to update event for screen with identifier.
	/// </summary>
	/// <param name="screenId">Screen identifier.</param>
	/// <param name="listener">Listener.</param>
	public void UnregisterToUpdateEventForScreenWithId(string screenId,UIScreenController.ScreenUpdatedEventHandler listener)
	{
		UIScreenController controller;
		if(_fastAccessScreenControllers.TryGetValue(screenId,out controller))
		{
			controller.OnScreenUpdated -= listener;
		}
	}



	#region EDITOR HELPING FUNCTIONS
	/// <summary>
	/// Tries to get the index of the UIScreen controller.
	/// </summary>
	/// <returns>The UIScreen controller index.</returns>
	/// <param name="screenId">Screen identifier.</param>
	public int TryGetUIScreenControllerIndex(string screenId)
	{
		for(int i = 0; i < _allScreenControllers.Count; i++)
		{
			if(_allScreenControllers[i]._uiUniqueId == screenId)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Gets the complement managers for screen with identifier.
	/// </summary>
	/// <returns>The complement managers for identifier.</returns>
	/// <param name="screenId">Screen identifier.</param>
	List<int> GetComplementManagersForId(string screenId)
	{
		List<int> complements = new List<int>();
		int controller = TryGetUIScreenControllerIndex(screenId);
		if(controller >= 0)
		{
			for(int i = 0; i < _allScreenControllers[controller]._complementScreenIds.Count; i++)
			{
				int complementIndex = TryGetUIScreenControllerIndex(_allScreenControllers[controller]._complementScreenIds[i]);
				if(complementIndex >= 0)
				{
					complements.Add(complementIndex);
				}
			}
		}
		return complements;
	}

	/// <summary>
	/// Calculates the new position in world.
	/// </summary>
	/// <returns>The new position in world.</returns>
	/// <param name="screenIndex">Screen index.</param>
	Vector3 CalculateNewPositionInWorld(int screenIndex)
	{
		Vector3 finalPosition = Vector3.zero;
		int xCoord = screenIndex%_screenPositionNumberOfColumns;
		int yCoord = screenIndex/_screenPositionNumberOfColumns;
		finalPosition = new Vector3(_screenSeparation.x*xCoord, _screenSeparation.y*yCoord, 0);
		return finalPosition;
	}

	/// <summary>
	/// Gets an unique identifier.
	/// </summary>
	/// <returns>The unique identifier.</returns>
	/// <param name="proposedIdComplement">Proposed identifier complement.</param>
	private string GetUniqueId(int proposedIdComplement)
	{
		string id = "UISM_"+proposedIdComplement;
		for(int i = 0; i < _allScreenControllers.Count; i++)
		{
			if(id == _allScreenControllers[i]._uiUniqueId)
			{
				int newProposal = proposedIdComplement+1;
				id = GetUniqueId(newProposal);
			}
		}
		return id;
	}

	/// <summary>
	/// Creates the a new UIScreen manager along with its controller.
	/// </summary>
	/// <returns>The new user interface screen manager.</returns>
	/// <param name="newScreenId">New screen identifier.</param>
	public GameObject CreateNewUIScreenManager(string newScreenId = "")
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Creating new UISCreen with layer["+LayerMask.LayerToName(_systemLayer)+"]["+_systemLayer.value+"]");
		}
		GameObject go = new GameObject((newScreenId == "" ? GetUniqueId(_allScreenControllers.Count) : newScreenId));
		go.layer = _systemLayer.value;
		Camera cam = go.AddComponent<Camera>();
		if(cam != null)
		{
			cam.orthographic = true;
			cam.orthographicSize = 5;
			cam.hdr = false;
			cam.useOcclusionCulling = true;
			cam.clearFlags = CameraClearFlags.Depth;
			cam.cullingMask = 1 << _systemLayer.value;
			cam.farClipPlane = 200.0f;
			//add child Canvas
			GameObject canvasGO = new GameObject("Canvas");
			canvasGO.transform.SetParent(go.transform);
			canvasGO.layer = _systemLayer.value;
			canvasGO.transform.localPosition = Vector3.zero;
			Canvas canvas = canvasGO.AddComponent<Canvas>();
			if(canvas != null)
			{
				canvas.renderMode = RenderMode.ScreenSpaceCamera;
				canvas.worldCamera = cam;
				canvas.planeDistance = 100;
				canvas.sortingLayerID = _systemLayer.value;
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
					Vector3 newPosition = CalculateNewPositionInWorld(_allScreenControllers.Count);
					//create controller for this screenManager
					UIScreenController controller = new UIScreenController(go.name,null,screenManager,newPosition);
					_allScreenControllers.Add(controller);
				}

				if(_addHelpFrameToCreatedScreens && _helpFramePrefab != null)
				{
					GameObject helpFrame = GameObject.Instantiate(_helpFramePrefab);
					helpFrame.transform.SetParent(canvasGO.transform);
					helpFrame.transform.localScale = Vector3.one;
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
	/// Creates the new empty controller alone.
	/// </summary>
	public void CreateNewEmptyControllerAlone()
	{
		//create controller for this screenManager
		Vector3 newPosition = CalculateNewPositionInWorld(_allScreenControllers.Count);
		UIScreenController controller = new UIScreenController(GetUniqueId(_allScreenControllers.Count),null,null,newPosition);
		_allScreenControllers.Add(controller);
	}

	/// <summary>
	/// Gets a unique identifier from the passed Id.
	/// </summary>
	/// <returns>The unique identifier from.</returns>
	/// <param name="currentId">Current identifier.</param>
	private string GetUniqueIdFrom(string currentId)
	{
		for(int i = 0; i < _allScreenControllers.Count; i++)
		{
			if(currentId == _allScreenControllers[i]._uiUniqueId)
			{
				currentId = GetUniqueId(_allScreenControllers.Count);
			}
		}
		return currentId;
	}

	/// <summary>
	/// Creates a new UIScreen Controller from an UIScreenManager in scene.
	/// </summary>
	/// <returns>The new user interface screen controller from scene object.</returns>
	/// <param name="screenManager">Screen manager.</param>
	public UIScreenManager CreateNewUIScreenControllerFromSceneObject(UIScreenManager screenManager)
	{
		bool alreadyExist = false;	
		for(int i = 0; i < _allScreenControllers.Count; i++)
		{
			if(_allScreenControllers[i]._uiUniqueId == screenManager._uniqueScreenId)
			{
				alreadyExist = true;
				break;
			}
		}
		if(_mustShowDebugInfo)
		{
			Debug.Log("Creating new UISCreenController. AlreadyExist?["+alreadyExist+"]");
		}
		if(!alreadyExist)
		{
			string id = GetUniqueIdFrom(screenManager._uniqueScreenId);
			Vector3 newPosition = CalculateNewPositionInWorld(_allScreenControllers.Count);
			//create controller for this screenManager
			UIScreenController controller = new UIScreenController(id,null,screenManager,newPosition);
			_allScreenControllers.Add(controller);
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
	/// Gets the sibling index in hierarchy for the lowest screen.
	/// </summary>
	/// <returns>The last screen sibling index.</returns>
	public int GetLastScreenSiblingIndex()
	{
		int lastSiblingIndex = CachedTransform.GetSiblingIndex();
		//Debug.Log("UIManagerSiblingIndex["+lastSiblingIndex+"]");
		for(int i = 0; i < _allScreenControllers.Count; i++)
		{
			if(_allScreenControllers[i]._uiScreenObject != null)
			{
				int newSiblingIndex = _allScreenControllers[i]._uiScreenObject.CachedTransform.GetSiblingIndex();
				if(newSiblingIndex > lastSiblingIndex)
				{
					lastSiblingIndex = newSiblingIndex;
				}
			}
		}
		return lastSiblingIndex;
	}

	/// <summary>
	/// Removes the user interface screen manager.
	/// </summary>
	/// <param name="screenToRemove">Screen to remove.</param>
	/// <param name="mustDestroyGameObject">If set to <c>true</c> must destroy game object.</param>
	public void RemoveUIScreenManager(UIScreenManager screenToRemove,bool mustDestroyGameObject = true)
	{
		if(screenToRemove != null)
		{
			int indexToRemove = -1;
			for(int  i = 0; i < _allScreenControllers.Count; i++)
			{
				
				bool hasSameGO = _allScreenControllers[i]._uiScreenObject == screenToRemove;
				bool hasSameId = _allScreenControllers[i]._uiUniqueId == screenToRemove._uniqueScreenId;
				if(hasSameGO || hasSameId)
				{
					if(_mustShowDebugInfo)
					{
						Debug.Log("Removing UIscreenManager["+screenToRemove.name+"] with Id["+screenToRemove._uniqueScreenId+"] by Object?["+hasSameGO+"] by Id?["+hasSameId+"]");
					}
					indexToRemove = i;
					break;
				}
			}
			if(indexToRemove >= 0)
			{
				_allScreenControllers.RemoveAt(indexToRemove);
				if(mustDestroyGameObject)
				{
					DestroyImmediate(screenToRemove.gameObject);
				}
				ResetAllPositions();
			}
		}
	}

	/// <summary>
	/// Removes the user interface screen manager controller.
	/// </summary>
	/// <param name="uniqueId">Unique identifier.</param>
	/// <param name="justSetAsNull">If set to <c>true</c> just set as null and do not remove.</param>
	public void RemoveUIScreenManagerController(string uniqueId, bool justSetAsNull)
	{
		int indexToRemove = -1;
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			bool hasSameId = _allScreenControllers[i]._uiUniqueId == uniqueId;
			if(hasSameId && indexToRemove == -1)
			{
				if(_mustShowDebugInfo)
				{
					Debug.Log("Removing UIscreenManagerController with Id["+uniqueId+"]");
				}
				indexToRemove = i;
			}
			if(!justSetAsNull)
			{
				_allScreenControllers[i].RemoveComplementId(uniqueId);
			}
		}
		if(indexToRemove >= 0)
		{
			if(justSetAsNull)
			{
				Debug.LogWarning("Setting screen object to null");
				_allScreenControllers[indexToRemove]._uiScreenObject = null;	
			}
			else
			{
				_allScreenControllers.RemoveAt(indexToRemove);
				ResetAllPositions();
			}
		}
	}

	/// <summary>
	/// Reset this instance.
	/// </summary>
	public void Reset()
	{
		if(_allScreenControllers != null)
		{
			while(_allScreenControllers.Count > 0)
			{
				if(_allScreenControllers[0]._uiScreenObject != null)
				{
					if(Application.isPlaying)
					{
						Destroy(_allScreenControllers[0]._uiScreenObject.gameObject);
					}
					else
					{
						DestroyImmediate(_allScreenControllers[0]._uiScreenObject.gameObject);
					}
				}
				_allScreenControllers.RemoveAt(0);
			}
			_allScreenControllers.Clear();
			ResetAllPositions();
		}
	}
		
	/// <summary>
	/// Despawns all screens on scene.
	/// </summary>
	public void DespawnAllScreensOnScene()
	{
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			_allScreenControllers[i].Switch(false);
		}
	}

	/// <summary>
	/// Despawns all screens on scene that are not initial.
	/// </summary>
	public void DespawnAllScreensOnSceneThatAreNotInitial()
	{
		List<int> allInitials = new List<int>();
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			if(_allScreenControllers[i]._isStartingScreen)	
			{
				if(!allInitials.Contains(i))
				{
					allInitials.Add(i);
				}
				List<int> complements = GetComplementManagersForId(_allScreenControllers[i]._uiUniqueId);
				for(int j = 0; j < complements.Count; j++)
				{
					if(complements[j] >= 0)
					{
						if(!allInitials.Contains(complements[j]))
						{
							allInitials.Add(complements[j]);
						}
					}
				}
			}
		}
		for(int  i = 0; i < _allScreenControllers.Count; i++)
		{
			if(!allInitials.Contains(i))
			{
				_allScreenControllers[i].Switch(false,false);	
			}
		}

	}
		
	#endregion
}
