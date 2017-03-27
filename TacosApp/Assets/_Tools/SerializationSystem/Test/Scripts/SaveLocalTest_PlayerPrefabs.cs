using UnityEngine;
using System.Collections;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

/// <summary>
/// Save local test using player prefabs.
/// </summary>
public class SaveLocalTest_PlayerPrefabs : CachedMonoBehaviour 
{
	/// <summary>
	/// The int data.
	/// </summary>
	public int _iData = 0;
	/// <summary>
	/// The float data.
	/// </summary>
	public float _fData = 0.0f;
	/// <summary>
	/// The string data.
	/// </summary>
	public string _strData = string.Empty;
	/// <summary>
	/// The int array data.
	/// </summary>
	public int[] _iArrData;
	/// <summary>
	/// The float arr data.
	/// </summary>
	public float[] _fArrData;
	/// <summary>
	/// The string array data.
	/// </summary>
	public string[] _strArrData;

	/// <summary>
	/// Print this instance data.
	/// </summary>
	public void Print()
	{
		string print = "["+CachedGameObject.name+"]:\n";
		print += 	"iData["+_iData+"]\n" +
			"fData["+_fData+"]\n" +
			"strData["+_strData+"]\n" +
			"iArrData["+_iArrData.Length+"]\n";
		for(int i = 0; i < _iArrData.Length; i++)
		{
			print += 	"["+i+"]->["+_iArrData[i]+"]\n";
		}
		print += 	"fArrData["+_fArrData.Length+"]\n";
		for(int i = 0; i < _fArrData.Length; i++)
		{
			print += 	"["+i+"]->["+_fArrData[i]+"]\n";
		}
		print += 	"strArrData["+_strArrData.Length+"]\n";
		for(int i = 0; i < _strArrData.Length; i++)
		{
			print += 	"["+i+"]->["+_strArrData[i]+"]\n";
		}
		Debug.Log(print);
	
	}

	/// <summary>
	/// Reset this instance data.
	/// </summary>
	public void Reset()
	{
		_iData = 0;
		_iArrData = new int[0];
		_fData = 0;
		_fArrData = new float[0];
		_strData = string.Empty;
		_strArrData = new string[0];

	}

	/// <summary>
	/// Save this instance using player prefs.
	/// </summary>
	public void Save()
	{
		SerializationManager.SetInt("iData",_iData);
		SerializationManager.SetFloat("fData",_fData);
		SerializationManager.SetString("strData",_strData);
		SerializationManager.SetIntArray("iArrData",_iArrData);
		SerializationManager.SetFloatArray("fArrData",_fArrData);
		SerializationManager.SetStringArray("strArrData",_strArrData);
	}

	/// <summary>
	/// Load this instance using player prefs.
	/// </summary>
	public void Load()
	{
		_iData = SerializationManager.GetInt("iData");
		_fData = SerializationManager.GetFloat("fData");
		_strData = SerializationManager.GetString("strData");
		_iArrData = SerializationManager.GetIntArray("iArrData");
		_fArrData = SerializationManager.GetFloatArray("fArrData");
		_strArrData = SerializationManager.GetStringArray("strArrData");
	}

	/// <summary>
	/// Raised by the mouse up event.
	/// </summary>
	void OnMouseUp()
	{
		Print();
	}

}
