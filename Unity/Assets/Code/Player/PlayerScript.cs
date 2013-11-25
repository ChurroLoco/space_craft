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

					Chunk chunk = TerrainControllerScript.getChunkAt(alteredHitPoint);
					if (chunk != null)
					{
						// Delete the block directly in front of the hit point.
						Block clickedBlock = TerrainControllerScript.getBlockAt(alteredHitPoint);
						if (clickedBlock.type.Breakable)
						{
							// TODO Move this to block logic and not chunk logic.
							chunk.SetBlock(alteredHitPoint, 0);
							GameObject particles = Instantiate(Resources.Load("Prefabs/Particles/Small Dust Burst")) as GameObject;
							particles.transform.position = hit.point;
						}
					}
				}
				else if (rightclick)
				{
					Vector3 alteredHitPoint = hit.point - (ray.direction * 0.001f);
					Debug.Log(string.Format("AlteredHitPoint {0}", alteredHitPoint));

					Chunk chunk = TerrainControllerScript.getChunkAt(alteredHitPoint);
					if (chunk != null)
					{
						// Create a block directly behind the hit point.
						chunk.SetBlock(alteredHitPoint, selectedBlockTypeId);
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
	}

	void OnGUI()
	{
		GUILayout.Label(string.Format("World Position: {0}", transform.position.ToString()));
		GUILayout.Label(string.Format("Selected Block Type: {0}", BlockType.All[selectedBlockTypeId].Name));

	}
}

