using UnityEngine;
using System.Collections;

public class TerrainControllerScript : MonoBehaviour
{
	public const int WIDTH = 4;
	public const int HEIGHT = 2;
	public const int DEPTH = 4;

	public const int TERRAIN_HEIGHT = HEIGHT * Chunk.HEIGHT;	

	public const string CHUNK_PREFAB_PATH = "Prefabs/Chunk";
	
	private static Chunk[,,] chunks = new Chunk[WIDTH, HEIGHT, DEPTH];
	
	void Start()
	{
		//Instantiate(Resources.Load("Prefabs/Chunk"));
		StartCoroutine("GenerateWorld");


	}
	
	public IEnumerator GenerateWorld()
	{
		Object resource = Resources.Load(CHUNK_PREFAB_PATH);
		
		for (int y = HEIGHT - 1; y >= 0; y--)
		{
			for (int z = 0; z < DEPTH; z++)
			{
				for (int x = 0; x < WIDTH; x++)
				{
					if (resource != null)
					{
						GameObject chunkObject = Instantiate(resource) as GameObject;
						if (chunkObject != null)
						{
							chunkObject.transform.position = new Vector3(x * Chunk.WIDTH, y * Chunk.HEIGHT, z * Chunk.DEPTH);
							chunkObject.name = string.Format("Chunk [{0},{1},{2}]", x, y, z);
							chunkObject.transform.parent = this.transform;
							Chunk chunkScript = chunkObject.GetComponent<Chunk>();
							chunks[x, y, z] = chunkScript;
						}
					}
					else
					{
						Debug.LogError(string.Format("Chunk prefab could not be loaded from '{0}'", CHUNK_PREFAB_PATH));	
					}
					yield return new WaitForSeconds(0.2f);
				}
			}
		}
	}

	public static Chunk getChunkAt(Vector3 position)
	{
		if (position.x < 0 || position.x > WIDTH * Chunk.WIDTH ||
		    position.y < 0 || position.y > HEIGHT * Chunk.HEIGHT ||
		    position.z < 0 || position.z > DEPTH * Chunk.DEPTH)
		{
			// We're off the map.
			return null;
		}
		// Get the chunk.
		return chunks[(int)(position.x / Chunk.WIDTH),(int)(position.y / Chunk.HEIGHT),(int)(position.z / Chunk.DEPTH)];
	}

	public static Block getBlockAt(Vector3 position)
	{
		if (position.x < 0 || position.x > WIDTH * Chunk.WIDTH ||
		    position.y < 0 || position.y > HEIGHT * Chunk.HEIGHT ||
		    position.z < 0 || position.z > DEPTH * Chunk.DEPTH)
		{
			// We're off the map.
			return null;
		}
		// Get the chunk.
		Chunk chunk = chunks[(int)(position.x / Chunk.WIDTH),(int)(position.y / Chunk.HEIGHT),(int)(position.z / Chunk.DEPTH)];
		// Get the block within the chunk.
		return chunk.blocks[(int)position.x % Chunk.WIDTH, (int)position.y % Chunk.HEIGHT, (int)position.z % Chunk.DEPTH];
	}

}