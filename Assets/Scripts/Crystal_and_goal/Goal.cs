using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

    public GameObject crys;

    private void OnTriggerEnter(Collider target)
    {
        if (PhotonNetwork.connected == true && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        //Debug.Log(target.transform.root.gameObject.name);

        if (target.transform.root.gameObject.tag == "Player"
            && crys.GetComponent<Crystal>().isHeld == true
            && crys.GetComponent<Crystal>().playerHolding == target.transform.root.gameObject)
        {
            //Debug.Log("Crystal is in goal collider OK!");

        
            crys.GetComponent<Crystal>().photonView.RPC("LeaveOnGround", PhotonTargets.All);
            //Debug.Log("Crystal left on ground");

            crys.GetComponent<Crystal>().photonView.RPC("ResetCrystalPosition", PhotonTargets.All);
            //Debug.Log("Crystal reset");
        }

        
    }

}
