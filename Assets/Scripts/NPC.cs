/*
* Author: Groben Tham
* Date: 12 August 2025
* Description: This script controls the behavior of non-player characters (NPCs) in the game.
* It manages the NPC's state (Idle or Patrol), handles movement using Unity's NavMesh system,
* and allows for random patrol points within a specified radius.
* The script uses coroutines to manage state transitions and movement logic.
* It is designed to be attached to a GameObject with a NavMeshAgent component.
*/

using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPC : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol
    }

    public State currentState;

    private NavMeshAgent agent;

    [Header("Patrol Settings")]
    public float patrolRadius = 10f;
    public float idleTime = 2f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = State.Idle;
        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Idle:
                    yield return StartCoroutine(Idle());
                    break;
                case State.Patrol:
                    yield return StartCoroutine(Patrol());
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator Idle()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(idleTime);
        ChangeState(State.Patrol);
    }

    private IEnumerator Patrol()
    {
        agent.isStopped = false;
        Vector3 target = GetRandomNavMeshPoint(transform.position, patrolRadius);
        agent.SetDestination(target);

        while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        ChangeState(State.Idle);
    }

    private void ChangeState(State newState)
    {
        currentState = newState;
        // Debug.Log($"State changed to: {newState}");
    }

    private Vector3 GetRandomNavMeshPoint(Vector3 center, float maxDistance)
    {
        for (int i = 0; i < 30; i++) // try up to 30 times to find a valid point
        {
            Vector3 randomPos = center + Random.insideUnitSphere * maxDistance;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return center; // fallback: stay in place if no valid point found
    }
}