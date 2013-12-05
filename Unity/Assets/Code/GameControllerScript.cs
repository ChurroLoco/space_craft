using UnityEngine;
using System.Collections;

public class GameControllerScript : MonoBehaviour
{
	Terrain terrain;

	// Use this for initialization
	void Start ()
	{
		BlockType.InitializeFromFile();
		Debug.Log(string.Format("BlockTypes imported count: '{0}'.", BlockType.All.Count));
		Invoke("SpawnPlayer", 3);
	}

	void SpawnPlayer()
	{
		float CenterOfTheEarth = (TerrainControllerScript.WIDTH * Sector.WIDTH * Chunk.WIDTH) / 2;
		PlayerScript.Spawn(new Vector3(CenterOfTheEarth, 63, CenterOfTheEarth));
	}
	
	void OnApplicationFocus(bool focusStatus) 
	{
		Screen.lockCursor = focusStatus;
	}
}