using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UIScreenManager))]
[CanEditMultipleObjects]
public class UIScreenManagerEditor : Editor 
{
	static UIScreenManager lastUIScreenManager;
	static bool lastUIScreenManagerWasDespawned = false;
	UIScreenManager uiScreenManager;
	SerializedObject serializedObj;
	MonoScript script;
	bool _isPopUp = false;

	protected void OnEnable()
	{
		uiScreenManager = (UIScreenManager)target;
		serializedObj = serializedObject;
		script = MonoScript.FromMonoBehaviour((UIScreenManager)target);
	}

	protected void OnDisable()
	{
		lastUIScreenManager = uiScreenManager;
	}

	protected void OnDestroy()
	{
		if (target == null && !Application.isPlaying)
		{
			if(_isPopUp)
			{
				//send remove instruction if possible
				PopUpsManager popUpManager = FindObjectOfType<PopUpsManager>();
				if(popUpManager != null)
				{
					popUpManager.RemoveUIPopUpBkgScreenManagerController(lastUIScreenManager._uniqueScreenId,lastUIScreenManagerWasDespawned);
					popUpManager.RemoveUIPopUpScreenManagerController(lastUIScreenManager._uniqueScreenId,lastUIScreenManagerWasDespawned);
				}
			}
			else
			{
				//send remove instruction if possible
				UIManager uiManager = FindObjectOfType<UIManager>();
				if(uiManager != null)
				{
					uiManager.RemoveUIScreenManagerController(lastUIScreenManager._uniqueScreenId,lastUIScreenManagerWasDespawned);
				}
			}

			lastUIScreenManagerWasDespawned = false;
			lastUIScreenManager = null;
		}
	}

	public override void OnInspectorGUI () 
	{
		bool wasDestroyed = false;
		if(uiScreenManager != null && serializedObj != null)
		{
			GUI.enabled = false;
			script = EditorGUILayout.ObjectField("Script:", script, typeof(MonoScript), false) as MonoScript;
			GUI.enabled = true;
			// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
			serializedObj.Update();
			_isPopUp = serializedObj.FindProperty("_isPopUp").boolValue;
			if(Selection.objects.Length == 1)
			{
				PrefabType pType = PrefabUtility.GetPrefabType(uiScreenManager.gameObject);
				bool isPrefab = pType == PrefabType.Prefab;
				if(!isPrefab)
				{
					if(GUILayout.Button("Update Id In Manager"))
					{
						if(_isPopUp)
						{
							PopUpsManager popUpsManager = FindObjectOfType<PopUpsManager>();
							if(popUpsManager != null)
							{
								UpdateIdOnPopUpsManager(popUpsManager);
							}
						}
						else
						{
							UIManager uiManager = FindObjectOfType<UIManager>();
							if(uiManager != null)
							{
								UpdateIdOnUIManager(uiManager);
							}
						}
					}
					if(GUILayout.Button("Switch "+(uiScreenManager.gameObject.activeSelf ? "OFF" : "ON" )))
					{
						if(_isPopUp)
						{
							PopUpsManager popUpManager = FindObjectOfType<PopUpsManager>();
							if(popUpManager != null)
							{
								popUpManager.SwitchById(uiScreenManager._uniqueScreenId,!uiScreenManager.gameObject.activeSelf);
							}
						}
						else
						{
							UIManager uiManager = FindObjectOfType<UIManager>();
							if(uiManager != null)
							{
								uiManager.SwitchScreenById(uiScreenManager._uniqueScreenId,!uiScreenManager.gameObject.activeSelf);
							}
						}
					}
					if(GUILayout.Button("Enable as SOLO"))
					{
						if(_isPopUp)
						{
							PopUpsManager popUpManager = FindObjectOfType<PopUpsManager>();
							if(popUpManager != null)
							{
								popUpManager.SwitchSolo(uiScreenManager._uniqueScreenId);
							}
						}
						else
						{
							UIManager uiManager = FindObjectOfType<UIManager>();
							if(uiManager != null)
							{
								uiManager.SwitchToScreenWithId(uiScreenManager._uniqueScreenId);
								ArrayList sceneViews = UnityEditor.SceneView.sceneViews;
								if(sceneViews != null)
								{
									if(sceneViews.Count > 0)
									{
										UnityEditor.SceneView sceneView = (UnityEditor.SceneView) sceneViews[0];
										sceneView.AlignViewToObject(uiScreenManager.transform);
									}
								}
							}
						}

					}
					if(GUILayout.Button("Despawn"))
					{
						wasDestroyed = true;
						lastUIScreenManagerWasDespawned = true;
						DestroyImmediate(uiScreenManager.gameObject);
					}
					if(GUILayout.Button("Destroy And Remove"))
					{
						wasDestroyed = true;
						DestroyImmediate(uiScreenManager.gameObject);
					}
				}
			}
		}
		if(target != null && !wasDestroyed)
		{
			//DrawDefaultInspector();
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_mustShowDebugInfo"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_mustRegisterForBackOperations"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_mustActiveRecursively"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_mustSurviveSceneChange"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_uniqueScreenId"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_isPopUp"));
			EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(serializedObj.FindProperty("_screenCameraDepth"));
			if(EditorGUI.EndChangeCheck())
			{
				//try to update camera depth
				Camera camera = uiScreenManager.gameObject.GetComponent<Camera>();
				if(camera != null)
				{
					float depth = serializedObj.FindProperty("_screenCameraDepth").floatValue;
					camera.depth = depth;
				}
			}
		}
		if(serializedObject != null && !wasDestroyed)
		{
			// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
			serializedObject.ApplyModifiedProperties();
		}
	}

	void UpdateIdOnUIManager(UIManager uiManager)
	{
		for(int i = 0; i < uiManager._allScreenControllers.Count; i++)
		{
			if(uiManager._allScreenControllers[i]._uiScreenObject == uiScreenManager)
			{
				string oldId = uiManager._allScreenControllers[i]._uiUniqueId;
				uiManager._allScreenControllers[i]._uiUniqueId = uiScreenManager._uniqueScreenId;
				for(int j = 0; j < uiManager._allScreenControllers.Count; j++)
				{
					if(j != i)
					{
						uiManager._allScreenControllers[j].UpdateComplementIds(oldId,uiManager._allScreenControllers[i]._uiUniqueId);
					}
				}
			}
			if(uiManager._allScreenControllers[i]._uiScreenPrefab != null)
			{
				uiManager._allScreenControllers[i]._uiScreenPrefab._uniqueScreenId = uiManager._allScreenControllers[i]._uiUniqueId;
			}
			AssetDatabase.SaveAssets();
		}
	}

	void UpdateIdOnPopUpsManager(PopUpsManager popUpsManager)
	{
		for(int i = 0; i < popUpsManager._allPopUpControllers.Count; i++)
		{
			if(popUpsManager._allPopUpControllers[i]._uiScreenObject == uiScreenManager)
			{
				popUpsManager._allPopUpControllers[i]._uiUniqueId = uiScreenManager._uniqueScreenId;
			}
			if(popUpsManager._allPopUpControllers[i]._uiScreenPrefab != null)
			{
				popUpsManager._allPopUpControllers[i]._uiScreenPrefab._uniqueScreenId = popUpsManager._allPopUpControllers[i]._uiUniqueId;
			}
		}

		for(int i = 0; i < popUpsManager._allPopUpBackgroundsControllers.Count; i++)
		{
			bool popUpsUpdated = false;
			string oldId = popUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId;
			if(popUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject == uiScreenManager)
			{
				popUpsUpdated = true;
				popUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId = uiScreenManager._uniqueScreenId;
				//update on related popups
				for(int j = 0; j < popUpsManager._allPopUpControllers.Count; j++)
				{
					popUpsManager._allPopUpControllers[j].UpdateComplementIds(oldId,popUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId);
				}
			}
			if(popUpsManager._allPopUpBackgroundsControllers[i]._uiScreenPrefab != null)
			{
				if(!popUpsUpdated)
				{
					popUpsUpdated = true;
					//update on related popups
					for(int j = 0; j < popUpsManager._allPopUpControllers.Count; j++)
					{
						popUpsManager._allPopUpControllers[j].UpdateComplementIds(oldId,popUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId);
					}
				}
				popUpsManager._allPopUpBackgroundsControllers[i]._uiScreenPrefab._uniqueScreenId = popUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId;
			}
		}
		AssetDatabase.SaveAssets();
	}

}
