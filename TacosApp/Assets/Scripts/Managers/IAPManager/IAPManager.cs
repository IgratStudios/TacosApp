using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Data;

#if UNITY_IAPS_ON
using UnityEngine.Purchasing;
#endif

public enum IAP_STORE
{
	NONE,
	GOOGLEPLAY,
	APPLEAPPSTORE,
	WINDOWSSTORE,
	TIZENSTORE,
	SAMSUNGAPPS,
	AMAZONAPPS,
	MACAPPSTORE

}
	
[ExecuteInEditMode]
// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
public class IAPManager : LocalDataManager<PromoData> 
#if UNITY_IAPS_ON
, IStoreListener
#endif
{
	public delegate void StoreInitializedWithResult(bool success);
	public static StoreInitializedWithResult OnStoreInitialized;

	public delegate void PromoChanged();
	public static PromoChanged OnPromoStarted;
	public static PromoChanged OnPromoEnded;

	public delegate void PromoCooldownChanged();
	public static PromoCooldownChanged OnPromoCooldownChanged;

	public delegate void ProductPurchased(PRODUCT_VALUE_TYPE productType, uint valueDelivered);
	public static ProductPurchased OnProductPurchased;

	public bool simulatePurchases = false;

	public uint promoDurationInMinutes = 10;

	#if UNITY_IAPS_ON
	public delegate void PurchaseComplete(bool success,string productId,ProductType type);
	public static PurchaseComplete OnProductPurchased;
	public delegate void PurchaseCompleteWithError(PurchaseFailureReason errorReason);
	#else
	public List<string> consumableGameplayIds = new List<string>();
	#endif

	/// <summary>
	/// Called when a purchase complete with error.
	/// Notes:
	/// PurchasingUnavailable -->	The system purchasing feature is unavailable.
	/// ExistingPurchasePending->   A purchase was already in progress when a new purchase was requested.
	/// ProductUnavailable ->		The product is not available to purchase on the store.
	/// SignatureInvalid ->			Signature validation of the purchase's receipt failed.
	/// UserCancelled ->			The user opted to cancel rather than proceed with the purchase.
	/// PaymentDeclined ->			There was a problem with the payment.
	/// Unknown ->					A catch-all for unrecognized purchase problems.
	/// </summary>
	#if UNITY_IAPS_ON
	public static PurchaseCompleteWithError OnPurchaseFailedWithError;

	private static IStoreController storeController;          // The Unity Purchasing system.
	private static IExtensionProvider storeExtensionProvider; // The store-specific Purchasing subsystems.
	#endif

	// Product identifiers for all products capable of being purchased: 
	// "convenience" general identifiers for use with Purchasing, and their store-specific identifier 
	// counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers 
	// also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)
	
	// General product identifiers for the consumable, non-consumable, and subscription products.
	// Use these handles in the code to reference which product to purchase. Also use these values 
	// when defining the Product Identifiers on the store. Except, for illustration purposes, the 
	// kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
	// specific mapping to Unity Purchasing's AddProduct, below.
	public IAPProduct[] allProducts;

	public string promoRelatedProductId = "NoAds";

	public Dictionary<string,IAPProduct> productsMap;

	private bool isWaitingInitialization = false;

	public DateTime lastPromoStartDate;
	public uint iapPopUpShowedTimesToStartPromo = 10;
	public uint miniIAPPopUpShowedTimesToStartPromo = 10;

	public bool IsInitializing
	{
		get
		{
			return isWaitingInitialization;
		}
	}

//	public static string kProductIDConsumable =    "consumable";   
//	public static string kProductIDNonConsumable = "nonconsumable";
//	public static string kProductIDSubscription =  "subscription"; 

	// Apple App Store-specific product identifier for the subscription product.
//	private static string kProductNameAppleSubscription =  "com.unity3d.subscription.new";

	// Google Play Store-specific product identifier subscription product.
//	private static string kProductNameGooglePlaySubscription =  "com.unity3d.subscription.original"; 

	public override void StartManager ()
	{
		base.StartManager ();
		if(isThisManagerValid)
		{
			Initialize();
			SessionManager.OnSessionValidated += Initialize;

			InvokeRepeating("UpdatePromoTime",1,1);
		}
	}

	protected override void fillDefaultData()
	{
		getCurrentData().fillDefaultData();
	}

	private void Initialize()
	{
		// If we haven't set up the Unity Purchasing reference
		if (!IsInitialized() && !isWaitingInitialization)
		{
			CreateProductsMap();

			lastPromoStartDate = SessionManager.GetInstance().LoadDateWithId("lastPromo",true);

			if(_mustShowDebugInfo)
			{
				Debug.Log("LastPromostartedAt["+lastPromoStartDate.ToString()+"]");
			}
				
			// Begin to configure our connection to Purchasing
			InitializePurchasingModule();
		}
	}

	public static IAPManager GetIAPManager()
	{
		return GetCastedInstance<IAPManager>();
	}

	public void CreateProductsMap()
	{
		if(productsMap == null)
		{
			productsMap = new Dictionary<string, IAPProduct>();
			for(int i = 0; i < allProducts.Length; i++)
			{
				productsMap.Add(allProducts[i].productId,allProducts[i]);
			}
		}
	}

	public IAPProduct GetProductWithId(string id, bool forceNoPromo = false)
	{
		IAPProduct product = null;
		if(!productsMap.TryGetValue(id,out product))
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("Product with ID["+id+"] NOT founded!");
			}
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.Log("Product with ID["+id+"] founded! ["+product.productId+"] Promo["+product.promoRelatedId+"]");
			}
			if(product.promoRelatedId != string.Empty && currentData.IsPromoActive && !forceNoPromo)
			{
				if(_mustShowDebugInfo)
				{
					Debug.Log("Getting Promo["+product.promoRelatedId+"] product.");
				}
				if(!productsMap.TryGetValue(product.promoRelatedId,out product))
				{
					if(_mustShowDebugInfo)
					{
						Debug.LogWarning("Promo Product with ID["+product.promoRelatedId+"] NOT founded!");
					}
				}
			}
		}
		return product;
	}

	public void InitializePurchasingModule() 
	{
		// If we have already connected to Purchasing ...
		if (IsInitialized() || isWaitingInitialization)
		{
			// ... we are done here.
			return;
		}
		isWaitingInitialization = true;
		#if UNITY_IAPS_ON
		// Create a builder, first passing in a suite of Unity provided stores.
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
	
		// Add a product to sell / restore by way of its identifier, associating the general identifier
		// with its store-specific identifiers.
		//builder.AddProduct(kProductIDConsumable, ProductType.Consumable);
		// Continue adding the non-consumable product.
		//builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);
		int i = 0;
		for(i = 0; i < allProducts.Length; i++)
		{
			if(allProducts[i].type != ProductType.Subscription)
			{
				builder.AddProduct(allProducts[i].productId,allProducts[i].type,allProducts[i].GetIDS());
			}
		}
		// And finish adding the subscription product. Notice this uses store-specific IDs, illustrating
		// if the Product ID was configured differently between Apple and Google stores. Also note that
		// one uses the general kProductIDSubscription handle inside the game - the store-specific IDs 
		// must only be referenced here. 
//		builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs(){
//			{ kProductNameAppleSubscription, AppleAppStore.Name },
//			{ kProductNameGooglePlaySubscription, GooglePlay.Name },
//		});
		for(i = 0; i < allProducts.Length; i++)
		{
			if(allProducts[i].type == ProductType.Subscription)
			{
				builder.AddProduct(allProducts[i].productId,allProducts[i].type,allProducts[i].GetIDS());
			}
		}
	
		// Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
		// and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
		UnityPurchasing.Initialize(this, builder);
		#endif
	}

	//  
	// --- IStoreListener
	//

	public bool IsInitialized()
	{
		#if UNITY_IAPS_ON
		// Only say we are initialized if both the Purchasing references are set.
		return storeController != null && storeExtensionProvider != null;
		#else
		return false;
		#endif
	}

	public uint GetCurrentValue(string productId)
	{
		IAPProduct relatedProduct = GetProductWithId(productId);
		if(relatedProduct != null)
		{
			return relatedProduct.productValue;
		}
		return (uint)0;
	}

	public bool HasPurchasedProductWithId(string productId)
	{
		IAPProduct relatedProduct = GetProductWithId(productId);
		if(relatedProduct != null)
		{
			return currentData.HasPurchased(relatedProduct.gameplayId);
		}
		return false;
	}

	private void DeliverPurchasedProduct(string productId)
	{
		bool canDeliver = false;
		//get product without promo
		IAPProduct relatedProduct = GetProductWithId(productId,true);
		#if UNITY_IAPS_ON
		switch(relatedProduct.type)
		{
		case ProductType.NonConsumable:
			canDeliver = !currentData.HasPurchased(relatedProduct.gameplayId);
		break;
		case ProductType.Consumable:
			canDeliver = true;
		break;
		case ProductType.Suscription:
			//TODO:suscription specific requirements
		break;
		default:break;
		}
		#else
		canDeliver = !currentData.HasPurchased(relatedProduct.gameplayId) || consumableGameplayIds.Contains(relatedProduct.gameplayId);
		#endif

		if(canDeliver)
		{
			bool mustEndPromo = false;
			//deliver by type
			switch(relatedProduct.valueType)
			{
			case PRODUCT_VALUE_TYPE.SOFT_CURRENCY:
				
				break;
			case PRODUCT_VALUE_TYPE.NO_ADS:
				
				AdsManager.GetInstance().DisableAds();
				if(currentData.IsPromoActive)
				{
					mustEndPromo = true;
				}
				break;
			default:
				//TODO: add other value types
				break;
			}
			//mark as purchased
			currentData.AddPurchase(relatedProduct.gameplayId);
			//save
			saveLocalData();
			if(OnProductPurchased != null)
			{
				OnProductPurchased(relatedProduct.valueType,relatedProduct.productValue);
			}

			if(mustEndPromo)
			{
				SetPromoEnded();	
			}
		}

	}

	public string GetCurrentPriceString(string productId)
	{
		IAPProduct relatedProduct = GetProductWithId(productId);
		if(relatedProduct != null)
		{
			if(_mustShowDebugInfo)
			{
				Debug.Log("Product with ID["+productId+"] founded as ["+relatedProduct.productId+"]");
			}
			return relatedProduct.GetPriceString();
		}
		else
		{
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("Product with ID["+productId+"] NOT founded!");
			}
		}
		return "NA";
	}

	#if UNITY_IAPS_ON


	/// <summary>
	/// Called when Unity IAP is ready to make purchases.
	/// </summary>
	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		isWaitingInitialization = false;
		// Purchasing has succeeded initializing. Collect our Purchasing references.
		if(_mustShowDebugInfo)
		{
			Debug.Log("OnInitialized: PASS ["+(controller != null)+"]["+extensions != null+"]");
		}

		// Overall Purchasing system, configured with products for this application.
		storeController = controller;
		// Store specific subsystem, for accessing device-specific store features.
		storeExtensionProvider = extensions;

		if(_mustShowDebugInfo)
		{
			Debug.Log("Registering products["+(controller.products.all != null ? controller.products.all.Length.ToString() : "NO PRODUCTS" )+"]");
		}
		for(int i = 0; i < controller.products.all.Length; i++)
		{
			IAPProduct product = GetProductWithId(controller.products.all[i].definition.id);
			if(product !=  null)
			{
				allProducts[i].SetAvailability(controller.products.all[i].availableToPurchase);
				allProducts[i].productData = controller.products.all[i].metadata;
				if(_mustShowDebugInfo && !allProducts[i].IsAvailableForPruchase)
				{
					Debug.LogWarning("Product not available To Purchase["+controller.products.all[i].definition.id+"]");
				}
			}
			else if(_mustShowDebugInfo)
			{
				Debug.LogWarning("METADATA NOT ASSIGNED FOR PRODUCT["+controller.products.all[i].definition.id+"]");
			}
		}


		if(OnStoreInitialized != null)
		{
			OnStoreInitialized(true);
		}
	}

	/// <summary>
	/// Called when Unity IAP encounters an unrecoverable initialization error.
	///
	/// Note that this will not be called if Internet is unavailable; Unity IAP
	/// will attempt initialization until it becomes available.
	/// </summary>
	public void OnInitializeFailed(InitializationFailureReason error)
	{
		isWaitingInitialization = false;
		storeController = null;
		storeExtensionProvider = null;
		// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning("OnInitializeFailed InitializationFailureReason:" + error);
		}
		if(OnStoreInitialized != null)
		{
			OnStoreInitialized(false);
		}
	}

	/// <summary>
	/// Called when a purchase completes.
	///
	/// May be called at any time after OnInitialized().
	/// </summary>
	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
	{
		// A consumable product has been purchased by this user.
		if(_mustShowDebugInfo)
		{
			Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
		}

		DeliverPurchasedProduct(args.purchasedProduct.definition.id);	

		if(OnProductPurchased != null)
		{
			OnProductPurchased(true,args.purchasedProduct.definition.id,args.purchasedProduct.definition.type);
		}

		// Return a flag indicating whether this product has completely been received, or if the application needs 
		// to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
		// saving purchased products to the cloud, and when that save is delayed. 
		return PurchaseProcessingResult.Complete;
	}

	/// <summary>
	/// Called when a purchase fails.
	/// </summary>
	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
		// this reason with the user to guide their troubleshooting actions.
		if(_mustShowDebugInfo)
		{
			Debug.LogWarning(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
		}
		if(OnProductPurchased != null)
		{
			OnProductPurchased(false,product.definition.id,product.definition.type);
		}
		if(OnPurchaseFailedWithError != null)
		{
			OnPurchaseFailedWithError(failureReason);
		}
	}
	#endif

	/// <summary>
	/// Buys the product with id passed if is valid.
	/// </summary>
	/// <returns><c>true</c>, if operation started correctly, <c>false</c> otherwise.</returns>
	/// <param name="productId">Product identifier.</param>
	public bool BuyProductWithId(string productId)
	{
		// Buy the subscription product using its the general identifier. Expect a response either 
		// through ProcessPurchase or OnPurchaseFailed asynchronously.
		// Notice how we use the general product identifier in spite of this ID being mapped to
		// custom store-specific identifiers above.
	//	BuyProductID(kProductIDSubscription);
		IAPProduct iapProduct = GetProductWithId(productId);
		if(iapProduct != null)
		{
			return BuyProductID(iapProduct.productId);	
		}
		return false;

	}

	bool BuyProductID(string productId)
	{
		if(simulatePurchases)
		{
			DeliverPurchasedProduct(productId);
			return true;
		}

		// If Purchasing has been initialized ...
		if (IsInitialized())
		{
			#if UNITY_IAPS_ON
			// ... look up the Product reference with the general product identifier and the Purchasing 
			// system's products collection.
			Product product = storeController.products.WithID(productId);
	
			// If the look up found a product for this device's store and that product is ready to be sold ... 
			if (product != null && product.availableToPurchase)
			{
				if(_mustShowDebugInfo)
				{
					Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				}
				// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
				// asynchronously.
				storeController.InitiatePurchase(product);
				return true;
			}
			// Otherwise ...
			else
			{
				// ... report the product look-up failure situation  
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
				}
			}
			#endif
			return false;
		}
		// Otherwise ...
		else
		{
			// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
			// retrying initiailization.
			if(_mustShowDebugInfo)
			{
				Debug.Log("BuyProductID FAIL. IAP Store Not initialized.");
			}
			return false;
		}
	}
		
	// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
	// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
	public void RestorePurchases()
	{
		// If Purchasing has not yet been set up ...
		if (!IsInitialized())
		{
			// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
			if(_mustShowDebugInfo)
			{
				Debug.Log("RestorePurchases FAIL. IAP Store Not initialized.");
			}
			return;
		}
	
		// If we are running on an Apple device ... 
		if (Application.platform == RuntimePlatform.IPhonePlayer || 
			Application.platform == RuntimePlatform.OSXPlayer)
		{
			// ... begin restoring purchases
			if(_mustShowDebugInfo)
			{
				Debug.Log("RestorePurchases started ...");
			}
	
			#if UNITY_IAPS_ON
			// Fetch the Apple store-specific subsystem.
			var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
			// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
			// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
			apple.RestoreTransactions((result) => 
				{
				// The first phase of restoration. If no more responses are received on ProcessPurchase then 
				// no purchases are available to be restored.
					if(_mustShowDebugInfo)
					{
						Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
					}
			});
			#endif
		}
		// Otherwise ...
		else
		{
			// We are not running on an Apple device. No work is necessary to restore purchases.
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
			}
		}
	}

	public bool AddIAPPopUpShowed()
	{
		bool canActivePromo = (promoRelatedProductId != string.Empty ? !HasPurchasedProductWithId(promoRelatedProductId) : true);
		bool promoStarted = currentData.IsPromoActive;
		if(canActivePromo)
		{
			promoStarted = currentData.AdIAPPopUpShowed(iapPopUpShowedTimesToStartPromo);
			if(promoStarted)
			{
				StartPromoCooldown();
			}
			saveLocalData();
		}
		return promoStarted;
	}

	public bool AddIAPMiniPopUpShowed()
	{
		bool canActivePromo = (promoRelatedProductId != string.Empty ? !HasPurchasedProductWithId(promoRelatedProductId) : true);
		bool promoStarted = currentData.IsPromoActive;
		if(canActivePromo)
		{
			promoStarted = currentData.AdIAPMiniPopUpShowed(miniIAPPopUpShowedTimesToStartPromo);
			if(promoStarted)
			{
				StartPromoCooldown();
			}
			saveLocalData();
		}
		return promoStarted;
	}

	public bool IsPromoActive()
	{
		return currentData.IsPromoActive;
	}

	private void StartPromoCooldown()
	{
		lastPromoStartDate = SessionManager.GetNow();
		SessionManager.GetInstance().SaveDateTimeWithId("lastPromo",lastPromoStartDate);
		if(OnPromoStarted != null)
		{
			OnPromoStarted();
		}
	}

	private void SetPromoEnded()
	{
		getCurrentData().EndPromo();
		saveLocalData();
		if(OnPromoEnded != null)
		{
			OnPromoEnded();
		}
	}

	public void UpdatePromoTime()
	{
		if(getCurrentData().IsPromoActive)
		{
			if(IsPromoEndedAtDate( SessionManager.GetNow() ))
			{
				SetPromoEnded();
			}
			else
			{
				if(_mustShowDebugInfo)
				{
					Debug.Log("Time Till Promo End["+GetTimeTillPromoEnd( SessionManager.GetNow() ).TotalSeconds+"] seconds.");
				}
				if(OnPromoCooldownChanged != null)
				{
					OnPromoCooldownChanged();
				}
			}
		}
	}

	public TimeSpan GetTimeTillPromoEnd(DateTime now)
	{
		if(getCurrentData().IsPromoActive)
		{
			if(_mustShowDebugInfo)
			{
				Debug.Log("Now["+now.ToString()+"] PromoStartedAt["+lastPromoStartDate.ToString()+"]");
			}
			TimeSpan diff = (lastPromoStartDate.AddMinutes(promoDurationInMinutes) - now);
			return diff;
		}
		return new TimeSpan(now.Ticks);
	}

	public bool IsPromoEndedAtDate(DateTime now)
	{
		if(getCurrentData().IsPromoActive)
		{
			TimeSpan diff = now - lastPromoStartDate;
			bool result =  diff.TotalSeconds >= promoDurationInMinutes*60;
			if(_mustShowDebugInfo)
			{
				Debug.Log("PromoActive?["+result+"] Diff["+diff.TotalSeconds+"] seconds. At Now["+now.ToString()+"] PromostartedAt["+lastPromoStartDate.ToString()+"]");
			}
			return result;
		}
		return false;
	}

	#if UNITY_EDITOR
	public bool eraseLocalData = false;
	void Update()
	{
		if(eraseLocalData && !Application.isPlaying)
		{
			eraseLocalData = false;
			if(UnityEditor.EditorUtility.DisplayDialog("Erase Promo Data?","Are you sure to erase Promo Local Data?","YES","NO"))
			{
				deleteData();
			}
		}
	}
	#endif

}
	