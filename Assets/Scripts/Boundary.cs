using PUNTutorial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour {

    public static Boundary instance;


    void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
    }

    private void OnTriggerExit(Collider other)
    {
        if(!PhotonNetwork.isMasterClient)
        {
            return;
        }
        GameObject otherGO = other.transform.root.gameObject;
        if(otherGO.tag.Equals("Player"))
        {
            if(Crystal.instance.playerHolding == otherGO)
            {
                Debug.Log("Player colliding with boundary holds the crystal");

                if (PhotonNetwork.connected)
                {
                    Crystal.instance.photonView.RPC("ResetCrystalPosition", PhotonTargets.All);
                    Crystal.instance.playerHolding = null;
                    Crystal.instance.isHeld = false;
                }
                else
                {
                    Crystal.instance.ResetCrystalPosition();
                    Crystal.instance.playerHolding = null;
                    Crystal.instance.isHeld = false;
                }
            }

            HealthScript healthScript = otherGO.GetComponent<HealthScript>();
            if(healthScript != null)
            {
                healthScript.photonView.RPC("KillPlayer", PhotonTargets.All);

                HealthScript2 healthScript2 = otherGO.GetComponent<HealthScript2>();
                if(healthScript2)
                {
                    healthScript2.EmptyBar();
                }

                Debug.Log("Player kill by boundary");
            }
            else
            {
                Debug.Log("Player don't have a HealthScript");
            }
        }
        else if(otherGO.tag.Equals("Crystal"))
        {
            Debug.Log("Crystal collides with boundary");

            Crystal crystalScript = otherGO.GetComponent<Crystal>();
            if (crystalScript)
            {
                if (PhotonNetwork.connected)
                    crystalScript.photonView.RPC("ResetCrystalPosition", PhotonTargets.All);
                else
                    crystalScript.ResetCrystalPosition();
            }
            else
                Debug.Log("Crystal colliding with the boundary does not have a Crystal script");
        }
        else
        {
            //Debug.Log("Object out of boundary -> destroy");
            //Destroy(otherGO);
            PhotonNetwork.Destroy(otherGO);
        } 
    }

    public void SetSize(int x, int y , int z)
    {
        GetComponent<BoxCollider>().size = new Vector3 (x, y, z);
    }
}
