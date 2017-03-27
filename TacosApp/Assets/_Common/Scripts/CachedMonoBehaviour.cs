using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CachedMonoBehaviour : MonoBehaviour 
{
	private GameObject	_cachedGameObject;
	private Transform	_cachedTransform;
	private RectTransform	_cachedRectTransform;
	private Rigidbody	_cachedRigidbody;
	private Rigidbody2D	_cachedRigidbody2D;
	private Collider	_cachedCollider;
	private Collider2D	_cachedCollider2D;
	private	Renderer	_cachedRenderer;
	private Material	_cachedMaterial;
	private Dictionary<System.Type,Component>	_cachedExtraComponents = new Dictionary<System.Type,Component>();
	
	public GameObject CachedGameObject
	{
		get
		{
			if(_cachedGameObject == null)
			{
				_cachedGameObject = gameObject;
			}
			return _cachedGameObject;
		}
	}

	public Transform CachedTransform
	{
		get
		{
			if(_cachedTransform == null)
			{
				_cachedTransform = transform;
			}
			return _cachedTransform;
		}
	}

	public RectTransform CachedRectTransform
	{
		get
		{
			if(_cachedRectTransform == null)
			{
				_cachedRectTransform = GetComponent<RectTransform>();
			}
			return _cachedRectTransform;
		}
	}

	public Rigidbody CachedRigidbody
	{
		get
		{
			if(_cachedRigidbody == null)
			{
				_cachedRigidbody = GetComponent<Rigidbody>();
			}
			return _cachedRigidbody;
		}
	}

	public Rigidbody ForceGetRigidbody()
	{
		if(CachedRigidbody == null)
		{
			_cachedRigidbody = CachedGameObject.AddComponent<Rigidbody>();
		}
		return _cachedRigidbody;
	}

	public Rigidbody2D CachedRigidbody2D
	{
		get
		{
			if(_cachedRigidbody2D == null)
			{
				_cachedRigidbody2D = GetComponent<Rigidbody2D>();
			}
			return _cachedRigidbody2D;
		}
	}

	public Rigidbody2D ForceGetRigidbody2D()
	{
		if(CachedRigidbody2D == null)
		{
			_cachedRigidbody2D = CachedGameObject.AddComponent<Rigidbody2D>();
		}
		return _cachedRigidbody2D;
	}

	public Collider CachedCollider
	{
		get
		{
			if(_cachedCollider == null)
			{
				_cachedCollider = GetComponent<Collider>();
			}
			return _cachedCollider;
		}
	}
		
	public Collider2D CachedCollider2D
	{
		get
		{
			if(_cachedCollider2D == null)
			{
				_cachedCollider2D = GetComponent<Collider2D>();
			}
			return _cachedCollider2D;
		}
	}
		
	public Renderer CachedRenderer
	{
		get
		{
			if(_cachedRenderer == null)
			{
				_cachedRenderer = GetComponent<Renderer>();
			}
			return _cachedRenderer;
		}
	}

	public T ForceGet<T>() where T : Component
	{
		Component component;	

		if(_cachedExtraComponents.TryGetValue(typeof(T),out component))
		{
			return (T)component;
		}
		else
		{
			component = CachedGameObject.GetComponent<T>();
			if(component == null)
			{
				component = CachedGameObject.AddComponent<T>();
			}
			_cachedExtraComponents.Add(typeof(T),component);
			return (T)component;
		}
	}

	//this method will create a copy of the material
	public Material CachedMaterial
	{
		get
		{
			if(CachedRenderer != null)
			{
				if(_cachedMaterial == null)
				{
					_cachedMaterial = Instantiate (CachedRenderer.sharedMaterial) as Material;
					CachedRenderer.sharedMaterial = _cachedMaterial;
				}
				return _cachedMaterial;
			}
			return null;
		}
	}

	public Material CachedSharedMaterial
	{
		get
		{
			if(CachedRenderer != null)
			{
				return CachedRenderer.sharedMaterial;
			}
			return null;
		}
	}

	protected void PrintExtraComponents()
	{
		foreach(KeyValuePair<System.Type,Component> pair in _cachedExtraComponents)
		{
			Debug.Log("CachedExtraComponent of type ["+pair.Key.ToString()+"]");
		}
	}
}
