using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//Flurry namespace
#if FLURRY_ON
using KHD;
#endif

public class AnalyticsManager : Manager<AnalyticsManager> 
{

	protected DateTime epoch = new DateTime(1970,1,1,0,0,0,DateTimeKind.Local);

	public bool _dontSendData = false;

	protected void registerSimpleEvent(string eventName)
	{
		#if FLURRY_ON
		FlurryAnalytics.Instance.LogEvent (eventName);
		#endif
	}

	protected void registerEventWithParameters(string eventName,Dictionary<string,string> parameters)
	{
		if (_dontSendData) 
		{
			return;
		}
		#if FLURRY_ON
		FlurryAnalytics.Instance.LogEventWithParameters (eventName, parameters);
		#endif
	}

	protected void startTimerEvent(string eventName)
	{
		#if FLURRY_ON
		FlurryAnalytics.Instance.LogEvent (eventName, true);
		#endif
	}

	protected void finishTimerEvent(string eventName)
	{
		#if FLURRY_ON
		FlurryAnalytics.Instance.EndTimedEvent (eventName);
		#endif
	}

	protected void switchForUnityAnalytics(bool activate)
	{
		#if FLURRY_ON
		FlurryAnalytics.Instance.replicateDataToUnityAnalytics = activate;
		#endif
	}

	protected override void Awake ()
	{
		base.Awake ();
		if(isThisManagerValid)
		{
			SessionManager.OnSessionValidated += OnSessionValidated;
		}
	}

	private void OnSessionValidated()
	{
		Dictionary<string,string>	dictionary = new Dictionary<string, string>();
		dictionary.Add("time",SessionManager.GetInstance().SecondsSinceLastSession.ToString());
		registerEventWithParameters("timeBetweenSessions",dictionary);
	}

	public void RegisterWithParameters(string eventName,KeyValuePair<string,string>[] parameters)
	{
		Dictionary<string,string>	dictionary = new Dictionary<string, string>();
		for(int i = 0; i < parameters.Length ;i++)
		{
			dictionary.Add(parameters[i].Key,parameters[i].Value);
		}
		registerEventWithParameters(eventName,dictionary);
	}

	public void RegisterSimple(string eventName)
	{
		registerSimpleEvent(eventName);
	}

	public override void StartManager()
	{
		if(alreadystarted)
			return;
		base.StartManager();

		switchForUnityAnalytics (true);

		if(_dontSendData)
		{
			Debug.Log("<color=red>ANALYTICS TEST MODE!!!!!!!!!!!!</color>");
		}
	}
}
