using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoreID
{
	public string storeSpecificId;
	public IAP_STORE[] relatedStores;
}

public enum PRODUCT_VALUE_TYPE
{
	SOFT_CURRENCY,
	HARD_CURRENCY,
	NO_ADS,
	OTHER
}

[System.Serializable]
public class IAPProduct
{
	public string productId;//in-game
	public string promoRelatedId;
	public string gameplayId;//used to save purchased products that are related to each other(like no ads & no ads promo)
	public string placeHolderPrice;
	public uint productValue = 0;
	public PRODUCT_VALUE_TYPE valueType;
	#if UNITY_IAPS_ON
	public ProductType type;
	#endif
	public bool alreadyOwned = false;
	public StoreID[] IDs;
	#if UNITY_IAPS_ON
	private IDs idsMap;
	public ProductMetadata productData;
	#endif
	private bool isAvailable;

	public bool IsAvailableForPruchase
	{
		get
		{
			return isAvailable;
		}
	}

	public void SetAvailability(bool canBePurchased)
	{
		isAvailable = canBePurchased;
	}

	private void CreateIDsMap()
	{
		#if UNITY_IAPS_ON
		idsMap = new UnityEngine.Purchasing.IDs();
		#endif
		for(int i = 0; i < IDs.Length; i++)
		{
			List<string> stores = new List<string>();
			#if UNITY_IAPS_ON
			for(int j = 0; j < IDs[i].relatedStores.Length; j++)
			{
				switch(IDs[i].relatedStores[j])
				{
				case IAP_STORE.APPLEAPPSTORE:
					stores.Add(AppleAppStore.Name);
				break;
				case IAP_STORE.GOOGLEPLAY:
					stores.Add(GooglePlay.Name);
				break;
				case IAP_STORE.AMAZONAPPS:
					stores.Add(AmazonApps.Name);
				break;
				case IAP_STORE.SAMSUNGAPPS:
					stores.Add(SamsungApps.Name);
				break;
				case IAP_STORE.WINDOWSSTORE:
					stores.Add(WindowsStore.Name);
				break;
				case IAP_STORE.TIZENSTORE:
					stores.Add(TizenStore.Name);
				break;
				case IAP_STORE.MACAPPSTORE:
					stores.Add(MacAppStore.Name);
				break;
				default:break;
				}
			}
			#endif
			if(stores.Count > 0)//if there is a valid Store
			{
				#if UNITY_IAPS_ON
				idsMap.Add(IDs[i].storeSpecificId,stores.ToArray());
				#endif
			}
		}
	}

	public string GetPriceString()
	{
		#if UNITY_IAPS_ON
		if(productData != null)
		{
			return productData.localizedPriceString;
		}
		#endif
		return placeHolderPrice;
	}

	#if UNITY_IAPS_ON
	public IDs GetIDS()
	{
		if(idsMap == null)
		{
			CreateIDsMap();
		}
		return idsMap;
	}
	#endif

}
