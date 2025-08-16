/*
* Author: Cheang Wei Cheng
* Date: 12 August 2025
* Description: This script controls the traffic light behavior in the game.
* It manages the light state (red/green), handles transitions, and provides visual feedback.
* The script also includes VFX for when the light turns green.
*/

using UnityEngine;
using System.Collections;

public class TrafficLight : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private bool startRed = true;
    [SerializeField] private float redDuration = 8f;
    [SerializeField] private float greenDuration = 5f;
    [SerializeField] private float initialDelay = 0f;

    [Header("Visual Feedback")]
    [SerializeField] private MeshRenderer lightRenderer;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material yellowMaterial;
    [SerializeField] private float yellowBlinkDuration = 0.5f;
    [SerializeField] private int yellowBlinks = 3;

    [Header("VFX")]
    [SerializeField] private GameObject greenLightVFXPrefab; // Assign your VFX prefab in inspector
    [SerializeField] private Transform vfxSpawnPoint; // Where to spawn the VFX
    [SerializeField] private float vfxDuration = 6f; // How long to keep VFX active

    private bool isRed;
    private Coroutine lightCycleCoroutine;
    private GameObject currentVFXInstance;

    public bool IsRed => isRed;

    /// <summary>
    /// Initializes the traffic light state and starts the light cycle coroutine.
    /// Sets the initial light state based on startRed and updates the visual representation.
    /// Spawns initial VFX if the light starts red.
    /// </summary>
    private void Start()
    {
        isRed = startRed;
        UpdateLightVisual();
        lightCycleCoroutine = StartCoroutine(LightCycle());
    }

    /// <summary>
    /// Cleans up the VFX and stops the light cycle coroutine when the script is disabled.
    /// This ensures no lingering effects or coroutines when the traffic light is not active.
    /// </summary>
    private void OnDisable()
    {
        if (lightCycleCoroutine != null)
        {
            StopCoroutine(lightCycleCoroutine);
        }
        CleanUpVFX();
    }

    /// <summary>
    /// Coroutine that manages the traffic light cycle.
    /// It alternates between red and green states, with a yellow warning phase when transitioning from green to red.
    /// It spawns VFX when the light turns green and handles the timing for each state.
    /// The cycle starts after an initial delay if specified.
    /// </summary>
    private IEnumerator LightCycle()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // Green -> Red transition (with yellow warning)
            if (!isRed)
            {
                yield return new WaitForSeconds(greenDuration);
                yield return StartCoroutine(BlinkYellow());
                SetLightState(true);
                SpawnRedLightVFX(); // Spawn VFX when turning red
            }
            // Red -> Green transition
            else
            {
                yield return new WaitForSeconds(redDuration);
                SetLightState(false);
            }
        }
    }

    /// <summary>
    /// Spawns the green light VFX at the specified spawn point.
    /// Cleans up any existing VFX before spawning a new one.
    /// The VFX will automatically destroy itself after a specified duration.
    /// </summary>
    private void SpawnRedLightVFX()
    {
        CleanUpVFX(); // Clean up any existing VFX first

        if (greenLightVFXPrefab != null && vfxSpawnPoint != null)
        {
            currentVFXInstance = Instantiate(greenLightVFXPrefab, vfxSpawnPoint.position, vfxSpawnPoint.rotation);
            Destroy(currentVFXInstance, vfxDuration); // Auto-destroy after duration
        }
    }

    /// <summary>
    /// Cleans up the current VFX instance if it exists.
    /// This method is called to ensure no lingering VFX when the light state changes or when the script is disabled.
    /// It checks if the currentVFXInstance is not null and destroys it.
    /// </summary>
    private void CleanUpVFX()
    {
        if (currentVFXInstance != null)
        {
            Destroy(currentVFXInstance);
            currentVFXInstance = null;
        }
    }

    /// <summary>
    /// Coroutine that handles the yellow blinking effect when transitioning from green to red.
    /// It blinks the yellow light a specified number of times with a defined duration for each blink.
    /// This provides a visual warning to players that the light is about to change.
    /// </summary>
    private IEnumerator BlinkYellow()
    {
        if (yellowMaterial == null) yield break;

        for (int i = 0; i < yellowBlinks; i++)
        {
            lightRenderer.material = yellowMaterial;
            yield return new WaitForSeconds(yellowBlinkDuration);
            lightRenderer.material = greenMaterial;
            yield return new WaitForSeconds(yellowBlinkDuration);
        }
    }

    /// <summary>
    /// Sets the traffic light state to red or green and updates the visual representation accordingly.
    /// This method is called to change the light state and update the material of the light renderer.
    /// It also logs the current state change for debugging purposes.
    /// </summary>
    private void SetLightState(bool red)
    {
        isRed = red;
        UpdateLightVisual();
        Debug.Log($"Traffic light changed to: {(isRed ? "RED" : "GREEN")}");
    }

    /// <summary>
    /// Updates the visual representation of the traffic light based on its current state.
    /// It changes the material of the light renderer to either red or green depending on the isRed flag.
    /// This method is called whenever the light state changes to ensure the visual feedback is accurate.
    /// </summary>
    private void UpdateLightVisual()
    {
        if (lightRenderer == null) return;
        lightRenderer.material = isRed ? redMaterial : greenMaterial;
    }
}