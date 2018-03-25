using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PUNTutorial
{
    public class Score : Photon.PUNBehaviour
    {

        //attacher le script au drapeau et ajouter les tags goalG et goalD au zone de marquage
        //poser le gobal UI sur la scène

        private ScoreUpdate scoreUpdate;
         public int endScore = 10;
        private void Start()
        {
            scoreUpdate = GameObject.Find("GlobalUI").GetComponent<ScoreUpdate>();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                if (other.gameObject.GetComponent<CrystalDrop>().crys != null)
                {
                    //GoalG.z < 0 => but de la team 1
                    //Seuls les membres de la team 0 peuvent marquer dans ce but
                    if (this.gameObject.tag == "GoalG" && other.gameObject.GetComponent<PlayerController>().team == 0)
                    {
                        scoreUpdate.scoreG += 1;
                        GameObject.Find("GlobalUI").GetComponent<PhotonView>().photonView.RPC("ChangeScore", PhotonTargets.All);
                        if (scoreUpdate.scoreG >= endScore)
                        {
                            GameObject.Find("WinnerManager").GetComponent<Winner>().winner = "gauche";
                            PhotonNetwork.LoadLevel("EndScene");
                        }
                    }
                    //GoalD.z < 0 => but de la team 0
                    //Seuls les membres de la team 1 peuvent marquer dans ce but
                    else if (this.gameObject.tag == "GoalD" && other.gameObject.GetComponent<PlayerController>().team == 1)
                    {
                        scoreUpdate.scoreD += 1;
                        GameObject.Find("GlobalUI").GetComponent<PhotonView>().photonView.RPC("ChangeScore", PhotonTargets.All);
                        if (scoreUpdate.scoreD >= endScore)
                        {
                            GameObject.Find("WinnerManager").GetComponent<Winner>().winner = "droite";
                            PhotonNetwork.LoadLevel("EndScene");
                        }

                    }
                }
            }
        }


       /* [PunRPC]
        void ChangeScore()
        {
            GameObject.Find("GlobalUI").GetComponentInChildren<Text>().text = "Score :\n" + scoreG + ":" + scoreD;
        }*/
    }
}
