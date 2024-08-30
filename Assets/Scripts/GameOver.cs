using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{

    public Text playerKills = null;
    public Text playerScore = null;

    private void Awake()
    {
        playerKills.text = "Demon Killed: "+GameController.instance.playerKills.ToString();
        playerScore.text = "Score: "+ GameController.instance.playerScore.ToString();
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
