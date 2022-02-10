using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowWinner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string winner = EndingScript.winner;

        transform.GetComponent<Text>().text = winner;
    }

    // Update is called once per frame

}
