using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("How quickly the player moves along the ground")]
    [SerializeField] private float movementSpeed = 10f;

    [Tooltip("How much gravity affects the player")]
    [SerializeField] private float gravityMultiplier = 5f;

    [Tooltip("The jump speed of the player")]
    [SerializeField] private float jumpSpeed = 20f;

    private CollisionFlags collisionFlags;
    private CharacterController characterController;
    //private Animator playerAnimator;
    private Vector3 moveDir = Vector3.zero;
    private float movementInputX;
    private float movementInputY;
    private float stickToGroundForce = 9.807f;
    private float turnSpeed = 0.25f;
    private bool isMoving = false;
    private bool hasJumped = false;
    private bool isFalling = false;

	// Use this for initialization
	private void Start ()
    {
        characterController = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	private void Update ()
    {
        GetPlayerInput();
	}

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void GetPlayerInput()
    {
        if(Input.GetAxisRaw("Movement X") > 0f || Input.GetAxisRaw("Movement X") < 0f
            || Input.GetAxisRaw("Movement Y") > 0f || Input.GetAxisRaw("Movement Y") < 0f)
        {
            movementInputX = Input.GetAxisRaw("Movement X");
            movementInputY = Input.GetAxisRaw("Movement Y");
            isMoving = true;
        }
        else if(Input.GetAxisRaw("Left Joystick X") > 0f || Input.GetAxisRaw("Left Joystick X") < 0f 
            || Input.GetAxisRaw("Left Joystick Y") > 0f || Input.GetAxisRaw("Left Joystick Y") < 0f)
        {
            movementInputX = Input.GetAxisRaw("Left Joystick X");
            movementInputY = Input.GetAxisRaw("Left Joystick Y");
            isMoving = true;
        }
        else
        {
            movementInputX = 0f;
            movementInputY = 0f;
            isMoving = false;
        }
    }

    private void PlayerMovement()
    {
        //Vector3 desiredMove = transform.forward * movementInputY + transform.right * movementInputX;
        Vector3 desiredMove = new Vector3(movementInputX, 0f, movementInputY);
        desiredMove = Camera.main.transform.TransformDirection(desiredMove);
        desiredMove.y = 0f;

        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
                               characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        moveDir.x = desiredMove.x * movementSpeed;
        moveDir.z = desiredMove.z * movementSpeed;

        if(characterController.isGrounded)
        {
            moveDir.y = -stickToGroundForce;
            hasJumped = false;

            if(Input.GetButtonDown("Jump"))
            {
                moveDir.y = jumpSpeed;
                hasJumped = true;
            }
        }
        else
        {
            moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        if(!characterController.isGrounded && hasJumped == false)
        {
            isFalling = true;
        }
        else
        {
            isFalling = false;
        }

        collisionFlags = characterController.Move(moveDir * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (collisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}
