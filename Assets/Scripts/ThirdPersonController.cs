/*
* Author: Cheang Wei Cheng
* Date: 14 June 2025
* Description: This script handels the movement of a third-person character controller in Unity.
* It allows the character to move relative to the camera's orientation.
* The character's movement is controlled using Rigidbody physics for smooth and responsive interactions.
* The script also includes an Animator component to handle character animations based on movement.
* This script is designed to be attached to a GameObject with a Rigidbody component and a Collider.
*/

using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    private Rigidbody rb;
    private Animator animator; // Animator for character animations
    private Camera mainCamera;

    /// <summary>
    /// Start is called before the first frame update.
    /// This method initializes the Rigidbody component and caches the main camera for later use.
    /// It is used to set up the initial state of the player character.
    /// </summary>
    void Start()
    {
        animator = GetComponent<Animator>();
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

        // Update the isMoving parameter in the animator
        if (move != Vector3.zero)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        // Rotate the player to face the direction of movement
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}