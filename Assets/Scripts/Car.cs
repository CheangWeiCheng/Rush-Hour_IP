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
    
    void OnDrawGizmosSelected()
    {
        // Calculate ray origin at specified height
        Vector3 rayOrigin = transform.position + (Vector3.up * 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin, rayOrigin + transform.forward * stoppingDistance);
    }

    IEnumerator SwitchState(string newState)
    {
        if (currentState == newState) yield break;
        currentState = newState;
        StartCoroutine(currentState);
    }

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

    IEnumerator Resetting()
    {
        Debug.Log("Resetting car");
        rb.linearVelocity = Vector3.zero;
        transform.SetPositionAndRotation(startPosition.position, startPosition.rotation);
        yield return new WaitForEndOfFrame();
        StartCoroutine(SwitchState("Moving"));
    }

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ResetZone"))
        {
            StartCoroutine(SwitchState("Resetting"));
        }
    }

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