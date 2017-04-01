using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects()]
[CustomEditor(typeof(UIButtonScaleUp))]
public class EditorUIButtonScaleUp : Editor 
{
	SerializedObject serializedObj;

	protected void OnEnable()
	{
		serializedObj = serializedObject;
	}

	public override void OnInspectorGUI()
	{
		serializedObj.Update();
		EditorGUILayout.PropertyField(serializedObj.FindProperty("targetScale"));
		EditorGUILayout.PropertyField(serializedObj.FindProperty("scaleUpDuration"));
		EditorGUILayout.PropertyField(serializedObj.FindProperty("colorTarget"));
		EditorGUILayout.PropertyField(serializedObj.FindProperty("pressedColor"));
		serializedObject.ApplyModifiedProperties();
	}

}
