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

						// Delete the block directly in front of the hit point.
						Block clickedBlock = TerrainControllerScript.GetBlockAt(alteredHitPoint);
						if (clickedBlock != null && clickedBlock.type.Breakable)
						{
							// TODO Move this to block logic and not chunk logic.
							clickedBlock.chunk.SetBlock(clickedBlock, 0);
							GameObject particles = Instantiate(Resources.Load("Prefabs/Particles/Small Dust Burst")) as GameObject;
							particles.transform.position = hit.point;
						}
					}
					else if (rightclick)
					{
						Vector3 alteredHitPoint = hit.point - (ray.direction * 0.001f);
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
				LoadNewSectors(sector.xIndex, sector.yIndex, sector.zIndex);
				previousSector = sector;
			}
		}
		else
		{
			// We fucked up.
			TerrainControllerScript.LoadSector((int)transform.position.x / (Sector.WIDTH * Chunk.WIDTH), 
			                                   (int)transform.position.y / (Sector.HEIGHT * Chunk.HEIGHT), 
			                                   (int)transform.position.z / (Sector.DEPTH * Chunk.DEPTH));
		}
	}

	private void LoadNewSectors(int sectorX, int sectorY, int sectorZ)
	{
		// Mark all current sectors as ready to delete.
		foreach(Sector tsector in TerrainControllerScript.instance.activeSectors)
		{
			tsector.canBeUnloaded = true;
		}
		foreach(Sector tsector in TerrainControllerScript.instance.SectorsToGenerate)
		{
			tsector.canBeUnloaded = true;
		}
		if (TerrainControllerScript.instance.loadingSector != null)
		{
			TerrainControllerScript.instance.loadingSector.canBeUnloaded = true;
		}

		// Load the 27 sectors of and around the player. Any prexisting sectors marked for delete will be unmarked instead of reloaded.

		// Center
		TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex, sector.zIndex);
		// Adjacents.
		TerrainControllerScript.LoadSector(sector.xIndex + 1, sector.yIndex, sector.zIndex);
		TerrainControllerScript.LoadSector(sector.xIndex - 1, sector.yIndex, sector.zIndex);
		TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex + 1, sector.zIndex);
		TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex - 1, sector.zIndex);
		TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex, sector.zIndex + 1);
		TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex, sector.zIndex - 1);
		// Center Diagonals
		TerrainControllerScript.LoadSector(sector.xIndex + 1, sector.yIndex, sector.zIndex + 1);
		TerrainControllerScript.LoadSector(sector.xIndex + 1, sector.yIndex, sector.zIndex - 1);
		TerrainControllerScript.LoadSector(sector.xIndex - 1, sector.yIndex, sector.zIndex + 1);
		TerrainControllerScript.LoadSector(sector.xIndex - 1, sector.yIndex, sector.zIndex - 1);
		// Top Diagonals
		TerrainControllerScript.LoadSector(sector.xIndex + 1, sector.yIndex + 1, sector.zIndex);
		TerrainControllerScript.LoadSector(sector.xIndex - 1, sector.yIndex + 1, sector.zIndex);
		TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex + 1, sector.zIndex + 1);
		TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex + 1, sector.zIndex - 1);
		// Bottom Diagonals
		TerrainControllerScript.LoadSector(sector.xIndex + 1, sector.yIndex - 1, sector.zIndex);
		TerrainControllerScript.LoadSector(sector.xIndex - 1, sector.yIndex - 1, sector.zIndex);
		TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex - 1, sector.zIndex + 1);
		TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex - 1, sector.zIndex - 1);
		// Top Corners
		TerrainControllerScript.LoadSector(sector.xIndex + 1, sector.yIndex + 1, sector.zIndex + 1);
		TerrainControllerScript.LoadSector(sector.xIndex + 1, sector.yIndex + 1, sector.zIndex - 1);
		TerrainControllerScript.LoadSector(sector.xIndex - 1, sector.yIndex + 1, sector.zIndex + 1);
		TerrainControllerScript.LoadSector(sector.xIndex - 1, sector.yIndex + 1, sector.zIndex - 1);
		// Bottom Corners
		TerrainControllerScript.LoadSector(sector.xIndex + 1, sector.yIndex - 1, sector.zIndex + 1);
		TerrainControllerScript.LoadSector(sector.xIndex + 1, sector.yIndex - 1, sector.zIndex - 1);
		TerrainControllerScript.LoadSector(sector.xIndex - 1, sector.yIndex - 1, sector.zIndex + 1);
		TerrainControllerScript.LoadSector(sector.xIndex - 1, sector.yIndex - 1, sector.zIndex - 1);
	}

	void OnGUI()
	{
		GUILayout.Label(string.Format("World Position: {0}", transform.position.ToString()));
		GUILayout.Label(string.Format("Selected Block Type: {0}", BlockType.All[selectedBlockTypeId].Name));

	}
}

