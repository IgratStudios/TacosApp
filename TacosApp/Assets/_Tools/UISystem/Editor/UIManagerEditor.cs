using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor 
{
	UIManager uiManager;
	SerializedObject serializedObj;
	MonoScript script;

	protected void OnEnable()
	{
		uiManager = (UIManager)target;
		serializedObj = serializedObject;
		script = MonoScript.FromMonoBehaviour((UIManager)target);
	}

	public override void OnInspectorGUI () 
	{
		if(uiManager != null  && serializedObj != null)
		{
			// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
			serializedObj.Update();
			GUI.enabled = false;
			script = EditorGUILayout.ObjectField("Script:", script, typeof(MonoScript), false) as MonoScript;
			GUI.enabled = true;

			if(!Application.isPlaying)
			{
				if(GUILayout.Button("Despawn Not Starting"))
				{
					Debug.Log("Despawn not starting");
					Undo.RegisterCreatedObjectUndo(uiManager,uiManager.name);
					uiManager.DespawnAllScreensOnSceneThatAreNotInitial();
				}
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Spawn All"))
				{
					SpawnAll();
				}
				if(GUILayout.Button("Despawn All"))
				{
					DespawnAll();
				}
				GUILayout.EndHorizontal();
				if(GUILayout.Button("Create new Screen"))
				{
					CreateNewScreen();
				}
				if(GUILayout.Button("Create Controller from Selected(Lock!)"))
				{
					CreateFromSelected();
				}
				if(GUILayout.Button("Update Screen Ids"))
				{
					UpdateIds();
				}
				if(GUILayout.Button("Update Prefab References"))
				{
					UpdatePrefabs();
				}
			}
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Enable All"))
			{
				uiManager.SwitchAllScreens(true);
			}
			if(GUILayout.Button("Disable All"))
			{
				uiManager.SwitchAllScreens(false);
			}
			GUILayout.EndHorizontal();
			if(GUILayout.Button("Recalculate Screen Positions"))
			{
				uiManager.ResetAllPositions();
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
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_justDisableOnScreenDeactivation"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasScalerReferenceResolution"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasScalerMode"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasScalerScreenMatchMode"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasMatchModeRange"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_canvasScalerPixelsPerUnit"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_screenPositionNumberOfColumns"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_addHelpFrameToCreatedScreens"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_helpFramePrefab"));
			EditorGUILayout.PropertyField(serializedObj.FindProperty("_screenSeparation"));
			SerializedProperty mask =  serializedObj.FindProperty("_systemLayer");
			mask.intValue = EditorGUILayout.LayerField("System Layer",mask.intValue);

			if(EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
			SerializedProperty list = serializedObj.FindProperty("_allScreenControllers");


			int startSize = (uiManager._allScreenControllers != null ? uiManager._allScreenControllers.Count : 0 );
			EditorGUILayout.PropertyField(list,new GUIContent("All Screen Controllers["+startSize+"]"));

			if(list.isExpanded)
			{
				bool added = GUILayout.Button("+",EditorStyles.miniButton);
				if(added)
				{
					//list.InsertArrayElementAtIndex(list.arraySize);
					uiManager.CreateNewEmptyControllerAlone();
				}
				else
				{
					//EditorGUILayout.SelectableLabel("Size   "+uiManager._allScreenControllers.Count);
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
								uiManager.RemoveUIScreenManagerController(elementId,false);
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

			if(serializedObject != null)
			{
				// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
				serializedObject.ApplyModifiedProperties();
			}
		}
	}



	void SpawnAll()
	{
		Undo.RegisterCreatedObjectUndo(uiManager,uiManager.name);
		for(int i = 0; i < uiManager._allScreenControllers.Count; i++)
		{
			if(uiManager._allScreenControllers[i]._uiScreenObject == null && 
				uiManager._allScreenControllers[i]._uiScreenPrefab != null)
			{						
				GameObject instancedGO = (GameObject)PrefabUtility.InstantiatePrefab(uiManager._allScreenControllers[i]._uiScreenPrefab.gameObject);
				if(instancedGO != null )
				{
					UIScreenManager instancedUISM = instancedGO.GetComponent<UIScreenManager>();
					if(instancedUISM != null)
					{
						uiManager._allScreenControllers[i]._uiScreenObject = instancedGO.GetComponent<UIScreenManager>();
						if(uiManager._allScreenControllers[i]._uiScreenObject == null)//delete newly created object
						{
							DestroyImmediate(instancedGO);
							EditorUtility.DisplayDialog("Cant create.","Cant Spawn Screen["+uiManager._allScreenControllers[i]._uiUniqueId+"].","Ok");
						}
						else
						{
							uiManager._allScreenControllers[i]._uiScreenObject.CachedTransform.SetSiblingIndex( uiManager.GetLastScreenSiblingIndex());
							uiManager._allScreenControllers[i].Switch(false,true);
						}
					}
				}
				else
				{
					Debug.Log("Cant spawn["+uiManager._allScreenControllers[i]._uiScreenPrefab.name+"]");
				}
			}	
		}
	}

	void DespawnAll()
	{
		Undo.RegisterCreatedObjectUndo(uiManager,uiManager.name);
		for(int i = 0; i < uiManager._allScreenControllers.Count; i++)
		{
			uiManager._allScreenControllers[i].Switch(false,false);
		}
	}

	void CreateNewScreen()
	{
		Undo.RegisterCreatedObjectUndo(uiManager,uiManager.name);
		GameObject newUIScreenManager = uiManager.CreateNewUIScreenManager();
		if(newUIScreenManager != null)
		{
			newUIScreenManager.transform.SetSiblingIndex( uiManager.GetLastScreenSiblingIndex());
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

	void CreateFromSelected()
	{
		if(Selection.gameObjects.Length > 0)
		{
			for(int i = 0; i < Selection.gameObjects.Length; i++)
			{
				UIScreenManager uism = Selection.gameObjects[i].GetComponent<UIScreenManager>();
				if(uism != null)
				{
					Undo.RegisterCreatedObjectUndo(uiManager,uiManager.name);

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
								newUIScreenManager = uiManager.CreateNewUIScreenControllerFromSceneObject(instancedUISM);
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
						newUIScreenManager = uiManager.CreateNewUIScreenControllerFromSceneObject(uism);
					}

					if(newUIScreenManager != null)
					{
						newUIScreenManager.CachedTransform.SetSiblingIndex( uiManager.GetLastScreenSiblingIndex());
						Object prefab = PrefabUtility.GetPrefabParent((Object)newUIScreenManager.gameObject);
						if(prefab != null)
						{
							//is a prefab
							string path = AssetDatabase.GetAssetPath(prefab);
							UIScreenManager prefabUIsm = AssetDatabase.LoadAssetAtPath<UIScreenManager>(path);
							if(prefabUIsm != null)
							{
								for(int j = 0; j < uiManager._allScreenControllers.Count; j++)
								{
									if(uiManager._allScreenControllers[j]._uiUniqueId == newUIScreenManager._uniqueScreenId)
									{
										uiManager._allScreenControllers[j]._uiScreenPrefab = prefabUIsm;
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

	void UpdateIds()
	{
		for(int i = 0; i < uiManager._allScreenControllers.Count; i++)
		{
			if(uiManager._allScreenControllers[i]._uiScreenObject != null)
			{
				string oldId = uiManager._allScreenControllers[i]._uiScreenObject._uniqueScreenId;
				uiManager._allScreenControllers[i]._uiScreenObject._uniqueScreenId = uiManager._allScreenControllers[i]._uiUniqueId;
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

	void UpdatePrefabs()
	{
		for(int i = 0; i < uiManager._allScreenControllers.Count; i++)
		{
			if(uiManager._allScreenControllers[i]._uiScreenObject != null)
			{
				Object prefab = PrefabUtility.GetPrefabParent((Object)uiManager._allScreenControllers[i]._uiScreenObject);
				if(prefab != null)
				{
					//is a prefab
					string path = AssetDatabase.GetAssetPath(prefab);
					UIScreenManager uism = AssetDatabase.LoadAssetAtPath<UIScreenManager>(path);
					if(uism != null)
					{
						uiManager._allScreenControllers[i]._uiScreenPrefab = uism;
					}
				}
			}
		}
	}

	void ResetManager()
	{
		bool result = EditorUtility.DisplayDialog("Reset UI Manager?","Are you sure you want to Reset UIManager?\nThis will clear all the controllers and destroy the current screens in scene.","Yes","No");
		if(result)
		{
			uiManager.Reset();
		}
	}


}
