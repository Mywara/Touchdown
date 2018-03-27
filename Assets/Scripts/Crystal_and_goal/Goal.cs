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
            couleurOrange = new Vector4(246f / 255f, 211f / 255f, 20f / 255f, 1);

            // On recupere la lumiere
            volumetricLight = GetComponentInChildren<Light>();


            // Debug.Log(" this.tag : " + this.tag);
            // On affiche le halo d'une couleur differente selon le joueur et le but auquel le script est attaché 
            if (this.tag == "GoalG" && PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team == 1
                || this.tag == "GoalD" && PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team == 2)
            {
                Debug.Log(" couleur allie ");
                // Cas le but correspond a l'equipe du joueur
                SetCouleurLight(couleurBleue, bleuIntensite);
            }
            else
            {
                Debug.Log(" couleur ennemi ");
                // Cas le but est le but adverse
                SetCouleurLight(couleurOrange, orangeIntensite);
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

        //Debug.Log(target.transform.root.gameObject.name);

        if (target.transform.root.gameObject.tag == "Player"
            && crys.GetComponent<Crystal>().isHeld == true
            && crys.GetComponent<Crystal>().playerHolding == target.transform.root.gameObject)
        {
            //Le cristal doit être reset si le but est marqué il faut donc que le butteur soit de l'equipe adverse au but atteint
            if (this.tag == "GoalG" && target.gameObject.GetComponent<PlayerController>().team == 0 ||
               this.tag == "GoalD" && target.gameObject.GetComponent<PlayerController>().team == 1)
            {
                //Debug.Log("Crystal is in goal collider OK!");

                crys.GetComponent<Crystal>().photonView.RPC("LeaveOnGround", PhotonTargets.All);
                //Debug.Log("Crystal left on ground");

                crys.GetComponent<Crystal>().photonView.RPC("ResetCrystalPosition", PhotonTargets.All);
                //Debug.Log("Crystal reset");
            }
        }
    }

    private void SetCouleurLight(Color c, int intensite)
    {
        
        volumetricLight.color = c;
        volumetricLight.intensity = intensite;
    }
}

