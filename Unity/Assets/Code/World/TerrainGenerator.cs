using UnityEngine;
using System.Collections;
using System.IO;

public class TerrainGenerator
{
	// The id of the terrain. Change this to tell the world to regenerate terrain.
	public const int REVISION_ID = 0;


	// Big function that sets the blocks in an entire sector.
	public static int[] GenerateSector(Sector sector)
	{
		int[] blockData = new int[Sector.HEIGHT * Sector.DEPTH * Sector.WIDTH * Chunk.HEIGHT * Chunk.DEPTH * Chunk.WIDTH];
		int pos = 0;

		// The starting data is the adjacent x and z frequencies of the adjacent sectors' shared edges.
		float[] tData = new float[4];
		tData[0] = GetTData(sector.xIndex - 1, sector.yIndex, sector.zIndex, 0);
		tData[1] = GetTData(sector.xIndex + 1, sector.yIndex, sector.zIndex, 1);
		tData[2] = GetTData(sector.xIndex, sector.yIndex, sector.zIndex + 1, 2);
		tData[3] = GetTData(sector.xIndex, sector.yIndex, sector.zIndex - 1, 3);

		// Now read our block data.
		for (int y = Sector.HEIGHT - 1; y >= 0; y--)
		{
			for (int z = 0; z < Sector.DEPTH; z++)
			{
				for (int x = 0; x < Sector.WIDTH; x++)
				{
					Chunk chunk = sector.chunks[x,y,z];
					if (chunk != null)
					{
						// TODO: Remake Perlin to use the Sector border Freqs.
						int[] chunkData = FillBlocks();//PerlinBlock(chunk, 80, 16, 1);
						chunkData.CopyTo(blockData, pos);
						pos += chunkData.Length;
					}
				}
			}
		}

		return blockData;
	}



	// Loads the given Sector's file and returns it's Freq value denoted by index.
	public static float GetTData(int xIndex, int yIndex, int zIndex, int index)
	{
		string filePath = string.Format("{0}/secdata", Application.persistentDataPath);
		string fileName = string.Format("sec_{0}_{1}_{2}.scs", xIndex, yIndex, zIndex);

		float result = -1;
		if (Directory.Exists(filePath))
		{
			if (File.Exists(filePath +"/"+ fileName))
			{
				Stream stream = File.Open(filePath +"/"+ fileName, FileMode.Open);
				BinaryReader bReader = new BinaryReader(stream);
				
				// Get our terrain version.
				int tVersion = bReader.ReadInt32();
				if (tVersion == TerrainGenerator.REVISION_ID)
				{
					// Get the rest of our terrainData data.
					float[] tData = new float[4];
					for (int q = 0; q < 4; q++)
					{
						tData[q] = bReader.ReadSingle();
					}

					result = tData[index];
				}
				bReader.Close();
			}
		}
		return result;
	}






	public static int[] FillBlocks()
	{
		int[] blockData = new int[Chunk.HEIGHT * Chunk.DEPTH * Chunk.WIDTH];
		int pos = 0;
		for (int y = Chunk.HEIGHT - 1; y >= 0; y--)
		{
			for (int z = 0; z < Chunk.DEPTH; z++)
			{
				for (int x = 0; x < Chunk.WIDTH; x++)
				{
					blockData[pos++] = 1;
				}
			}
		}
		return blockData;
	}
	
	public static int[] SinBlock(Chunk chunk, float xFreq, float zFreq, float xAmp, float zAmp, int top, int bottom)
	{
		int[] blockData = new int[Chunk.HEIGHT * Chunk.DEPTH * Chunk.WIDTH];
		int pos = 0;
		for (int y = Chunk.HEIGHT - 1; y >= 0; y--)
		{
			for (int z = 0; z < Chunk.DEPTH; z++)
			{
				for (int x = 0; x < Chunk.WIDTH; x++)
				{
					float xWorldPos = (((TerrainControllerScript.WIDTH * chunk.sector.xIndex) + chunk.xIndex) * Chunk.WIDTH) + x;
					float zWorldPos = (((TerrainControllerScript.DEPTH * chunk.sector.zIndex) + chunk.zIndex) * Chunk.DEPTH) + z;
					float xSin = ((Mathf.Sin(xWorldPos * xFreq) + 1)/2) * xAmp;
					float zSin = ((Mathf.Sin(zWorldPos * zFreq) + 1)/2) * zAmp;
					float cutOff = bottom + ((top - bottom) * ((xSin + zSin)/2));
					
					float blocksHeight =  (((chunk.sector.yIndex * Sector.HEIGHT) + chunk.yIndex) * Chunk.HEIGHT) + y;
					
					if (blocksHeight <= cutOff / 6)
					{
						blockData[pos++] = 1;
					}
					else
					{
						blockData[pos++] = (blocksHeight <= cutOff) ? 2: 0;
					}
				}
			}
		}
		return blockData;
	}

	
	public static int[] PerlinBlock(Chunk chunk, int top, int bottom, float freq)
	{
		int[] blockData = new int[Chunk.HEIGHT * Chunk.DEPTH * Chunk.WIDTH];
		int pos = 0;
		for (int y = Chunk.HEIGHT - 1; y >= 0; y--)
		{
			for (int z = 0; z < Chunk.DEPTH; z++)
			{
				for (int x = 0; x < Chunk.WIDTH; x++)
				{
					float xWorldPos = (((Sector.WIDTH * chunk.sector.xIndex) + chunk.xIndex) * Chunk.WIDTH) + x;
					float yWorldPos =  (((Sector.HEIGHT * chunk.sector.yIndex) + chunk.yIndex) * Chunk.HEIGHT) + y;
					float zWorldPos = (((Sector.DEPTH * chunk.sector.zIndex) + chunk.zIndex) * Chunk.DEPTH) + z;
					
					float xPerlin = Mathf.PerlinNoise((xWorldPos * freq) / (Chunk.HEIGHT * Sector.HEIGHT), (zWorldPos * freq)/ (Chunk.DEPTH * Sector.DEPTH));
					float zPerlin = Mathf.PerlinNoise((zWorldPos * freq)/ (Chunk.DEPTH * Sector.DEPTH), (xWorldPos * freq) / (Chunk.HEIGHT * Sector.HEIGHT));
					
					float topCutOff = bottom + ((top - bottom) * xPerlin);
					float bottomCutOff = (bottom * 0.75f) + ((top - bottom) * zPerlin);
					if (yWorldPos <= topCutOff)
					{
						if (yWorldPos <= (topCutOff * 0.15f))
						{
							blockData[pos] = 1;
						}
						else if (yWorldPos <= bottomCutOff)
						{
							blockData[pos] = 5;
						}
						else
						{
							blockData[pos] = 2;
						}
						
					}
					else
					{
						blockData[pos] = 0;
					}
					
					// Subtraction pass for caves.
					topCutOff = (bottom * 0.55f) + (((top - bottom) * 0.5f) * (1 - zPerlin)); 
					bottomCutOff = (bottom * 0.7f) + (((top - bottom)) * (1 - xPerlin)); 
					
					if (yWorldPos < topCutOff && yWorldPos > bottomCutOff)
					{
						blockData[pos] = 0;
					}
					pos++;
				}
			}
		}
		return blockData;
	}
}

public class SectorTerrainData
{
	// The revision Id to know if we should regenerate the sector when loading it. 
	public int terrainVersion = -1;
	// The perlin frequency of the edges of a sector.
	public float xFreqStart = -1;
	public float xFreqEnd = -1;
	public float zFreqStart = -1;
	public float zFreqEnd = -1;

	public float[] getData()
	{
		float[] data = new float[4];
		data[0] = xFreqStart;
		data[1] = xFreqEnd;
		data[2] = zFreqStart;
		data[3] = zFreqEnd;
		return data;
	}

	public void setData(float[] data)
	{
		xFreqStart = data[0];
		xFreqEnd = data[1];
		zFreqStart = data[2];
		zFreqEnd = data[3];
	}
}
