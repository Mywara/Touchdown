﻿ using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//attaque large peu profonde
//repousse les ennemis et fait peu de dégâts
public class GrapeShot : Photon.PunBehaviour
{

    private int damage;
    private float fireRate = 1;
    public int team;

    private bool netWorkingDone = false;
    private float nextFire = 0f;
    private List<GameObject> directHitObjs = new List<GameObject>();
    private bool syncTeam = true;
    private GameObject owner;

    //the radius of the circular zone
    //= depth of the spell
    //private float radius;
    //the angle of the circular zone
    //private float angle;

    public void Start()
    {
        damage = Constants.GRAPE_DAMAGE;
        //radius = Constants.GRAPE_RADIUS;
        //angle  = Constants.GRAPE_ANGLE;

    }

    public void SetTeam(int newTeam)
    {
        this.team = newTeam;
    }

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }

    //fonction pour le repoussement des ennemis
    public void repulse(GameObject target)
    {
        //direction de la cible
        Vector3 targetDir = target.transform.position - transform.position;

        float distance = Mathf.Sqrt(Mathf.Pow(targetDir.x, 2) + Mathf.Pow(targetDir.z, 2) + Mathf.Pow(targetDir.z, 2));

        if (PhotonNetwork.connected == true)
        {
            target.GetComponent<PlayerController>().photonView.RPC("AddForceTo", PhotonTargets.All, targetDir.normalized * 1 / distance * 5000);
        }
        else
        {
            target.GetComponent<Rigidbody>().AddForce(targetDir.normalized * 1 / distance * 5000);
        }
    }

    //Ici other = l'object que l'on a touché
    //Modif a faire, limiter dmg au cible valide -> layer + test
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        GameObject otherGO = other.transform.root.gameObject;
        if (otherGO.tag.Equals("Respawn") || otherGO.tag.Equals("Boundary") || other.tag.Equals("GoalD") || other.tag.Equals("GoalG"))
        {
            return;
        }
        
        if (!RoomManager.instance.FriendlyFire)
        {
            if (otherGO.tag.Equals("Player") && otherGO != owner)
            {
                PlayerController playerControllerScript = otherGO.GetComponent<PlayerController>();
                if (playerControllerScript != null)
                {
                    if (playerControllerScript.Team == this.team)
                    {
                        //Debug.Log("Friend hit, not FF, do nothing");
                        return;
                    }
                }
            }
        }

        if (!directHitObjs.Contains(otherGO) && otherGO.tag.Equals("Player") && otherGO.GetComponent<PlayerController>().team != team)
        {
            directHitObjs.Add(otherGO);
        }
    }

    private void Update()
    {
        if(directHitObjs.Count == 0 && this.gameObject) { PhotonNetwork.Destroy(this.gameObject); return; }

        foreach (GameObject directHitObj in directHitObjs.ToArray())
        {
            ApplyDamage(directHitObj, damage);
            repulse(directHitObj);
            directHitObjs.Remove(directHitObj);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        directHitObjs.Remove(other.transform.root.gameObject);
        //PhotonNetwork.Destroy(this.gameObject);
    }

    private void ApplyDamage(GameObject target, int damage)
    {
        if(PhotonNetwork.connected == true)
        {
            PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
            if (healthScript != null)
            {
                //healthScript.Damage(damage);
                healthScript.photonView.RPC("Damage", PhotonTargets.All, damage);

            }

            PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
            if (healthScript2 != null)
            {
                //healthScript2.Damage2(damage);
                healthScript2.photonView.RPC("Damage2", PhotonTargets.All, damage);
            }
        }
        else
        {
            target.GetComponent<PUNTutorial.HealthScript>().Damage(damage);
        }
    }
}
