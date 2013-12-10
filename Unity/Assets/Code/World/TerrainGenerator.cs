using UnityEngine;
using System.Collections;
using System.IO;

public class TerrainGenerator
{
	// Big function that sets the blocks in an entire sector.
	public static int[] GenerateSector(Sector sector)
	{
		SectorTerrainData tData = sector.terrainData;
		int[] blockData = new int[Sector.HEIGHT * Sector.DEPTH * Sector.WIDTH * Chunk.HEIGHT * Chunk.DEPTH * Chunk.WIDTH];
		int pos = 0;

		float top = 64;
		float bottom = 32;

		// First gather our sector terrain Data. Start with the values stored in each corner.
		for (int i = 0; i < SectorTerrainData.VALUE_COUNT; i++)
		{
			if (tData.data[i] <= 0)
			{
				// Try to get data from the corners around ours.
				switch (i)
				{
				case 0: // Top Right
					tData.data[i] = GetTDataCorner(sector.xIndex + 1, sector.zIndex, 1);
					if (tData.data[i] <= 0)
					{
					tData.data[i] = GetTDataCorner(sector.xIndex, sector.zIndex + 1, 3);
						if (tData.data[i] <= 0)
						{
						tData.data[i] = GetTDataCorner(sector.xIndex + 1, sector.zIndex + 1, 2);
						} 
					}
					break;

				case 1: // Top Left
					tData.data[i] = GetTDataCorner(sector.xIndex - 1, sector.zIndex, 0);
					if (tData.data[i] <= 0)
					{
						tData.data[i] = GetTDataCorner(sector.xIndex, sector.zIndex + 1, 2);
						if (tData.data[i] <= 0)
						{
							tData.data[i] = GetTDataCorner(sector.xIndex - 1, sector.zIndex + 1, 3);
						} 
					}
					break;

				case 2: // Bottom Left
					tData.data[i] = GetTDataCorner(sector.xIndex - 1, sector.zIndex, 3);
					if (tData.data[i] <= 0)
					{
						tData.data[i] = GetTDataCorner(sector.xIndex, sector.zIndex - 1, 1);
						if (tData.data[i] <= 0)
						{
							tData.data[i] = GetTDataCorner(sector.xIndex - 1, sector.zIndex - 1, 0);
						} 
					}
					break;

				case 3: // Bottom Right
					tData.data[i] = GetTDataCorner(sector.xIndex + 1, sector.zIndex, 2);
					if (tData.data[i] <= 0)
					{
						tData.data[i] = GetTDataCorner(sector.xIndex, sector.zIndex - 1, 0);
						if (tData.data[i] <= 0)
						{
							tData.data[i] = GetTDataCorner(sector.xIndex + 1, sector.zIndex - 1, 1);
						} 
					}
					break;
				}
				// If we can't find our value, create a new one.
				if (tData.data[i] <= 0)
				{
					tData.data[i] = Mathf.SmoothStep(0.0f, 1.0f, Random.Range(0.0f, 1.0f));

					// Turn on Adjacent Based Location.
					/*
					float random = Random.Range(-1.0f, 1.0f) * (Sector.WIDTH * Chunk.WIDTH);
					switch (i)
					{
						case (int)SectorTerrainData.DATA_VALUES.x1:
						tData.data[i] = //tData.data[i+1] > 0 ? tData.data[i+1] - random : 
							((Sector.WIDTH * sector.xIndex) * Chunk.WIDTH);// + random;
						break;
						case (int)SectorTerrainData.DATA_VALUES.x2:
						tData.data[i] = //tData.data[i-1] > 0 ? tData.data[i-1] + random : 
							((Sector.WIDTH * (sector.xIndex + 1)) * Chunk.WIDTH) - 1.0f;// + random;
						break;
						case (int)SectorTerrainData.DATA_VALUES.z1:
						tData.data[i] = //tData.data[i+1] > 0 ? tData.data[i+1] - random : 
							((Sector.DEPTH * sector.zIndex) * Chunk.DEPTH);// - random;
						break;
						case (int)SectorTerrainData.DATA_VALUES.z2:
						tData.data[i] = //tData.data[i-1] > 0 ? tData.data[i-1] + random : 
							((Sector.DEPTH * (sector.zIndex + 1)) * Chunk.DEPTH);// - 1.0f + random;
						break;
					}
					//*/
					Debug.Log(string.Format("{0} GENERATED CORNER [{1}] as: {2}", sector.name, i, tData.data[i]));
				}
				else
				{
					Debug.Log(string.Format("{0} FOUND CORNER [{1}] as: {2}", sector.name, i, tData.data[i]));
				}
			}
		}

		tData.Save(string.Format("{0}/secdata", Application.persistentDataPath), string.Format("tData_{0}_{1}.scs", sector.xIndex, sector.zIndex));

		// Stupid constants needed to get a "good" perlin spread. 
		float xTick = 0.035f;
		float zTick = 0.035f;

		// Sector Level.
		for (int cx = 0; cx < Sector.WIDTH; cx++) 
		{
			for (int cz = 0; cz < Sector.DEPTH; cz++)
			{
				for (int cy = Sector.HEIGHT - 1; cy >= 0; cy--)
				{
					// Chunk Level.
					Chunk chunk = sector.chunks[cx,cy,cz];
					if (chunk != null)
					{	
						for (int x = 0; x < Chunk.WIDTH; x++)
						{
							for (int z = 0; z < Chunk.DEPTH; z++)
							{
								// For Each column of Blocks Within a Chunk.

								// Turn on independent positioning.
								float xPos = (((Sector.WIDTH * sector.xIndex) + cx) * Chunk.WIDTH) + x;
								float zPos = (((Sector.DEPTH * sector.zIndex) + cz) * Chunk.DEPTH) + z;
								
								// Turn on lerping of random sector height map.
								float xDist = ((float)(cx * Chunk.WIDTH) + x) / ((float)(Sector.WIDTH * Chunk.WIDTH) - 1);
								float zDist = ((float)(cz * Chunk.DEPTH) + z) / ((float)(Sector.DEPTH * Chunk.DEPTH) - 1);
								xTick = Mathf.SmoothStep(tData.data[1], tData.data[0], xDist);
								zTick = Mathf.SmoothStep(tData.data[2], tData.data[3], xDist);
								float yTick = Mathf.SmoothStep(zTick, xTick, zDist);
								
								float xPerlin = Mathf.PerlinNoise(xPos * 0.035f, zPos * 0.035f);
								float zPerlin = Mathf.PerlinNoise(zPos * 0.035f, xPos * 0.035f);
								
								float topCutOff = bottom + ((top - bottom) * yTick * ((xPerlin + zPerlin) / 2.0f));


								for (int y = Chunk.HEIGHT - 1; y >= 0; y--)
								{
									// Block Level.

									float yWorldPos = (((Sector.HEIGHT * sector.yIndex) + cy) * Chunk.HEIGHT) + y;

									//float bottomCutOff = (bottom * 0.75f) + ((top - bottom) * zPerlin);
									if (yWorldPos <= topCutOff)
									{
										if (yWorldPos <= (topCutOff * 0.15f))
										{
											blockData[pos] = 1;
										}
										//else if (yWorldPos <= bottomCutOff)
										//{
										//	blockData[pos] = 5;
										//}
										else
										{
											blockData[pos] = 2;
										}
										
									}
									else
									{
										blockData[pos] = 0;
									}
									pos++;
								}
							}
						}
					}
				}
			}
		}
		return blockData;
	}



	// Loads the given Sector's file and returns it's Freq value denoted by index.
	public static float GetTDataCorner(int xIndex, int zIndex, int index)
	{
		string filePath = string.Format("{0}/secdata", Application.persistentDataPath);
		string fileName = string.Format("tData_{0}_{1}.scs", xIndex, zIndex);

		float result = -1;
		if (Directory.Exists(filePath))
		{
			if (File.Exists(filePath +"/"+ fileName))
			{
				Stream stream = File.Open(filePath +"/"+ fileName, FileMode.Open);
				BinaryReader bReader = new BinaryReader(stream);

				// Get the rest of our terrainData data.
				float[] tData = new float[SectorTerrainData.VALUE_COUNT];
				for (int q = 0; q < SectorTerrainData.VALUE_COUNT; q++)
				{
					tData[q] = bReader.ReadSingle();
				}

				result = tData[index];
				bReader.Close();
			}
		}
		return result;
	}
}


public class SectorTerrainData
{
	public const int VALUE_COUNT = 4;
	public enum DATA_VALUES
	{
		TR = 0,
		TL,
		BL,
		BR
	}

	public float[] data = new float[VALUE_COUNT];

	// Saves the current TData over the given tData's file.
	public void Save(string filePath, string fileName)
	{
		if (Directory.Exists(filePath))
		{
			Stream stream = File.Open(filePath +"/"+ fileName, FileMode.Create);
			BinaryWriter bWriter = new BinaryWriter(stream);

			// Write all our terrain Data.
			for (int q = 0; q < SectorTerrainData.VALUE_COUNT; q++)
			{
				bWriter.Write(data[q]);
			}
			bWriter.Close();
		}
	}

	// Loads the tData file.
	public void Load(string filePath, string fileName)
	{
		if (Directory.Exists(filePath))
		{
			if (File.Exists(filePath +"/"+ fileName))
			{
				Stream stream = File.Open(filePath +"/"+ fileName, FileMode.Open);
				BinaryReader bReader = new BinaryReader(stream);

				// Get the rest of our terrainData data.
				for (int q = 0; q < SectorTerrainData.VALUE_COUNT; q++)
				{
					data[q] = bReader.ReadSingle();
				}
				bReader.Close();
			}
		}
	}
}
