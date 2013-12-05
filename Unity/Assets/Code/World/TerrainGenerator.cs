using UnityEngine;
using System.Collections;

public class TerrainGenerator
{





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
	// The perlin frequency of the edges of a sector.
	Vector2 startFreq;
	Vector2 endFreq;
}
