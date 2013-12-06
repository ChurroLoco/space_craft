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
					case (int)SectorTerrainData.DATA_VALUES.xFreqStart:
						tData.data[i] = GetTData(sector.xIndex + 1, sector.zIndex, (int)SectorTerrainData.DATA_VALUES.xFreqEnd);
						break;
					case (int)SectorTerrainData.DATA_VALUES.xFreqEnd:
						tData.data[i] = GetTData(sector.xIndex - 1, sector.zIndex, (int)SectorTerrainData.DATA_VALUES.xFreqStart);
						break;
					case (int)SectorTerrainData.DATA_VALUES.zFreqStart:
						tData.data[i] = GetTData(sector.xIndex, sector.zIndex + 1, (int)SectorTerrainData.DATA_VALUES.zFreqEnd);
						break;
					case (int)SectorTerrainData.DATA_VALUES.zFreqEnd:
						tData.data[i] = GetTData(sector.xIndex, sector.zIndex - 1, (int)SectorTerrainData.DATA_VALUES.zFreqStart);
						break;
				}
				// If we can't find our value, create a new one.
				if (tData.data[i] <= 0)
				{
					Debug.Log(string.Format("{0} MADE [{1}]", sector.name, i));
					tData.data[i] = Random.Range(0.5f, 1.2f);
				}
			}
			else
			{
				Debug.Log(string.Format("{0} FOUND [{1}] in File", sector.name, i));
			}

		}

		tData.Save(string.Format("{0}/secdata", Application.persistentDataPath), string.Format("tData_{0}_{1}.scs", sector.xIndex, sector.zIndex));

		float xFreq = 0;
		float zFreq = 0;

		float top = 80;
		float bottom = 24;
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
									xFreq = Mathf.Lerp(tData.data[0], tData.data[1], ((float)(cx * Chunk.WIDTH) + x) / (float)(Sector.WIDTH * Chunk.WIDTH) - 1);
									zFreq = Mathf.Lerp(tData.data[2], tData.data[3], ((float)(cz * Chunk.DEPTH) + z) / (float)(Sector.DEPTH * Chunk.DEPTH) - 1);

									float xWorldPos = (((Sector.WIDTH * chunk.sector.xIndex) + cx) * Chunk.WIDTH) + x;
									float yWorldPos = (((Sector.HEIGHT * chunk.sector.yIndex) + cy) * Chunk.HEIGHT) + y;
									float zWorldPos = (((Sector.DEPTH * chunk.sector.zIndex) + cz) * Chunk.DEPTH) + z;

									float xPerlin = Mathf.PerlinNoise((xWorldPos * xFreq) / (Chunk.HEIGHT * Sector.HEIGHT), (zWorldPos * zFreq)/ (Chunk.DEPTH * Sector.DEPTH));
									//float zPerlin = Mathf.PerlinNoise((zWorldPos * zFreq)/ (Chunk.DEPTH * Sector.DEPTH), (xWorldPos * xFreq) / (Chunk.HEIGHT * Sector.HEIGHT));
									
									float topCutOff = bottom + ((top - bottom) * xPerlin);
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
				Debug.Log(string.Format("FOUND [{0}] in {1}", index, fileName));
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
		xFreqStart = 0,
		xFreqEnd,
		zFreqStart,
		zFreqEnd
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
