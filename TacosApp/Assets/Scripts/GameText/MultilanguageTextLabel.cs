using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(Text))]
public class MultilanguageTextLabel : MonoBehaviour 
{
	public bool registerToLanguageChange = false;
	public Text label; 
	public string id = string.Empty;
	private string processedText;

	// Use this for initialization
	void Start () 
	{	
		if(Application.isPlaying)
		{
			OnLanguageChanged();
			if(registerToLanguageChange)
			{
				GameTextManager.OnLanguagechanged += OnLanguageChanged;
			}
		}
	}

	void OnLanguageChanged()
	{
		if(!string.IsNullOrEmpty(id))
		{
			processedText = GameTextManager.GetTextByID(id);
		}	
		if(!string.IsNullOrEmpty(processedText))
		{
			label.text = processedText;
		}
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
