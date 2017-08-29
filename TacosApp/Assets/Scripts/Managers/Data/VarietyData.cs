using UnityEngine;
using Data;


[System.Serializable]
public struct VarietyData 
{
    public string id;
    public int version;
    public bool isDirty;
    public string name;
	public float price;

    public VarietyData(int defaultVersion = -1)
    {
        id = string.Empty;
        version = defaultVersion;
        isDirty = true;
        name = string.Empty;
        price = 0;
    }

    public void updateFrom (VarietyData readOnlyRemote, bool ignoreVersion = false)
	{
        //Si se ignora la version entonces no la cambiamos
        if (ignoreVersion)
        {
            version = readOnlyRemote.version;
        }
        Copy(readOnlyRemote);
        isDirty = false;
    }

	public void Copy(VarietyData other)
	{
        this.id = other.id;
        this.version = other.version;
        this.name = other.name;
		this.price = other.price;
	}

    public bool compareAndUpdate(VarietyData remote, bool ignoreVersion = false)
    {
        if (remote.version > version || ignoreVersion)
        {
            updateFrom(remote, ignoreVersion);
            return true;
        }

        return false;
    }


    public void AfterFirstRead()
    {

    }

    public string PriceToString()
	{
		if (price > 0) 
		{
			return "+" + price.ToString ("F2");
		} 
		else if (price < 0) 
		{
			return "-" + Mathf.Abs (price).ToString ("F2");
		} 
		else 
		{
			return "0.00";
		}
	}

    public void SetAsNotDirty()
    {
        isDirty = false;
    }
}
