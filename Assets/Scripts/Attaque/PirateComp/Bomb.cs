﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Photon.PUNBehaviour {
    
    public float AOERadius = 3;
    public float projectileSpeed = 10;
    public int degat;
    public int team;
    public float maxAnglularVelocity = 10;
    public float verticalVelocity = 10;
    public GameObject effetExplosionCloche;
    public GameObject model;
    private GameObject owner;

    private Rigidbody myRb;

    private bool animLance = false;


    // Use this for initialization
    void Start()
    {
        myRb = this.gameObject.GetComponent<Rigidbody>();
        if (myRb != null)
        {
            //On lance l'object vers l'avant (vers où on vise) avec une certaine vitesse
            myRb.velocity = transform.forward * projectileSpeed + transform.up * verticalVelocity;
            myRb.angularVelocity = Random.insideUnitSphere * maxAnglularVelocity;
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

    //Ici other = l'object que l'on a touché
    //Modif a faire, limiter dmg au cible valide -> layer + test
    private void OnTriggerEnter(Collider other)
    {
        //Si l'objet n'est pas controlé localement quand on est connecté, on ne fait rien
        //Donc c'est le possesseur de l'objet qui detectera les collisions et notifira les degats
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        //on recupère l'object le plus haut de hierachie sur l'objet touché
        GameObject directHitObj = other.transform.root.gameObject;
        //On enlève les collisions pour appliquer des dégâts avec le respawn et la bordure
        if (directHitObj.tag.Equals("Respawn") || directHitObj.tag.Equals("Boundary") || directHitObj == owner || other.tag.Equals("GoalD") || other.tag.Equals("GoalG"))
        {
            
            return;
        }
        if (!animLance)
        {
            LanceAnim();
            animLance = true;
        }
        

        //on recupère les colliders dans la zone d'AOE
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, AOERadius);
        //On parcours les objets dans l'AOE
        foreach (Collider colliITE in hitColliders)
        {
            GameObject objInAOE = colliITE.transform.root.gameObject;
            //Si c'est un joueur, on peut continuer
            if (objInAOE.tag.Equals("Player"))
            {
                PlayerController playerControllerScript = objInAOE.GetComponent<PlayerController>();
                if (playerControllerScript != null)
                {
                    //On test si la cible est de notre cible ou non
                    if (playerControllerScript.Team != this.team)
                    {
                        ApplyDamage(objInAOE, degat);
                    }
                }
            }
        }

        //On detruit le projectile apres l'impacte
        if (PhotonNetwork.connected == false)
        {
            Destroy(this.gameObject);
        }
        else
        {
            PhotonNetwork.Destroy(this.gameObject);
        }



    }

    //Fonction pour appliquer les heals sur un player
    private void ApplyDamage(GameObject target, int degat)
    {
        PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
        if (healthScript != null)
        {

            healthScript.photonView.RPC("Damage", PhotonTargets.All, degat);
        }

        PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
        if (healthScript2 != null)
        {
            healthScript2.photonView.RPC("Damage2", PhotonTargets.All, degat);
        }
    }

    private void LanceAnim()
    {
        GameObject effetExplosion;
        //Pour le local
        if (PhotonNetwork.connected == false)
        {
            
            effetExplosion = Instantiate(effetExplosionCloche, this.transform.position + Vector3.up * 0.1f, effetExplosionCloche.transform.rotation).gameObject as GameObject;
        }
        else
        {
            //Pour le reseau
            effetExplosion = PhotonNetwork.Instantiate(this.effetExplosionCloche.name, this.transform.position, effetExplosionCloche.transform.rotation, 0);
        }
    }


}
