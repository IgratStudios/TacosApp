using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Security.Cryptography;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// Serialization manager used to save data locally either in prefabs or in binary files.
/// </summary>
public class SerializationManager : Manager<SerializationManager>
{
	//ENCRYPTION AND DECRYPTION
	/// <summary>
	/// The must use encryption flag.
	/// </summary>
	public bool _mustUseEncryption = true;
	/// <summary>
	/// The server security key. Change it for every project. obtained from: 256 bit WEP key (http://randomkeygen.com/)
	/// </summary>
	public string _commonSecurityKey = "82727D458B45DBE1A3CD5E5A673F5";
	/// <summary>
	/// The is encryption available flag.
	/// </summary>
	private bool 	_isEncryptionAvailable = false;
	/// <summary>
	/// The device Id. Is unique to each device and will be used to generate a Local Key to encrypt/decrypt 
	/// local saved data.
	/// </summary>
	private string _deviceID = string.Empty;
	/// <summary>
	/// The client security key used to encrypt/decrypt local saved data.
	/// </summary>
	private string _clientSecurityKey = string.Empty;
	/// <summary>
	/// The dictionary with all the encripted keys for fast access.
	/// </summary>
	private Dictionary<string,string> _fastAccessEncryptedKeys = new Dictionary<string, string>(StringComparer.Ordinal);
	/// <summary>
	/// The triple DES service provider.
	/// </summary>
	TripleDESCryptoServiceProvider _tripleDes;
	/// <summary>
	/// The md5 hash provider.
	/// </summary>
	MD5CryptoServiceProvider _md5Hash;

	//PLAYER PREFS
	/// <summary>
	/// The first endian offset depending if the system is Little Endian or not.
	/// </summary>
	static private int endianDiff1;
	/// <summary>
	/// The second endian offset depending if the system is Little Endian or not.
	/// </summary>
	static private int endianDiff2;
	/// <summary>
	/// The index for converting to bytes functions.
	/// </summary>
	static private int idx;
	/// <summary>
	/// The byte block used to convert from and to bytes.
	/// </summary>
	static private byte [] byteBlock;
	/// <summary>
	/// Array type used to save arrays of some types.
	/// </summary>
	enum ArrayType {Float, Int32, Bool, String, Vector2, Vector3, Quaternion, Color}

	//FILE I/O
	/// <summary>
	/// The name of the local data folder.
	/// </summary>
	private const string _localDataFolderName = "AppData";
	/// <summary>
	/// The local data file extension.
	/// </summary>
	private const string _localDataFileExtension = ".igrat";
	/// <summary>
	/// The local file URL used to save and load binary files.
	/// </summary>
	private string _localFileURL = string.Empty;
	/// <summary>
	/// The bynaryformatter used to save and load data from binary files.
	/// </summary>
	BinaryFormatter _bynaryformatter;

	/// <summary>
	/// Awake this instance and register the instance of this gameObject.
	/// </summary>
	protected override void Awake ()
	{
		base.Awake ();
		if(_mustUseEncryption)
		{
			_tripleDes = new TripleDESCryptoServiceProvider();
			_md5Hash = new MD5CryptoServiceProvider();
			InitSecuritySystem();
		}
		_localFileURL = Application.persistentDataPath + "/" + _localDataFolderName + "_";
		_bynaryformatter = new BinaryFormatter();
		if(_mustShowDebugInfo)
		{
			Debug.Log("SaveURL["+_localFileURL+"]");
		}
	} 

	#region ENCRYPTION/DECRYPTION
	/// <summary>
	/// Inits the security system. Generating the Code and Decode unique keys
	/// </summary>
	private void InitSecuritySystem()
	{
		if(!_isEncryptionAvailable)
		{
			if(_mustShowDebugInfo)
			{
				Debug.Log("Generate Code and Decode Keys.");
			}
			GenerateCODECKey();
		}
	}

	/// <summary>
	/// Sets the unique identifier for this Device. It is not persistant between installs.
	/// </summary>
	private void SetUniqueIdentifier()
	{
		_deviceID = SystemInfo.deviceUniqueIdentifier;//not persistant between installs
	}

	/// <summary>
	/// Generates the CODE and DECODE key.
	/// </summary>
	private void GenerateCODECKey()
	{
		//Get/Set Device Unique Identifier (UDID)
		SetUniqueIdentifier();
		if(_mustShowDebugInfo)
		{
			Debug.Log("UDID ["+_deviceID+"]");
			Debug.Log("Common Key ["+_commonSecurityKey+"]");
			Debug.Log("Creating Client Unique Key...");
		}
		_clientSecurityKey = Encrypt(_deviceID,_commonSecurityKey);
		if(_mustShowDebugInfo)
		{
			Debug.Log("Unique Client Key ["+_clientSecurityKey+"]");
			//	to check functionality uncomment next code
			//	Debug.Log("To Check Functionality...");
			//	string tempUid = Decrypt(_clientSecurityKey,_serverSecurityKey);
			//	Debug.Log("Decrypted Key ["+tempUid+"] == ["+m_UID+"] =>["+tempUid.Equals(m_UID)+"]");
		}
		//from this moment all Encryption must be made with the generated client Key
		_isEncryptionAvailable = true;
		if(_mustShowDebugInfo)
		{
			Debug.Log("Encryption is now available!");
		}
	}

	/// <summary>
	/// Encodes the passed key.
	/// </summary>
	/// <returns>The encoded key.</returns>
	/// <param name="key">String key.</param>
	private string EncodeKey(string key)
	{
		if(_isEncryptionAvailable)
		{
			string encryptedString;
			if(!_fastAccessEncryptedKeys.TryGetValue(key,out encryptedString))
			{
				encryptedString = Encrypt(key,_clientSecurityKey);
				_fastAccessEncryptedKeys.Add(key,encryptedString);
			}
			return encryptedString;
		}
		return key;
	}
		
	/// <summary>
	/// Gets the Unique Device ID.
	/// </summary>
	/// <value>The UDI.</value>
	public string UDID
	{
		get 
		{
			return _deviceID;
		}
	}

	/// <summary>
	/// Gets the server key.
	/// </summary>
	/// <value>The server key.</value>
	public string CommonKey
	{
		get 
		{
			return _commonSecurityKey;
		}
	}

	/// <summary>
	/// Gets the client key.
	/// </summary>
	/// <value>The client key.</value>
	public string ClientKey
	{
		get 
		{
			return _clientSecurityKey;
		}
	}

	/// <summary>
	/// Encodes the int value passed.
	/// </summary>
	/// <returns>The int value encoded as a string</returns>
	/// <param name="val">Value.</param>
	public string EncodeInt(int val)
	{
		if(_isEncryptionAvailable)
		{
			return Encrypt(val.ToString(),_clientSecurityKey);
		}
		else
		{
			return val.ToString();
		}
	}

	/// <summary>
	/// Decodes the passed value into an int.
	/// </summary>
	/// <returns>The int obtained.</returns>
	/// <param name="str">String.</param>
	public int DecodeInt(string str)
	{
		int finalValue = 0;
		if(_isEncryptionAvailable)
		{
			string decryptedValue = Decrypt(str,_clientSecurityKey);
			Int32.TryParse(decryptedValue,out finalValue);
		}
		else
		{
			Int32.TryParse(str,out finalValue);
		}
		return finalValue;	
	}

	/// <summary>
	/// Encodes the float value passed.
	/// </summary>
	/// <returns>The float value encoded as a string.</returns>
	/// <param name="val">Value.</param>
	public string EncodeFloat(float val)
	{
		if(_isEncryptionAvailable)
		{
			return Encrypt(val.ToString(),_clientSecurityKey);
		}
		else
		{
			return val.ToString();
		}
	}

	/// <summary>
	/// Decodes the passed value into a float.
	/// </summary>
	/// <returns>The float.</returns>
	/// <param name="str">String.</param>
	public float DecodeFloat(string str)
	{
		float finalValue = 0;
		if(_isEncryptionAvailable)
		{
			string decryptedValue = Decrypt(str,_clientSecurityKey);
			float.TryParse(decryptedValue,out finalValue);
		}
		else
		{
			float.TryParse(str,out finalValue);
		}
		return finalValue;	
	}

	/// <summary>
	/// Encodes the double value passed.
	/// </summary>
	/// <returns>The double value encoded as a string.</returns>
	/// <param name="val">Value.</param>
	public string EncodeDouble(double val)
	{
		if(_isEncryptionAvailable)
		{
			return Encrypt(val.ToString(),_clientSecurityKey);
		}
		else
		{
			return val.ToString();
		}
	}

	/// <summary>
	/// Decodes the passed value to a double.
	/// </summary>
	/// <returns>The double.</returns>
	/// <param name="str">String.</param>
	public double DecodeDouble(string str)
	{
		double finalValue = 0;
		if(_isEncryptionAvailable)
		{
			string decryptedValue = Decrypt(str,_clientSecurityKey);
			double.TryParse(decryptedValue ,out finalValue);
		}
		else
		{
			double.TryParse(str ,out finalValue);
		}
		return finalValue;	
	}

	/// <summary>
	/// Encodes the string.
	/// </summary>
	/// <returns>The string encoded.</returns>
	/// <param name="str">String.</param>
	public string EncodeString(string str)
	{
		if(_isEncryptionAvailable)
		{
			return Encrypt(str,_clientSecurityKey);
		}
		else
		{
			return str;
		}
	}

	/// <summary>
	/// Decodes the string.
	/// </summary>
	/// <returns>The string decoded.</returns>
	/// <param name="str">String.</param>
	public string DecodeString(string str)
	{
		if(_isEncryptionAvailable)
		{
			return Decrypt(str,_clientSecurityKey);
		}
		else
		{
			return str;
		}
	}
		

	/// <summary>
	/// Encrypt the specified data and key.
	/// </summary>
	/// <remarks>>Taken from http://evolutionminds.blogspot.mx/2012/07/cifrado-con-triple-des.html</remarks>
	/// <param name="data">Data.</param>
	/// <param name="key">Key.</param>
	private string Encrypt(string data, string key)
	{
		byte[] buffer;
		try
		{
			//Se convierte la cadena en un arreglo de bytes
			buffer = UTF8Encoding.UTF8.GetBytes(data);
			//Definimos la llave de encripción, en este caso se me ocurrio incrementar la seguridad
			//usando como llave  de encripción, la hash MD5 de la llave que se especificada en el metodo
			_tripleDes.Key = _md5Hash.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
			//Se define el modo de cifrado que se desea utilizar
			_tripleDes.Mode = CipherMode.ECB;
			//Se crea el encriptor y se transforma el bloque de bits, se convierte a texto en base 64 para poderlo manejar
			//ya que asi es mas viable el uso sobre con HTTP
			ICryptoTransform crypto = _tripleDes.CreateEncryptor();
			byte[] result = crypto.TransformFinalBlock (buffer,0,buffer.Length);
			return Convert.ToBase64String(result,0,result.Length);
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("Error produced while encrypting data! {0} = [{1}] => {2}",key,data, ex.ToString());
			//throw ex;
			return data;
		}
	}

	/// <summary>
	/// Decrypt the specified data and key.
	/// </summary>
	/// <remarks>>Taken from http://evolutionminds.blogspot.mx/2012/07/cifrado-con-triple-des.html</remarks>
	/// <param name="data">Data.</param>
	/// <param name="key">Key.</param>
	private string Decrypt(string data, string key)
	{
		byte[] buffer;
		try
		{
			//Se convierte la cadena en un arreglo de bytes
			buffer = Convert.FromBase64String(data);
			//Definimos la llave de encripción, en este caso se me ocurrio incrementar la seguridad
			//usando como llave  de encripción, la hash MD5 de la llave que se especificada en el método
			_tripleDes.Key = _md5Hash.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
			//Se define el modo de cifrado que se desea utilizar
			_tripleDes.Mode = CipherMode.ECB;
			//Se crea el desencriptor y se transforma el bloque de bits, se convierte a texto en base 64 para poderlo manejar
			//ya que asi es mas viable el uso en ambientes de redes
			ICryptoTransform crypto = _tripleDes.CreateDecryptor();
			byte[] result = crypto.TransformFinalBlock(buffer, 0, buffer.Length);
			return UTF8Encoding.UTF8.GetString(result);
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("Error produced while decrypting data! {0} = [{1}] => {2}",key,data, ex.ToString());
			//throw ex;
			return data;
		}
	}
	#endregion

	#region FILE I/O
	/// <summary>
	/// Saves to file.
	/// </summary>
	/// <param name="uniqueId">Unique identifier.</param>
	/// <param name="data">Data.</param>
	public void SaveToFile(string uniqueId, string data)
	{
		string path = _localFileURL+uniqueId+_localDataFileExtension;
		FileStream file = File.Create (path);
		_bynaryformatter.Serialize(file, data);
		file.Close();
	}

	/// <summary>
	/// Loads from file.
	/// </summary>
	/// <returns>The value readed.</returns>
	/// <param name="uniqueId">Unique identifier.</param>
	public string LoadFromFile(string uniqueId)
	{
		string result = string.Empty;
		string path = _localFileURL+uniqueId+_localDataFileExtension;
		if(File.Exists(path)) 
		{
			FileStream file = File.Open(path, FileMode.Open);
			result = (string)_bynaryformatter.Deserialize(file);
			file.Close();
		}
		return result;
	}
	#endregion

	#region PLAYER PREFS
	/// <summary>
	/// Resets this instance endian offsets.
	/// </summary>
	private static void Initialize ()
	{
		if (System.BitConverter.IsLittleEndian)
		{
			endianDiff1 = 0;
			endianDiff2 = 0;
		}
		else
		{
			endianDiff1 = 3;
			endianDiff2 = 1;
		}
		if (byteBlock == null)
		{
			byteBlock = new byte[4];
		}
		idx = 1;
	}

	/// <summary>
	/// Saves the bytes.
	/// </summary>
	/// <returns><c>true</c>, if bytes was saved, <c>false</c> otherwise.</returns>
	/// <param name="key">Key.</param>
	/// <param name="bytes">Bytes.</param>
	private static bool SaveBytes (String key, byte[] bytes)
	{
		try
		{
			PlayerPrefs.SetString(_cachedInstance.EncodeKey(key),_cachedInstance.EncodeString(System.Convert.ToBase64String(bytes)));
		}
		catch
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Converts the int32 to bytes.
	/// </summary>
	/// <param name="i">The index.</param>
	/// <param name="bytes">Bytes.</param>
	private static void ConvertInt32ToBytes (int i, byte[] bytes)
	{
		byteBlock = System.BitConverter.GetBytes (i);
		ConvertTo4Bytes (bytes);
	}

	/// <summary>
	/// Converts the bytes to int32.
	/// </summary>
	/// <returns>The bytes to int32.</returns>
	/// <param name="bytes">Bytes.</param>
	private static int ConvertBytesToInt32 (byte[] bytes)
	{
		ConvertFrom4Bytes (bytes);
		return System.BitConverter.ToInt32 (byteBlock, 0);
	}

	/// <summary>
	/// Converts the float to bytes.
	/// </summary>
	/// <param name="i">The index.</param>
	/// <param name="bytes">Bytes.</param>
	private static void ConvertFloatToBytes (float i, byte[] bytes)
	{
		byteBlock = System.BitConverter.GetBytes (i);
		ConvertTo4Bytes (bytes);
	}

	/// <summary>
	/// Converts the bytes to float.
	/// </summary>
	/// <returns>The bytes to float.</returns>
	/// <param name="bytes">Bytes.</param>
	private static float ConvertBytesToFloat (byte[] bytes)
	{
		ConvertFrom4Bytes (bytes);
		return System.BitConverter.ToSingle(byteBlock, 0);
	}
		
	/// <summary>
	/// Converts the to4 bytes.
	/// </summary>
	/// <param name="bytes">Bytes.</param>
	private static void ConvertTo4Bytes (byte[] bytes)
	{
		bytes[idx  ] = byteBlock[    endianDiff1];
		bytes[idx+1] = byteBlock[1 + endianDiff2];
		bytes[idx+2] = byteBlock[2 - endianDiff2];
		bytes[idx+3] = byteBlock[3 - endianDiff1];
		idx += 4;
	}

	/// <summary>
	/// Converts the from4 bytes.
	/// </summary>
	/// <param name="bytes">Bytes.</param>
	private static void ConvertFrom4Bytes (byte[] bytes)
	{
		byteBlock[    endianDiff1] = bytes[idx  ];
		byteBlock[1 + endianDiff2] = bytes[idx+1];
		byteBlock[2 - endianDiff2] = bytes[idx+2];
		byteBlock[3 - endianDiff1] = bytes[idx+3];
		idx += 4;
	}

	/// <summary>
	/// Sets the value in  player prefs.
	/// </summary>
	/// <returns><c>true</c>, if value was set, <c>false</c> otherwise.</returns>
	/// <param name="key">Key.</param>
	/// <param name="array">Array.</param>
	/// <param name="arrayType">Array type.</param>
	/// <param name="vectorNumber">Vector number.</param>
	/// <param name="convert">Convert.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	private static bool SetValue<T> (String key, T array, ArrayType arrayType, int vectorNumber, Action<T, byte[],int> convert) where T : IList
	{
		byte[] bytes = new byte[(4*array.Count)*vectorNumber + 1];
		bytes[0] = System.Convert.ToByte (arrayType);	// Identifier
		Initialize();
		for (var i = 0; i < array.Count; i++) 
		{
			convert (array, bytes, i);	
		}
		return SaveBytes (key, bytes);
	}

	/// <summary>
	/// Gets the value form payer prefs.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="list">List.</param>
	/// <param name="arrayType">Array type.</param>
	/// <param name="vectorNumber">Vector number.</param>
	/// <param name="convert">Convert.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	private static void GetValue<T> (String key, T list, ArrayType arrayType, int vectorNumber, Action<T, byte[]> convert) where T : IList
	{
		string codedKey = _cachedInstance.EncodeKey(key);
		if ( HasCodedKey(codedKey) )
		{
			byte[] bytes = null;
			bytes = System.Convert.FromBase64String ( _cachedInstance.DecodeString( PlayerPrefs.GetString( codedKey ) ) );

			if ((bytes.Length-1) % (vectorNumber*4) != 0)
			{
				Debug.LogError ("Corrupt preference file for " + key);
				return;
			}
			if ((ArrayType)bytes[0] != arrayType)
			{
				Debug.LogError (key + " is not a " + arrayType.ToString() + " array");
				return;
			}

			Initialize();

			var end = (bytes.Length-1) / (vectorNumber*4);
			for (var i = 0; i < end; i++)
			{
				convert (list, bytes);
			}
		}
	}

	/// <summary>
	/// Converts from int.
	/// </summary>
	/// <param name="array">Array.</param>
	/// <param name="bytes">Bytes.</param>
	/// <param name="i">The index.</param>
	private static void ConvertFromInt (int[] array, byte[] bytes, int i)
	{
		ConvertInt32ToBytes (array[i], bytes);
	}

	/// <summary>
	/// Converts to int.
	/// </summary>
	/// <param name="list">List.</param>
	/// <param name="bytes">Bytes.</param>
	private static void ConvertToInt (List<int> list, byte[] bytes)
	{
		list.Add (ConvertBytesToInt32(bytes));
	}

	/// <summary>
	/// Converts from float.
	/// </summary>
	/// <param name="array">Array.</param>
	/// <param name="bytes">Bytes.</param>
	/// <param name="i">The index.</param>
	private static void ConvertFromFloat (float[] array, byte[] bytes, int i)
	{
		ConvertFloatToBytes (array[i], bytes);
	}

	/// <summary>
	/// Converts to float.
	/// </summary>
	/// <param name="list">List.</param>
	/// <param name="bytes">Bytes.</param>
	private static void ConvertToFloat (List<float> list, byte[] bytes)
	{
		list.Add (ConvertBytesToFloat(bytes));
	}
	#endregion

	/// <summary>
	/// Determines if the player prefs has key the specified.
	/// </summary>
	/// <returns><c>true</c> if the key was founded in PlayerPrefs; otherwise, <c>false</c>.The key must not be encoded.</returns>
	/// <param name="key">Key.</param>
	public static bool HasKey( string key )
	{
		if(_cachedInstance != null)
		{
			return PlayerPrefs.HasKey( _cachedInstance.EncodeKey(key) );
		}
		else
		{
			return PlayerPrefs.HasKey( key );
		}
	}

	/// <summary>
	/// Determines if has the coded key specified.
	/// </summary>
	/// <returns><c>true</c> if the coded key was founded in PlayerPrefs; otherwise, <c>false</c>.</returns>
	/// <param name="key">Key.</param>
	public static bool HasCodedKey( string key)
	{
		return PlayerPrefs.HasKey( key );
	}

	/// <summary>
	/// Removes the passed key.
	/// </summary>
	/// <param name="key">Key.</param>
	public static void RemoveKey( string key)
	{
		if(_cachedInstance != null)
		{
			if(HasCodedKey(key))
			{
				PlayerPrefs.DeleteKey(_cachedInstance.EncodeKey(key));
			}
		}
	}

	/// <summary>
	/// Sets the int using the key passed.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="val">Value.</param>
	public static void SetInt( string key, int val )
	{
		if(_cachedInstance != null)
		{
			PlayerPrefs.SetString(_cachedInstance.EncodeKey(key),_cachedInstance.EncodeInt(val));
		}
	}

	/// <summary>
	/// Gets the int using the key passed.
	/// </summary>
	/// <returns>The int.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public static int GetInt( string key, int defaultValue = 0)
	{
		if(_cachedInstance != null)
		{
			string codedKey = _cachedInstance.EncodeKey(key);
			if(PlayerPrefs.HasKey(codedKey))
			{
				return _cachedInstance.DecodeInt( PlayerPrefs.GetString( codedKey ) );
			}
			else//create it with the default value and return that
			{
				PlayerPrefs.SetString(codedKey,_cachedInstance.EncodeInt(defaultValue));
				return defaultValue;
			}
		}
		return PlayerPrefs.GetInt( key , defaultValue);
	}

	/// <summary>
	/// Sets the float using the key passed.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="val">Value.</param>
	public static void SetFloat( string key, float val)
	{
		if(_cachedInstance != null)
		{
			PlayerPrefs.SetString(_cachedInstance.EncodeKey(key),_cachedInstance.EncodeFloat(val));
		}
	}

	/// <summary>
	/// Gets the float using the key passed.
	/// </summary>
	/// <returns>The float.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public static float GetFloat( string key, float defaultValue = 0.0f)
	{
		if(_cachedInstance != null)
		{
			string codedKey = _cachedInstance.EncodeKey(key);
			if(PlayerPrefs.HasKey(codedKey))
			{
				return _cachedInstance.DecodeFloat( PlayerPrefs.GetString( codedKey ) );
			}
			else//create it with the default value and return that
			{
				PlayerPrefs.SetString(codedKey,_cachedInstance.EncodeFloat(defaultValue));
				return defaultValue;
			}
		}
		return PlayerPrefs.GetFloat( key , defaultValue);
	}

	/// <summary>
	/// Sets the double using the key passed.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="val">Value.</param>
	public static void SetDouble( string key, double val)
	{
		if(_cachedInstance != null)
		{
			PlayerPrefs.SetString(_cachedInstance.EncodeKey(key),_cachedInstance.EncodeDouble(val));
		}
	}

	/// <summary>
	/// Gets the double using the key passed.
	/// </summary>
	/// <returns>The double.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public static double GetDouble( string key, double defaultValue = 0.0)
	{
		if(_cachedInstance != null)
		{
			string codedKey = _cachedInstance.EncodeKey(key);
			if(PlayerPrefs.HasKey(codedKey))
			{
				return _cachedInstance.DecodeDouble( PlayerPrefs.GetString( codedKey ) );	
			}
			else//create it with the default value and return that
			{
				PlayerPrefs.SetString(codedKey,_cachedInstance.EncodeDouble(defaultValue));
				return defaultValue;
			}
		}
		return defaultValue;

	}

	/// <summary>
	/// Sets the double as string using the key passed.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="val">Value.</param>
	public static void SetDoubleAsString(string key,double val)
	{
		if(_cachedInstance != null)
		{
			PlayerPrefs.SetString(_cachedInstance.EncodeKey(key),_cachedInstance.EncodeString(val.ToString()));
		}
	}

	/// <summary>
	/// Gets the double as string using the key passed and parse it.
	/// </summary>
	/// <returns>The double.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public static double GetDoubleAsString(string key,double defaultValue = 0.0)
	{
		if(_cachedInstance != null)
		{
			string codedKey = _cachedInstance.EncodeKey(key);
			if(PlayerPrefs.HasKey(codedKey))
			{
				double result = 0.0;
				string strResult =  _cachedInstance.DecodeString( PlayerPrefs.GetString( codedKey ) );	
				if( double.TryParse(strResult,out result))
				{
					return result;
				}
				return defaultValue;
			}
			else//create it with the default value and return that
			{
				PlayerPrefs.SetString(codedKey,_cachedInstance.EncodeString(defaultValue.ToString()));
				return defaultValue;
			}
		}
		return defaultValue;
	}

	/// <summary>
	/// Sets the string using the key passed.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="val">Value.</param>
	public static void SetString( string key, string val )
	{
		if(_cachedInstance != null)
		{
			PlayerPrefs.SetString(_cachedInstance.EncodeKey(key),_cachedInstance.EncodeString(val));
		}
	}

	/// <summary>
	/// Gets the string using the key passed.
	/// </summary>
	/// <returns>The string.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public static string GetString( string key , string defaultValue = "")
	{
		if(_cachedInstance != null)
		{
			string codedKey = _cachedInstance.EncodeKey(key);
			if(PlayerPrefs.HasKey(codedKey))
			{
				return _cachedInstance.DecodeString( PlayerPrefs.GetString( codedKey ) );	
			}
			else//create it with the default value and return that
			{
				PlayerPrefs.SetString(codedKey,_cachedInstance.EncodeString(defaultValue));
				return defaultValue;
			}
		}
		return defaultValue;
	}

	/// <summary>
	/// Sets the bool using the key passed.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="val">If set to <c>true</c> value.</param>
	public static void SetBool( string key, bool val)
	{
		if(_cachedInstance != null)
		{
			PlayerPrefs.SetString( _cachedInstance.EncodeKey(key),_cachedInstance.EncodeInt( (val ? 1 : 0) ) );
		}
	}

	/// <summary>
	/// Gets the bool using the key passed.
	/// </summary>
	/// <returns><c>true</c>, if bool was gotten, <c>false</c> otherwise.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">If set to <c>true</c> default value.</param>
	public static bool GetBool( string key,bool defaultValue = false)
	{
		if(_cachedInstance != null)
		{
			string codedKey = _cachedInstance.EncodeKey(key);
			if(PlayerPrefs.HasKey(codedKey))
			{
				return (_cachedInstance.DecodeInt( PlayerPrefs.GetString( codedKey ) ) == 1);	
			}
			else//create it with the default value and return that
			{

				PlayerPrefs.SetString( codedKey,_cachedInstance.EncodeInt( (defaultValue ? 1 : 0) ) );
				return defaultValue;
			}
		}
		return defaultValue;
	}

	/// <summary>
	/// Sets the bool array using the key passed.
	/// </summary>
	/// <returns><c>true</c>, if bool array was set, <c>false</c> otherwise.</returns>
	/// <param name="key">Key.</param>
	/// <param name="boolArray">Bool array.</param>
	public static bool SetBoolArray (String key, bool[] boolArray)
	{
		if(_cachedInstance != null)
		{
			if (boolArray.Length == 0)
			{
				return false;
			}
			// Make a byte array that's a multiple of 8 in length, plus 5 bytes to store the number of entries as an int32 (+ identifier)
			// We have to store the number of entries, since the boolArray length might not be a multiple of 8, so there could be some padded zeroes
			var bytes = new byte[(boolArray.Length + 7)/8 + 5];
			bytes[0] = System.Convert.ToByte (ArrayType.Bool);	// Identifier
			var bits = new BitArray(boolArray);
			bits.CopyTo (bytes, 5);
			Initialize();
			ConvertInt32ToBytes (boolArray.Length, bytes); // The number of entries in the boolArray goes in the first 4 bytes

			return SaveBytes (key, bytes);	

		}
		return false;
	}

	/// <summary>
	/// Gets the bool array using the key passed.
	/// </summary>
	/// <returns>The bool array.</returns>
	/// <param name="key">Key.</param>
	public static bool[] GetBoolArray (String key)
	{
		if(_cachedInstance != null)
		{
			string codedKey = _cachedInstance.EncodeKey(key);

			if (HasCodedKey( codedKey))
			{
				
				byte[] bytes = null;
				bytes = System.Convert.FromBase64String ( _cachedInstance.DecodeString( PlayerPrefs.GetString( codedKey ) ) );

				if (bytes.Length < 6)
				{
					return new bool[0];
				}
				if ((ArrayType)bytes[0] != ArrayType.Bool)
				{
					return new bool[0];
				}
				Initialize();

				// Make a new bytes array that doesn't include the number of entries + identifier (first 5 bytes) and turn that into a BitArray
				var bytes2 = new byte[bytes.Length-5];
				System.Array.Copy(bytes, 5, bytes2, 0, bytes2.Length);
				var bits = new BitArray(bytes2);
				// Get the number of entries from the first 4 bytes after the identifier and resize the BitArray to that length, then convert it to a boolean array
				bits.Length = ConvertBytesToInt32 (bytes);
				var boolArray = new bool[bits.Count];
				bits.CopyTo (boolArray, 0);

				return boolArray;
			}
			return new bool[0];
		}
		return new bool[0];
	}
		
	/// <summary>
	/// Sets the int array using the key passed.
	/// </summary>
	/// <returns><c>true</c>, if int array was set, <c>false</c> otherwise.</returns>
	/// <param name="key">Key.</param>
	/// <param name="intArray">Int array.</param>
	public static bool SetIntArray (string key, int[] intArray)
	{
		if(_cachedInstance != null)
		{
			return SetValue(key, intArray, ArrayType.Int32, 1, ConvertFromInt);
		}
		return false;
	}

	/// <summary>
	/// Gets the int array using the key passed.
	/// </summary>
	/// <returns>The int array.</returns>
	/// <param name="key">Key.</param>
	public static int[] GetIntArray (string key)
	{
		List<int> intList = new List<int>();
		if(_cachedInstance != null)
		{
			GetValue (key, intList, ArrayType.Int32, 1, ConvertToInt);
		}
		return intList.ToArray();
	}
		
	/// <summary>
	/// Sets the float array using the key passed.
	/// </summary>
	/// <returns><c>true</c>, if float array was set, <c>false</c> otherwise.</returns>
	/// <param name="key">Key.</param>
	/// <param name="floatArray">Float array.</param>
	public static bool SetFloatArray (string key, float[] floatArray)
	{
		if(_cachedInstance != null)
		{
			return SetValue(key, floatArray, ArrayType.Float, 1, ConvertFromFloat);
		}
		return false;
	}

	/// <summary>
	/// Gets the float array using the key passed.
	/// </summary>
	/// <returns>The float array.</returns>
	/// <param name="key">Key.</param>
	public static float[] GetFloatArray (string key)
	{
		List<float> floatList = new List<float>();
		if(_cachedInstance != null)
		{
			GetValue (key, floatList, ArrayType.Float, 1, ConvertToFloat);
		}
		return floatList.ToArray();
	}
		
	/// <summary>
	/// Sets the string array using the key passed.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="stringArray">String array.</param>
	public static void SetStringArray (string key, string[] stringArray)
	{
		if(_cachedInstance != null)
		{
			PlayerPrefs.SetString(_cachedInstance.EncodeKey(key),_cachedInstance.EncodeString(stringArray.Length.ToString()));
			for(int i = 0; i < stringArray.Length; i++)
			{
				string newKey = key+"["+i+"]";
				PlayerPrefs.SetString(_cachedInstance.EncodeKey(newKey),_cachedInstance.EncodeString(stringArray[i]));
			}	
		}

	}

	/// <summary>
	/// Gets the string array using the key passed.
	/// </summary>
	/// <returns>The string array.</returns>
	/// <param name="key">Key.</param>
	public static string[] GetStringArray (string key)
	{
		List<string> stringList = new List<string>();
		if(_cachedInstance != null)
		{
			int arraySize = 0;
			PlayerPrefs.GetInt(_cachedInstance.EncodeKey(key));
			for(int i = 0; i < arraySize; i++)
			{
				string newKey = key+"["+i+"]";
				stringList.Add(PlayerPrefs.GetString(_cachedInstance.EncodeKey(newKey)));
			}
		}
		return stringList.ToArray();
	}

	/// <summary>
	/// Save the full object passed(only serializable fields) in a binary local file.
	/// </summary>
	/// <param name="dataToSave">Object to save(must be serializable).</param>
	/// <param name="uniqueId">Unique identifier for this object.</param>
	public static void LocalSave(object dataToSave,string uniqueId)
	{
		if(_cachedInstance != null)
		{
			string json = JsonUtility.ToJson(dataToSave,_cachedInstance._mustShowDebugInfo);
			if(_cachedInstance._mustShowDebugInfo)
			{
				Debug.Log("Save data json obtained["+json+"]");
			}
			if(_cachedInstance._isEncryptionAvailable)
			{
				//encrypt local json and save to binary file
				string encryptedJson = _cachedInstance.Encrypt(json,_cachedInstance._clientSecurityKey);
				_cachedInstance.SaveToFile(uniqueId,encryptedJson);
			}
			else
			{
				_cachedInstance.SaveToFile(uniqueId,json);	
			}
		}
	}

	/// <summary>
	/// Loads the object and overrides it with data founded in a local file.
	/// </summary>
	/// <param name="objectToLoad">Object to load(must be serializable).</param>
	/// <param name="uniqueId">Unique identifier for this object.</param>
	public static void LocalLoad(object objectToLoad ,string uniqueId)
	{
		if(_cachedInstance != null)
		{
			string loadedData = _cachedInstance.LoadFromFile(uniqueId);
			if(loadedData != string.Empty)
			{
				if(_cachedInstance._isEncryptionAvailable)
				{
					//decrypt json file
					string decryptedJson = _cachedInstance.Decrypt(loadedData,_cachedInstance._clientSecurityKey);
					if(_cachedInstance._mustShowDebugInfo)
					{
						Debug.Log("Load data json obtained["+decryptedJson+"]");
					}
					JsonUtility.FromJsonOverwrite(decryptedJson, objectToLoad);
				}
				else
				{
					if(_cachedInstance._mustShowDebugInfo)
					{
						Debug.Log("Load data json obtained["+loadedData+"]");
					}
					JsonUtility.FromJsonOverwrite(loadedData, objectToLoad);
				}
			}
		}
	}

	//TODO server save and load helper functions
}
