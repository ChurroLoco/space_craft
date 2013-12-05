using UnityEngine;
using System.Collections;
using System.IO;


public class Sector : MonoBehaviour 
{
	public const int WIDTH = 4;
	public const int HEIGHT = 4;	
	public const int DEPTH = 4;
	public long id;

	public int xIndex;
	public int yIndex;
	public int zIndex;

	public Vector3 center;

	public const string CHUNK_PREFAB_PATH = "Prefabs/Chunk";
	
	public Chunk[,,] chunks = new Chunk[WIDTH, HEIGHT, DEPTH];

	public bool dirty = false;
	public bool canBeUnloaded = false;

	private const float MIN_SAVE_TIME = 300;
	private float saveTimer;

	// Data to be stored in the sector's block data file. Used for generating varied terrain.
	public SectorTerrainData terrainData;

	public void init (int x, int y, int z)
	{
		xIndex = x;
		yIndex = y;
		zIndex = z;

		center = transform.position + new Vector3((WIDTH * Chunk.WIDTH) / 2, (HEIGHT * Chunk.HEIGHT) / 2, (DEPTH * Chunk.DEPTH) / 2);
		saveTimer = Time.time + MIN_SAVE_TIME;
	}

	public string fileName
	{
		get 
		{
			return string.Format("sec_{0}_{1}_{2}.scs", xIndex, yIndex, zIndex);
		}
	}
	public string filePath
	{
		get
		{
			return string.Format("{0}/secdata", Application.persistentDataPath);
		}
	}

	public void ActiveUpdate()
	{
		if (dirty && Time.time > saveTimer) 
		{
			dirty = false;
			saveTimer = Time.time + MIN_SAVE_TIME;
			Save();
			Debug.Log(name);
		}

		foreach(Chunk chunk in chunks)
		{
			chunk.ActiveUpdate();
		}
	}

	public IEnumerator Generate()
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
							chunkObject.transform.parent = this.transform;
							chunkObject.transform.localPosition = new Vector3(x * Chunk.WIDTH, y * Chunk.HEIGHT, z * Chunk.DEPTH);
							chunkObject.name = string.Format("Chunk [{0},{1},{2}]", x, y, z);
							Chunk chunkScript = chunkObject.GetComponent<Chunk>();
							chunkScript.init(this, x, y, z);
							chunks[x, y, z] = chunkScript;
						}
					}
					else
					{
						Debug.LogError(string.Format("Chunk prefab could not be loaded from '{0}'", CHUNK_PREFAB_PATH));	
					}
					yield return null;
				}
			}
		}

		// Create the folder if we don't have one made.
		if (!Directory.Exists(filePath))
		{
			Directory.CreateDirectory(filePath);
		}
		// Now populate the newly initialized chunks with data from a save file.
		if (!File.Exists(filePath +"/"+ fileName))
		{
			// Stupid, but effective!
			Save(true);
		}
		Stream stream = File.Open(filePath +"/"+ fileName, FileMode.Open);
		BinaryReader bReader = new BinaryReader(stream);
		
		for (int y = HEIGHT - 1; y >= 0; y--)
		{
			for (int z = 0; z < DEPTH; z++)
			{
				for (int x = 0; x < WIDTH; x++)
				{
					// Read A Chunk's BlockData out one chunk at a time.
					Chunk chunk = chunks[x,y,z];
					int[] blockData = new int[Chunk.HEIGHT * Chunk.DEPTH * Chunk.WIDTH];
					for (int i = 0; i < blockData.Length; i++)
					{
						blockData[i] = (int)bReader.ReadUInt16();
					}
					
					if (chunk != null)
					{
						chunk.SetBlockData(blockData);
						chunk.GenerateGeometry();
					}
					
					yield return new WaitForSeconds(0.02f);
				}
			}
		}
		bReader.Close();

		TerrainControllerScript.instance.ActivateSector(this);
		Debug.Log("Finished Generating " + name);
	}

	// Savse the block data of a sector.
	public void Save(bool create = false)
	{
		// Create the folder if we don't have one made.
		if (!Directory.Exists(filePath))
		{
			Directory.CreateDirectory(filePath);
		}

		Stream stream = File.Open(filePath +"/"+ fileName, FileMode.Create);
		BinaryWriter bWriter = new BinaryWriter(stream);

		for (int y = HEIGHT - 1; y >= 0; y--)
		{
			for (int z = 0; z < DEPTH; z++)
			{
				for (int x = 0; x < WIDTH; x++)
				{
					// Save out Data one Chunk at a time.
					int[] blockdata;
					Chunk chunk = chunks[x,y,z];
					if (chunk != null)
					{
						if (create)
						{
							// In the future, we do seed generation here...
							blockdata = TerrainGenerator.PerlinBlock(chunk, 24, 80, 0.5f);//chunk.SinBlock(0.1f, 0.2f, 0.6f, 0.7f, 48, 16);
						}
						else
						{
							blockdata = chunk.GetBlockData();
						}
					}
					else
					{
						// create a blank Chunk.
						blockdata = new int[Chunk.HEIGHT * Chunk.DEPTH * Chunk.WIDTH];
					}

					// Write the chunk's BlockData to the file.
					for (int i = 0; i < blockdata.Length; i++)
					{
						bWriter.Write((ushort)blockdata[i]);
					}
				}
			}
		}
		bWriter.Close();
		Debug.Log(name + " has been " + (create ? "created." : "updated."));
	}



	
}
