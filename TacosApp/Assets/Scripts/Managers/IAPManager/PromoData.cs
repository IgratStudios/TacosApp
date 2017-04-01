using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

[System.Serializable]
public class PromoData : BasicData 
{

	[SerializeField]
	private bool isPromoActive = false;
	[SerializeField]
	private uint iapPopUpShowedTimes = 0;
	[SerializeField]
	private uint iapMiniPopUpShowedTimes = 0;
	[SerializeField]
	private List<string> purchased;

	public void fillDefaultData ()
	{
		isPromoActive = false;
		iapPopUpShowedTimes = 0;
		iapMiniPopUpShowedTimes = 0;
		purchased = new List<string>();
	}

	public override void updateFrom (BasicData readOnlyRemote, bool ignoreVersion = false)
	{
		//PromoData remote = (PromoData)readOnlyRemote;
		base.updateFrom (readOnlyRemote,ignoreVersion);
	}

	public bool IsPromoActive
	{
		get
		{
			return isPromoActive;
		}
	}

	public bool AddPurchase(string id)
	{
		if(!purchased.Contains(id))
		{
			purchased.Add(id);
			return true;
		}
		return false;
	}

	public bool HasPurchased(string id)
	{
		return purchased.Contains(id);
	}

	private void StartPromo()
	{
		isPromoActive = true;
	}

	public void EndPromo()
	{
		isPromoActive = false;
	}

	public bool AdIAPPopUpShowed(uint promoLimit)
	{
		iapPopUpShowedTimes++;
		if(promoLimit <= iapPopUpShowedTimes)
		{
			StartPromo();
			iapPopUpShowedTimes = 0;
			iapMiniPopUpShowedTimes = 0;
			return true;
		}
		return false;
	}

	public bool AdIAPMiniPopUpShowed(uint promoLimit)
	{
		iapMiniPopUpShowedTimes++;
		if(promoLimit <= iapMiniPopUpShowedTimes)
		{
			StartPromo();
			iapPopUpShowedTimes = 0;
			iapMiniPopUpShowedTimes = 0;
			return true;
		}
		return false;
	}


}
