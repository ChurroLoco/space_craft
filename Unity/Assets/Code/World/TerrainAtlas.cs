using UnityEngine;
using System.Collections;

public static class TerrainAtlas 
{
	public const int ATLAS_WIDTH = 512;
	public const int ATLAS_HEIGHT = 512;
	public const int TEXTURE_WIDTH = 16;
	public const int TEXTURE_HEIGHT = 16;

	public static int COLUMNS = ATLAS_WIDTH / TEXTURE_WIDTH;
	public static int ROWS = ATLAS_HEIGHT / TEXTURE_HEIGHT;
	public static float UV_WIDTH = (float)TEXTURE_WIDTH / ATLAS_WIDTH;
	public static float UV_HEIGHT = (float)TEXTURE_HEIGHT / ATLAS_HEIGHT;
	public static int MAX_TEXTURES = COLUMNS * ROWS;

	public static Vector2 UVCordFromIndex(int index, Vector2 normalizedOffset)
	{

		index = Mathf.Clamp(index, 0, MAX_TEXTURES);
		float uCoord = (index % COLUMNS) * UV_WIDTH;

		int rowIndex =  (ROWS - 1) - (int)(index / COLUMNS);
		float vCoord = rowIndex * UV_HEIGHT;

		Vector2 result = new Vector2(uCoord + (normalizedOffset.x * UV_WIDTH), vCoord + (normalizedOffset.y * UV_HEIGHT));
		return result;
	}
}
