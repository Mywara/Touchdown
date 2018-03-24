using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalDrop : Photon.PUNBehaviour
{


    public GameObject crys = null;


    private void OnTriggerEnter(Collider target)
    {

        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        if (target.transform.root.gameObject.tag == "Crystal")
        {
            //Debug.Log("crystal_player collider OK!");
            crys = target.transform.root.gameObject;

            if (this.transform.root.gameObject != crys.GetComponent<Crystal>().justDroppedCrystal)
            {
                crys.GetComponent<Crystal>().photonView.RPC("PickupCrystal", PhotonTargets.All, this.transform.root.gameObject.GetPhotonView().viewID);
            }


        }
    }


    //// Use this for initialization
    //void Start () {

    //}

    // Update is called once per frame
    void Update()
    {

        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        if (Input.GetButtonDown("Drop")
            && crys != null
            && crys.GetComponent<Crystal>().playerHolding == this.transform.root.gameObject
            && crys.GetComponent<Crystal>().isHeld == true)
        {

            //Debug.Log("Le boutton Drop est pressé");
            crys.GetComponent<Crystal>().photonView.RPC("UpdateJustDroppedCrystal", PhotonTargets.All);

            crys.GetComponent<Crystal>().photonView.RPC("LeaveOnGround", PhotonTargets.All);
        }

    }
}
