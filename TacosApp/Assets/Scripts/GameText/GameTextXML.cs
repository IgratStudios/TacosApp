using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

//[System.Serializable]
[XmlRoot("gameText")]
public class GameTextXML
{
	protected List<TextXML> _texts = new List<TextXML>();
	private Dictionary<string,TextXML> _textsMap = new Dictionary<string, TextXML>();
	
	[XmlArray("texts"),XmlArrayItem("text")]
	public TextXML[] texts
	{
		set{_texts = new List<TextXML>(value);}
		get{return _texts.ToArray();}
	}
	
	public static GameTextXML LoadFromText(string text) 
	{
		var serializer = new XmlSerializer(typeof(GameTextXML));
		GameTextXML gText = serializer.Deserialize(new StringReader(text)) as GameTextXML;
		gText.CreateMap();
		return gText;
	}

	internal void CreateMap()
	{
		for(int i = 0;i < texts.Length;i++)
		{
			if(!_textsMap.ContainsKey(texts[i].id))
			{
				_textsMap.Add(texts[i].id,texts[i]);
			}
			else
			{
				UnityEngine.Debug.LogWarning("TextId repetido ["+texts[i].id+"]");
			}
		}
		_texts.Clear();
		_texts = null;
	}

	public TextXML getTextByID(string id)
	{
		TextXML textXml = null;
		_textsMap.TryGetValue(id,out textXml);
		return textXml;
	}

	public bool isValid()
	{
		return _textsMap.Count > 0;
	}
}