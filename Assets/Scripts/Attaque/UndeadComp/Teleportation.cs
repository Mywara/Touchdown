using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportation : Photon.PunBehaviour
{
    public float speed = 10;
    private int team;
    private GameObject owner;

    private Rigidbody myRb;
    private GameObject target;

    // Use this for initialization
    void Start()
    {
        myRb = this.gameObject.GetComponent<Rigidbody>();
        if (myRb != null)
        {
            myRb.velocity = transform.forward * speed;
        }
        else
        {
            Debug.Log("error, no rigidbody on the projectile");
        }
    }

    public void SetTeam(int newTeam)
    {
        this.team = newTeam;
    }

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        target = other.transform.root.gameObject;
        if (target.tag.Equals("Respawn") || target.tag.Equals("Boundary"))
        {
            return;
        }
        
        if (!RoomManager.instance.FriendlyFire)
        {
            if (target.tag.Equals("Player") && target != owner)
            {
                PlayerController playerControllerScript = target.GetComponent<PlayerController>();
                if (playerControllerScript != null)
                {
                    if (playerControllerScript.Team == this.team)
                    {
                        //Debug.Log("TP on friend !");
                    }
                    else
                    {
                        //Debug.Log("TP on ennemy !");
                    }
                }
            }
        }
        
        if (target.tag.Equals("Player") && target != owner)
        {
            // lancer animation et CD
            Tp(target);
        }
        //Destroy(this.gameObject);
        PhotonNetwork.Destroy(this.gameObject);
    }

    private void Tp(GameObject target)
    {
        if(PhotonNetwork.connected == true)
        {
            owner.GetComponent<PlayerController>().photonView.RPC("SetPosition", PhotonTargets.All, target.transform.position - target.transform.forward);
        }
        else
        {
            owner.transform.position = target.transform.position - target.transform.forward;
            owner.transform.forward = target.transform.forward;
        }
        //On maudit la cible si c'est un ennemi
        if (target.GetComponent<PlayerController>().team != team)
        {
            target.GetComponent<PlayerController>().Curse();
        }
    }

    private void Update()
    {
        if (target == null) { PhotonNetwork.Destroy(this.gameObject); return; }
    }
}
