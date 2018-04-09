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
}
