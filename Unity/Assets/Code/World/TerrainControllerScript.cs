using UnityEngine;
using System.Collections;

public class TerrainControllerScript : MonoBehaviour
{
	public const int WIDTH = 1;
	public const int HEIGHT = 1;
	public const int DEPTH = 1;
	
	public const string CHUNK_PREFAB_PATH = "Prefabs/Chunk";
	
	private BlockChunkScript[,,] chunks = new BlockChunkScript[WIDTH, HEIGHT, DEPTH];
	
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
							chunkObject.transform.position = new Vector3(x * BlockChunkScript.WIDTH, y * BlockChunkScript.HEIGHT, z * BlockChunkScript.DEPTH);
							chunkObject.name = string.Format("Chunk [{0},{1},{2}]", x, y, z);
							chunkObject.transform.parent = this.transform;
							BlockChunkScript chunkScript = chunkObject.GetComponent<BlockChunkScript>();
							chunks[x, y, z] = chunkScript;
						}
								
						Debug.Log(string.Format("Chunk Generated: '{0}'", new Vector3(x, y, z)));
					}
					else
					{
						Debug.LogError(string.Format("Chunk prefab could not be loaded from '{0}'", CHUNK_PREFAB_PATH));	
					}
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
	}

}