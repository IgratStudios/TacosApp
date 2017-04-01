using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif

[RequireComponent(typeof(Text))]
public class FixedSizeText : MonoBehaviour
{
	public Text label;
	public TextSizeManager.TEXT_SIZE_TYPE sizeType;

	void Start()
	{
		if(Application.isPlaying)
		{
			OnSizeChanged();
			TextSizeManager.OnSizesChanged += OnSizeChanged;
		}
	}

	private void OnSizeChanged()
	{
		label.fontSize = (int)(TextSizeManager.getSizeByType(sizeType));
	}

	#if UNITY_EDITOR
	public bool setLabel = false;
	void Update()
	{
		if(setLabel)
		{
			setLabel = false;
			label = GetComponent<Text>();
		}	
	}
	#endif
}
