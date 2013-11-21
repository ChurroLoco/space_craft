using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]
public class PlayerScript : MonoBehaviour
{
	public CharacterController characterController;
	public Camera camera;
	private float walkSpeed = 2.5f;
	private float jumpSpeed = 5.0f;
	private float turnSpeed = 190;
	public bool isGrounded;
	public float verticalSpeed = 0;

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
		
		if (Screen.lockCursor)
		{
			transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime);
			camera.transform.RotateAround(camera.transform.position, camera.transform.right, -Input.GetAxis("Mouse Y") * turnSpeed * Time.deltaTime);
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
		GUILayout.Label(transform.position.ToString());
	}
}

