using System.Collections;
using UnityEngine;

public class Car : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float normalSpeed = 10f;
    [SerializeField] private float slowSpeed = 5f;
    [SerializeField] private Transform startPosition;
    
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    
    private float currentSpeed;
    private string currentState;
    private TrafficLight currentTrafficLight;
    private ZebraCrossing currentZebraCrossing;
    private Transform currentTurnTrigger;
    private Car frontCar;

    [Header("Traffic Settings")]
    [SerializeField] private float stoppingDistance = 3f; // Distance to maintain from cars ahead
    [SerializeField] private LayerMask carDetectionMask; // Set to your Car layer

    /// <summary>
    /// Start is called before the first frame update.
    /// This method initializes the Rigidbody component and sets the initial speed.
    /// It also starts the car in the "Moving" state.
    /// The car will check for front vehicles and respond to traffic lights and zebra crossings.
    /// </summary>
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        currentSpeed = normalSpeed;
        StartCoroutine(SwitchState("Moving"));
    }
    
    private void FixedUpdate()
    {
        CheckFrontVehicle();
    }

    /// <summary>
    /// Checks for a vehicle in front of the car using Raycast and updates the state accordingly.
    /// If a car is detected, it switches to "Stopped" state.
    /// If no car is detected, it resumes moving if not blocked by a traffic light or pedestrian crossing.
    /// This method is called every FixedUpdate to ensure continuous checking.
    /// </summary>
    private void CheckFrontVehicle()
    {
        // Calculate ray origin at specified height
        Vector3 rayOrigin = transform.position + (Vector3.up * 0.5f);

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, transform.forward, out hit, stoppingDistance, carDetectionMask))
        {
            if (hit.collider.CompareTag("Car"))
            {
                frontCar = hit.collider.GetComponent<Car>();

                if (frontCar)
                {
                    if (currentState != "Stopped")
                    {
                        StartCoroutine(SwitchState("Stopped"));
                    }
                }
            }
        }
        else // No car detected ahead
        {
            if (frontCar != null)
            {
                frontCar = null;
            }
            // Resume moving if not blocked by light or pedestrian
            if (currentState == "Stopped" &&
                (currentTrafficLight == null || !currentTrafficLight.IsRed) &&
                (currentZebraCrossing == null || !currentZebraCrossing.HasPedestrian))
            {
                StartCoroutine(SwitchState("Moving"));
            }
        }
    }
    
    /// <summary>
    /// Draws a gizmo in the editor to visualize the raycast used for detecting front vehicles.
    /// This helps in debugging and understanding the detection range.
    /// The ray is drawn from the car's position at a height of 0.5 units forward for the stopping distance.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Calculate ray origin at specified height
        Vector3 rayOrigin = transform.position + (Vector3.up * 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin, rayOrigin + transform.forward * stoppingDistance);
    }

    /// <summary>
    /// Switches the car's state and starts the corresponding coroutine.
    /// This method is used to change the car's behavior based on interactions with traffic lights, zebra crossings, or other triggers.
    /// It ensures that the car's movement logic is updated according to the current state.
    /// </summary>
    IEnumerator SwitchState(string newState)
    {
        if (currentState == newState) yield break;
        currentState = newState;
        StartCoroutine(currentState);
    }

    /// <summary>
    /// Coroutine for the "Moving" state where the car moves forward at the current speed.
    /// It checks for traffic lights and zebra crossings to determine if it should stop or continue moving.
    /// If a traffic light is red or a pedestrian is crossing, it switches to "Stopped" state.
    /// </summary>
    IEnumerator Moving()
    {
        while (currentState == "Moving")
        {
            // Check traffic light continuously if we're near one
            if (currentTrafficLight != null && currentTrafficLight.IsRed)
            {
                StartCoroutine(SwitchState("Stopped"));
                yield break;
            }
            // Check for pedestrians on zebra crossing
            if (currentZebraCrossing != null && currentZebraCrossing.HasPedestrian)
            {
                StartCoroutine(SwitchState("Stopped"));
                yield break;
            }
            rb.linearVelocity = transform.forward * currentSpeed;
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine for the "Stopped" state where the car stops moving.
    /// It checks for conditions to resume moving, such as traffic lights turning green or pedestrians leaving the crossing.
    /// If conditions are met, it switches back to "Moving" state.
    /// </summary>
    IEnumerator Stopped()
    {
        while (currentState == "Stopped")
        {
            rb.linearVelocity = Vector3.zero;
            
            // Check if light turns green
            if (currentTrafficLight != null && !currentTrafficLight.IsRed)
            {
                currentSpeed = normalSpeed;
                StartCoroutine(SwitchState("Moving"));
                yield break;
            }
            // Check for pedestrians on zebra crossing
            if (currentZebraCrossing != null && !currentZebraCrossing.HasPedestrian)
            {
                currentSpeed = slowSpeed;
                StartCoroutine(SwitchState("Moving"));
                yield break;
            }
            // Check if front car is gone
            Vector3 rayOrigin = transform.position + (Vector3.up * 0.5f);
            if (!Physics.Raycast(rayOrigin, transform.forward, stoppingDistance, carDetectionMask) &&
                (currentTrafficLight == null || !currentTrafficLight.IsRed) &&
                (currentZebraCrossing == null || !currentZebraCrossing.HasPedestrian))
            {
                currentSpeed = normalSpeed;
                StartCoroutine(SwitchState("Moving"));
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine for the "Resetting" state where the car resets its position and velocity.
    /// This is typically triggered when the car collides with a reset zone.
    /// It sets the car back to its starting position and rotation, and resumes moving.
    /// </summary>
    IEnumerator Resetting()
    {
        Debug.Log("Resetting car");
        rb.linearVelocity = Vector3.zero;
        transform.SetPositionAndRotation(startPosition.position, startPosition.rotation);
        yield return new WaitForEndOfFrame();
        StartCoroutine(SwitchState("Moving"));
    }

    /// <summary>
    /// Coroutine for the "Turning" state where the car turns towards a specified direction.
    /// This is triggered when the car enters a turn trigger area.
    /// It instantly rotates the car to face the new direction and resumes moving.
    /// </summary>
    IEnumerator Turning()
    {
        if (currentTurnTrigger != null)
        {
            // Get direction to face from the turn trigger's forward vector
            Vector3 newDirection = currentTurnTrigger.forward;
            newDirection.y = 0; // Keep it horizontal
            
            // Instantly rotate car to face new direction
            transform.forward = newDirection;
            
            // Continue moving in new direction
            rb.linearVelocity = transform.forward * currentSpeed;
        }
        
        // Immediately go back to moving state
        StartCoroutine(SwitchState("Moving"));
        yield break;
    }

    /// <summary>
    /// Handles collision with reset zones to reset the car's position and state.
    /// This method is called when the car collides with a GameObject tagged as "ResetZone".
    /// It starts the Resetting coroutine to handle the reset logic.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ResetZone"))
        {
            StartCoroutine(SwitchState("Resetting"));
        }
    }

    /// <summary>
    /// Handles trigger events for traffic lights, zebra crossings, and turns.
    /// It updates the current traffic light or zebra crossing state and switches to appropriate states.
    /// This method is called when the car enters a trigger collider.
    /// It checks for traffic lights, zebra crossings, and turn triggers to manage the car's behavior.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TrafficLight"))
        {
            currentTrafficLight = other.GetComponent<TrafficLight>();
            if (currentTrafficLight.IsRed)
            {
                StartCoroutine(SwitchState("Stopped"));
            }
        }
        else if (other.CompareTag("ZebraCrossing"))
        {
            currentZebraCrossing = other.GetComponent<ZebraCrossing>();
            if (currentZebraCrossing.HasPedestrian)
            {
                StartCoroutine(SwitchState("Stopped"));
            }
            else
            {
                currentSpeed = slowSpeed;
                StartCoroutine(SwitchState("Moving"));
            }
        }
        else if (other.CompareTag("Turn"))
        {
            currentTurnTrigger = other.transform;
            StartCoroutine(SwitchState("Turning"));
        }
    }

    /// <summary>
    /// Handles exit events for traffic lights and zebra crossings.
    /// It resets the current traffic light or zebra crossing references when the car exits their trigger areas.
    /// This method is called when the car exits a trigger collider.
    /// It ensures that the car stops checking for conditions related to the exited traffic light or zebra crossing.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TrafficLight"))
        {
            currentTrafficLight = null;
        }
        else if (other.CompareTag("ZebraCrossing"))
        {
            currentZebraCrossing = null;
            currentSpeed = normalSpeed;
        }
    }
}