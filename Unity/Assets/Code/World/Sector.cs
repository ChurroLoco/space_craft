﻿using UnityEngine;
using System.Collections;
using System.IO;


public class Sector : MonoBehaviour 
{
	public const int WIDTH = 4;
	public const int HEIGHT = 2;	
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

	public void init (int x, int y, int z)
	{
		xIndex = x;
		yIndex = y;
		zIndex = z;

		center = transform.position + new Vector3((WIDTH * Chunk.WIDTH) / 2, (HEIGHT * Chunk.HEIGHT) / 2, (DEPTH * Chunk.DEPTH) / 2);
		saveTimer = Time.time;
	}

	public string fileName
	{
		get 
		{
			return "Data/sector_" + xIndex + "_" + yIndex + "_" + zIndex + ".scs";
		}
	}

	public void ActiveUpdate()
	{
		if (dirty && Time.time - saveTimer > MIN_SAVE_TIME) 
		{
			dirty = false;
			saveTimer = Time.time;
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

		// Now populate the newly initialized chunks with data from a save file.
		if (!File.Exists(fileName))
		{
			// Stupid, but effective!
			Save(true);
		}
		Stream stream = File.Open(fileName, FileMode.Open);
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
						blockData[i] = bReader.ReadInt32();
					}
					
					if (chunk != null)
					{
						chunk.init(this, blockData, x, y, z);
					}
					
					yield return new WaitForSeconds(0.02f);
				}
			}
		}
		bReader.Close();

		// This stuff should probably be turned into a callback or something.
		TerrainControllerScript.instance.loadingSector = false;
		TerrainControllerScript.instance.SectorsToGenerate.Remove(this);
		TerrainControllerScript.instance.activeSectors.Add(this);
		Debug.Log("Finished Generating " + name);
	}

	// Savse the block data of a sector.
	public void Save(bool create = false)
	{
		Stream stream = File.Open(fileName, FileMode.Create);
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
							blockdata = chunk.SinBlock(0.3f, 0.6f, 0.7f, 0.3f);
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
						bWriter.Write(blockdata[i]);
					}
				}
			}
		}
		bWriter.Close();
		Debug.Log(name + " has been " + (create ? "created." : "updated."));
	}



	
}
