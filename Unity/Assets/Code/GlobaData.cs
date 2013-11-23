using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A GlobalData object manages data the must be globally accesbile throughout
/// the entire Unity3D project.
/// </summary>
public class GlobalData : MonoBehaviour 
{
	const string GAME_OBJECT_NAME = "Global Data";
	Dictionary<string, object> data = new Dictionary<string, object>();

	/// <summary>
	/// Gets the global data object.
	/// </summary>
	/// <value>The global data object.</value>
	static GlobalData GlobalDataObject
	{
		get 
		{
			if (_globalDataObject == null)
			{
				GameObject go = GameObject.Find(GAME_OBJECT_NAME);
				if (go == null)
				{
					go = new GameObject(GAME_OBJECT_NAME, new Type[]{Type.GetType("GlobalData")});
					DontDestroyOnLoad(go);
				}
				_globalDataObject = go.GetComponent<GlobalData>();
			}
			return _globalDataObject;
		}
	}
	static GlobalData _globalDataObject;

	/// <summary>
	/// Get the specified key.
	/// </summary>
	/// <param name="key">Key.</param>
	public static System.Object Get(string key)
	{
		System.Object result = null;
		if(GlobalDataObject.data.TryGetValue(key, out result) == false)
		{
			Debug.LogWarning(string.Format("GlobalData.Get(string): Failed to find key '{0}'", key));
		}
		return result;
	}

	/// <summary>
	/// Set the specified key, value and optionally overwrite the current value.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	/// <param name="overwrite">If set to <c>true</c> overwrite.</param>
	public static void Set(string key, System.Object value, bool overwrite = false)
	{
		if(GlobalDataObject.data.ContainsKey(key))
		{
			if (overwrite)
			{
				GlobalDataObject.data[key] = value;
			}
			else
			{
				Debug.LogError(string.Format("GlobalData.Set(string): Over writing value at key '{0}' is not allowed.", key));
			}
		}
		else
		{
			GlobalDataObject.data.Add(key, value);
		}
	}

	/// <summary>
	/// Remove the specified key.
	/// </summary>
	/// <param name="key">Key.</param>
	public static void Remove(string key)
	{
		if (GlobalDataObject.data.ContainsKey(key))
		{
			GlobalDataObject.data.Remove(key);
		}
	}

}
