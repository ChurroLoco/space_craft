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

	private Vector3 sectorQuadrant;

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
						if (clickedBlock.type.Breakable)
						{
							// TODO Move this to block logic and not chunk logic.
							chunk.SetBlock(block, 0);
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

			// Load Our Adjacent Sectors based on Player's location within their own sector.
			// Only load if the player has moved into a new sector quadrant. 
			Vector3 tempSectorQuadrant = transform.position - sector.center;
			tempSectorQuadrant = new Vector3(Mathf.Sign(tempSectorQuadrant.x), Mathf.Sign(tempSectorQuadrant.y), Mathf.Sign(tempSectorQuadrant.z));

			if (tempSectorQuadrant != sectorQuadrant)
			{
				sectorQuadrant = tempSectorQuadrant;
				if (sectorQuadrant.x != 0 && sector.xIndex + sectorQuadrant.x >= 0)
				{
					TerrainControllerScript.LoadSector(sector.xIndex + (int)sectorQuadrant.x, sector.yIndex, sector.zIndex);
					//TerrainControllerScript.UnloadSector((int)-sectorQuadrant.x, sector.yIndex, sector.zIndex);
				}
				
				if (sectorQuadrant.y != 0 && sector.yIndex + sectorQuadrant.y >= 0)
				{
					TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex + (int)sectorQuadrant.y, sector.zIndex);
					//TerrainControllerScript.UnloadSector(sector.xIndex, (int)-sectorQuadrant.y, sector.zIndex);
				}
				
				if (sectorQuadrant.z != 0 && sector.zIndex + sectorQuadrant.z >= 0)
				{
					TerrainControllerScript.LoadSector(sector.xIndex, sector.yIndex, sector.zIndex + (int)sectorQuadrant.z);
					//TerrainControllerScript.UnloadSector(sector.xIndex, sector.yIndex, (int)-sectorQuadrant.z);
				} 

				// This should be smarter, but we'll need a "chunkwise" face occlusion thing to make it into what it should be.
				// That and diagonals. 
			}
		}
		else
		{
			// Fuck. 
			TerrainControllerScript.LoadSector((int)(transform.position.x) / (Sector.WIDTH * Chunk.WIDTH), 
			                                   (int)(transform.position.y) / (Sector.HEIGHT * Chunk.HEIGHT),
			                                   (int)(transform.position.z) / (Sector.DEPTH * Chunk.DEPTH));
		}
	}

	void OnGUI()
	{
		GUILayout.Label(string.Format("World Position: {0}", transform.position.ToString()));
		GUILayout.Label(string.Format("Selected Block Type: {0}", BlockType.All[selectedBlockTypeId].Name));

	}
}

