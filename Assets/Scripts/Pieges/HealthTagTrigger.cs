using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthTagTrigger : Photon.PUNBehaviour
{

    public GameObject zoneEffect;
    bool notAlreadyActivate = true;
    public GameObject Owner;


    private void Start()
    {

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            //déclenche le piege et fait apparaitre la zone
            if (notAlreadyActivate)
            {
                PhotonNetwork.Instantiate(zoneEffect.name, this.transform.position, Quaternion.identity, 0);
                notAlreadyActivate = false;
                //supprime une charge (on ne peut pas remplacer un piège en cd ou activer)
                Owner.GetComponent<HealthTag>().nbCharges--;
                Owner.GetComponent<HealthTag>().nbtrap.text = "" + Owner.GetComponent<HealthTag>().nbCharges;
                //démarre la coroutine pour recuperer une charge dés que le piège est activé
                Owner.GetComponent<HealthTag>().startCoroutineGetACharge = true;
            }
        }
    }

    [PunRPC]
    void DestroyTrap()
    {
        Destroy(this.transform.gameObject);
    }


}
