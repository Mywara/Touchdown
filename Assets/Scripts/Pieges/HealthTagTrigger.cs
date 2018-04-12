using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthTagTrigger : Photon.PUNBehaviour
{

    public GameObject zoneEffect;
    bool notAlreadyActivate = true;
    public GameObject Owner;

    public GameObject baliseSoinAlliee;
    public GameObject baliseSoinEnnemi;

    private void Start()
    {
        zoneEffect.GetComponent<HealthTagEffect>().setOwner(Owner);
        PlayerController playerControllerScript = Owner.GetComponent<PlayerController>();

        this.photonView.RPC("SetCouleurBalise", PhotonTargets.All, playerControllerScript.team);
    }

    [PunRPC]
    private void SetCouleurBalise(int team)
    {
        if (team == PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team)
        {
            baliseSoinAlliee.SetActive(true);
        }
        else
        {
            baliseSoinEnnemi.SetActive(true);
        }
    }

    

    public void OnTriggerEnter(Collider other)
    {/*
        if (other.tag == "Player")
        {
            //déclenche le piege et fait apparaitre la zone
            if (notAlreadyActivate)
            {
                PhotonNetwork.Instantiate(zoneEffect.name, this.transform.position, Quaternion.identity, 0);
                notAlreadyActivate = false;
                photonView.RPC("DestroyTrap", PhotonTargets.All);
                //supprime une charge (on ne peut pas remplacer un piège en cd ou activer)
                Owner.GetComponent<PGlace>().nbCharges--;
                Owner.GetComponent<PGlace>().nbtrap.text = "" + Owner.GetComponent<PGlace>().nbCharges;
                //démarre la coroutine pour recuperer une charge dés que le piège est activé
                Owner.GetComponent<PGlace>().startCoroutineGetACharge = true;
            }
        }*/
    }


}
