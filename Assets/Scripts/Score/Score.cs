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

         private int scoreG = 0;
         private int scoreD = 0;
         public int endScore = 10;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "GoalG")
            {
                scoreG = scoreG + 1;
                photonView.RPC("ChangeScore", PhotonTargets.All);
                if (scoreG >= endScore)
                {
                    PhotonNetwork.LoadLevel("EndScene");
                }
            }
            if (other.gameObject.tag == "GoalD")
            {
                scoreD = scoreD + 1;
                photonView.RPC("ChangeScore", PhotonTargets.All);
                if(scoreD >= endScore)
                {
                    PhotonNetwork.LoadLevel("EndScene");
                }
            }
        }

        [PunRPC]
        void ChangeScore()
        {
            GameObject.Find("GlobalUI").GetComponentInChildren<Text>().text = "Score :\n" + scoreG + ":" + scoreD;
        }
        

    }
}
