using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PUNTutorial
{
    public class Score : Photon.PUNBehaviour
    {

        //attacher le script au drapeau et ajouter les tags goalG et goalD au zone de marquage
        //poser le global UI sur la scène

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
                if (other.gameObject.GetComponent<CrystalDrop>().crys.GetComponent<Crystal>().playerHolding == other.gameObject)
                {
                    //GoalG.z < 0 => but de la team 1
                    //Seuls les membres de la team 0 peuvent marquer dans ce but
                    if (this.gameObject.tag == "GoalG" && other.gameObject.GetComponent<PlayerController>().team == 2)
                    {
                        scoreUpdate.scoreD += 1;
                        Debug.Log("GOAL GAUCHE !");
                        RoomManager.instance.GoalMarked();

                        int localTeam = GameManager.localPlayer.GetComponent<PlayerController>().Team;
                        GameObject.Find("GlobalUI").GetComponent<PhotonView>().photonView.RPC("ChangeScore", PhotonTargets.All);

                        if (scoreUpdate.scoreG >= endScore)
                        {
                            Winner winnerManager = GameObject.Find("WinnerManager").GetComponent<Winner>();

                            winnerManager.winner = "Team 2";
                            if(localTeam == 2)
                            {
                                winnerManager.hasWon = true;
                            }
                            else
                            {
                                winnerManager.hasWon = false;
                            }

                            PhotonNetwork.LoadLevel("EndScene");
                        }
                    }
                    //GoalD.z < 0 => but de la team 0
                    //Seuls les membres de la team 1 peuvent marquer dans ce but
                    else if (this.gameObject.tag == "GoalD" && other.gameObject.GetComponent<PlayerController>().team == 1)
                    {
                        scoreUpdate.scoreG += 1;
                        Debug.Log("GOAL DROITE !");
                        RoomManager.instance.GoalMarked();

                        int localTeam = GameManager.localPlayer.GetComponent<PlayerController>().Team;
                        GameObject.Find("GlobalUI").GetComponent<PhotonView>().photonView.RPC("ChangeScore", PhotonTargets.All);

                        if (scoreUpdate.scoreD >= endScore)
                        {
                            Winner winnerManager = GameObject.Find("WinnerManager").GetComponent<Winner>();

                            winnerManager.winner = "Team 1";
                            if (localTeam == 1)
                            {
                                winnerManager.hasWon = true;
                            }
                            else
                            {
                                winnerManager.hasWon = false;
                            }
                            
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
