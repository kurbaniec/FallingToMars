using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Falling_to_Mars.Scripts.Position;
using UnityEngine;
using UnityEngine.InputSystem;
using Player;

public class PlayerHandler : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject FollowCam;

    [SerializeField] private Camera Cam;

    //[SerializeField]
    //private PlayerInput myInput;
    [SerializeField] private GameObject Boost;


    public void InitializePlayer(
        PlayerConfiguration pc, int playerNumbers, AudioSource audioSource,
        PositionManager positionManager
    )
    {
        //Setup Follow Cam
        if (pc.PlayerIndex == 0)
        {
            FollowCam.layer = 14;
            Cam.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Hazard", "Water", "UI", "Environment",
                "P1 Cam", "VissibleCollider");
            Cam.GetComponent<AudioListener>().enabled = true;
            if (playerNumbers == 2)
            {
                Cam.rect = new Rect(-0.5f, 0, 1, 1);

            }
            else
            {
                Cam.rect = new Rect(-0.5f, 0.5f, 1, 1);
            }
        }
        else if (pc.PlayerIndex == 1)
        {
            FollowCam.layer = 15;
            Cam.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Hazard", "Water", "UI", "Environment",
                "P2 Cam", "VissibleCollider");
            if (playerNumbers == 2)
            {
                Cam.rect = new Rect(0.5f, 0, 1, 1);
            }
            else
            {
                Cam.rect = new Rect(0.5f, 0.5f, 1, 1);
            }
        }
        else if (pc.PlayerIndex == 2)
        {
            FollowCam.layer = 16;
            Cam.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Hazard", "Water", "UI", "Environment",
                "P3 Cam", "VissibleCollider");

            if (playerNumbers == 3)
            {
                Cam.rect = new Rect(0, -0.5f, 1, 1);
            }
            else
            {
                Cam.rect = new Rect(-0.5f, -0.5f, 1, 1);
            }
        }
        else if (pc.PlayerIndex == 3)
        {
            FollowCam.layer = 17;
            Cam.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Hazard", "Water", "UI", "Environment",
                "P4 Cam", "VissibleCollider");
            Cam.rect = new Rect(0.5f, -0.5f, 1, 1);
        }

        Debug.Log("Set the Playerinput for: " + pc.PlayerIndex);
        Player.GetComponent<GalaxyMinifigController>().initializeMyPlayer(pc);
        Debug.Log("Playerinput is done for: " + pc.PlayerIndex);
        Player.GetComponent<GalaxyMinifigController>().audioSource = audioSource;
        Player.name = "Player " + (pc.PlayerIndex + 1);

        positionManager.players.Add(Player);
        positionManager.OrderPlayers();
    }

    // Update is called once per frame
}