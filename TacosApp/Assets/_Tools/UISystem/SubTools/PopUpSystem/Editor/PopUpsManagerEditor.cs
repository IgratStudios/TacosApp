using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PopUpsManager))]
public class PopUpsManagerEditor : Editor 
{
	PopUpsManager uiPopUpsManager;
	SerializedObject serializedObj;
	MonoScript script;

	protected void OnEnable()
	{
		uiPopUpsManager = (PopUpsManager)target;
		serializedObj = serializedObject;
		script = MonoScript.FromMonoBehaviour((PopUpsManager)target);
	}

	public override void OnInspectorGUI () 
	{
		GUI.enabled = false;
		script = EditorGUILayout.ObjectField("Script:", script, typeof(MonoScript), false) as MonoScript;
		GUI.enabled = true;
		if(uiPopUpsManager != null && serializedObj != null)
		{
			// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
			serializedObj.Update();
			if(!Application.isPlaying)
			{
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Spawn All Pop Ups"))
				{
					SpawnAllPopUps();
				}
				if(GUILayout.Button("Despawn All Pop Ups"))
				{
					DespawnAllPopUps();
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Spawn All Backgrounds"))
				{
					SpawnAllPopUpBackgrounds();
				}
				if(GUILayout.Button("Despawn All Backgrounds"))
				{
					DespawnAllBackgrounds();
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Create new Pop Up"))
				{
					CreateNewScreenPopUp();
				}
				if(GUILayout.Button("Create new Background"))
				{
					CreateNewScreenPopUpBackground();
				}
				GUILayout.EndHorizontal();
				if(GUILayout.Button("Create Controller from Selected Pop Up(Lock!)"))
				{
					CreatePopUpControllersFromSelected();
				}
				if(GUILayout.Button("Create Controller from Selected Background(Lock!)"))
				{
					CreatePopUpBkgControllersFromSelected();
				}
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Update Pop Up Ids"))
				{
					UpdatePopUpIds();
				}
				if(GUILayout.Button("Update Background Ids"))
				{
					UpdatePopUpBkgIds();
				}
				GUILayout.EndHorizontal();
				if(GUILayout.Button("Update All References"))
				{
					UpdatePrefabs();
				}
			}
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Enable All"))
			{
				uiPopUpsManager.SwitchAllPopUps(true);
			}
			if(GUILayout.Button("Disable All"))
			{
				uiPopUpsManager.SwitchAllPopUps(false);
			}
			GUILayout.EndHorizontal();
			if(GUILayout.Button("Recalculate Positions"))
			{
				uiPopUpsManager.ResetAllPositions();
			}
			bool eliminated = false;
			if(!Application.isPlaying)
			{
				if(GUILayout.Button("Reset"))
				{
					ResetManager();
					eliminated = true;
				}
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_mustAutoStart"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_mustPersistSceneChange"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_mustShowDebugInfo"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_justDisableOnPopUpDeactivation"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasScalerMode"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasScalerScreenMatchMode"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasMatchModeRange"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasScalerPixelsPerUnit"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasScalerReferenceResolution"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_popupsPositionNumberOfColumns"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_addHelpFrameToCreatedPopUps"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_popUpHelpFramePrefab"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_popUpBkgHelpFramePrefab"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_screenSeparation"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_disponibilityProvider"));
			SerializedProperty prohibitedList = serializedObj.FindProperty("_screenProhibitedForPopUps");
			EditorGUILayout.PropertyField(prohibitedList,true);
			SerializedProperty mask =  serializedObj.FindProperty("_systemLayer");
			mask.intValue = EditorGUILayout.LayerField("System Layer",mask.intValue);
			if(EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
			SerializedProperty list = serializedObj.FindProperty("_allPopUpControllers");
			EditorGUILayout.PropertyField(list);
			bool added = false;
			if(list.isExpanded)
			{
				if(GUILayout.Button("+"))
				{
					//list.InsertArrayElementAtIndex(list.arraySize);
					uiPopUpsManager.CreateNewEmptyPopUpControllerAlone();
					added = true;
				}
				else
				{
					//EditorGUILayout.SelectableLabel("Size   "+list.FindPropertyRelative("Array.size").intValue);
					if(!eliminated)
					{
						for (int i = 0; i < list.arraySize; i++) 
						{
							SerializedProperty element = list.GetArrayElementAtIndex(i);
							string elementId = element.FindPropertyRelative("_uiUniqueId").stringValue;
							EditorGUILayout.BeginHorizontal();
							element.isExpanded = EditorGUILayout.Foldout(element.isExpanded,elementId);
							if(GUILayout.Button("-",EditorStyles.miniButton,GUILayout.MaxWidth(30)))
							{
								uiPopUpsManager.RemoveUIPopUpScreenManagerController(elementId,false);
								break;
							}
							EditorGUILayout.EndHorizontal();

							if(element.isExpanded)
							{
								EditorGUILayout.PropertyField(element);
								EditorGUIHelper.LineSeparator();
							}
							EditorGUIHelper.LineSeparator();
						}
					}
				}
			}
			if(!added)
			{
				SerializedProperty bkgList = serializedObj.FindProperty("_allPopUpBackgroundsControllers");
				EditorGUILayout.PropertyField(bkgList);
				if(bkgList.isExpanded)
				{
					if(GUILayout.Button("+"))
					{
						//bkgList.InsertArrayElementAtIndex(bkgList.arraySize);
						uiPopUpsManager.CreateNewEmptyPopUpBackgroundControllerAlone();
					}
					else
					{
						//EditorGUILayout.SelectableLabel("Size   "+list.FindPropertyRelative("Array.size").intValue);
						if(!eliminated)
						{
							for (int i = 0; i < bkgList.arraySize; i++) 
							{
								SerializedProperty element = bkgList.GetArrayElementAtIndex(i);
								string elementId = element.FindPropertyRelative("_uiUniqueId").stringValue;
								EditorGUILayout.BeginHorizontal();
								element.isExpanded = EditorGUILayout.Foldout(element.isExpanded,elementId);
								if(GUILayout.Button("-",EditorStyles.miniButton,GUILayout.MaxWidth(30)))
								{
									uiPopUpsManager.RemoveUIPopUpBkgScreenManagerController(elementId,false);
									break;
								}
								EditorGUILayout.EndHorizontal();

								if(element.isExpanded)
								{
									EditorGUILayout.PropertyField(element);
									EditorGUIHelper.LineSeparator();
								}
								EditorGUIHelper.LineSeparator();
							}

						}
					}
				}
			}
			if(serializedObject != null)
			{
				// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
				serializedObject.ApplyModifiedProperties();
			}
		}
	}

	void SpawnAllPopUps()
	{
		Undo.RegisterCreatedObjectUndo(uiPopUpsManager,uiPopUpsManager.name);
		for(int i = 0; i < uiPopUpsManager._allPopUpControllers.Count; i++)
		{
			if(uiPopUpsManager._allPopUpControllers[i]._uiScreenObject == null && 
				uiPopUpsManager._allPopUpControllers[i]._uiScreenPrefab != null)
			{						
				GameObject instancedGO = (GameObject)PrefabUtility.InstantiatePrefab(uiPopUpsManager._allPopUpControllers[i]._uiScreenPrefab.gameObject);
				if(instancedGO != null )
				{
					UIScreenManager instancedUISM = instancedGO.GetComponent<UIScreenManager>();
					if(instancedUISM != null)
					{
						uiPopUpsManager._allPopUpControllers[i]._uiScreenObject = instancedGO.GetComponent<UIScreenManager>();
						if(uiPopUpsManager._allPopUpControllers[i]._uiScreenObject == null)//delete newly created object
						{
							DestroyImmediate(instancedGO);
							EditorUtility.DisplayDialog("Cant create.","Cant Spawn PopUp Screen["+uiPopUpsManager._allPopUpControllers[i]._uiUniqueId+"].","Ok");
						}
						else
						{
							uiPopUpsManager._allPopUpControllers[i]._uiScreenObject.CachedTransform.SetSiblingIndex( uiPopUpsManager.GetLastPopUpSiblingIndex());
							uiPopUpsManager._allPopUpControllers[i].Switch(false,true);
						}
					}
				}
				else
				{
					Debug.Log("Cant spawn["+uiPopUpsManager._allPopUpControllers[i]._uiScreenPrefab.name+"]");
				}
			}	
		}
	}

	void SpawnAllPopUpBackgrounds()
	{
		Undo.RegisterCreatedObjectUndo(uiPopUpsManager,uiPopUpsManager.name);
		for(int i = 0; i < uiPopUpsManager._allPopUpBackgroundsControllers.Count; i++)
		{
			if(uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject == null && 
				uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenPrefab != null)
			{						
				GameObject instancedGO = (GameObject)PrefabUtility.InstantiatePrefab(uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenPrefab.gameObject);
				if(instancedGO != null )
				{
					UIScreenManager instancedUISM = instancedGO.GetComponent<UIScreenManager>();
					if(instancedUISM != null)
					{
						uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject = instancedGO.GetComponent<UIScreenManager>();
						if(uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject == null)//delete newly created object
						{
							DestroyImmediate(instancedGO);
							EditorUtility.DisplayDialog("Cant create.","Cant Spawn PopUp Screen["+uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId+"].","Ok");
						}
						else
						{
							uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject.CachedTransform.SetSiblingIndex( uiPopUpsManager.GetLastPopUpBkgSiblingIndex());
							uiPopUpsManager._allPopUpBackgroundsControllers[i].Switch(false,true);
						}
					}
				}
				else
				{
					Debug.Log("Cant spawn["+uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenPrefab.name+"]");
				}
			}	
		}
	}
		
	void DespawnAllPopUps()
	{
		Undo.RegisterCreatedObjectUndo(uiPopUpsManager,uiPopUpsManager.name);
		for(int i = 0; i < uiPopUpsManager._allPopUpControllers.Count; i++)
		{
			uiPopUpsManager._allPopUpControllers[i].Switch(false,false);
		}
	}

	void DespawnAllBackgrounds()
	{
		Undo.RegisterCreatedObjectUndo(uiPopUpsManager,uiPopUpsManager.name);
		for(int i = 0; i < uiPopUpsManager._allPopUpBackgroundsControllers.Count; i++)
		{
			uiPopUpsManager._allPopUpBackgroundsControllers[i].Switch(false,false);
		}
	}

	void CreateNewScreenPopUp()
	{
		Undo.RegisterCreatedObjectUndo(uiPopUpsManager,uiPopUpsManager.name);
		GameObject newUIScreenManager = uiPopUpsManager.CreateNewUIPopUpScreenManager();
		if(newUIScreenManager != null)
		{
			newUIScreenManager.transform.SetSiblingIndex( uiPopUpsManager.GetLastPopUpSiblingIndex());
			ArrayList sceneViews = UnityEditor.SceneView.sceneViews;
			if(sceneViews != null)
			{
				if(sceneViews.Count > 0)
				{
					UnityEditor.SceneView sceneView = (UnityEditor.SceneView) sceneViews[0];
					sceneView.AlignViewToObject(newUIScreenManager.transform);
				}
			}
		}
	}

	void CreateNewScreenPopUpBackground()
	{
		Undo.RegisterCreatedObjectUndo(uiPopUpsManager,uiPopUpsManager.name);
		GameObject newUIScreenManager = uiPopUpsManager.CreateNewUIPopUpBKGScreenManager();
		if(newUIScreenManager != null)
		{
			newUIScreenManager.transform.SetSiblingIndex( uiPopUpsManager.GetLastPopUpBkgSiblingIndex());
			ArrayList sceneViews = UnityEditor.SceneView.sceneViews;
			if(sceneViews != null)
			{
				if(sceneViews.Count > 0)
				{
					UnityEditor.SceneView sceneView = (UnityEditor.SceneView) sceneViews[0];
					sceneView.AlignViewToObject(newUIScreenManager.transform);
				}
			}
		}
	}

	void CreatePopUpControllersFromSelected()
	{
		if(Selection.gameObjects.Length > 0)
		{
			for(int i = 0; i < Selection.gameObjects.Length; i++)
			{
				UIScreenManager uism = Selection.gameObjects[i].GetComponent<UIScreenManager>();
				if(uism != null)
				{
					Undo.RegisterCreatedObjectUndo(uiPopUpsManager,uiPopUpsManager.name);

					PrefabType pType = PrefabUtility.GetPrefabType(uism.gameObject);
					bool isPrefab = pType == PrefabType.Prefab;
					UIScreenManager newUIScreenManager = null;
					//Debug.Log("Selection type ["+pType+"]");
					if(isPrefab)
					{
						//	Debug.Log("Creating screen from prefab reference");
						GameObject instancedGO = (GameObject)PrefabUtility.InstantiatePrefab(Selection.gameObjects[i]);
						if(instancedGO != null )
						{
							UIScreenManager instancedUISM = instancedGO.GetComponent<UIScreenManager>();
							if(instancedUISM != null)
							{
								newUIScreenManager = uiPopUpsManager.CreateNewUIPopUpScreenControllerFromSceneObject(instancedUISM);
								if(newUIScreenManager == null)//delete newly created object
								{
									DestroyImmediate(instancedGO);
									EditorUtility.DisplayDialog("Cant create.","For a new UIScreenManager to be registered/created it must have a unique Id.","Ok");
								}
							}
						}
					}
					else
					{
						newUIScreenManager = uiPopUpsManager.CreateNewUIPopUpScreenControllerFromSceneObject(uism);
					}

					if(newUIScreenManager != null)
					{
						newUIScreenManager.CachedTransform.SetSiblingIndex( uiPopUpsManager.GetLastPopUpSiblingIndex());
						Object prefab = PrefabUtility.GetPrefabParent((Object)newUIScreenManager.gameObject);
						if(prefab != null)
						{
							//is a prefab
							string path = AssetDatabase.GetAssetPath(prefab);
							UIScreenManager prefabUIsm = AssetDatabase.LoadAssetAtPath<UIScreenManager>(path);
							if(prefabUIsm != null)
							{
								for(int j = 0; j < uiPopUpsManager._allPopUpControllers.Count; j++)
								{
									if(uiPopUpsManager._allPopUpControllers[j]._uiUniqueId == newUIScreenManager._uniqueScreenId)
									{
										uiPopUpsManager._allPopUpControllers[j]._uiScreenPrefab = prefabUIsm;
										break;
									}
								}	
							}
						}

						ArrayList sceneViews = UnityEditor.SceneView.sceneViews;
						if(sceneViews != null)
						{
							if(sceneViews.Count > 0)
							{
								UnityEditor.SceneView sceneView = (UnityEditor.SceneView) sceneViews[0];
								sceneView.AlignViewToObject(newUIScreenManager.transform);
							}
						}
					}

				}
			}
		}
	}

	void CreatePopUpBkgControllersFromSelected()
	{
		if(Selection.gameObjects.Length > 0)
		{
			for(int i = 0; i < Selection.gameObjects.Length; i++)
			{
				UIScreenManager uism = Selection.gameObjects[i].GetComponent<UIScreenManager>();
				if(uism != null)
				{
					Undo.RegisterCreatedObjectUndo(uiPopUpsManager,uiPopUpsManager.name);

					PrefabType pType = PrefabUtility.GetPrefabType(uism.gameObject);
					bool isPrefab = pType == PrefabType.Prefab;
					UIScreenManager newUIScreenManager = null;
					//Debug.Log("Selection type ["+pType+"]");
					if(isPrefab)
					{
						//	Debug.Log("Creating screen from prefab reference");
						GameObject instancedGO = (GameObject)PrefabUtility.InstantiatePrefab(Selection.gameObjects[i]);
						if(instancedGO != null )
						{
							UIScreenManager instancedUISM = instancedGO.GetComponent<UIScreenManager>();
							if(instancedUISM != null)
							{
								newUIScreenManager = uiPopUpsManager.CreateNewUIPopUpScreenBkgControllerFromSceneObject(instancedUISM);
								if(newUIScreenManager == null)//delete newly created object
								{
									DestroyImmediate(instancedGO);
									EditorUtility.DisplayDialog("Cant create.","For a new UIScreenManagerBKG to be registered/created it must have a unique Id.","Ok");
								}
							}
						}
					}
					else
					{
						newUIScreenManager = uiPopUpsManager.CreateNewUIPopUpScreenBkgControllerFromSceneObject(uism);
					}

					if(newUIScreenManager != null)
					{
						newUIScreenManager.CachedTransform.SetSiblingIndex( uiPopUpsManager.GetLastPopUpBkgSiblingIndex());
						Object prefab = PrefabUtility.GetPrefabParent((Object)newUIScreenManager.gameObject);
						if(prefab != null)
						{
							//is a prefab
							string path = AssetDatabase.GetAssetPath(prefab);
							UIScreenManager prefabUIsm = AssetDatabase.LoadAssetAtPath<UIScreenManager>(path);
							if(prefabUIsm != null)
							{
								for(int j = 0; j < uiPopUpsManager._allPopUpBackgroundsControllers.Count; j++)
								{
									if(uiPopUpsManager._allPopUpBackgroundsControllers[j]._uiUniqueId == newUIScreenManager._uniqueScreenId)
									{
										uiPopUpsManager._allPopUpBackgroundsControllers[j]._uiScreenPrefab = prefabUIsm;
										break;
									}
								}	
							}
						}

						ArrayList sceneViews = UnityEditor.SceneView.sceneViews;
						if(sceneViews != null)
						{
							if(sceneViews.Count > 0)
							{
								UnityEditor.SceneView sceneView = (UnityEditor.SceneView) sceneViews[0];
								sceneView.AlignViewToObject(newUIScreenManager.transform);
							}
						}
					}

				}
			}
		}
	}

	void UpdatePopUpIds()
	{
		for(int i = 0; i < uiPopUpsManager._allPopUpControllers.Count; i++)
		{
			if(uiPopUpsManager._allPopUpControllers[i]._uiScreenObject != null)
			{
				uiPopUpsManager._allPopUpControllers[i]._uiScreenObject._uniqueScreenId = uiPopUpsManager._allPopUpControllers[i]._uiUniqueId;
			}
			if(uiPopUpsManager._allPopUpControllers[i]._uiScreenPrefab != null)
			{
				uiPopUpsManager._allPopUpControllers[i]._uiScreenPrefab._uniqueScreenId = uiPopUpsManager._allPopUpControllers[i]._uiUniqueId;
			}
			AssetDatabase.SaveAssets();
		}
	}

	void UpdatePopUpBkgIds()
	{
		for(int i = 0; i < uiPopUpsManager._allPopUpBackgroundsControllers.Count; i++)
		{
			bool popUpsUpdated = false;
			if(uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject != null)
			{
				popUpsUpdated = true;
				string oldId = uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject._uniqueScreenId;
				uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject._uniqueScreenId = uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId;
				//update on related popups
				for(int j = 0; j < uiPopUpsManager._allPopUpControllers.Count; j++)
				{
					uiPopUpsManager._allPopUpControllers[j].UpdateComplementIds(oldId,uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId);
				}
			}
			if(uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenPrefab != null)
			{
				if(!popUpsUpdated)
				{
					popUpsUpdated = true;
					string oldId = uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenPrefab._uniqueScreenId;
					//update on related popups
					for(int j = 0; j < uiPopUpsManager._allPopUpControllers.Count; j++)
					{
						uiPopUpsManager._allPopUpControllers[j].UpdateComplementIds(oldId,uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId);
					}
				}
				uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenPrefab._uniqueScreenId = uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiUniqueId;
			}
			AssetDatabase.SaveAssets();
		}
	}

	void UpdatePrefabs()
	{
		for(int i = 0; i < uiPopUpsManager._allPopUpControllers.Count; i++)
		{
			if(uiPopUpsManager._allPopUpControllers[i]._uiScreenObject != null)
			{
				Object prefab = PrefabUtility.GetPrefabParent((Object)uiPopUpsManager._allPopUpControllers[i]._uiScreenObject);
				if(prefab != null)
				{
					//is a prefab
					string path = AssetDatabase.GetAssetPath(prefab);
					UIScreenManager uism = AssetDatabase.LoadAssetAtPath<UIScreenManager>(path);
					if(uism != null)
					{
						uiPopUpsManager._allPopUpControllers[i]._uiScreenPrefab = uism;
					}
				}
			}
		}
		for(int i = 0; i < uiPopUpsManager._allPopUpBackgroundsControllers.Count; i++)
		{
			if(uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject != null)
			{
				Object prefab = PrefabUtility.GetPrefabParent((Object)uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenObject);
				if(prefab != null)
				{
					//is a prefab
					string path = AssetDatabase.GetAssetPath(prefab);
					UIScreenManager uism = AssetDatabase.LoadAssetAtPath<UIScreenManager>(path);
					if(uism != null)
					{
						uiPopUpsManager._allPopUpBackgroundsControllers[i]._uiScreenPrefab = uism;
					}
				}
			}
		}
	}

	void ResetManager()
	{
		bool result = EditorUtility.DisplayDialog("Reset PopUps Manager?","Reset this PopUps Manager?\nThis will clear all the controllers and destroy the current popups and backgrounds in scene.","Yes","No");
		if(result)
		{
			uiPopUpsManager.Reset();
		}
	}
}