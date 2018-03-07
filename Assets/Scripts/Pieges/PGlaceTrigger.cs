using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PGlaceTrigger : Photon.PUNBehaviour {

    public GameObject zoneEffect;
    bool notAlreadyActivate = true;
    public GameObject Owner;



    public void OnTriggerEnter(Collider other)
    {
        //déclenche le piege et fait apparaitre la zone
        if (notAlreadyActivate) {
            PhotonNetwork.Instantiate(zoneEffect.name,this.transform.position,Quaternion.identity,0);
            notAlreadyActivate = false;
            photonView.RPC("DestroyTrap", PhotonTargets.All);
           //supprime une charge (on ne peut pas remplacer un piège en cd ou activer)
            Owner.GetComponent<PGlace>().nbCharges--;
            Owner.GetComponent<PGlace>().nbtrap.text = "" + Owner.GetComponent<PGlace>().nbCharges;
            //démarre la coroutine pour recuperer une charge dés que le piège est activé
            Owner.GetComponent<PGlace>().startCoroutineGetACharge = true;
        }
    }

    [PunRPC]
    void DestroyTrap()
    {
        Destroy(this.transform.gameObject);
    }


}
