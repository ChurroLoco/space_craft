using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainControllerScript : MonoBehaviour
{
	public const int WIDTH = 2;
	public const int HEIGHT = 1;
	public const int DEPTH = 2;

	public const int TERRAIN_HEIGHT = HEIGHT * Chunk.HEIGHT;	

	public const string SECTOR_PREFAB_PATH = "Prefabs/Sector";
	
	private static List<Sector> loadedSectors = new List<Sector>();

	void Start()
	{
		//StartCoroutine("GenerateWorld");
		GenerateSector(0,0,0);
	}

	private void GenerateSector(int x, int y, int z)
	{
		Object resource = Resources.Load(SECTOR_PREFAB_PATH);
		if (resource != null)
		{
			GameObject SectorObject = Instantiate(resource) as GameObject;
			if (SectorObject != null)
			{
				SectorObject.transform.position = new Vector3(x * Sector.WIDTH * Chunk.WIDTH, y * Sector.HEIGHT * Chunk.HEIGHT, z * Sector.DEPTH * Chunk.DEPTH);
				SectorObject.name = string.Format("Sector [{0},{1},{2}]", x, y, z);
				SectorObject.transform.parent = this.transform;
				Sector SectorScript = SectorObject.GetComponent<Sector>();
				loadedSectors.Add(SectorScript);
				SectorScript.init(x,y,z);
			}
			else
			{
				Debug.LogError(string.Format("Sector prefab could not be loaded from '{0}'", SECTOR_PREFAB_PATH));	
			}
		}
	}


	public static Sector GetSectorAt(Vector3 position)
	{
		Sector result = null;
		foreach (Sector sector in loadedSectors)
		{
			if (sector.xIndex == (int)(position.x / (Sector.WIDTH * Chunk.WIDTH)) &&
			    sector.yIndex == (int)(position.y / (Sector.HEIGHT * Chunk.HEIGHT)) &&
			    sector.zIndex ==  (int)(position.z / (Sector.DEPTH * Chunk.DEPTH)))
			{
				result = sector;
				break;
			}
		}
		Debug.Log(result);
		return result;
	}

	private static Vector3 PositionRelativeToSector(Vector3 position, Sector sector)
	{
		return new Vector3(
			position.x - (sector.xIndex * Sector.WIDTH * Chunk.WIDTH),
			position.y - (sector.yIndex * Sector.HEIGHT * Chunk.HEIGHT),
			position.z - (sector.zIndex * Sector.DEPTH * Chunk.DEPTH));
	}

	public static Chunk GetChunkAt(Vector3 position, Sector sector)
	{
		if (sector == null)
		{
			return null;
		}
		Vector3 relativePosition = PositionRelativeToSector(position, sector);

		if (relativePosition.x < 0 || relativePosition.x >= Sector.WIDTH * Chunk.WIDTH ||
			    relativePosition.y < 0 || relativePosition.y >= Sector.HEIGHT * Chunk.HEIGHT ||
			    relativePosition.z < 0 || relativePosition.z >= Sector.DEPTH * Chunk.DEPTH)
		{
			// Asking for a position on the wrong Chunk. Return null. 
			return null;
		}
		// Get the chunk.
		return sector.chunks[(int)(relativePosition.x / Chunk.WIDTH),(int)(relativePosition.y / Chunk.HEIGHT),(int)(relativePosition.z / Chunk.DEPTH)];
	}

	public static Block GetBlockAt(Vector3 position, Sector sector)
	{
		Block result = null;
		// Get the chunk.
		Chunk chunk = GetChunkAt(position, sector);
		if (chunk != null)
		{
			// Modulo the player's position by the measure of blocks in a chunk to get their position within their chunk.
			result = chunk.blocks[(int)position.x % Chunk.WIDTH, (int)position.y % Chunk.HEIGHT, (int)position.z % Chunk.DEPTH];
		}

		return result;
	}

}