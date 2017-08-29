using Data;

[System.Serializable]
public struct CategoryData
{
    public string id;
    public int version;
    public bool isDirty;
    public string name;

    public CategoryData(int defaultVersion = -1)
    {
        id = string.Empty;
        version = defaultVersion;
        isDirty = true;
        name = string.Empty;
    }

	public void updateFrom (CategoryData readOnlyRemote, bool ignoreVersion = false)
	{
        //Si se ignora la version entonces no la cambiamos
        if (ignoreVersion)
        {
            version = readOnlyRemote.version;
        }
		Copy(readOnlyRemote);
        isDirty = false;
	}

	public void Copy(CategoryData other)
	{
        this.id = other.id;
        this.version = other.version;
        this.name = other.name;
	}

    public bool compareAndUpdate(CategoryData remote, bool ignoreVersion = false)
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

    public void SetAsNotDirty()
    {
        isDirty = false;
    }

}
