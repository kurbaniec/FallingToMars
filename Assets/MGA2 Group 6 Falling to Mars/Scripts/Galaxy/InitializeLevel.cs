using System.Collections;
using System.Collections.Generic;
using Falling_to_Mars.Scripts.Position;
using UnityEngine;

public class InitializeLevel : MonoBehaviour
{
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private PositionManager positionManager;
    
    void Start()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();
        for (int i = 0; i < playerConfigs.Length; i++)
        {
            Debug.Log("playerConfigsLenght is: " + playerConfigs.Length);
            Debug.Log("Initialize Player: " + (i + 1));
            var player = Instantiate(playerPrefab, playerSpawns[i].position, playerSpawns[i].rotation,
                gameObject.transform);
            player.name = "Player " + (i + 1);
            player.GetComponent<PlayerHandler>()
                .InitializePlayer(playerConfigs[i], playerConfigs.Length, audioSource, positionManager);
        }
    }
}