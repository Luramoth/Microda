using UnityEngine;

/*
	This file is part of KamiKaze.

	KamiKaze is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
	KamiKaze is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
	You should have received a copy of the GNU Affero General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/agpl-3.0.html>.
*/

// TO/DO: fix bug where player will float in air on slopes, possibly by making it so the palyer doesent float in the air and will instead be able to jump mid air for a few frames
// TO/DO: possibly add double jump?
// TODO: add dash
// TODO: add roll

public class PlayerControl : MonoBehaviour
{
	//vars
	[Header("Basic movement stuff")]
	public float speed = 6f;
	public float runSpeed = 10f;
	public float jumpPower = 3f;
	public float gravity = 19.62f;
	public bool mouseLock = true;
	public bool isRunning = false;
	public bool isJumping = false;
	public bool hasDoubleJumped = false;

	[Header("Advanced tweaks")]

	public float turnSmoothSpeed = 0.1f;
	public int coyoteTimeFrames = 30; 
	public int cTFrames = 0;
	public bool coyoteTime = false;
	public float turnSmoothSpeed = 0.1f;
	private float turnSmoothVel;
	private Vector3 velocity;


	//Objects
	[Header("References")]
	public CharacterController controller;
	public Transform cam;

	// Update is called once per frame
	void Update()
	{
		ControlHandler();
	}

	//simple system to handle player input
	void ControlHandler()
	{
		float charSpeed;

		if (controller.isGrounded && velocity.y < 0)// if the player is on the ground, slow the velocity
		{
			velocity.y = -2f;
		}

		if (Input.GetKey("left shift"))
		{
			charSpeed = runSpeed;

			isRunning = true;
		}
		else
		{
			charSpeed = speed;

			isRunning = false;
		}

		//gather axis movements
		Vector3 inputAxis = new Vector3
		(
			Input.GetAxisRaw("Horizontal"),
			0f,
			Input.GetAxisRaw("Vertical")
		).normalized;

		if (controller.isGrounded) // if the player is on the ground then reset jump and double jump
		{
			isJumping = false;
			hasDoubleJumped = false;
		}

		//make character jump if space is pressed
		if (Input.GetButtonDown("Jump")) 
		{
			if (canJump(inputAxis))
			{
				velocity.y = Mathf.Sqrt(jumpPower * -2.0f * -gravity);

				isJumping = true;
			}
		}

		velocity.y -= gravity * Time.deltaTime; //handle gravity


		//apply movements if input is detected
		if (inputAxis.magnitude >= 0.1f)
		{
			// based ont he players movement direction, try to rotate the player model to match it
			float targetAngle = Mathf.Atan2(inputAxis.x,inputAxis.z) * Mathf.Rad2Deg + cam.eulerAngles.y; //this find the target angle the player should be facing in
			float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVel, turnSmoothSpeed);// the creates a variable to make sure the transition is smooth when switching angles
			transform.rotation = Quaternion.Euler(0f,angle,0f); // the applies the angle

			// move the player
			Vector3 moveDir = Quaternion.Euler(0f,targetAngle,0f) * Vector3.forward; // this will take the current direction the camera is facing
			controller.Move(moveDir.normalized * charSpeed * Time.deltaTime);// this uses the direction the camera is facing in order to move forward
		}

		controller.Move(velocity * Time.deltaTime);// handle character vertical movement

		//unlock cursor when escape is pressed
		if (Input.GetButtonDown("Cancel"))
		{
			mouseLock = !mouseLock;
		}

		if (mouseLock == true) // if the cursor should be locked, lock it, if not, then dont
		{
			Cursor.lockState = CursorLockMode.Locked;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	bool canJump(Vector3 inputAxis)
	{
		if (controller.isGrounded)// if the player is on the ground they cant possibly be on coyote time or double jumping so allow them to jump
		{
			return true;
		}
		else //  else check if they are double jumping or have coyote time
		{
			if (hasDoubleJumped == false)
			{
				if (coyoteTimeAvailable(inputAxis))
				{
					if (cTFrames == 0 && coyoteTime != true) // if the amount of frames is at 0 and coyote time hasent went through yet, set up starting frames
					{
						cTFrames = coyoteTimeFrames;
						coyoteTime = true;

						return true;
					}
					else // else make it tso the player can jump mid air
					{
						cTFrames --;

						return true;
					}
				}
				else // if coyote time isent available then set frames to 0 and have the player double jump
				{
					cTFrames = 0;

					hasDoubleJumped = true;

					return true;
				}
			}
			else
			{
				return false;
			}
		}
	}

	// function that checks if the player can utilize coyote time at all
	bool coyoteTimeAvailable(Vector3 inputAxis)
	{
		if (controller.isGrounded || isJumping) // if the player is touching the ground or has already jumped, cancel coyote time
		{
			return false;
		}
		else
		{
			if (inputAxis.magnitude >= 0.1f) // see if the player is moving forward at all, if not cancel coyote time
			{
				if (cTFrames <= 0 && coyoteTime) // if player has run out of coyote time frames then cancel coyote time, else keep it going
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}
	}
}