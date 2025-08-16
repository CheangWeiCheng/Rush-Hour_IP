/*
* Author: Cheang Wei Cheng
* Date: 5 August 2025
* Description:This script controls the player's behavior in the game.
* It handles player interactions with game objects, as well as player health and score management.
* The player takes damage when touching hazards like cars and bikes, and sees a congratulatory message upon touching the destination.
* The player can also jump using the ThirdPersonController component.
* The script uses Unity's Input System for firing projectiles and handles raycasting to detect interactable objects in the game world.
* The player's score and health are displayed on the UI, and the player respawns at a designated spawn point upon death.
* The script also includes audio feedback for taking damage.
*/

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    /// <summary>
    /// Store variables for player behaviour, for example:
    /// maxHealth is the maximum health the player can have,
    /// currentHealth is the player's current health,
    /// scoreUI is the UI text element that displays the player's score,
    /// and healthUI is the UI text element that displays the player's health.
    /// </summary>

    int maxHealth = 100;
    int currentHealth = 100;

    [SerializeField]
    TMP_Text scoreUI;
    [SerializeField]
    TMP_Text healthUI;
    [SerializeField]
    TMP_Text timerText;
    public float elapsedTime;
    public int currentScore = 0;
    bool canInteract = false;
    CoinBehaviour currentCoin = null;
    AudioSource hurtAudioSource;

    [SerializeField]
    float interactionDistance = 2f;

    public Transform spawnPoint;

    Camera mainCamera;

    /// <summary>
    /// Initializes the player by setting up the UI texts and hiding the keycard image.
    /// It also retrieves the main camera and the AudioSource component for firing projectiles.
    /// The score and health UI texts are set to their initial values.
    /// </summary>
    void Start()
    {
        scoreUI.text = "STARS: " + currentScore.ToString();
        healthUI.text = "HEALTH: " + currentHealth.ToString();
        if (!mainCamera) mainCamera = Camera.main;
        hurtAudioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// This method calls HandleRaycastHighlighting every frame to check for interactable objects.
    /// </summary>
    void Update()
    {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        HandleRaycastHighlighting();
    }

    /// <summary>
    /// Handles player interaction with collectibles, keycards, and doors.
    /// This method is triggered by the interact input action.
    /// It checks if the player can interact with a star, and performs the appropriate action.
    /// If a coin is collected, it calls the Collect method on the CoinBehaviour script.
    /// </summary>
    void OnInteract(InputValue value)
    {
        if (canInteract)
            if (currentCoin != null)
            {
                Debug.Log("Interacting with coin");
                currentCoin.Collect(this);
                currentCoin = null; // Reset current coin after interaction
            }
    }

    /// <summary>
    /// Handles raycasting to detect and highlight interactable objects in the game world.
    /// This method checks for collectibles (stars) within a specified interaction distance.
    /// If an interactable object is detected, it highlights the object and allows interaction,
    /// while unhighlighting any previously highlighted objects by setting them to null.
    /// If no interactable objects are detected, it resets the current objects and disables interaction.
    /// </summary>
    void HandleRaycastHighlighting()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitinfo;

        if (Physics.Raycast(ray, out hitinfo, interactionDistance))
        {
            // Handle coin detection
            if (hitinfo.collider.CompareTag("Collectible"))
            {
                var newCoin = hitinfo.collider.GetComponent<CoinBehaviour>();
                if (currentCoin != newCoin)
                {
                    // Unhighlight the previous coin if it exists
                    if (currentCoin != null) currentCoin.Unhighlight();
                    currentCoin = newCoin;
                    currentCoin.Highlight();
                    canInteract = true; // Enable interaction
                    Debug.Log("Coin detected");
                }
                return;
            }
        }

        // If no valid object is detected, reset current objects and disable interaction
        if (currentCoin != null)
        {
            currentCoin.Unhighlight();
            currentCoin = null;
        }
    }

    /// <summary>
    /// Modifies the player's score by a specified amount.
    /// This method updates the current score and refreshes the score UI text to reflect the new score.
    /// </summary>
    public void ModifyScore(int amount)
    {
        currentScore += amount;
        scoreUI.text = "STARS: " + currentScore.ToString();
    }

    /// <summary>
    /// Modifies the player's health by a specified amount.
    /// This method updates the current health and refreshes the health UI text to reflect the new health.
    /// If the health exceeds the maximum health, it is capped at the maximum value.
    /// If the health drops to zero or below, the player is considered dead, and their health is reset to maximum.
    /// The player is then respawned at a designated spawn point.
    /// </summary>
    public void ModifyHealth(int amount)
    {
        if (currentHealth <= maxHealth)
        {
            currentHealth += amount;
            healthUI.text = "HEALTH: " + currentHealth.ToString();
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
                healthUI.text = "HEALTH: " + currentHealth.ToString();
            }
            if (currentHealth <= 0)
            {
                Debug.Log("You died.");
                currentHealth = maxHealth;
                healthUI.text = "HEALTH: " + currentHealth.ToString();
                transform.position = spawnPoint.position;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car"))
        {
            Debug.Log("You were hit by a car!");
            // Play the hurt audio
            if (hurtAudioSource != null)
            {
                hurtAudioSource.Play();
            }
            // Respawn the player at the spawn point
            transform.position = spawnPoint.position;
            // Reset the player's health to maximum
            currentHealth = maxHealth;
            healthUI.text = "HEALTH: " + currentHealth.ToString();
        }
        else if (collision.gameObject.CompareTag("Bike"))
        {
            Debug.Log("You were hit by a bike!");
            // Play the hurt audio
            if (hurtAudioSource != null)
            {
                hurtAudioSource.Play();
            }
            currentHealth -= 50; // Reduce health by 50
            healthUI.text = "HEALTH: " + currentHealth.ToString();
            if (currentHealth <= 0)
            {
                Debug.Log("You died from a bike hit.");
                // Respawn the player at the spawn point
                transform.position = spawnPoint.position;
                // Reset the player's health to maximum
                currentHealth = maxHealth;
                healthUI.text = "HEALTH: " + currentHealth.ToString();
            }
        }
    }
}