using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSetupMenuController : MonoBehaviour
{
    private int PlayerIndex;

    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private GameObject readyPanel;

    [SerializeField]
    private GameObject menuPanel;



    private float ignoreInputTime = 1.5f;
    private bool inputEnabled;

    public void SetPlayerIndex(int pi)
    {
        Debug.Log("Plaxer index" + pi);
        PlayerIndex = pi;
        titleText.SetText("Player " + (pi + 1).ToString());
        ignoreInputTime = Time.time + ignoreInputTime;
    }
    // Update is called once per frame
    void Update()
    {
        if (Time.time > ignoreInputTime)
        {
            inputEnabled = true;
        }

    }

    public void ReadyPlayer()
    {

        Debug.Log("I am clicked");
        if (!inputEnabled)
        {
            return;
        }

        Debug.Log(PlayerIndex);
        PlayerConfigurationManager.Instance.ReadyPlayer(PlayerIndex);

        readyPanel.SetActive(true);
        //Debug.Log(readyPanel.activeSelf);

        //Debug.Log(menuPanel.activeSelf);
        menuPanel.SetActive(false);
        //Debug.Log(menuPanel.activeSelf);


    }


}
