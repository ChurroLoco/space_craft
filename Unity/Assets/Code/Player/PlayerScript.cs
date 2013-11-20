using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]
public class PlayerScript : MonoBehaviour
{
	public CharacterController characterController;
	public Camera camera;
	private float walkSpeed = 8f;
	private float jumpSpeed = 5f;
	private float turnSpeed = 190;
	public bool isGrounded;
	private float verticalSpeed = 0;
	
	
	// Use this for initialization
	void Start ()
	{
		Screen.lockCursor = true;
	}
	
	// Update is called once per frame
	void Update ()
	{	
		this.isGrounded = characterController.isGrounded;
		
		if (characterController.isGrounded)
		{
			verticalSpeed = 0;

		}
		else
		{
			verticalSpeed -= 9.8f * Time.deltaTime;
		}
		
		if (Input.GetKeyDown(KeyCode.Space))
		{
			verticalSpeed = jumpSpeed;	
		}
		
		characterController.Move(Vector3.up * verticalSpeed * Time.deltaTime);
		characterController.Move(transform.forward * Input.GetAxis("Vertical") * walkSpeed * Time.deltaTime);
		characterController.Move(transform.right * Input.GetAxis("Horizontal") * walkSpeed * Time.deltaTime);
		
		if (Screen.lockCursor)
		{
			transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime);
			//camera.transform.RotateAround(camera.transform.position, camera.transform.right, -Input.GetAxis("Mouse Y") * turnSpeed * Time.deltaTime);
		}

	}
}

