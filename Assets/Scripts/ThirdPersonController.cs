/*
* Author: Cheang Wei Cheng
* Date: 14 June 2025
* Description: This script handels the movement and jumping mechanics of a third-person character controller in Unity.
* It allows the character to move relative to the camera's orientation, jump when grounded, and plays a sound effect upon jumping.
* The character's movement is controlled using Rigidbody physics for smooth and responsive interactions.
* The script also includes a ground check using raycast to ensure the character can only jump when on the ground.
* This script is designed to be attached to a GameObject with a Rigidbody component and a Collider.
*/

using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [SerializeField]
    AudioClip jumpSound; // Sound to play when jumping
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    private Camera mainCamera;
    [SerializeField] float groundCheckDistance = .5f;

    /// <summary>
    /// Property to check if the character is grounded
    /// This property uses a raycast to check if there is ground directly below the character.
    /// The raycast checks a distance defined by groundCheckDistance.
    /// If the raycast hits something, it means the character is grounded and can jump.
    /// </summary>
    private bool IsGrounded
    {
        get
        {
            return Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance);
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// This method initializes the Rigidbody component and caches the main camera for later use.
    /// It is used to set up the initial state of the player character.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main; // Cache the main camera
    }

    /// <summary>
    /// FixedUpdate is called at a fixed interval and is used for physics calculations.
    /// This method handles player movement and rotation based on keyboard input.
    /// It calculates the movement direction relative to the camera's orientation and applies it to the Rigidbody.
    /// The player will also rotate to face the direction of movement.
    /// </summary>
    void FixedUpdate()
    {
        // Get input from keyboard
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Get camera forward and right vectors (ignoring Y axis)
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Create camera-relative movement vector
        Vector3 move = (cameraForward * moveZ + cameraRight * moveX) * moveSpeed * Time.fixedDeltaTime;

        // Apply movement using Rigidbody
        rb.MovePosition(rb.position + move);

        // Rotate the player to face the direction of movement
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Method to make the character jump
    /// This method checks if the character is grounded before applying the jump force.
    /// This is done by using a raycast directly pointed downwards to check the distance to the ground.
    /// If the character is grounded, it plays a jump sound and applies an upward force to the Rigidbody.
    /// </summary>
    public void Jump()
    {
        if (IsGrounded)
        {
            if (jumpSound != null)
            {
                AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            }
            // Apply jump force
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Reset vertical velocity
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}