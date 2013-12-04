using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]
public class PlayerScript : MonoBehaviour
{
	public CharacterController characterController;
	public Camera playerCamera;
	private float walkSpeed = 2.5f;
	private float jumpSpeed = 5.0f;
	private float turnSpeed = 190;
	public bool isGrounded;
	public float verticalSpeed = 0;

	private Sector sector = null;
	private Chunk chunk = null;
	private Block block = null;

	private Sector previousSector = null;

	[SerializeField]
	private int selectedBlockTypeId = 1;

	public static PlayerScript Spawn(Vector3 position)
	{
		GameObject player = Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
		player.name = "Player";
		player.transform.position = position;
		PlayerScript script = player.GetComponent<PlayerScript>();
		return script;
	}
	
	// Use this for initialization
	void Start ()
	{
		//Camera.main.enabled = false;
		Screen.lockCursor = true;
	}
	
	// Update is called once per frame
	void Update ()
	{	
		// Get our positioning strucures.
		this.block = TerrainControllerScript.GetBlockAt(transform.position);
		if (block != null)
		{
			this.chunk = block.chunk;
			this.sector = chunk.sector;
		}
		else
		{
			this.chunk = TerrainControllerScript.GetChunkAt(transform.position);
			if (chunk != null)
			{
				this.sector = chunk.sector;
				this.chunk = null;
			}
			else
			{
				this.sector = TerrainControllerScript.GetSectorAt(transform.position);
			}
		}

		if (sector != null)
		{
			// Do Input stuff.
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				sector.Save();
			}

			Vector3 movement = Vector3.zero;
			movement = transform.forward * Input.GetAxis("Vertical") * walkSpeed;
			movement += transform.right * Input.GetAxis("Horizontal") * walkSpeed;

			if (Input.GetKeyDown(KeyCode.E))
			{
				selectedBlockTypeId++;
				selectedBlockTypeId = Mathf.Clamp(selectedBlockTypeId, 1, BlockType.All.Count);
			}

			if (Input.GetKeyDown(KeyCode.Q))
			{
				selectedBlockTypeId--;
				selectedBlockTypeId = Mathf.Clamp(selectedBlockTypeId, 1, BlockType.All.Count);
			}

			if (Input.GetKeyDown(KeyCode.F))
			{
				GameObject flare = Instantiate(Resources.Load("Prefabs/Projectiles/Flare")) as GameObject;
				flare.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 0.2f;
				flare.rigidbody.AddRelativeTorque(playerCamera.transform.right * 3);
				flare.rigidbody.AddForce((playerCamera.transform.forward * 35) + (playerCamera.transform.up * 15));
			}

			bool leftClick = Input.GetMouseButtonDown(0);
			bool rightclick = Input.GetMouseButtonDown(1);
			if (rightclick || leftClick)
			{
				Screen.lockCursor = true;
				Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 4.0f))
				{

					// We hit something!
					if (leftClick)
					{
						Vector3 alteredHitPoint = hit.point + (ray.direction * 0.001f);
						Debug.Log(string.Format("AlteredHitPoint {0}", alteredHitPoint));


						// Delete the block directly in front of the hit point.
						Block clickedBlock = TerrainControllerScript.GetBlockAt(alteredHitPoint);
						if (clickedBlock != null && clickedBlock.type.Breakable)
						{
							// TODO Move this to block logic and not chunk logic.
							block.chunk.SetBlock(block, 0);
							GameObject particles = Instantiate(Resources.Load("Prefabs/Particles/Small Dust Burst")) as GameObject;
							particles.transform.position = hit.point;
						}
					}
					else if (rightclick)
					{
						Vector3 alteredHitPoint = hit.point - (ray.direction * 0.001f);
						Debug.Log(string.Format("AlteredHitPoint {0}", alteredHitPoint));

						Chunk chunk = TerrainControllerScript.GetChunkAt(alteredHitPoint);
						if (chunk != null)
						{
							// Create a block directly behind the hit point.
							chunk.SetBlock(alteredHitPoint, selectedBlockTypeId, chunk.sector);
						}
					}
				}
			}

			if (Screen.lockCursor)
			{
				transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime);
				playerCamera.transform.RotateAround(playerCamera.transform.position, playerCamera.transform.right, -Input.GetAxis("Mouse Y") * turnSpeed * Time.deltaTime);
			}
			
			if (characterController.isGrounded)
			{
				verticalSpeed = 0;
				if (Input.GetKey(KeyCode.Space))
				{
					verticalSpeed = jumpSpeed;	
				}
			}
			else
			{
				verticalSpeed -= 9.8f * Time.deltaTime;
			}
			
			movement.y = verticalSpeed;
			characterController.Move(movement * Time.deltaTime);

			// Load Sectors around the player.
			if (sector != previousSector)
			{
				if (previousSector != null)
				{
					int deltaX = Mathf.Clamp(sector.xIndex - previousSector.xIndex, -1, 1);
					int deltaY = Mathf.Clamp(sector.yIndex - previousSector.yIndex, -1, 1);
					int deltaZ = Mathf.Clamp(sector.zIndex - previousSector.zIndex, -1, 1);

					// Load the next nine sectors in the direction the player moved.
					LoadNewSectors(deltaX, deltaY, deltaZ);
					// Unload the nine sectors now two sectors away from the player.
					UnloadOldSectors(deltaX * -2, deltaY * -2, deltaZ * -2);
				}
				else
				{
					// We just entered the game, Load All 27 Sectors.
					LoadNewSectors(0, 0, 0);
				}
				previousSector = sector;
			}
		}
	}

	private void LoadNewSectors(int deltaX, int deltaY, int deltaZ)
	{
		// Forward
		TerrainControllerScript.LoadSector(sector.xIndex + deltaX, sector.yIndex + deltaY, sector.zIndex + deltaZ);

		// X Adjacents.
		if (deltaX == 0)
		{
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY, sector.zIndex + deltaZ);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY, sector.zIndex + deltaZ);
		}
		// Y Adjacents.
		if (deltaY == 0)
		{
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ);
		}
		// Z Adjacents.
		if (deltaZ == 0)
		{
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX, sector.yIndex + deltaY, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX, sector.yIndex + deltaY, sector.zIndex + deltaZ - 1);
		}
		// X and Y Diagonals
		if (deltaX == 0 && deltaY == 0)
		{
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ);
		}
		// X and Z Diagonals
		if (deltaX == 0 && deltaZ == 0)
		{
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY, sector.zIndex + deltaZ - 1);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY, sector.zIndex + deltaZ - 1);
		}
		// Y and Z Diagonals
		if (deltaY == 0 && deltaZ == 0)
		{
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ - 1);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.LoadSector(sector.xIndex + deltaX, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ - 1);
		}
	}

	private void UnloadOldSectors(int deltaX, int deltaY, int deltaZ)
	{
		// Forward
		TerrainControllerScript.UnloadSector(sector.xIndex + deltaX, sector.yIndex + deltaY, sector.zIndex + deltaZ);
		
		// X Adjacents.
		if (deltaX == 0)
		{
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY, sector.zIndex + deltaZ);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY, sector.zIndex + deltaZ);
		}
		// Y Adjacents.
		if (deltaY == 0)
		{
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ);
		}
		// Z Adjacents.
		if (deltaZ == 0)
		{
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX, sector.yIndex + deltaY, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX, sector.yIndex + deltaY, sector.zIndex + deltaZ - 1);
		}
		// X and Y Diagonals
		if (deltaX == 0 && deltaY == 0)
		{
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ);
		}
		// X and Z Diagonals
		if (deltaX == 0 && deltaZ == 0)
		{
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY, sector.zIndex + deltaZ - 1);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX - 1, sector.yIndex + deltaY, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX + 1, sector.yIndex + deltaY, sector.zIndex + deltaZ - 1);
		}
		// Y and Z Diagonals
		if (deltaY == 0 && deltaZ == 0)
		{
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ - 1);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX, sector.yIndex + deltaY - 1, sector.zIndex + deltaZ + 1);
			TerrainControllerScript.UnloadSector(sector.xIndex + deltaX, sector.yIndex + deltaY + 1, sector.zIndex + deltaZ - 1);
		}
	}

	void OnGUI()
	{
		GUILayout.Label(string.Format("World Position: {0}", transform.position.ToString()));
		GUILayout.Label(string.Format("Selected Block Type: {0}", BlockType.All[selectedBlockTypeId].Name));

	}
}

