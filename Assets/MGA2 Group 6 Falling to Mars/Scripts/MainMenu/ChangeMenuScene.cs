using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeMenuScene : MonoBehaviour
{
    // Start is called before the first frame update

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackToMainFromGameOver()
    {
        Destroy(GameObject.Find("PlayerConfigurationManager")); ;
        SceneManager.LoadScene("MainMenu");

    }


}
