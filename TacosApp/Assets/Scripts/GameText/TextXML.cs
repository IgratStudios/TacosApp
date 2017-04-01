using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

//[System.Serializable]
public class TextXML
{
	[XmlAttribute("id")]
	public string id;

	[XmlAttribute("text")]
	public string text;
}