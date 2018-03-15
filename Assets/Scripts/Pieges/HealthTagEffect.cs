using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthTagEffect : Photon.PunBehaviour
{

    // Use this for initialization
    public float HealthContenance = 500f;
    private Animator anim;
    private int team;
    private GameObject owner;

    private List<GameObject> directHitObjs;

    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (HealthContenance <= 0) { PhotonNetwork.Destroy(this.gameObject); return; }

        foreach (GameObject directHitObj in directHitObjs.ToArray())
        {
            if(HealthContenance > 0 && directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value < directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthMax)
            {
                if(directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthMax - directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value < 20)
                {
                    ApplyHealing(directHitObj, directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthMax - directHitObj.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value);
                }
                else
                {
                    ApplyHealing(directHitObj, 20);
                }
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        Debug.Log("Entrée dans le OnTriggerStay du Heal");
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
                    if (playerControllerScript.Team == this.team)
                    {
                        Debug.Log("Friend hit, not FF, do nothing");
                        return;
                    }
                }
            }
        }

        if (!directHitObjs.Contains(otherGO) && otherGO.tag.Equals("Player") && otherGO.GetComponent<PlayerController>().team == team)
        {
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

    private void ApplyHealing(GameObject target, float heal)
    {
        if (PhotonNetwork.connected == true)
        {
            PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
            if (healthScript != null)
            {
                //healthScript.Damage(damage);
                if(HealthContenance >= heal)
                {
                    healthScript.photonView.RPC("Heal", PhotonTargets.All, -heal);
                    HealthContenance -= heal;
                }
                else
                {
                    healthScript.photonView.RPC("Heal", PhotonTargets.All, -HealthContenance);
                    HealthContenance = 0;
                }
            }

            PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
            if (healthScript2 != null)
            {
                //healthScript2.Damage2(damage);
                if (HealthContenance >= heal)
                {
                    healthScript2.photonView.RPC("Damage2", PhotonTargets.All, -heal);
                    HealthContenance -= heal;
                }
                else
                {
                    healthScript2.photonView.RPC("Damage2", PhotonTargets.All, -HealthContenance);
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
