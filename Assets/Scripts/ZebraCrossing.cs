/*
* Author: Cheang Wei Cheng
* Date: 12 August 2025
* Description: This script controls the zebra crossing behavior in the game.
* It detects when a player is crossing the zebra crossing and sets a flag accordingly.
* The script can be used to trigger events or behaviors when a player is on the crossing.
* This script is designed to be attached to a GameObject with a Collider component set as a trigger.
*/

using UnityEngine;

public class ZebraCrossing : MonoBehaviour
{
    [SerializeField]
    private bool hasPedestrian = false;

    public bool HasPedestrian => hasPedestrian;

    /// <summary>
    /// This method is called when a player enters the trigger collider of the zebra crossing.
    /// It sets the hasPedestrian flag to true, indicating that a player is currently crossing.
    /// This can be used to trigger specific behaviors or events in the game when a player is on the crossing.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            hasPedestrian = true;
            Debug.Log("Pedestrian is crossing the zebra crossing.");
        }
    }
    /// <summary>
    /// This method is called when a player exits the trigger collider of the zebra crossing.
    /// It sets the hasPedestrian flag to false, indicating that no player is currently crossing.
    /// This can be used to reset behaviors or events related to the zebra crossing.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            hasPedestrian = false;
            Debug.Log("Pedestrian has left the zebra crossing.");
        }
    }
}
