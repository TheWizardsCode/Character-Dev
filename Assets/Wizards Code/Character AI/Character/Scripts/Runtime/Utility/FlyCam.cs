using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace WizardsCode.Character
{
	public class FlyCam : MonoBehaviour
	{
		/*
	EXTENDED FLYCAM
		Desi Quintans (CowfaceGames.com), 17 August 2012.
		Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
 
	LICENSE
		Free as in speech, and free as in beer.
 
	FEATURES
		WASD/Arrows:    Movement
		          E:    Climb
		          Q:    Drop
                      Shift:    Move faster
                    Control:    Move slower
                        End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
	*/

		public float cameraSensitivity = 100;
		public float climbSpeed = 15;
		public float normalMoveSpeed = 30;
		public float slowMoveFactor = 0.25f;
		public float fastMoveFactor = 3;

		private float rotationX = 0.0f;
		private float rotationY = 0.0f;

		private NavMeshAgent m_Agent;

		void Start()
		{
			// X and Y are reversed deliberatly, it's maths, don't ask me!
			rotationX = transform.rotation.eulerAngles.y;
			rotationY = -transform.rotation.eulerAngles.x;

			m_Agent = GetComponent<NavMeshAgent>();
		}

		void Update()
		{
			if (Mouse.current != null && Mouse.current.rightButton.isPressed)
			{
				rotationX += Mouse.current.delta.x.ReadValue() * (cameraSensitivity * 0.05f) * Time.deltaTime;
				rotationY += Mouse.current.delta.y.ReadValue() * (cameraSensitivity * 0.05f) * Time.deltaTime;
				rotationY = Mathf.Clamp(rotationY, -90, 90);
			}

			transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
			transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

			float horizontalInput = 0f;
			float verticalInput = 0f;
			if (Keyboard.current != null)
			{
				if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput += 1f;
				if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput -= 1f;
				if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) verticalInput += 1f;
				if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) verticalInput -= 1f;
			}

			bool isShiftPressed = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
			bool isCtrlPressed = Keyboard.current != null && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);

			if (isShiftPressed)
			{
				transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * verticalInput * Time.deltaTime;
				transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * horizontalInput * Time.deltaTime;
			}
			else if (isCtrlPressed)
			{
				transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * verticalInput * Time.deltaTime;
				transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * horizontalInput * Time.deltaTime;
			}
			else
			{
				transform.position += transform.forward * normalMoveSpeed * verticalInput * Time.deltaTime;
				transform.position += transform.right * normalMoveSpeed * horizontalInput * Time.deltaTime;
			}


			if (Keyboard.current != null && Keyboard.current.eKey.isPressed) { transform.position += transform.up * climbSpeed * Time.deltaTime; }
			if (Keyboard.current != null && Keyboard.current.qKey.isPressed) { transform.position -= transform.up * climbSpeed * Time.deltaTime; }

			if (Keyboard.current != null && Keyboard.current.endKey.wasPressedThisFrame)
			{
				if (Cursor.lockState == CursorLockMode.Locked)
				{
					Cursor.lockState = CursorLockMode.None;
				}
				else
				{
					Cursor.lockState = CursorLockMode.Locked;
				}
				Cursor.visible = !Cursor.visible;
			}
		}
	}
}