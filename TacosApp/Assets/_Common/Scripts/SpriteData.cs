using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SpriteData
{
	public string spriteName;
	public float u;
	public float v;
	public float width;
	public float height;
	public float pivotX;
	public float pivotY;

	public SpriteData(
		string spriteName,
		float u,
		float v,
		float width,
		float height,
		float pivotX,
		float pivotY)
	{
		this.spriteName = spriteName;
		this.u = u;
		this.v = v;
		this.width = width;
		this.height = height;
		this.pivotX = pivotX;
		this.pivotY = pivotY;
	}

}

[System.Serializable]
public class SpriteSheetDatas
{
	public List<SpriteData> sprites =  new List<SpriteData>();

	public void AddSpriteData(
		string spriteName,
		float u,
		float v,
		float width,
		float height,
		float pivotX,
		float pivotY)
	{
		SpriteData newSprite = new SpriteData(spriteName,u,v,width,height,pivotX,pivotY);
		sprites.Add(newSprite);
	}

	public Sprite GetSpriteAt(int index,Texture2D texture)
	{
		if(index >= 0  && index < sprites.Count)
		{
			SpriteData data = sprites[index];
		//	Debug.Log("Getting Sprite["+data.spriteName+"] at["+index+"] from Texture["+texture.name+"]");
			Sprite sprite = Sprite.Create(texture,new Rect(data.u,data.v,data.width,data.height),new Vector2(data.pivotX,data.pivotY));
			sprite.name = data.spriteName;
			return sprite;
		}
		return null;
	}

	public string GetSpriteNameAt(int index)
	{
		if(index >= 0  && index < sprites.Count)
		{
			return sprites[index].spriteName;
		}
		return string.Empty;
	}
}

[System.Serializable]
public class SpriteSheet
{
	public SpriteSheetDatas spriteSheetData;
	public Texture2D texture;

	public SpriteSheet(Texture2D texture,SpriteSheetDatas spriteSheetData)
	{
		this.texture = texture;
		this.spriteSheetData = spriteSheetData;
	}
}