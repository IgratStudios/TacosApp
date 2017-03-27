using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Custom asset(ScriptableObject's) creator.
/// </summary>
public static class CustomAssetCreator
{
	/*
   *  Function: Creates an Asset of the type passed on the current selected folder.
   *  Parameters: <ScriptableObjectType> None
   *  Return: None
   */
	public static void CreateAsset<T> ()  where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T> ();

		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		if (path == "") //if is not founded
		{
			path = "Assets";
		} 
		else if (Path.GetExtension (path) != "") //if there is another file with the same path and extension
		{
			path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
		}

		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New " + typeof(T).ToString() + ".asset");

		AssetDatabase.CreateAsset (asset, assetPathAndName);

		AssetDatabase.SaveAssets ();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = asset;
	}

	/*
   *  Function: Creates an Asset of the type passed on the folder path passed
   *  Parameter: <ScriptableObjectType> Type of ScriptableObject to create
   *  Parameter: folderPath is the path where the asset will be created, should not include Assets and 
   *             if the path does not exist it will create it
   *  Parameter: assetName is the proposedName for the new asset.           
   *  Return: None
   */
	public static T CreateAsset<T>(string folderPath, string assetName) where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T> ();

		folderPath = CreateIfPathDoesNotExist(folderPath);

		string assetPath = folderPath +"/"+ assetName + ".asset";
		Object existingAsset = AssetDatabase.LoadAssetAtPath(assetPath,typeof(T));
		if (existingAsset != null)
		{
			assetPath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + assetName + ".asset");
		}
		//Debug.Log("Creating Asset At Path["+assetPath+"]");

		AssetDatabase.CreateAsset (asset, assetPath);

		AssetDatabase.SaveAssets ();

		Selection.activeObject = asset;
		EditorUtility.FocusProjectWindow();
		return asset;
	}

	/*
    *  Function: Creates a folder structure if it does not exist.
    *  Parameter: path is the path that will be checked for existance and create it if does not exist.          
    *  Return: The full Path for asset creation use.
    */
	public static string CreateIfPathDoesNotExist(string path)
	{
		string[] folders = path.Split('/');
		string folderFullPath = "Assets";

		for (int i = 0; i < folders.Length; i++ )
		{
			string oldFullPath = string.Copy(folderFullPath);
			folderFullPath += "/"+folders[i];

			if (!System.IO.Directory.Exists(folderFullPath))
			{
				Debug.Log("Folder Does not Exist[" + folderFullPath + "]");

				string newfolderPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(oldFullPath, folders[i]));
				Debug.Log("Folder created at[" + newfolderPath + "]");
			}
		}
		return folderFullPath;
	}


}