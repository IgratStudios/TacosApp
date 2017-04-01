using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextSizeManager : Manager<TextSizeManager> 
{
	public delegate void SizesChanged();
	public static SizesChanged OnSizesChanged;

	public enum TEXT_SIZE_TYPE
	{
		BIG,TITTLE_BIG,TITTLE,SUBTITTLE_BIG,SUBTITTLE,REGULAR,SMALL,MICRO
	}

	[System.Serializable]
	public class TextSizeReference
	{
		public TEXT_SIZE_TYPE sizeType;
		public int textSize;
		public Text referenceText;

		public void CalculateSize(float scaleFactor)
		{
			if(referenceText != null)
			{
				textSize =  Mathf.FloorToInt(referenceText.cachedTextGenerator.fontSizeUsedForBestFit/scaleFactor);
			}
		}

		public override string ToString ()
		{
			return string.Format ("Calculated Text Size["+textSize+"] " +
				"Current Text size["+(referenceText == null?"NULL":referenceText.cachedTextGenerator.fontSizeUsedForBestFit.ToString())+"] " +
				"FontSize["+(referenceText == null?"NULL":referenceText.fontSize.ToString())+"]");
		}
	}

	public static int defaultSize = 14;

	public TextSizeReference[] textSizes;

	public int framesToWaitBeforeReadSizes = 2;

	public GameObject referenceTextsHolder;
	public Canvas canvas;

	private Dictionary<TEXT_SIZE_TYPE,int> sizesMap;

	protected override void Awake ()
	{
		if(Application.isPlaying)
		{
			base.Awake ();
		}
	}

	public override void StartManager ()
	{
		base.StartManager ();
		if(isThisManagerValid)
		{
			CreateSizesMap();
			StartCoroutine("ReadSizes");
		}
	}

	private void CreateSizesMap()
	{
		sizesMap = new Dictionary<TEXT_SIZE_TYPE, int>();
		for(int i = 0; i < textSizes.Length; i++)
		{
			sizesMap.Add(textSizes[i].sizeType,i);
		}
	}

	public static int getSizeByType(TEXT_SIZE_TYPE sizeType)
	{
		if(_cachedInstance != null)
		{
			return _cachedInstance.getSize(sizeType);
		}
		return defaultSize;
	}

	public int getSize(TEXT_SIZE_TYPE sizeType)
	{
		int index = -1;
		if(sizesMap.TryGetValue(sizeType, out index))
		{
			if(_mustShowDebugInfo)
			{
				Debug.Log( textSizes[index].ToString() );
				//Debug.Log("TestVal["+(textSizes[index].textSize/canvas.scaleFactor)+"]");


			}
			return textSizes[index].textSize;
		}
		return defaultSize;
	}

	private IEnumerator ReadSizes()
	{
		for(int i = 0; i < framesToWaitBeforeReadSizes; i++)
		{
			yield return 0;
		}

		for(int i = 0; i < textSizes.Length; i++)
		{
			textSizes[i].CalculateSize(canvas.scaleFactor);
		}
			
		//we dont need this object anymore
		if(referenceTextsHolder != null)
		{
			Destroy( referenceTextsHolder );
			referenceTextsHolder = null;
		}

		if(OnSizesChanged != null)
		{
			OnSizesChanged();
		}
	}

	#if UNITY_EDITOR
	public bool getDefaultSizes = false;

	void Update()
	{
		if(getDefaultSizes)
		{
			getDefaultSizes = false;

			for(int i = 0; i < textSizes.Length; i++)
			{
				textSizes[i].CalculateSize(canvas.scaleFactor);
			}

		}

	}
	#endif
}
