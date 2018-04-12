using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalDrop : Photon.PUNBehaviour
{


    public GameObject crys = null;
    public AudioSource audioSource;
    public AudioClip sfxCrystalDrop;

    private void Start()
    {
        //on va chercher l'instance du crystal, comme ca on la connait tout le temps
        crys = GameObject.FindGameObjectWithTag("Crystal");
    }

    private void OnTriggerEnter(Collider target)
    {

        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        if (target.transform.root.gameObject.tag == "Crystal" && !RoomManager.instance.IsInWaitForStartPhase())
        {
            //fait dans le start
            //crys = target.transform.root.gameObject;

            if (!crys.GetComponent<Crystal>().isHeld && this.transform.root.gameObject != crys.GetComponent<Crystal>().justDroppedCrystal)
            {
                crys.GetComponent<Crystal>().photonView.RPC("PickupCrystal", PhotonTargets.All, this.transform.root.gameObject.GetPhotonView().viewID);
            }


        }
    }

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

            crys.GetComponent<Crystal>().photonView.RPC("UpdateJustDroppedCrystal", PhotonTargets.All);

            crys.GetComponent<Crystal>().photonView.RPC("LeaveOnGround", PhotonTargets.All);

            PlaySFXCrystalDrop();
        }

    }

    public void PlaySFXCrystalDrop()
    {
        //audioRPC.minDistance = 1;
        audioSource.maxDistance = 100;
        audioSource.clip = sfxCrystalDrop;
        audioSource.Play();
    }
}
