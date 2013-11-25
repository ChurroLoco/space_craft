using UnityEngine;
using System.Collections;

public class Chuck : MonoBehaviour 
{
	public const int WIDTH = 4;
	public const int HEIGHT = 2;	
	public const int DEPTH = 4;
	public long id;

	public int xIndex;
	public int yIndex;
	public int zIndex;

	public const string CHUNK_PREFAB_PATH = "Prefabs/Chunk";
	
	public Chunk[,,] chunks = new Chunk[WIDTH, HEIGHT, DEPTH];

	public void init (int x, int y, int z)
	{
		Debug.Log(name + " has been loaded.");
		xIndex = x;
		yIndex = y;
		zIndex = z;

		StartCoroutine(GenerateChunks());
	}

	public IEnumerator GenerateChunks()
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
							chunkScript.init(this);
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
}
