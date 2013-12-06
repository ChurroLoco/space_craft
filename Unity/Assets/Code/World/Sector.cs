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
		terrainData = new SectorTerrainData();
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

	public string tDataFileName
	{
		get
		{
			return string.Format("tData_{0}_{1}.scs", xIndex, zIndex);
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
		// Instantiate the Chunks.
		Object resource = Resources.Load(CHUNK_PREFAB_PATH);
		
		for (int x = 0; x < WIDTH; x++)
		{
			for (int z = 0; z < DEPTH; z++)
			{
				for (int y = HEIGHT - 1; y >= 0; y--)
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

		int[] blockData = new int[Sector.HEIGHT * Sector.DEPTH * Sector.WIDTH * Chunk.HEIGHT * Chunk.DEPTH * Chunk.WIDTH];

		if (!Directory.Exists(filePath))
		{
			Directory.CreateDirectory(filePath);
		}

		terrainData.Load(filePath, tDataFileName);
		// Attempt to load our fileData;
		if (!File.Exists(filePath +"/"+ fileName))
		{
			// If this is a new sector, generate it some new sector data.
			Save(TerrainGenerator.GenerateSector(this));
		}

		Stream stream = File.Open(filePath +"/"+ fileName, FileMode.Open);
		BinaryReader bReader = new BinaryReader(stream);

		// get our blockData;
		for (int i = 0; i < blockData.Length; i++)
		{
			blockData[i] = (int)bReader.ReadUInt16();
		}
		bReader.Close();


		StartCoroutine(setBlockData(blockData));
		
		TerrainControllerScript.instance.ActivateSector(this);
		Debug.Log("Finished Generating " + name);
	}

	public IEnumerator setBlockData(int[] blockData)
	{
		int pos = 0;
		for (int x = 0; x < WIDTH; x++)
		{
			for (int z = 0; z < DEPTH; z++)
			{
				for (int y = HEIGHT - 1; y >= 0; y--)
				{
					// Read A Chunk's BlockData out one chunk at a time.
					Chunk chunk = chunks[x,y,z];
					int[] chunkData = new int[Chunk.WIDTH * Chunk.HEIGHT * Chunk.DEPTH];
					for (int i = 0; i < chunkData.Length; i++)
					{
						chunkData[i] = blockData[pos++];
					}
					if (chunk != null)
					{
						chunk.SetBlockData(chunkData);
						chunk.GenerateGeometry();
					}
					
					yield return new WaitForSeconds(0.01f);
				}
			}
		}
	}

	// Savse the block data of a sector.
	public void Save(int[] newData = null)
	{
		// Create the folder if we don't have one made.
		if (!Directory.Exists(filePath))
		{
			Directory.CreateDirectory(filePath);
		}

		Stream stream = File.Open(filePath +"/"+ fileName, FileMode.Create);
		BinaryWriter bWriter = new BinaryWriter(stream);

		// Now, save our block data.
		if (newData == null)
		{
			for (int x = 0; x < WIDTH; x++)
			{
				for (int z = 0; z < DEPTH; z++)
				{
					for (int y = HEIGHT - 1; y >= 0; y--)
					{
						// Data is saved by chunk.
						int[] blockdata;
						Chunk chunk = chunks[x,y,z];
						if (chunk != null)
						{
							blockdata = chunk.GetBlockData();
						}
						else
						{
							// create a blank Chunk data file.
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
		}
		else
		{
			// Overwrite the file with our new data.
			for(int i = 0; i < newData.Length; i++)
			{
				bWriter.Write((ushort)newData[i]);
			}
		}
		bWriter.Close();
		Debug.Log(name + " has been saved");
	}



	
}
