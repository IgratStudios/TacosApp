using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if FYBER_ON
using FyberPlugin;
#endif

//disable warning for variables that are only used in editor
#pragma warning disable 0414

public enum AD_FORMATS
{
	BANNER,
	INTERSTITIAL,
	REWARDED_VIDEO
}

public class AdsManager : Manager<AdsManager> 
{
	public delegate void AdStockChanged(AD_FORMATS adFormat);
	public static AdStockChanged OnAdStockChanged;
	public delegate void VideoAdStarted();
	public static VideoAdStarted OnVideoAdStarted; 
	public delegate void VideoAdFinished(bool success);
	public static VideoAdFinished OnVideoAdFinished; 
	public delegate void BannerAdChanged();
	public static BannerAdChanged OnBannerAdStarted;
	public static BannerAdChanged OnBannerAdFinished;
	public static BannerAdChanged OnBannerAdBecameVisible;
	public static BannerAdChanged OnBannerAdBecameInvisible;
	public delegate void InterstitialAdChanged();
	public static InterstitialAdChanged OnInterstitialAdStarted;
	public static InterstitialAdChanged OnInterstitialAdFinished;

	public string iOSKey = "86639";
	public string androidKey = "86638";
	public string iOSSecurityToken = "45712381926b8cdfef6d4c560ad68b30";
	public string androidSecurityToken = "326182a0fa39ccbca4c7663313f47db2";

	public bool disableGameInputDuringFullScreenAds = true;
	public bool displayBannerAtTop = false;
	public bool useInterstitials = true;
	public float timeToRequestFirstAds = 10.0f;
	public float onAdNotAvailableRetryTime = 5.0f;
	public float waitTimeBetweenAskingAds = 15.0f;

	public bool emulateInEditor = true;
	public bool emulateInDevice = false;
	public float emulatedVideoDuration = 5;
	private float emulatedVideoCurrentDuration = 0;

	public bool notifyUserOnVideoFinished = false;
	private bool adsInitialized = false;
	private bool bannerAdActivated = false;
	private bool isBannerVisible = false;
	private bool mustShowBannerAsSoonAsPossible = false;
	private bool videoAdActivated = false;
	private bool interstitialAdActivated = false;
	private bool isRequestingFirstAds = false;

	public float bannerRefreshRateInSeconds = -1;
	private float currentBannerTime = 0;
	private bool waitingBannerRefresh = false;

	private uint interstitialsShownThisSession = 0;
	private uint videosShownThisSession = 0;

	#if FYBER_ON
	private Settings fyberSettings;
	Ad rewardedVideoAd = null;
	BannerAd bannerAd = null;
	Ad interstitialAd;
	#endif

	private bool isRequestingVideoAd = false;
	private bool isRequestingBannerAd = false;
	private bool isRequestingInterstitialAd = false;

	private GUIStyle style = null;

	protected override void Awake ()
	{
		base.Awake ();
		if(isThisManagerValid)
		{
			SessionManager.OnSessionValidated += ResetSessionData;
			//TODO:
			//Register to User id provider
			// += OnUserIdObtained;
		}
	}
		
	private void InitEmulationGUIStyle()
	{
		if(style != null)
			return;
		style = new GUIStyle(GUI.skin.button);
		style.fontSize = Mathf.FloorToInt( (Screen.width)*0.05f );
	}

	public override void StartManager()
	{
		if(alreadystarted)
			return;
		base.StartManager();
		StartAdsManager(string.Empty);
	}

	void ResetSessionData()
	{
		videosShownThisSession = 0;
		interstitialsShownThisSession = 0;
	}

	void OnUserIdObtained(string userId)
	{
		if(!adsInitialized)
		{
			StartAdsManager(userId);
		}
		#if FYBER_ON
		else if(fyberSettings != null)
		{
			if(fyberSettings.GetUserId() != userId)
			{
				fyberSettings.UpdateUserId(userId);
			}
		}
		#endif
	}

	// Use this for initialization
	public void StartAdsManager (string userId) 
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Starting Ads Manager. Emulated["+IsEmulated()+"]");
		}

		#if FYBER_ON
		if(_mustShowDebugInfo)
		{
			Debug.Log("ADS SDK version["+Fyber.Version+"] for userId["+userId+"]...");
		}

		//CALLBACKS
		// Called when there is an error in the device
		FyberCallback.NativeError += OnNativeError;
		// Called when there is an ad available
		FyberCallback.AdAvailable += OnAdAvailable;
		// Called when there is no ad available
		FyberCallback.AdNotAvailable += OnAdNotAvailable;   
		// Called when there is a failure in the request
		FyberCallback.RequestFail += OnRequestFail;
		// Called when an ad has started
		FyberCallback.AdStarted += OnAdStarted;
		// Called when an ad has finished
		FyberCallback.AdFinished += OnAdFinished; 
		// Called when the banner was clicked
		FyberCallback.BannerAdClicked += OnBannerAdClicked;
		// Called when the banner triggers an error
		FyberCallback.BannerAdError += OnBannerAdError;
		// Called when the banner was loaded
		// this is usually called when the banner rotates (i.e., a new content is shown to the user)
		FyberCallback.BannerAdLoaded += OnBannerAdLoaded;
		// Called when the banner interaction causes an external application to be open
		FyberCallback.BannerAdLeftApplication += OnBannerAdLeftApplication;
		// iOS only
		// The banner will present a modal view
		FyberCallback.BannerAdWillPresentModalView += OnBannerAdWillPresentModalView;
		// iOS only
		// The user did dismiss a modal view
		FyberCallback.BannerAdDidDismissModalView += OnBannerAdDidDismissModalView;
		#endif

		if(Application.isEditor || emulateInDevice)
		{
			adsInitialized = emulateInEditor || emulateInDevice;
		}
		else
		{
			#if FYBER_ON
			string appId = string.Empty;
			string securityToken = string.Empty;
			#if UNITY_ANDROID
			appId = androidKey;
			securityToken = androidSecurityToken;//in Order to use Banners or interstitials this must be provided
			adsInitialized = true;
			#elif UNITY_IPHONE
			appId = iOSKey;
			securityToken = iOSSecurityToken;//in Order to use Banners or interstitials this must be provided
			adsInitialized = true;
			#endif	

			if(_mustShowDebugInfo)
			{
				FyberLogger.EnableLogging(true);
			}

			fyberSettings = Fyber.With(appId)
				// optional chaining methods
				.WithUserId(userId)
				//.WithParameters(dictionary)
				.WithSecurityToken(securityToken)
				//.WithManualPrecaching()
				.Start();
			#endif
		}

		//this time is recommended by the SDK developers
		StartCoroutine("RequestAds",timeToRequestFirstAds);
	}

	public void DisableAds()
	{
		if(adsInitialized)
		{
			if(IsAdBannerVisible())
			{
				DestroyBanner(true);
			}
		}
	}

	private IEnumerator RequestAds(float seconds)
	{
		bool canRequest = (Application.isEditor ? false : !emulateInDevice);
		if(canRequest)
		{
			isRequestingFirstAds = true;
			if(_mustShowDebugInfo)
			{
				Debug.Log("Will request All Ads after ["+seconds+"]");
			}
			yield return new WaitForSeconds(seconds);

			while(!adsInitialized)
			{
				yield return 0;
			}
			if(_mustShowDebugInfo)
			{
				Debug.Log("Will request Video Ad.");
			}
			RequestVideoAd();
			while(isRequestingVideoAd)
			{
				yield return 0;
			}
			//if(!DataManager.GetDataManager().IsNoAdsPurchased())
			{
				if(_mustShowDebugInfo)
				{
					Debug.Log("Will request Banner Ad.After["+waitTimeBetweenAskingAds+"]");
				}
				yield return new WaitForSeconds(waitTimeBetweenAskingAds);
				RequestBannerAd();
				while(isRequestingBannerAd)
				{
					yield return 0;
				}
				if(useInterstitials)
				{
					if(_mustShowDebugInfo)
					{
						Debug.Log("Will request Interstitial Ad.After["+waitTimeBetweenAskingAds+"]");
					}
					yield return new WaitForSeconds(waitTimeBetweenAskingAds);
					RequestInterstitialAd();
				}
			}
			isRequestingFirstAds = false;
		}
		else
		{
			if(mustShowBannerAsSoonAsPossible)	
			{
				ShowBanner();
			}
		}
		yield return 0;
	}

	private IEnumerator RequestVideoAds(float seconds)
	{
		if(!Application.isEditor)
		{
			yield return new WaitForSeconds(seconds);
			while(!adsInitialized)
			{
				yield return 0;
			}
			while(isRequestingBannerAd || isRequestingInterstitialAd || isRequestingFirstAds)
			{
				yield return 0;
			}
			RequestVideoAd();
		}
		yield return 0;
	}

	private IEnumerator RequestBannerAds(float seconds)
	{
		if(!Application.isEditor)// && !DataManager.GetDataManager().IsNoAdsPurchased())
		{
			yield return new WaitForSeconds(seconds);
			while(!adsInitialized)
			{
				yield return 0;
			}
			while(isRequestingVideoAd || isRequestingInterstitialAd || isRequestingFirstAds)
			{
				yield return 0;
			}
			RequestBannerAd();
		}
		yield return 0;
	}

	private IEnumerator RequestInterstitialAds(float seconds)
	{
		if(!Application.isEditor && useInterstitials)//&& !DataManager.GetDataManager().IsNoAdsPurchased()
		{
			yield return new WaitForSeconds(seconds);
			while(!adsInitialized)
			{
				yield return 0;
			}
			while(isRequestingBannerAd || isRequestingVideoAd || isRequestingFirstAds)
			{
				yield return 0;
			}
			RequestInterstitialAd();
		}
		yield return 0;
	}
		
	private void RequestVideoAd()
	{
		#if FYBER_ON
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("Try to Request Video Ad. Current AdAvailable["+(rewardedVideoAd != null)+"] requesting["+isRequestingVideoAd+"]");
		}
		if(rewardedVideoAd == null && !isRequestingVideoAd)
		{
			isRequestingVideoAd = true;
			RewardedVideoRequester.Create()
			// optional method chaining
			//.AddParameter("key", "value")
			//.AddParameters(dictionary)
			// changing the GUI notification behaviour when the user finishes viewing the video
				.NotifyUserOnCompletion(notifyUserOnVideoFinished)
			// requesting the video
				.Request(); 
			if(_mustShowDebugInfo)
			{
				Debug.Log("Video Ad Requested");
			}
		}
		#endif
	}

	private void RequestBannerAd()
	{
		#if FYBER_ON
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("Try to Request Banner Ad. Current AdAvailable["+(bannerAd != null)+"] requesting["+isRequestingBannerAd+"]");
		}
		if(bannerAd == null && !isRequestingBannerAd)
		{
			BannerRequester requester = BannerRequester.Create();
			// optional method chaining
			//.AddParameter("key", "value")
			//.AddParameters(dictionary)
			//.WithPlacementId(placementId)

			#region BANNER_CONFIGURATIONS
			//PER DEVICE / ASPECT RATIO _BANNER_ CONFIGURATIONS
			//GENERAL
			//FB Rule:	 	width <= 720 then BANNER_50, else BANNER_90
			if(Screen.width <= 720)
			{
				//requester.WithNetworkSize(FacebookNetworkBannerSizes.BANNER_50);
			}
			else
			{
				//requester.WithNetworkSize(FacebookNetworkBannerSizes.BANNER_90);
			}

		//IOS
			#if UNITY_IPHONE
			//AdMob Rule: 	SMART_PORTRAIT
			requester.WithNetworkSize(AdMobNetworkBannerSizes.SMART_PORTRAIT);
			//Inneractive Rule: BANNER(50)
			//requester.WithNetworkSize(InneractiveNetworkBannerSizes.BANNER);
			#elif UNITY_ANDROID
		//ANDROID
			//AdMob Rule:	SMARTBANNER
			requester.WithNetworkSize(AdMobNetworkBannerSizes.SMART_BANNER);
			//Inneractive Rule: width <= 720 then BANNER(50) else LARGE_BANNER(90)
			if(Screen.width <= 720)
			{
				//requester.WithNetworkSize(InneractiveNetworkBannerSizes.BANNER);
			}
			else
			{
				//requester.WithNetworkSize(InneractiveNetworkBannerSizes.LARGE_BANNER);
			}
			#endif	
			#endregion
			isRequestingBannerAd = true;
			// request the ad
			requester.Request();
			if(_mustShowDebugInfo)
			{
				Debug.Log("Banner Ad Requested");
			}
		}
		#endif
	}

	private void RequestInterstitialAd()
	{
	#if FYBER_ON
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("Try to Request Interstitial Ad. Current AdAvailable["+(interstitialAd != null)+"] requesting["+isRequestingInterstitialAd+"]");
		}
		if(interstitialAd == null && !isRequestingInterstitialAd)
		{
			
			InterstitialRequester requester = InterstitialRequester.Create();
			// optional method chaining
			//.AddParameter("key", "value")
			//.AddParameters(dictionary)
			//.WithPlacementId(placementId)
			// request the ad
			isRequestingInterstitialAd = true;
			// request the ad
			requester.Request();
			if(_mustShowDebugInfo)
			{
				Debug.Log("Interstitial Ad Requested");
			}
		}
	#endif
	}

	public void ShowVideo()
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Show Video Ad");
		}
		if(IsEmulated())
		{
			if(_mustShowDebugInfo)
			{
				Debug.Log("Show Video Ad Emulated!");
			}
			videoAdActivated = true;
			if(isBannerVisible && IsEmulated())
			{
				HideBanner();
			}
			emulatedVideoCurrentDuration = emulatedVideoDuration;
			if(OnVideoAdStarted != null)
			{
				OnVideoAdStarted();			
			}
			if(disableGameInputDuringFullScreenAds)
			{
				AppManager.GetInstance().DisableUIInput();
			}
			videosShownThisSession++;
		}
		else
		{
		#if FYBER_ON
			if(_mustShowDebugInfo)
			{
				Debug.Log("Show Video Ad is Valid["+(rewardedVideoAd != null)+"]");
			}
			if (rewardedVideoAd != null)
			{
				rewardedVideoAd.Start();
				videosShownThisSession++;
			}       
		#endif
		}
	}

	private bool IsEmulated()
	{
		return (Application.isEditor ? emulateInEditor : emulateInDevice );
	}

	public void ShowBanner()
	{
//		if(_mustShowDebugInfo)
//		{
//			Debug.Log("Show Banner Ad. No Ads Purchased?["+DataManager.GetDataManager().IsNoAdsPurchased()+"]");
//		}
		//if(!DataManager.GetDataManager().IsNoAdsPurchased())
		{
			if(_mustShowDebugInfo)
			{
				Debug.Log("IsBannerActivated["+bannerAdActivated+"] isVisible["+isBannerVisible+"]");
			}

			if(!bannerAdActivated)//initialize
			{
				if(IsEmulated())
				{
					if(_mustShowDebugInfo)
					{
						Debug.Log("Banner Ad is Emulated!");
					}
					isBannerVisible = true;
					if(OnBannerAdStarted != null)
					{
						OnBannerAdStarted();
					}
					if(OnBannerAdBecameVisible != null)
					{
						OnBannerAdBecameVisible();
					}
					bannerAdActivated = true;
				}
				else
				{
					#if FYBER_ON
					if (bannerAd != null)
					{
						if(displayBannerAtTop)
						{
							bannerAd.DisplayAtTop().Start();
						}
						else
						{
							bannerAd.DisplayAtBottom().Start();
						}
						bannerAdActivated = true;
					}
					else
					{
						mustShowBannerAsSoonAsPossible = true;
					}
					#endif
				}

				currentBannerTime = bannerRefreshRateInSeconds;

			}
			else  if(!isBannerVisible) //is hiding, show it
			{
				if(IsEmulated())
				{
					if(_mustShowDebugInfo)
					{
						Debug.Log("Banner Ad is Emulated and visible");
					}
					isBannerVisible = true;
					if(OnBannerAdBecameVisible != null)
					{
						OnBannerAdBecameVisible();
					}
				}
				else
				{
					
					#if FYBER_ON
					if(_mustShowDebugInfo)
					{
						Debug.Log("Banner Ad is valid?["+(bannerAd != null)+"]");
					}
					if (bannerAd != null)
					{
						if(_mustShowDebugInfo)
						{
							Debug.Log("Showing Banner Ad!");
						}
						mustShowBannerAsSoonAsPossible = false;
						bannerAd.Show();
						isBannerVisible = true;
						if(OnBannerAdBecameVisible != null)
						{
							OnBannerAdBecameVisible();
						}
					}
					#endif
				}
				currentBannerTime = bannerRefreshRateInSeconds;

			}
		}
	}

	public void ShowInterstitial()
	{
//		if(_mustShowDebugInfo)
//		{
//			Debug.Log("Show Interstitial Ad");
//		}
	//	if(!DataManager.GetDataManager().IsNoAdsPurchased() && useInterstitials)
		{
			if(IsEmulated())
			{
				interstitialAdActivated = true;
				if(isBannerVisible && IsEmulated())
				{
					HideBanner();
				}
				if(OnInterstitialAdStarted != null)
				{
					OnInterstitialAdStarted();			
				}
				if(disableGameInputDuringFullScreenAds)
				{
					AppManager.GetInstance().DisableUIInput();
				}
				interstitialsShownThisSession++;
			}
			else
			{
			#if FYBER_ON
				if (interstitialAd != null)
				{
					if(isBannerVisible)
					{
						HideBanner();
					}
					interstitialAd.Start();
					interstitialsShownThisSession++;
				}       
			#endif
			}      
		}
	}

	public void DestroyBanner(bool stopAllBanners)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Destroy Banner Ad, hide it first. StopAllBanners["+stopAllBanners+"]");
		}

		HideBanner();
		#if FYBER_ON
		if (bannerAd != null)
		{
			if(_mustShowDebugInfo)
			{
				Debug.Log("Destroying Banner Ad definitely");
			}
			bannerAd.Destroy();
			bannerAd = null;
			if(OnAdStockChanged != null)
			{
				OnAdStockChanged(AD_FORMATS.BANNER);
			}
			if(!IsEmulated() && !DataManager.GetDataManager().IsNoAdsPurchased())
			{
				RequestBannerAd();
			}
		}
		#endif
		bannerAdActivated = !stopAllBanners;
	}

	public void HideBanner()
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Hide Banner Ad");
		}

		mustShowBannerAsSoonAsPossible = false;
		#if FYBER_ON
		if (bannerAd != null)
		{
			bannerAd.Hide();
		}
		if(OnBannerAdBecameInvisible != null)
		{
			OnBannerAdBecameInvisible();
		}
		#endif
		isBannerVisible = false;
	}

	public bool AdsInitialized
	{
		get{ return adsInitialized;}
	}

	public bool IsVideoAdReady()
	{
	#if FYBER_ON
		return (rewardedVideoAd != null || IsEmulated()) && adsInitialized;
	#else
		return IsEmulated() && adsInitialized;
	#endif
	}

	public bool IsAdBannerReady()
	{
	#if FYBER_ON
		return !DataManager.GetDataManager().IsNoAdsPurchased() &&
			((bannerAd != null || IsEmulated()) && adsInitialized);
	#else
	return //!DataManager.GetDataManager().IsNoAdsPurchased() &&
			(IsEmulated() && adsInitialized);
	#endif
	}

	public bool IsAdInterstitialReady()
	{
		#if FYBER_ON
		return !DataManager.GetDataManager().IsNoAdsPurchased() &&
			((interstitialAd != null || IsEmulated()) && adsInitialized);
		#else
		return //!DataManager.GetDataManager().IsNoAdsPurchased() &&
		(IsEmulated() && adsInitialized);
		#endif
	}

	public bool IsVideoAdVisible()
	{
		return videoAdActivated;
	}

	public bool IsAdBannerVisible()
	{
		return isBannerVisible;
	}

	public bool IsInterstitialVisible()
	{
		return interstitialAdActivated;
	}

	public uint GetVideosShownThisSession()
	{
		return videosShownThisSession;
	}

	public uint GetInterstitialsShownThisSession()
	{
		return interstitialsShownThisSession++;
	}

	// Update is called once per frame
	void Update () 
	{
		if (bannerAdActivated) 
		{
			if(!waitingBannerRefresh)
			{
				if(isBannerVisible)
				{
					if(currentBannerTime > 0.0f)
					{
						currentBannerTime -= Time.deltaTime;
						if(currentBannerTime < 0.0f)
						{
							if(_mustShowDebugInfo)
							{
								Debug.LogWarning("Refresh banner...");
							}
							waitingBannerRefresh = true;
							currentBannerTime = bannerRefreshRateInSeconds;
							//try to refresh banner
							DestroyBanner(false);

						}
					}
				}
			}
			else if(IsAdBannerReady())
			{
				waitingBannerRefresh = false;
				ShowBanner();
			}

		}
	}
		
	//DELEGATES
	void OnNativeError (string error)
	{
		adsInitialized = false;
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("Native Error On Ads["+error+"]");
		}
	}

#if FYBER_ON
	private void OnAdAvailable(Ad ad)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Ad Available["+ad.AdFormat+"] with Id["+ad.PlacementId+"]");
		}

		switch(ad.AdFormat)
		{
		case AdFormat.REWARDED_VIDEO:
			rewardedVideoAd = ad;
			isRequestingVideoAd = false;
			if(OnAdStockChanged != null)
			{
				OnAdStockChanged(AD_FORMATS.REWARDED_VIDEO);
			}
			break;
		case AdFormat.BANNER:
			bannerAd = (BannerAd)ad;
			isRequestingBannerAd = false;
			if(OnAdStockChanged != null)
			{
				OnAdStockChanged(AD_FORMATS.BANNER);
			}
			if(_mustShowDebugInfo)
			{
				Debug.Log("Showing Banner as availability changed. mustShowAsSoonAsPossible["+mustShowBannerAsSoonAsPossible+"]");
			}
			if(mustShowBannerAsSoonAsPossible)
			{
				ShowBanner();
			}
			break;
		case AdFormat.INTERSTITIAL:
			interstitialAd = ad;
			isRequestingInterstitialAd = false;
			if(OnAdStockChanged != null)
			{
				OnAdStockChanged(AD_FORMATS.INTERSTITIAL);
			}
			break;
		}

	}

	private void OnAdNotAvailable(AdFormat adFormat)
	{
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("Ad NOT Available["+adFormat+"] CurrentlyRequesting Video["+isRequestingVideoAd+"] Banner["+isRequestingBannerAd+"] Interstitial["+isRequestingInterstitialAd+"]");
		}
		switch(adFormat)
		{
		case AdFormat.REWARDED_VIDEO:
			rewardedVideoAd = null;
			isRequestingVideoAd = false;
			StartCoroutine("RequestVideoAds",onAdNotAvailableRetryTime);
			break;
		case AdFormat.BANNER:
			//bannerAd = null;
			isRequestingBannerAd = false;
			StartCoroutine("RequestBannerAds",onAdNotAvailableRetryTime);
			break;
		case AdFormat.INTERSTITIAL:
			interstitialAd = null;
			isRequestingInterstitialAd = false;
			StartCoroutine("RequestInterstitialAds",onAdNotAvailableRetryTime);
			break;
		}
	}

	private void OnRequestFail(RequestError error)
	{
		if(_mustShowDebugInfo)
		{
			// process error
			Debug.LogWarning("OnRequestFail with Error[" + error.Description+"]");
		}
		//ERROR Chart
		//RequestError					Description
		//DEVICE_NOT_SUPPORTED__________Only devices running Android API level 10 and above are supported
		//CONNECTION_ERROR______________Internet connection error
		//UNKNOWN_ERROR_________________An unknown error occurred
		//SDK_NOT_START_________________You need to start the SDK
		//NULL_CONTEXT_REFERENCE________Context reference cannot be null
		//SECURITY_TOKEN_NOT_PROVIDED___The security token was not provided when starting the SDK
		//ERROR_REQUESTING_ADS__________An error happened while trying to retrieve ads
		//UNABLE_TO_REQUEST_ADS_________The SDK is unable to request right now because it is either already performing a request or showing an ad
		switch(error.Description)
		{
		case "Only devices running Android API level 10 and above are supported":
		case "You need to start the SDK":
			adsInitialized = false;
			break;
		case "An error happened while trying to retrieve ads":
		case "Internet connection error":
		case "The SDK is unable to request right now because it is either already performing a request or showing an ad":
			//retry in 5 seconds
			if(rewardedVideoAd == null && isRequestingVideoAd)
			{
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Request video again...in ["+onAdNotAvailableRetryTime+"] seconds.");
				}
				isRequestingVideoAd = false;
			}
			if(bannerAd == null && isRequestingBannerAd)
			{
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Request banner again...in ["+onAdNotAvailableRetryTime+"] seconds.");
				}
				isRequestingBannerAd = false;
			}
			if(interstitialAd == null && isRequestingInterstitialAd)
			{
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Request interstitial again...in ["+onAdNotAvailableRetryTime+"] seconds.");
				}
				isRequestingInterstitialAd = false;
			}
			StartCoroutine("RequestAds",onAdNotAvailableRetryTime);
			break;
		}
	}

	private void OnAdStarted(Ad ad)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Ad Started["+ad.AdFormat+"] with Id["+ad.PlacementId+"]");
		}
		switch(ad.AdFormat)
		{
		case AdFormat.REWARDED_VIDEO:
			videoAdActivated = true;
			if(OnVideoAdStarted != null)
			{
				OnVideoAdStarted();			
			}
			if(OnAdStockChanged != null)
			{
				OnAdStockChanged(AD_FORMATS.REWARDED_VIDEO);
			}
			if(disableGameInputDuringFullScreenAds)
			{
				AppManager.GetInstance().DisableUIInput();
			}
			break;
		case AdFormat.BANNER:
			mustShowBannerAsSoonAsPossible = false;
			isBannerVisible = true;
			if(OnBannerAdStarted != null)
			{
				OnBannerAdStarted();
			}
			if(OnBannerAdBecameVisible != null)
			{
				OnBannerAdBecameVisible();
			}
			if(OnAdStockChanged != null)
			{
				OnAdStockChanged(AD_FORMATS.BANNER);
			}
			break;
		case AdFormat.INTERSTITIAL:
			interstitialAd = null;
			interstitialAdActivated = true;
			if(OnInterstitialAdStarted != null)
			{
				OnInterstitialAdStarted();			
			}
			if(OnAdStockChanged != null)
			{
				OnAdStockChanged(AD_FORMATS.INTERSTITIAL);
			}
			if(disableGameInputDuringFullScreenAds)
			{
				AppManager.GetInstance().DisableUIInput();
			}
			break;
			//handle other ad formats if needed
		}
	}

	private void OnAdFinished(AdResult result)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("ON ADD FINISHED FORMAT["+result.AdFormat+"] STATUS["+result.Status+"] MESSAGE["+result.Message+"]");
		}
		switch (result.AdFormat)
		{
		case AdFormat.REWARDED_VIDEO:
			if(_mustShowDebugInfo)
			{
				Debug.Log("Rewarded video closed with result ["+result.Status+"] and message["+result.Message+"]");
			}
			rewardedVideoAd = null;
			videoAdActivated = false;
			if(OnVideoAdFinished != null)
			{
				OnVideoAdFinished(result.Status == AdStatus.OK && result.Message.Equals("CLOSE_FINISHED"));
			}
			if(!Application.isEditor)
			{
				RequestVideoAd();
			}
			if(disableGameInputDuringFullScreenAds)
			{
				AppManager.GetInstance().EnableUIInput();
			}
			break;
		case AdFormat.INTERSTITIAL:
			if(_mustShowDebugInfo)
			{
				Debug.Log("Interstitial closed with result: " + result.Status +
				"and message: " + result.Message);
				// result.Status can have one of these values:
				// AdStatus			Description
				// AdStatus.OK		The Interstitial was closed successfully
				// AdStatus.Error	An error occurred while displaying the ad
				// if result.Status is equal to AdStatus.OK then result.Message can have one of these values:
				// value					Description
				// "ReasonUserClickedOnAd"	The Interstitial was closed because the user clicked on the ad.
				// "ReasonUserClosedAd"		The Interstitial was explicitly closed by the user.
				// "ReasonUnknown"			The Interstitial was dismissed for an unknown reason.
			}
			interstitialAdActivated = false;
			if(OnInterstitialAdFinished != null)
			{
				OnInterstitialAdFinished();
			}
			if(!Application.isEditor && !DataManager.GetDataManager().IsNoAdsPurchased()  && useInterstitials)
			{
				RequestInterstitialAd();
			}
			if(disableGameInputDuringFullScreenAds)
			{
				AppManager.GetInstance().EnableUIInput();
			}
			break;
			//hande other ad formats if needed (interstitials or offer walls...maybe)
		}
		//Result.Messages Chart
		//Value______________Meaning
		//"CLOSE_FINISHED"___The video has finished after completing. The user will be rewarded.
		//"CLOSE_ABORTED"____The video has finished before completing. The user might have aborted it, either explicitly (by tapping the close button) or implicitly (by switching to another app) or it was interrupted by an asynchronous event like an incoming phone call.
		//"ERROR"____________The video was interrupted or failed to play due to an error.
	}

	// Called when the banner triggers an error
	void OnBannerAdError(BannerAd ad, string error)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Banner Ad Error. Id["+ad.PlacementId+"] error["+error+"]");
		}
		if(ad == bannerAd)
		{
			DestroyBanner(false);
			if(!Application.isEditor && !DataManager.GetDataManager().IsNoAdsPurchased())
			{
				RequestBannerAd();
			}
		}
	}

	// Called when the banner was loaded
	void OnBannerAdLoaded(BannerAd ad)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Banner Ad New Content Loaded. Id["+ad.PlacementId+"]");
		}
	}

	// Called when the banner was clicked
	void OnBannerAdClicked(BannerAd ad)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Banner Ad Clicked. Id["+ad.PlacementId+"]");
		}
	}

	// Called when the banner interaction causes an external application to be open
	void OnBannerAdLeftApplication(BannerAd ad)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Banner Ad Left Application. Id["+ad.PlacementId+"]");
		}
	}

	// iOS only
	// The banner will present a modal view
	void OnBannerAdWillPresentModalView(BannerAd ad)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Banner Ad will present modal view. Id["+ad.PlacementId+"]");
		}
	}

	// iOS only
	// The user did dismiss a modal view
	void OnBannerAdDidDismissModalView(BannerAd ad)
	{
		if(_mustShowDebugInfo)
		{
			Debug.Log("Banner Ad did dismiss modal view. Id["+ad.PlacementId+"]");
		}
	}
#endif

	void OnApplicationPause(bool isPausing)
	{
		if(isPausing)
		{
			AnalyticsManager.GetInstance().RegisterWithParameters("AdsShownPerSession",new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>("videos",videosShownThisSession.ToString() ),
					new KeyValuePair<string, string>("interstitials",interstitialsShownThisSession.ToString())
				}
			);
		}
	}

	//#if UNITY_EDITOR
	void OnGUI()
	{
		if( (Application.isEditor ? emulateInEditor : emulateInDevice) && adsInitialized)
		{
			InitEmulationGUIStyle();

			if(videoAdActivated)
			{
				emulatedVideoCurrentDuration -= Time.deltaTime;
				if(emulatedVideoCurrentDuration > 0.0f)
				{
					GUI.TextArea(new Rect(0,0,Screen.width,Screen.height),"I AM A VIDEO AD WITH LEFT DURATION\n["+(Mathf.CeilToInt(emulatedVideoCurrentDuration))+"] SECONDS.",style);
				}
				else
				{
					emulatedVideoCurrentDuration = 0.0f;
					if(GUI.Button(new Rect(0,0,Screen.width,Screen.height),"I AM DONE CLICK ME TO CLOSE ME =P",style) )
					{
						videoAdActivated = false;
						if(OnVideoAdFinished != null)
						{
							OnVideoAdFinished(true);
						}
						if(bannerAdActivated && !isBannerVisible)
						{
							ShowBanner();
						}
						if(disableGameInputDuringFullScreenAds)
						{
							AppManager.GetInstance().EnableUIInput();
						}
					}
				}
			}
			else if(bannerAdActivated && isBannerVisible)
			{
				float height = 90;
				if(Screen.width <= 720)
				{
					height = 50;
				}
				if(displayBannerAtTop)	
				{
					GUI.TextArea(new Rect(0,0,Screen.width,height),"I AM A BANNER =P.",style);
				}
				else
				{
					GUI.TextArea(new Rect(0,Screen.height-height,Screen.width,height),"I AM A BANNER =P.",style);
				}
			}
			else if(interstitialAdActivated)
			{
				if(GUI.Button(new Rect(0,0,Screen.width,Screen.height),"I AM AN INTERSTITIAL AD\nCLICK ME TO CLOSE ME =P",style) )
				{
					interstitialAdActivated = false;
					if(OnInterstitialAdFinished != null)
					{
						OnInterstitialAdFinished();
					}
					if(bannerAdActivated && !isBannerVisible)
					{
						ShowBanner();
					}
					if(disableGameInputDuringFullScreenAds)
					{
						AppManager.GetInstance().EnableUIInput();
					}
				}
			}
		}
	}
	//#endif


}
