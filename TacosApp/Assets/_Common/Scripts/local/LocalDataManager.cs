using UnityEngine;
using System;
using System.IO;
using System.Collections;

namespace Data
{
	public class LocalDataManager<T> : Manager<LocalDataManager<T>> where T:BasicData
	{
		public string ANONYMOUS_USER = "anon_user";
		public string currentUserId = "anon_user";

		public string mainDirectoryName = "Data";
		public string fileName 			= "data.json";

		protected T currentData;

		public override void StartManager()
		{
			base.StartManager();

			createLocalFileStructure();

			//Nueva data
			if (!existLocalData()) 
			{
				saveLocalData (true);
			}
			else
			{
				readLocalData();
			}

		}

		private void createLocalFileStructure()
		{
			if(!Directory.Exists(getMainFolderPath()))
			{
				Directory.CreateDirectory(getMainFolderPath());
			}
		}

		public string getMainFolderPath()
		{
			return Application.persistentDataPath + "/" + mainDirectoryName;
		}

		public string getLocalDataPath()
		{
			return getMainFolderPath() + "/" + fileName;
		}

		public void deleteData()
		{
			File.Delete (getLocalDataPath());

			saveLocalData (true);

			readLocalData();
		}

		public bool existLocalData()
		{
			return File.Exists (getLocalDataPath ());
		}

		public void saveLocalData(bool rewriteEmpty = false)
		{
			if (rewriteEmpty) 
			{
				currentData = Activator.CreateInstance<T>();
				fillDefaultData();
			}

			FileStream stream = new FileStream(getLocalDataPath(),FileMode.Create,FileAccess.Write,FileShare.None);
			StreamWriter streamWriter = new StreamWriter(stream);
			string jsonData = JsonUtility.ToJson(currentData,true);
			streamWriter.Write(jsonData);
			streamWriter.Close();
			streamWriter.Dispose();
			stream.Close();
			stream.Dispose();
		}

		protected virtual void fillDefaultData(){}

		public void readLocalData()
		{
			string json = File.ReadAllText(getLocalDataPath());
			currentData = JsonUtility.FromJson<T>(json);
			afterFirstRead();
		}

		protected virtual void afterFirstRead(){}
			

		public T getCurrentData()
		{
			return currentData;
		}

		public virtual void changeCurrentuser(string newUserId)
		{
			currentUserId = newUserId;
		}

		public virtual void setUserAsAnonymous()
		{
			currentUserId = ANONYMOUS_USER;
		}
	}
}