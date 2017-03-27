using UnityEngine;
using System.Collections;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

/// <summary>
/// Save local test for partial object with serilization on file(json on file) and helper class.
/// </summary>
public class SaveLocalTest_PartialObj : CachedMonoBehaviour 
{
	/// <summary>
	/// Internal data helping class to save what it is interesting to save.
	/// </summary>
	[System.Serializable]
	class InternalData
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
		/// The float array data.
		/// </summary>
		public float[] _fArrData;
		/// <summary>
		/// The string array data.
		/// </summary>
		public string[] _strArrData;
	}

	/// <summary>
	/// The serializable data helper class.
	/// </summary>
	private InternalData _serializableData;

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
	/// The float array data.
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
	/// Save this instance usign helper class.
	/// </summary>
	public void Save()
	{
		if(_serializableData == null)
		{
			_serializableData = new InternalData();
		}
		//update data to serializable class
		_serializableData._iData = _iData;
		_serializableData._fData = _fData;
		_serializableData._strData = _strData;
		_serializableData._iArrData = _iArrData;
		_serializableData._fArrData = _fArrData;
		_serializableData._strArrData = _strArrData;
		SerializationManager.LocalSave(_serializableData,CachedGameObject.name);
	}

	/// <summary>
	/// Load this instance using helper class.
	/// </summary>
	public void Load()
	{
		if(_serializableData == null)
		{
			_serializableData = new InternalData();
		}
		SerializationManager.LocalLoad(_serializableData,CachedGameObject.name);
		_iData = _serializableData._iData;
		_fData = _serializableData._fData;
		_strData = _serializableData._strData;
		_iArrData = _serializableData._iArrData;
		_fArrData = _serializableData._fArrData;
		_strArrData = _serializableData._strArrData;
	}

	/// <summary>
	/// Raised by the mouse up event.
	/// </summary>
	void OnMouseUp()
	{
		Print();
	}

}