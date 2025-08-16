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

    /// <summary>
    /// Start is called before the first frame update.
    /// This method initializes the NavMeshAgent component and sets the initial state to Idle.
    /// It starts the state machine coroutine to manage NPC behavior.
    /// </summary>
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = State.Idle;
        StartCoroutine(StateMachine());
    }

    /// <summary>
    /// StateMachine coroutine that manages the NPC's behavior based on its current state.
    /// It handles transitions between Idle and Patrol states, executing the corresponding logic for each state.
    /// This coroutine runs continuously to update the NPC's behavior.
    /// </summary>
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

    /// <summary>
    /// Coroutine that handles the Idle state of the NPC.
    /// It stops the NavMeshAgent and waits for a specified idle time before transitioning to Patrol state.
    /// This provides a pause in the NPC's movement before it starts patrolling again.
    /// </summary>
    private IEnumerator Idle()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(idleTime);
        ChangeState(State.Patrol);
    }

    /// <summary>
    /// Coroutine that handles the Patrol state of the NPC.
    /// It sets a random destination within a specified radius and moves the NPC towards it.
    /// Once the NPC reaches the destination, it stops and transitions back to Idle state.
    /// This allows the NPC to patrol randomly within its designated area.
    /// </summary>
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

    /// <summary>
    /// Changes the current state of the NPC to a new state.
    /// This method is called to transition between Idle and Patrol states.
    /// It updates the currentState variable and can be extended to include additional state logic in the future.
    /// </summary>
    private void ChangeState(State newState)
    {
        currentState = newState;
        // Debug.Log($"State changed to: {newState}");
    }

    /// <summary>
    /// Gets a random point on the NavMesh within a specified radius from a center point.
    /// This method is used to generate random patrol points for the NPC.
    /// It attempts to find a valid point on the NavMesh up to 30 times before returning the center point if no valid point is found.
    /// </summary>
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