using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreUpdate : Photon.PUNBehaviour {

    public int scoreG = 0;
    public int scoreD = 0;
    
    private Text uiScoreG;
    private Text uiScoreD;


    void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("RandomMap"))
        {
            uiScoreG = this.transform.Find("TopPanel/ScoreGPanel/ScoreG").GetComponentInChildren<Text>();
            uiScoreD = this.transform.Find("TopPanel/ScoreDPanel/ScoreD").GetComponentInChildren<Text>();
        }
    }

    [PunRPC]
    void ChangeScore()
    {
        uiScoreG.text = scoreG.ToString();
        uiScoreD.text = scoreD.ToString();
    }

    public void SetWinner()
    {
        int localTeam = PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team;
        Winner winnerManager = GameObject.Find("WinnerManager").GetComponent<Winner>();

        if(scoreG > scoreD)
        {
            winnerManager.winner = "Team 1";
            winnerManager.tie = false;
            if (localTeam == 1)
            {
                winnerManager.hasWon = true;
            }
            else
            {
                winnerManager.hasWon = false;
            }
        }
        else if(scoreG < scoreD)
        {
            winnerManager.winner = "Team 2";
            winnerManager.tie = false;
            if (localTeam == 2)
            {
                winnerManager.hasWon = true;
            }
            else
            {
                winnerManager.hasWon = false;
            }
        }
        else
        {
            winnerManager.tie = true;
        }
    }

    [PunRPC]
    void AddScore(bool goalG)
    {
        if (goalG)
        {
            scoreG += 1;
        }
        else
        {
            scoreD += 1;
        }
    }
}
