using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class BlockType
{
	public const string LOCAL_FILE_PATH = "Data/block_types";
	public const string GLOBAL_DATA_KEY = "block_types";

	public static Dictionary<int, BlockType> All 
	{
		get
		{
			return GlobalData.Get(GLOBAL_DATA_KEY) as Dictionary<int, BlockType>;
		}

		private set
		{
			GlobalData.Set(GLOBAL_DATA_KEY, value);
		}
	}

	// Let's keep them alphabetical since the list could get long.
	public bool Breakable { get; private set; }
	public string DataClass { get; private set; }
	public int Id { get; private set; }
	public string Name { get; private set; }
	public bool OccludesNeighbors { get; private set; }
	public int TextureIndex { get; private set;}

	/// <summary>
	/// Imports BlockTypes a from JSON file.
	/// </summary>
	/// <returns>Dictionary<int, BlockType></returns>
	/// <param name="filePath">File path.</param>
	static Dictionary<int, BlockType> ImportFromFile(string filePath)
	{
		Dictionary<int, BlockType> result = null;
		TextAsset blockTypeJsonFile = Resources.Load(filePath, typeof(TextAsset)) as TextAsset;
		if (blockTypeJsonFile != null)
		{
			string blockTypeJsonString = blockTypeJsonFile.text;
			result = ImportFromJson(blockTypeJsonString);
		}
		else
		{
			Debug.LogError(string.Format("BlockType.InitializeFromFile(string) Failed to load data file '{0}'.", filePath));
		}
		return result;
	}

	/// <summary>
	/// Imports from a JSON string.
	/// </summary>
	/// <returns>Dictionary<int, BlockType></returns>
	/// <param name="json">Json.</param>
	static Dictionary<int, BlockType> ImportFromJson(string json)
	{	
		Dictionary<int, BlockType> result = new Dictionary<int, BlockType>();
		try
		{
			var typeDataJson = JSON.Parse(json);
			JSONArray types = typeDataJson["block_types"].AsArray;
			foreach (JSONNode type in types)
			{
				BlockType newType = new BlockType();

				// Parse these in alphabetical order also please.
				newType.Breakable = type["breakable"].AsBool;
				newType.DataClass = type["data_class"].Value;
				newType.Id = type["id"].AsInt;
				newType.Name = type["name"].Value;
				newType.TextureIndex = type["texture_index"].AsInt;
				newType.OccludesNeighbors = type["occludes_neighbors"].AsBool;

				// If we already have a BlockType with this ID than output an error message and don't add the type.
				if (result.ContainsKey(newType.Id))
				{
					Debug.LogError(string.Format("BlockType.ImportFromJson(string): Duplicate BlockType id detected. [{0}]", newType.Id));
				}
				else
				{
					result.Add(newType.Id, newType);
				}
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError(e.Message);
		}
		return result;
	}

	public static void InitializeFromFile(string filePath = LOCAL_FILE_PATH)
	{
		BlockType.All = ImportFromFile(filePath);
	}

	public static void InitializeFromJSON(string json)
	{
		BlockType.All = ImportFromJson(json);
	}

}

