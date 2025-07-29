using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    [SerializeField]
    private bool isRed = true;

    public bool IsRed => isRed;

    // Method to toggle the traffic light state
    public void ToggleLight()
    {
        isRed = !isRed;
    }

    // Optional: Method to set the traffic light state directly
    public void SetLightState(bool red)
    {
        isRed = red;
    }
}
