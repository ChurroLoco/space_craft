using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainControllerScript : MonoBehaviour
{
	public const int WIDTH = 2;
	public const int HEIGHT = 1;
	public const int DEPTH = 2;

	public const int TERRAIN_HEIGHT = HEIGHT * Chunk.HEIGHT;	

	public const string CHUCK_PREFAB_PATH = "Prefabs/Chuck";
	
	private static List<Chuck> loadedChucks = new List<Chuck>();

	void Start()
	{
		//StartCoroutine("GenerateWorld");
		LoadChuck(0,0,0);
	}
	
	//public IEnumerator GenerateWorld()
	//{
		
	//}

	private void LoadChuck(int x, int y, int z)
	{
		Object resource = Resources.Load(CHUCK_PREFAB_PATH);
		if (resource != null)
		{
			GameObject chuckObject = Instantiate(resource) as GameObject;
			if (chuckObject != null)
			{
				chuckObject.transform.position = new Vector3(x * Chuck.WIDTH * Chunk.WIDTH, y * Chuck.HEIGHT * Chunk.HEIGHT, z * Chuck.DEPTH * Chunk.DEPTH);
				chuckObject.name = string.Format("Chuck [{0},{1},{2}]", x, y, z);
				chuckObject.transform.parent = this.transform;
				Chuck chuckScript = chuckObject.GetComponent<Chuck>();
				loadedChucks.Add(chuckScript);
				chuckScript.init(x,y,z);
			}
			else
			{
				Debug.LogError(string.Format("Chuck prefab could not be loaded from '{0}'", CHUCK_PREFAB_PATH));	
			}
		}
	}


	public static Chuck getChuckAt(Vector3 position)
	{
		Chuck result = null;
		foreach (Chuck chuck in loadedChucks)
		{
			if (chuck.xIndex == (int)(position.x / (Chuck.WIDTH * Chunk.WIDTH)) &&
			    chuck.yIndex == (int)(position.y / (Chuck.HEIGHT * Chunk.HEIGHT)) &&
			    chuck.zIndex ==  (int)(position.z / (Chuck.DEPTH * Chunk.DEPTH)))
			{
				result = chuck;
				break;
			}
		}
		Debug.Log(result);
		return result;
	}

	private static Vector3 PositionRelativeToChuck(Vector3 position, Chuck chuck)
	{
		return new Vector3(
			position.x - (chuck.xIndex * Chuck.WIDTH * Chunk.WIDTH),
			position.y - (chuck.yIndex * Chuck.HEIGHT * Chunk.HEIGHT),
			position.z - (chuck.zIndex * Chuck.DEPTH * Chunk.DEPTH));
	}

	public static Chunk GetChunkAt(Vector3 position, Chuck chuck)
	{
		if (chuck == null)
		{
			return null;
		}
		Vector3 relativePosition = PositionRelativeToChuck(position, chuck);

		if (relativePosition.x < 0 || relativePosition.x >= Chuck.WIDTH * Chunk.WIDTH ||
			    relativePosition.y < 0 || relativePosition.y >= Chuck.HEIGHT * Chunk.HEIGHT ||
			    relativePosition.z < 0 || relativePosition.z >= Chuck.DEPTH * Chunk.DEPTH)
		{
			// Asking for a position on the wrong Chunk. Return null. 
			return null;
		}
		// Get the chunk.
		return chuck.chunks[(int)(relativePosition.x / Chunk.WIDTH),(int)(relativePosition.y / Chunk.HEIGHT),(int)(relativePosition.z / Chunk.DEPTH)];
	}

	public static Block GetBlockAt(Vector3 position, Chuck chuck)
	{
		Block result = null;
		// Get the chunk.
		Chunk chunk = GetChunkAt(position, chuck);
		if (chunk != null)
		{
			// Modulo the player's position by the measure of blocks in a chunk to get their position within their chunk.
			result = chunk.blocks[(int)position.x % Chunk.WIDTH, (int)position.y % Chunk.HEIGHT, (int)position.z % Chunk.DEPTH];
		}

		return result;
	}

}