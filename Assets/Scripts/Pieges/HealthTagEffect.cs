using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthTagEffect : Photon.PunBehaviour
{

    // Use this for initialization
    public int HealthContenance = 500;
    private Animator anim;
    private GameObject owner;

    private List<GameObject> directHitObjs = new List<GameObject>();

    void Start()
    {

    }

    public void setOwner(GameObject o)
    {
        owner = o;
    }

    // Update is called once per frame
    void Update()
    {
        if (HealthContenance <= 0) {
            PhotonNetwork.Destroy(this.transform.root.gameObject);
            owner.GetComponent<HealthTag>().startCoroutineGetACharge = true;
            return;
        }

        if(directHitObjs.Count == 0)
        {
            Debug.Log("Personne ne se trouve dans la zone de heal");
        }

        foreach (GameObject directHitObj in directHitObjs)
        {
            if(HealthContenance > 0 && directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value < directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthMax)
            {
                if(directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthMax - directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value < 20)
                {
                    ApplyHealing(directHitObj, (int)(directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthMax - directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value));
                }
                else
                {
                    ApplyHealing(directHitObj, 20);
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entrée dans le OnTriggerEnter du Heal");
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            Debug.Log("PhotonView");
            return;
        }
        GameObject otherGO = other.transform.root.gameObject;
        if (otherGO.tag.Equals("Respawn") || otherGO.tag.Equals("Boundary"))
        {
            Debug.Log("In Respawn");
            return;
        }

        if (!RoomManager.instance.FriendlyFire)
        {
            Debug.Log("Room Manager");
            if (otherGO.tag.Equals("Player") && otherGO != owner)
            {
                PlayerController playerControllerScript = otherGO.GetComponent<PlayerController>();
                if (playerControllerScript != null)
                {
                    if (playerControllerScript.team == owner.GetComponent<PlayerController>().team)
                    {
                        Debug.Log("Friend hit, not FF, do nothing");
                    }
                }
            }
        }

        Debug.Log("Ajout ?");
        if (otherGO.tag.Equals("Player") &&
            !directHitObjs.Contains(otherGO) &&
            otherGO.GetComponent<PlayerController>().team == owner.GetComponent<PlayerController>().team)
        {
            Debug.Log(otherGO.name + " a été ajouté à la liste des alliés à soigner");
            directHitObjs.Add(otherGO);
            Debug.Log(otherGO.name + " appartient à la liste : " + directHitObjs.Contains(otherGO) + " " + directHitObjs.Count);
        }

    }
    
    public void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            Debug.Log(other.name + " suppression de la liste");
            directHitObjs.Remove(other.gameObject);
            Debug.Log(other.name + " appartient à la liste : " + directHitObjs.Contains(other.gameObject) + " " + directHitObjs.Count);
        }
    }

    private void ApplyHealing(GameObject target, int heal)
    {
        Debug.Log("Heal de " + heal);
        if (PhotonNetwork.connected == true)
        {
            PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
            if (healthScript != null)
            {
                //healthScript.Damage(damage);
                if(HealthContenance >= heal)
                {
                    healthScript.photonView.RPC("Heal", PhotonTargets.All, heal);
                    HealthContenance -= heal;
                }
                else
                {
                    healthScript.photonView.RPC("Heal", PhotonTargets.All, HealthContenance);
                    HealthContenance = 0;
                }
            }

            PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
            if (healthScript2 != null)
            {
                //healthScript2.Damage2(damage);
                if (HealthContenance >= heal)
                {
                    healthScript2.photonView.RPC("Heal2", PhotonTargets.All, heal);
                    HealthContenance -= heal;
                }
                else
                {
                    healthScript2.photonView.RPC("Heal2", PhotonTargets.All, HealthContenance);
                    HealthContenance = 0;
                }
            }
        }
        else
        {
            //healthScript.Damage(damage);
            if (HealthContenance >= heal)
            {
                target.GetComponent<PUNTutorial.HealthScript>().Heal(heal);
                HealthContenance -= heal;
            }
            else
            {
                target.GetComponent<PUNTutorial.HealthScript>().Heal(HealthContenance);
                HealthContenance = 0;
            }
        }
    }
}
