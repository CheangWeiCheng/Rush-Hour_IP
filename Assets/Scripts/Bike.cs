/*
* Author: Chui Yan Zhuo
* Date: 12 August 2025
* Description: This script handles the behavior of a chaser that follows and destroys targets with a specific tag.
* It manages the chasing logic, target detection, and respawn mechanics.
* The chaser can switch between "Chasing" and "Stopped" states based on interactions with the player.
* The script is designed to be attached to a GameObject with a Rigidbody component for movement.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TagChaserDestroy : MonoBehaviour
{
    [Header("Chase Settings")]
    public string targetTag = "Target";
    public float speed = 5f;
    public float reachThreshold = 0.5f;
    [SerializeField]
    public float stopDuration = 5f;
   public float respawnDelay = 10f;

    [Header("State")]
    public string currentState;
    private Transform currentTarget;
    private Dictionary<GameObject, Coroutine> respawnCoroutines = new Dictionary<GameObject, Coroutine>();

    void Start()
    {
        StartCoroutine(SwitchState("Chasing"));
    }

    /// <summary>
    /// Switches the current state of the chaser to a new state.
    /// This method is used to transition between "Chasing" and "Stopped" states.
    /// It starts the corresponding coroutine for the new state.
    /// </summary>
    IEnumerator SwitchState(string newState)
    {
        if (currentState == newState) yield break;
        currentState = newState;
        StartCoroutine(currentState);
    }

    /// <summary>
    /// Coroutine that handles the "Chasing" state of the chaser.
    /// It continuously moves towards the current target and destroys it upon reaching.
    /// If no target is found, it searches for the closest active target with the specified tag.
    /// </summary>
    IEnumerator Chasing()
    {
        while (currentState == "Chasing")
        {
            if (currentTarget == null)
            {
                FindNextTarget();
                yield return null;
                continue;
            }

            // Move toward target
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);
            transform.position += direction * speed * Time.deltaTime;

            // Check if reached target
            if (Vector3.Distance(transform.position, currentTarget.position) < reachThreshold)
            {
                DestroyAndRespawnTarget(currentTarget.gameObject);
                currentTarget = null;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Destroys the current target and starts a respawn coroutine to bring it back after a delay.
    /// It stores the original position and rotation of the target before destroying it.
    /// This allows the target to be reactivated at its original state after the respawn delay.
    /// </summary>
    void DestroyAndRespawnTarget(GameObject target)
    {
        // Store original position and rotation
        Vector3 originalPosition = target.transform.position;
        Quaternion originalRotation = target.transform.rotation;

        // Destroy the target
        target.SetActive(false);

        // Start respawn coroutine
        if (!respawnCoroutines.ContainsKey(target))
        {
            respawnCoroutines[target] = StartCoroutine(RespawnTarget(target, originalPosition, originalRotation));
        }
    }

    /// <summary>
    /// Coroutine that handles the respawn of a target after a delay.
    /// It waits for the specified respawn delay before reactivating the target at its original position and rotation.
    /// This allows the target to reappear in the game after being destroyed.
    /// </summary>
    IEnumerator RespawnTarget(GameObject target, Vector3 position, Quaternion rotation)
    {
        yield return new WaitForSeconds(respawnDelay);
        
        if (target != null)
        {
            target.transform.position = position;
            target.transform.rotation = rotation;
            target.SetActive(true);
        }

        respawnCoroutines.Remove(target);
    }

    /// <summary>
    /// Coroutine that handles the "Stopped" state of the chaser.
    /// It waits for a specified duration before switching back to the "Chasing" state.
    /// This allows the chaser to pause its activity for a while before resuming.
    /// </summary>
    IEnumerator Stopped()
    {
        float stopTimer = 0f;

        while (currentState == "Stopped" && stopTimer < stopDuration)
        {
            stopTimer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(SwitchState("Chasing"));
    }

    /// <summary>
    /// Finds the next target with the specified tag that is active in the scene.
    /// It searches for all GameObjects with the target tag and selects the closest one to chase.
    /// If no active targets are found, it will not change the current target.
    /// </summary>
    void FindNextTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        float shortestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject target in targets)
        {
            if (!target.activeInHierarchy) continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = target.transform;
            }
        }

        currentTarget = closest;
    }

    /// <summary>
    /// Handles collision with the player.
    /// If the chaser collides with the player and is not already in the "Stopped" state,
    /// it switches to the "Stopped" state, pausing its chasing behavior.
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentState != "Stopped")
        {
            StartCoroutine(SwitchState("Stopped"));
        }
    }

    /// <summary>
    /// Cleans up all respawn coroutines when the chaser is destroyed.
    /// This ensures that no lingering coroutines are left running after the chaser is removed from the scene.
    /// It iterates through all stored respawn coroutines and stops them.
    /// </summary>
    void OnDestroy()
    {
        // Clean up all respawn coroutines when this object is destroyed
        foreach (var coroutine in respawnCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }
}