using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
public class SpawnPlayerSetupMenu : MonoBehaviour
{
    public GameObject playerSetupMenuPrefab;

    public PlayerInput input;


    private void Awake()
    {

        var rootMenu = GameObject.Find("MainLayout");
        if (rootMenu != null)
        {
            Debug.Log("I am in SpawnPlayerSetupMenu");

            Debug.Log("This is my PlayerIndex " + input.playerIndex);
            var menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);
            input.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
            menu.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(input.playerIndex);


        }
    }
}
