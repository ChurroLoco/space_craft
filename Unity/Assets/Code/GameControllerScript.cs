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
		PlayerScript.Spawn(new Vector3(16, 12, 16));
	}
	
	void OnApplicationFocus(bool focusStatus) 
	{
		Screen.lockCursor = focusStatus;
	}
}