using UnityEngine;

public class ZebraCrossing : MonoBehaviour
{
    [SerializeField]
    private bool hasPedestrian = false;

    public bool HasPedestrian => hasPedestrian;

    // Method to set pedestrian presence
    public void SetPedestrianPresence(bool presence)
    {
        hasPedestrian = presence;
    }

    // Optional: Method to toggle pedestrian presence
    public void TogglePedestrianPresence()
    {
        hasPedestrian = !hasPedestrian;
    }
}
