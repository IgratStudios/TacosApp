using UnityEngine;
using System.Collections;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.
#pragma warning disable 0649 // array is never assigned to, and will always have its default value `null'

/// <summary>
/// Save local test for full object serialization(json on file).
/// </summary>
public class SaveLocalTest_FullObj : CachedMonoBehaviour 
{
	/// <summary>
	/// Internal data (a simple serializable class).
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

		/// <summary>
		/// Print datas with the specified startInfo as a prefix.
		/// </summary>
		/// <param name="startInfo">Start info.</param>
		public void Print(string startInfo = "")
		{
			string print = startInfo+"[InternalData]:\n";
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
			print += 	"+++++++++++++++++++++++++++++++++++++++++++++\n";
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
	}
		
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
	/// The data class.
	/// </summary>
	[SerializeField]
	private InternalData _data;
	/// <summary>
	/// The array of data class.
	/// </summary>
	[SerializeField]
	private InternalData[] _arrData;

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
		_data.Print();
		for(int i = 0; i < _arrData.Length; i++)
		{
			_arrData[i].Print("["+i+"]");
		}
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
		_data.Reset();
		_arrData = new InternalData[0];
	}

	/// <summary>
	/// Save this instance.
	/// </summary>
	public void Save()
	{
		SerializationManager.LocalSave(this,CachedGameObject.name);
	}

	/// <summary>
	/// Load this instance.
	/// </summary>
	public void Load()
	{
		SerializationManager.LocalLoad(this,CachedGameObject.name);
	}

	/// <summary>
	/// Raised by the mouse up event.
	/// </summary>
	void OnMouseUp()
	{
		Print();
	}


}
