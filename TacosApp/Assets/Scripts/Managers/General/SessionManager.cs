using UnityEngine;
using System;
using System.Collections;

public class SessionManager : Manager<SessionManager> 
{
	public bool showExtraMenu = false;

	public bool emulateNewDayEverySession = false;

	/// <summary>
	/// The use local date flag.
	/// </summary>
	public bool _useLocalDate = true;

	public int _framesToWaitBeforeValidation = 1;
	/// <summary>
	/// The is first validation done.
	/// </summary>
	private bool isFirstValidationDone = false;

	/// <summary>
	/// The is first session of all flag.
	/// </summary>
	private bool isFirstSessionOfAll = false;
	/// <summary>
	/// The last validation detected new day.
	/// </summary>
	private bool _lastValidationDetectedNewDay = false;
	/// <summary>
	/// The last validation detected new week.
	/// </summary>
	private bool _lastValidationDetectedNewWeek = false;

	/// <summary>
	/// The hour to end day.
	/// </summary>
	[Range(0,23)]
	public int _hourToEndDay = 0;
	/// <summary>
	/// The minute to end day.
	/// </summary>
	[Range(0,59)]
	public int _minuteToEndDay = 0;
	/// <summary>
	/// The day week starts.
	/// </summary>
	public DayOfWeek _dayWeekStarts = DayOfWeek.Monday;

	/// <summary>
	/// New day detected delegate.
	/// </summary>
	public delegate void NewDayDetected();
	/// <summary>
	/// New week detected delegate.
	/// </summary>
	public delegate void NewWeekDetected();
	/// <summary>
	/// Session validated delegate.
	/// </summary>
	public delegate void SessionValidated();

	/// <summary>
	/// The on new day detected event.
	/// </summary>
	public static NewDayDetected OnNewDayDetected;
	/// <summary>
	/// The on new week detected event.
	/// </summary>
	public static NewWeekDetected OnNewWeekDetected;
	/// <summary>
	/// The on session validated.
	/// </summary>
	public static SessionValidated OnSessionValidated;
	/// <summary>
	/// The is session validated flag.
	/// </summary>
	private bool _isSessionValidated = false;

	/// <summary>
	/// The last validated with server date.
	/// </summary>
	private DateTime _lastValidDate;
	/// <summary>
	/// The synced local date.
	/// </summary>
	private DateTime _syncedLocalDate;

	private int sessionCounter = 0;

	private DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0,DateTimeKind.Local);

	private double secondsPassedSinceLastSession = 0;

	private int daysPassedSinceLastSession = 0;

	//int currentGMT;

	public void SaveDateWithId(string id,bool useSecureDate = false)
	{
		#if UNBIASED_TIME
		if(useSecureDate)
		{
			WriteTimestamp("Date_"+id,UnbiasedTime.Instance.Now());
			if(_mustShowDebugInfo)
			{
				Debug.Log("Saving Date ["+UnbiasedTime.Instance.Now().ToString()+"]  as [Date_"+id+"]");
			}
		}
		else
		#endif
		{
			WriteTimestamp("Date_"+id,DateTime.Now);
			if(_mustShowDebugInfo)
			{
				Debug.Log("Saving Date ["+DateTime.Now.ToString()+"]  as [Date_"+id+"]");
			}
		}
		//double millisecondsSinceEpoch = (DateTime.Now - epoch).TotalMilliseconds;
		//PlayerPrefs.SetString("Date_"+id,millisecondsSinceEpoch.ToString());

	}

	public DateTime LoadDateWithId(string id,bool useSecureDate = false)
	{
		DateTime loadedDate = DateTime.Now;
		#if UNBIASED_TIME
		if(useSecureDate)
		{
			loadedDate = ReadTimestamp("Date_"+id,UnbiasedTime.Instance.Now());
		}
		else
		#endif
		{
			loadedDate = ReadTimestamp("Date_"+id,DateTime.Now);
		}

		if(_mustShowDebugInfo)
		{
			Debug.Log("Loading Date ["+loadedDate.ToString()+"]  as [Date_"+id+"]");
		}

		return loadedDate;
	}

	public void SaveDateTimeWithId(string id,DateTime dt)
	{
		WriteTimestamp("Date_"+id,dt);
		if(_mustShowDebugInfo)
		{
			Debug.Log("Saving Date ["+dt.ToString()+"]  as [Date_"+id+"]");
		}
	}

	public double GenerateNowTimestamp(bool inSeconds,bool useSecureDate = false)
	{
		#if UNBIASED_TIME
		if(useSecureDate)
		{
			if(inSeconds)
			{
				return ( UnbiasedTime.Instance.Now() - epoch).TotalSeconds;
			}
			else
			{
				return ( UnbiasedTime.Instance.Now() - epoch).TotalMilliseconds;
			}
		}
		else
		#endif
		{
			if(inSeconds)
			{
				return ( DateTime.Now - epoch).TotalSeconds;
			}
			else
			{
				return ( DateTime.Now - epoch).TotalMilliseconds;
			}
		}
	}

	public double SecondsSinceLastSession
	{
		get{return secondsPassedSinceLastSession;}
	}

	public int DaysSinceLastSession
	{
		get{return daysPassedSinceLastSession;}
	}

	public override void StartManager()
	{
		if(alreadystarted)
			return;
		base.StartManager();
		StartCoroutine("InitManager");
	}

	/// <summary>
	/// Start this instance. Registers to events and start update check.
	/// </summary>
	IEnumerator InitManager()
	{
		for(int i = 0; i < _framesToWaitBeforeValidation; i++)
		{
			yield return 0;
		}
		CheckSession();
		isFirstValidationDone = true;
		InvokeRepeating("UpdateValidDate",1,1);
	}

	public bool IsFirstSessionOfAll()
	{
		return isFirstSessionOfAll;
	}

	public void CheckSession()
	{
		//currentGMT = System.TimeZoneInfo.Local.BaseUtcOffset.Hours;
		if(_useLocalDate)
		{
			LoadSessionCounter();
			LoadLastDate();	
			#if UNBIASED_TIME
			_syncedLocalDate =  UnbiasedTime.Instance.Now();
			#else
			_syncedLocalDate = DateTime.Now;
			#endif
			if(isFirstSessionOfAll)
			{
				SaveLastDate();
				//notify interested ones
				if(OnNewDayDetected != null)
				{
					OnNewDayDetected();
				}
				if(OnNewWeekDetected != null)
				{
					OnNewWeekDetected();
				}	
				_isSessionValidated = true;
			}
			else
			{
				CheckForNewDayAndWeek(ref _lastValidationDetectedNewDay,ref _lastValidationDetectedNewWeek,_lastValidDate,_syncedLocalDate);

				if(emulateNewDayEverySession)
				{
					_lastValidationDetectedNewDay = true;
				}

				if(_lastValidationDetectedNewDay)
				{
					if(OnNewDayDetected != null)
					{
						OnNewDayDetected();
					}
				}
				if(_lastValidationDetectedNewWeek)
				{
					if(OnNewWeekDetected != null)
					{
						OnNewWeekDetected();
					}
				}
				UpdateLastDate();
				_isSessionValidated = true;
			}
			AdvanceSession();
			if(OnSessionValidated != null)
			{
				OnSessionValidated();
			}
		}
		else
		{
			//TODO: check with server
		}
	}

	public int GetSessionNumber()
	{
		return sessionCounter;
	}

	private void AdvanceSession()
	{
		sessionCounter++;
		SaveSessionCounter();
	}

	private void LoadSessionCounter()
	{
		sessionCounter = PlayerPrefs.GetInt("sc",0);
	}

	private void SaveSessionCounter()
	{
		PlayerPrefs.SetInt("sc",sessionCounter);
	}

	private void LoadLastDate(bool useSecureDate = false)
	{
		#if UNBIASED_TIME
		_lastValidDate = ReadTimestamp("lastDate",UnbiasedTime.Instance.Now());
		#else
		_lastValidDate = ReadTimestamp("lastDate",DateTime.Now);
		#endif
		if(_mustShowDebugInfo)
		{
			Debug.Log("LastValidDate Loaded["+_lastValidDate.ToString()+"] SessionNumber["+sessionCounter+"]");
		}

		#if UNBIASED_TIME
		isFirstSessionOfAll = _lastValidDate == UnbiasedTime.Instance.Now();
		#else
		isFirstSessionOfAll = _lastValidDate == DateTime.Now;
		#endif
	}

	public void SaveLastDate()
	{
		WriteTimestamp("lastDate",_lastValidDate);

		if(_mustShowDebugInfo)
		{
			Debug.Log("Saving LastDate As["+_lastValidDate.ToString()+"]");
		}
//		double millisecondsSinceEpoch = (_lastValidDate - epoch).TotalMilliseconds;
//		PlayerPrefs.SetString("lastDate",millisecondsSinceEpoch.ToString());

	}

	private DateTime ReadTimestamp (string key, DateTime defaultValue) 
	{
		long tmp = Convert.ToInt64(PlayerPrefs.GetString(key, "0"));
		if ( tmp == 0 ) 
		{
			return defaultValue;
		}
		return DateTime.FromBinary(tmp);
	}

	private void WriteTimestamp (string key, DateTime time) 
	{
		PlayerPrefs.SetString(key, Convert.ToInt64( time.ToBinary()).ToString());
	}

	public void UpdateLastDate()
	{
		#if UNBIASED_TIME
		_lastValidDate =  UnbiasedTime.Instance.Now();
		#else
		_lastValidDate =  DateTime.Now;
		#endif
		SaveLastDate();
	}

	/// <summary>
	/// Checks for new day and week given 2 dates and returns whether or not 
	/// the lastSession date is before and the currenTime is after the new day or new week date.
	/// </summary>
	/// <param name="isDifferentDay">Is different day result.</param>
	/// <param name="isDifferentWeek">Is different week result.</param>
	/// <param name="lastSession">Last session.</param>
	/// <param name="currentTime">Current time.</param>
	public void CheckForNewDayAndWeek(ref bool isDifferentDay, ref bool isDifferentWeek,DateTime lastSession,DateTime currentTime)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Checking for new Day Or Week current["+currentTime.ToString()+"] last["+lastSession.ToString()+"]");
		}
		TimeSpan difference = (currentTime-lastSession);
		secondsPassedSinceLastSession = difference.TotalSeconds;
		daysPassedSinceLastSession = (int)difference.TotalDays;

		//lets check different days of the week
		isDifferentDay = lastSession.DayOfWeek != currentTime.DayOfWeek;
		if(!isDifferentDay)
		{
			//same day, lets check before and after end hour
			isDifferentDay = currentTime.Hour >= _hourToEndDay && lastSession.Hour < _hourToEndDay ;
			if(!isDifferentDay)
			{
				//same day and hour, lets check before and after end minute
				isDifferentDay = currentTime.Minute >= _minuteToEndDay && lastSession.Minute < _minuteToEndDay;
			}
		}

		if(isDifferentDay && daysPassedSinceLastSession > 1)
		{
			daysPassedSinceLastSession -= 1;
		}

		int daysSinceLastWeekRestart = 0;

		isDifferentWeek = isDifferentDay;
		if(isDifferentWeek)
		{
			if(_lastValidDate.DayOfWeek == DayOfWeek.Sunday)
			{
				daysSinceLastWeekRestart = 7 - (int)_dayWeekStarts;		
			}
			else
			{
				if(_lastValidDate.DayOfWeek < _dayWeekStarts)
				{
					daysSinceLastWeekRestart =  7 - ((int)_dayWeekStarts - (int)currentTime.DayOfWeek);
				}
				else
				{
					daysSinceLastWeekRestart =  (int)currentTime.DayOfWeek - (int)_dayWeekStarts;
				}
			}


			isDifferentWeek = (lastSession < currentTime.AddDays(-daysSinceLastWeekRestart) );
		}
	}

	/// <summary>
	/// Gets the current session time span.
	/// </summary>
	/// <returns>The current session time span from server date.</returns>
	public TimeSpan GetCurrentSessionTimeSpan()
	{
		if(_isSessionValidated)
		{
			return (_syncedLocalDate - _lastValidDate);
		}
		return TimeSpan.Zero;
	}

	/// <summary>
	/// Gets the current synced session time.
	/// </summary>
	/// <returns>The current synced session time.</returns>
	public DateTime GetCurrentSyncedSessionTime()
	{
		if(_isSessionValidated)
		{
			return _syncedLocalDate;
		}
		#if UNBIASED_TIME
		return  UnbiasedTime.Instance.Now();
		#else
		return DateTime.Now;
		#endif
	}

	public static DateTime GetNow()
	{
		if(_cachedInstance != null)
		{
			return _cachedInstance.GetCurrentSyncedSessionTime();
		}
		#if UNBIASED_TIME
		return  UnbiasedTime.Instance.Now();
		#else
		return DateTime.Now;
		#endif
	}
		
	/// <summary>
	/// Updates the valid date by adding 1 second to the local synced date.
	/// </summary>
	void UpdateValidDate()
	{
		if(_isSessionValidated)
		{
			_syncedLocalDate = _syncedLocalDate.AddSeconds(1);
			secondsPassedSinceLastSession++;
			if(!_lastValidationDetectedNewDay)
			{
				CheckForNewDayAndWeek(ref _lastValidationDetectedNewDay, ref _lastValidationDetectedNewWeek,_lastValidDate,_syncedLocalDate);

				if(_lastValidationDetectedNewDay)
				{
					if(_mustShowDebugInfo)
					{
						Debug.LogWarning("New Day Detected during session ["+_lastValidationDetectedNewDay+"]. New Week Detected["+_lastValidationDetectedNewWeek+"]");
					}
					if(OnNewDayDetected != null)
					{
						OnNewDayDetected();
					}
					if(_lastValidationDetectedNewWeek)
					{
						if(OnNewWeekDetected != null)
						{
							OnNewWeekDetected();
						}	
					}
				}
			}
		}
	}

	/// <summary>
	/// Raised by the application pause event. If is Suspending it will send a session date update to the server, is is resuming then will check if possible the new date with the server.
	/// </summary>
	/// <param name="isPausing">If set to <c>true</c> ispausing the application.</param>
	void OnApplicationPause(bool isPausing)
	{
		if(isPausing)
		{
			if(isFirstValidationDone)
			{
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Application Pausing.");
				}
				UpdateLastDate();
				isFirstSessionOfAll = false;
				_isSessionValidated = false;
			}
		}
		else
		{
			if(isFirstValidationDone)
			{
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Application Unpausing.");
				}
				CheckSession();
			}
		}
	}
		
	//TGS SPECIAL
	public void AddSession()
	{
		_lastValidationDetectedNewDay = false;
		_lastValidationDetectedNewWeek = false;
		UpdateLastDate();
		#if UNBIASED_TIME
		_syncedLocalDate =  UnbiasedTime.Instance.Now().AddSeconds(86399);
		#else
		_syncedLocalDate =  DateTime.Now.AddSeconds(86399);
		#endif
		secondsPassedSinceLastSession = 86399;

	}

	#if UNITY_EDITOR
	int currentSessionDay = 0;
	void OnGUI()
	{
		if(showExtraMenu)
		{
			if(UIManager.GetInstance().IsScreenActive("MainOverlay"))
			{
				if(GUI.Button(new Rect(0,0,Screen.width,50),"Change Day NOW! Day["+currentSessionDay+"]"))
				{
					AddSession();
					currentSessionDay++;
				}
			}
		}
	}
	#endif

//	void OnApplicationFocus(bool isLosingFocus)
//	{
//		if(isLosingFocus)
//		{
//			if(_mustShowDebugInfo)
//			{
//				Debug.LogWarning("Application losing focus.");
//			}
//			UpdateLastDate();
//			isFirstSessionOfAll = false;
//			_isSessionValidated = false;
//		}
//		else
//		{
//			if(isFirstValidationDone)
//			{
//				if(_mustShowDebugInfo)
//				{
//					Debug.LogWarning("Application Focusing.");
//				}
//				CheckSession();
//			}
//		}
//	}
}
