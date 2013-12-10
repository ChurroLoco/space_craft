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

		for (int i = 0; i < SectorTerrainData.VALUE_COUNT; i++)
		{
			if (tData.data[i] <= 0)
			{
				// Try to get data from sectors around us.
				switch (i)
				{
					case (int)SectorTerrainData.DATA_VALUES.x1:
						tData.data[i] = GetTData(sector.xIndex - 1, sector.zIndex, (int)SectorTerrainData.DATA_VALUES.x2); // +1
						break;
					case (int)SectorTerrainData.DATA_VALUES.x2:
						tData.data[i] = GetTData(sector.xIndex + 1, sector.zIndex, (int)SectorTerrainData.DATA_VALUES.x1); // -1
						break;
					case (int)SectorTerrainData.DATA_VALUES.z1:
						tData.data[i] = GetTData(sector.xIndex, sector.zIndex - 1, (int)SectorTerrainData.DATA_VALUES.z2); // +1
						break;
					case (int)SectorTerrainData.DATA_VALUES.z2:
						tData.data[i] = GetTData(sector.xIndex, sector.zIndex + 1, (int)SectorTerrainData.DATA_VALUES.z1); // -1
						break;
				}
				// If we can't find our value, create a new one.
				if (tData.data[i] <= 0)
				{
					//tData.data[i] = Random.Range(0.035f, 0.05f);
					///*
					//float posRandom = Random.Range(0.5f, 2.0f) * (Sector.WIDTH * Chunk.WIDTH);
					switch (i)
					{
						case (int)SectorTerrainData.DATA_VALUES.x1:
						tData.data[i] = //tData.data[i+1] > 0 ? tData.data[i+1] - random : 
							((Sector.WIDTH * sector.xIndex) * Chunk.WIDTH);// - random;
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
							((Sector.DEPTH * (sector.zIndex + 1)) * Chunk.DEPTH) - 1.0f;// + random;
						break;
					}
					//*/
					Debug.Log(string.Format("{0} GENERATED EDGE [{1}] as: {2}", sector.name, i, tData.data[i]));
				}
				else
				{
					Debug.Log(string.Format("{0} GOT ADJECENT EDGE [{1}] as: {2}", sector.name, i, tData.data[i]));
				}
			}
		}

		// Perhaps attempt a different system. Dynamically change the tick size (test as linear ping pong between 0.001 and 0.005 between sectors) 
		// Resave x and z Pos to include tick value? Perhaps save TickSizes instead of x and z Pos and move based on world Pos?  

		tData.Save(string.Format("{0}/secdata", Application.persistentDataPath), string.Format("tData_{0}_{1}.scs", sector.xIndex, sector.zIndex));

		// Stupid constants needed to get a "good" perlin spread. 
		float xTick = 0.035f;
		float zTick = 0.035f;

		float top = 64;
		float bottom = 16;

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
								for (int y = Chunk.HEIGHT - 1; y >= 0; y--)
								{
									// Block Level.

									// Turn on independent positioning.
									 //float xPos = (((Sector.WIDTH * sector.xIndex) + cx) * Chunk.WIDTH) + x;
									 //float zPos = (((Sector.DEPTH * sector.zIndex) + cz) * Chunk.DEPTH) + z;

									// Turn on dependent positioning.
									//float xPos = tData.data[0] + ((cx * Chunk.WIDTH) + x);
									//float zPos = tData.data[2] + ((cz * Chunk.DEPTH) + z);
								
									// Turn on dependant lerp positioning.
									float xPos = Mathf.Lerp(tData.data[0], tData.data[1], ((float)(cx * Chunk.WIDTH) + x) / ((float)(Sector.WIDTH * Chunk.WIDTH) - 1));
									float zPos = Mathf.Lerp(tData.data[2], tData.data[3], ((float)(cz * Chunk.DEPTH) + z) / ((float)(Sector.DEPTH * Chunk.DEPTH) - 1));

									// Turn on lerping of tick sizes.
									//xTick = Mathf.Lerp(tData.data[0], tData.data[1], ((float)(cx * Chunk.WIDTH) + x) / ((float)(Sector.WIDTH * Chunk.WIDTH) - 1));
									//zTick = Mathf.Lerp(tData.data[2], tData.data[3], ((float)(cz * Chunk.DEPTH) + z) / ((float)(Sector.DEPTH * Chunk.DEPTH) - 1));

									float yWorldPos = (((Sector.HEIGHT * sector.yIndex) + cy) * Chunk.HEIGHT) + y;

									float xPerlin = Mathf.PerlinNoise(xPos * xTick, zPos * zTick);
									float zPerlin = Mathf.PerlinNoise(zPos * zTick, xPos * xTick);

									float topCutOff = bottom + ((top - bottom) * ((xPerlin + zPerlin) / 2.0f));

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
	public static float GetTData(int xIndex, int zIndex, int index)
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
		x1 = 0,
		x2,
		z1,
		z2
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
