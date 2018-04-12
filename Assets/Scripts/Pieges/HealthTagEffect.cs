using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthTagEffect : Photon.PunBehaviour
{

    // Use this for initialization
    public int HealthContenance = 500;
    private Animator anim;
    private GameObject owner;
    private float tempsProchainHeal;
    public float delaiHOT;
    public int AmountOfHeal;

    private List<GameObject> directHitObjs = new List<GameObject>();

    void Start()
    {
        tempsProchainHeal = Time.time;
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
            //Debug.Log("Personne ne se trouve dans la zone de heal");
        }

        if(Time.time > tempsProchainHeal)
        {
            foreach (GameObject directHitObj in directHitObjs)
            {
                if (HealthContenance > 0 && directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value < directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthMax)
                {
                    if (directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthMax - directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value < AmountOfHeal)
                    {
                        ApplyHealing(directHitObj, (int)(directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthMax - directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value));
                    }
                    else
                    {
                        ApplyHealing(directHitObj, AmountOfHeal);
                    }
                }
            }

            tempsProchainHeal = Time.time + delaiHOT;
        }

        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        GameObject otherGO = other.transform.root.gameObject;
        if (otherGO.tag.Equals("Respawn") || otherGO.tag.Equals("Boundary"))
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
                    if (playerControllerScript.team == owner.GetComponent<PlayerController>().team)
                    {
                        //Debug.Log("Friend hit, not FF, do nothing");
                    }
                }
            }
        }

        if (otherGO.tag.Equals("Player") &&
            !directHitObjs.Contains(otherGO) &&
            otherGO.GetComponent<PlayerController>().team == owner.GetComponent<PlayerController>().team)
        {
            //Debug.Log(otherGO.name + " a été ajouté à la liste des alliés à soigner");
            directHitObjs.Add(otherGO);
            //Debug.Log(otherGO.name + " appartient à la liste : " + directHitObjs.Contains(otherGO) + " " + directHitObjs.Count);
            directHitObjs.Add(otherGO);
        }

    }
    
    public void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            directHitObjs.Remove(other.gameObject);
        }
    }

    private void ApplyHealing(GameObject target, int heal)
    {
        if (PhotonNetwork.connected == true)
        {
            PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
            PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
            if (healthScript != null && healthScript2 != null)
            {
                //healthScript.Damage(damage);
                if(HealthContenance >= heal)
                {
                    healthScript.photonView.RPC("Heal", PhotonTargets.All, heal);
                    healthScript2.photonView.RPC("Heal2", PhotonTargets.All, heal);
                    HealthContenance -= heal;
                }
                else
                {
                    healthScript.photonView.RPC("Heal", PhotonTargets.All, HealthContenance);
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
