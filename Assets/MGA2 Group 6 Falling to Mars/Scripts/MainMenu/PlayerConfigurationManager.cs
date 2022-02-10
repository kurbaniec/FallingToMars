using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.SceneManagement;
public class PlayerConfigurationManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs;

    [SerializeField]
    private int MaxPlayers = 4;

    public static PlayerConfigurationManager Instance { get; private set; }

    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("PlayerConfigurationManager Instance was created");
            Instance.playerConfigs = new List<PlayerConfiguration>();
            DontDestroyOnLoad(Instance);

        }

    }

    public void ReadyPlayer(int index)
    {
        Debug.Log("We are in ReadyPlayer");
        playerConfigs[index].IsReady = true;
        Debug.Log((index).ToString() + "is set to true");

        if (playerConfigs.Count > 1 && playerConfigs.All(player => player.IsReady == true))
        {
            SceneManager.LoadScene("Falling to Mars Level");
        }
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        Debug.Log("Player joined");
        if (pi != null)
        {
            Debug.Log("We are in HandlePlayerJoin: " + (pi.playerIndex).ToString());
        }
        if (!playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
        {
            pi.transform.SetParent(transform);
            playerConfigs.Add(new PlayerConfiguration(pi));

        }
    }


}

public class PlayerConfiguration
{

    public PlayerConfiguration(PlayerInput pi)
    {
        Debug.Log("We are in PlayerConfiguration");
        Input = pi;
        PlayerIndex = pi.playerIndex;
    }

    public PlayerConfiguration(int playerIndex)
    {
        PlayerIndex = playerIndex;
    }
    
    public PlayerInput Input { get; set; }

    public int PlayerIndex { get; set; }

    public bool IsReady { get; set; }
}



