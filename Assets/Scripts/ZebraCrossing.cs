using UnityEngine;

public class ZebraCrossing : MonoBehaviour
{
    [SerializeField]
    private bool hasPedestrian = false;

    public bool HasPedestrian => hasPedestrian;

    // Method to detect if a player is crossing
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            hasPedestrian = true;
            Debug.Log("Pedestrian is crossing the zebra crossing.");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            hasPedestrian = false;
            Debug.Log("Pedestrian has left the zebra crossing.");
        }
    }
}
