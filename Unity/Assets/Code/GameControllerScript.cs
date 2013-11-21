using UnityEngine;
using System.Collections;

public class GameControllerScript : MonoBehaviour
{
	Terrain terrain;

	// Use this for initialization
	void Start ()
	{
		Invoke("SpawnPlayer", 7);
	}

	void SpawnPlayer()
	{
		PlayerScript.Spawn(new Vector3(0, (TerrainControllerScript.HEIGHT * BlockChunkScript.HEIGHT) + 250, 0));
	}
	
	void OnApplicationFocus(bool focusStatus) 
	{
		Screen.lockCursor = focusStatus;
	}

}