using UnityEngine;
using System.Collections;
using System;
using System.IO;

/*
 * Clase que controla los textos del juego, para siempre seleccionar los del idioma adecuado
 */
public class GameTextManager : Manager<GameTextManager>
{
	public delegate void LanguageChangeEvent();
	public static LanguageChangeEvent OnLanguagechanged;

	public bool saveAndLoadLanguagePreference = false;
	public string defaultLanguage = "Spanish";
	public bool overrideWithDefault = false;
	protected string gameLanguage = string.Empty;

	protected GameTextXML gameTextData;

	protected override void Awake()
	{
		base.Awake ();

		if(isThisManagerValid)
		{
			if(!overrideWithDefault)
			{
				//Busca si ya se ha guardado un idioma, de lo contrario toma el del sistema
				if(PlayerPrefs.HasKey("language") && saveAndLoadLanguagePreference)
				{
					setLanguage( PlayerPrefs.GetString("language"));
				}
				else
				{
					setLanguage(Application.systemLanguage);
				}
			}
			else
			{
				setLanguage(defaultLanguage);
			}
		}
	}

	public static string GetTextByID(string id)
	{
		if(_cachedInstance != null)
		{
			return _cachedInstance.getTextByID(id);
		}
		return string.Empty;
	}

	/*
	 * Devuelve el idioma en string
	 * 
	 * @return {string}: El lenguage a manera de string
	 */
	public string getLanguage()
	{
		return gameLanguage.ToString();
	}

	/*
	 * Le cambia el idioma al GameTextManager y lo guarda en PlayerPreferences
	 * 
	 * @params newLanguage{string}: Es el texto que se convertira en SystemLanguage.Enum
	 */
	public void setLanguage(string newLanguage)
	{	
		gameLanguage = newLanguage;
		PlayerPrefs.SetString("language",newLanguage);
		UpdateLanguage();
	}

	/*
	 * Le cambia el idioma al GameTextManager y lo guarda en PlayerPreferences
	 * 
	 * @params newLanguage{SystemLanguage}: Este se guarda de manera directa pero se convierte a string para PlayerPreferences
	 */
	public void setLanguage(SystemLanguage newLanguage)
	{	
		gameLanguage = newLanguage.ToString();
		PlayerPrefs.SetString("language",gameLanguage);
		UpdateLanguage();
	}

	public void UpdateLanguage()
	{
		if(!string.IsNullOrEmpty(gameLanguage))
		{
			//revisar si el archivo del idioma existe.
			TextAsset tempTxt = (TextAsset)Resources.Load ("Languages/"+gameLanguage);
			if(tempTxt == null && gameTextData == null)//there is no other language loaded
			{
				tempTxt = (TextAsset)Resources.Load ("Languages/"+defaultLanguage);
			}

			gameTextData = GameTextXML.LoadFromText(tempTxt.text);

			if(_mustShowDebugInfo)
			{
				Debug.Log("<Color=yellow>Language loaded["+gameLanguage+"]</color>");
			}

			if(gameTextData.isValid() && OnLanguagechanged != null)
			{
				OnLanguagechanged();
			}
		}
	}


	/*
	 * @params idText{string}: El ID del texto que se quiere, este debe coincidir con el XML
	 * 
	 * @params language{string}: Idioma en particular o una cadena vacia para el lenguaje default
	 * 
	 */
	public string getTextByID(string idText)
	{
		if(gameTextData != null)
		{
			TextXML text = gameTextData.getTextByID(idText);
			if(text != null)
			{
				return text.text;
			}
			return "NOT_FOUNDED_"+idText;
		}
		return string.Empty;
	}

	/**
	 * @params target{string}: La cadena en la que se buscará el texto de textToReplace para poder sustituirlo
	 * 
	 * @params textToReplace{string[]}: Una lista de Textos o simbolos que se quieren remplazar. Usualmente para agregar texto dinamico.
	 * 									Este sera ignorado si no se pasa tambien valor a newText que sea del mismo tamaño.
	 * 
	 * @params replacements{string[]}: Una lista de Textos o simbolos que reemplazaran los textos de textToReplace. Usualmente para agregar texto dinamico.
	 * 									Este sera ignorado si no se pasa tambien valor a textToReplace que sea del mismo tamaño.
	 **/ 
	public string multipleReplace(string target,string[] textToReplace,string[] replacements)
	{
		string resultText = target;

		if(textToReplace != null && replacements != null)
		{
			if (textToReplace.Length == replacements.Length) 
			{
				for (int i = 0; i < replacements.Length; i++) 
				{
					resultText = resultText.Replace (textToReplace [i], replacements [i]);
				}
			} 
			else 
			{
				//DONE: Cambie el mensaje a ingles y agrege que el error es por diferentes lengths
				Debug.LogWarning("THE TEXT HAS NOT CHANGED, because the two lists have diferent length");
			}
		}

		return resultText;
	}
}