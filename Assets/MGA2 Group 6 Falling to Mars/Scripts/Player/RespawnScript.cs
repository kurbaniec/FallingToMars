using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnScript : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn"))
        {
            if (other.name.Contains("RespawnTrigger"))
            {
                player.transform.position = respawnPoint.transform.position;
                Physics.SyncTransforms();
            }
            else if (other.name.Contains("Point"))
            {
                respawnPoint = other.transform;
            }
        }
    }
}
