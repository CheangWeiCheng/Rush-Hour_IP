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

    IEnumerator SwitchState(string newState)
    {
        if (currentState == newState) yield break;
        currentState = newState;
        StartCoroutine(currentState);
    }

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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentState != "Stopped")
        {
            StartCoroutine(SwitchState("Stopped"));
        }
    }

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