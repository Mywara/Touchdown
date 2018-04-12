using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : Photon.PunBehaviour
{

    public GameObject crys;
    // lumière
    private Light volumetricLight;
    private Color couleurBleue;
    private Color couleurOrange;
    public int bleuIntensite;
    public int orangeIntensite;
    private bool couleurSet = false;
    //public GameObject animBut;
    public GameObject goalBleu;
    public GameObject goalRouge;


    private void Update()
    {
        if (PUNTutorial.GameManager.localPlayer == null || PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team == 0)
        {
            return;
        }

        if (!couleurSet)
        {
            // On set les couleurs
            couleurBleue = new Vector4(0, 5f / 255f, 1, 1);
            couleurOrange = new Vector4(1, 0, 0, 1);

            // On recupere la lumiere
            volumetricLight = GetComponentInChildren<Light>();


            // On affiche le halo d'une couleur differente selon le joueur et le but auquel le script est attaché 
            if (this.tag == "GoalG" && PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team == 1
                || this.tag == "GoalD" && PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team == 2)
            {
                // Cas le but correspond a l'equipe du joueur
                SetCouleurLight(couleurBleue, bleuIntensite);
                goalBleu.SetActive(true);
            }
            else
            {
                // Cas le but est le but adverse
                SetCouleurLight(couleurOrange, orangeIntensite);
                goalRouge.SetActive(true);
            }

            couleurSet = true;
        }

        //tests
        //SetCouleurLight(couleurBleue, bleuIntensite);
        //SetCouleurLight(couleurRouge, rougeIntensite);
        // fin tests
    }


    private void OnTriggerEnter(Collider target)
    {
        if (PhotonNetwork.connected == true && !PhotonNetwork.isMasterClient)
        {
            return;
        }


        crys = GameObject.FindGameObjectWithTag("Crystal");


        if (target.transform.root.gameObject.tag == "Player"
            && crys.GetComponent<Crystal>().isHeld == true
            && crys.GetComponent<Crystal>().playerHolding == target.transform.root.gameObject)
        {
            // On lance l'animation
            //TODO lancé une animation sur l'UI du joueur (but allié ou ennemi)

            //Le cristal doit être reset si le but est marqué il faut donc que le butteur soit de l'equipe adverse au but atteint
            if (this.tag == "GoalG" && target.gameObject.GetComponent<PlayerController>().team == 2 ||
               this.tag == "GoalD" && target.gameObject.GetComponent<PlayerController>().team == 1)
            {

                crys.GetComponent<Crystal>().photonView.RPC("LeaveOnGround", PhotonTargets.All);

                crys.GetComponent<Crystal>().photonView.RPC("ResetCrystalPosition", PhotonTargets.All);

                crys.GetComponent<Crystal>().photonView.RPC("Goal", PhotonTargets.All);
            }
        }
    }

    private void SetCouleurLight(Color c, int intensite)
    {
        
        volumetricLight.color = c;
        volumetricLight.intensity = intensite;
    }
}

