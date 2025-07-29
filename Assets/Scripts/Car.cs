using System.Collections;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField]
    private float normalSpeed = 10f;
    [SerializeField]
    private float slowSpeed = 5f;
    [SerializeField]
    private Transform startPosition;
    
    private float currentSpeed;
    private string currentState;

    void Start()
    {
        currentSpeed = normalSpeed;
        StartCoroutine(SwitchState("Moving"));
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
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator Stopped()
    {
        float stoppedSpeed = 0f;
        while (currentState == "Stopped")
        {
            transform.Translate(Vector3.forward * stoppedSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator Resetting()
    {
        // Immediately reset position/rotation
        transform.position = startPosition.position;
        transform.rotation = startPosition.rotation;
        
        yield return new WaitForEndOfFrame(); // Small delay
        
        StartCoroutine(SwitchState("Moving"));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ResetZone"))
        {
            StartCoroutine(SwitchState("Resetting"));
        }
        else if (other.CompareTag("TrafficLight"))
        {
            if (other.GetComponent<TrafficLight>().IsRed)
            {
                StartCoroutine(SwitchState("Stopped"));
            }
            else
            {
                currentSpeed = normalSpeed;
                StartCoroutine(SwitchState("Moving"));
            }
        }
        else if (other.CompareTag("ZebraCrossing"))
        {
            currentSpeed = other.GetComponent<ZebraCrossing>().HasPedestrian ? 0f : slowSpeed;
            StartCoroutine(currentSpeed == 0f ? SwitchState("Stopped") : SwitchState("Moving"));
        }
    }
}