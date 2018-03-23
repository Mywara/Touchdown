using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUpdate : Photon.PUNBehaviour {

    public int scoreG = 0;
    public int scoreD = 0;

    [PunRPC]
    void ChangeScore()
    {
            GameObject.Find("GlobalUI").GetComponentInChildren<Text>().text = "Score :\n" + scoreG + ":" + scoreD;
           
    }
}
