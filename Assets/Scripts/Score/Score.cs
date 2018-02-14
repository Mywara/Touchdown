using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PUNTutorial
{
    public class Score : Photon.MonoBehaviour
    {

        //attacher le script au drapeau et ajouter les tags goalG et goalD au zone de marquage
        //poser le gobal UI sur la scène

         private int scoreG = 0;
         private int scoreD = 0;


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "goalG")
            {
                scoreG = scoreG + 1;
                photonView.RPC("ChangeScore", PhotonTargets.All);
            }
            if (other.gameObject.tag == "goalD")
            {
                scoreD = scoreD + 1;
                photonView.RPC("ChangeScore", PhotonTargets.All);
            }
        }

        [PunRPC]
        void ChangeScore()
        {
            GameObject.Find("GlobalUI").GetComponentInChildren<Text>().text = "Score :\n" + scoreG + ":" + scoreD;
        }       
    }
}
