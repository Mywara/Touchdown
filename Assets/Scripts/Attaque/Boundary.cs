using PUNTutorial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerExit(Collider other)
    {
        GameObject otherGO = other.transform.root.gameObject;
        if(otherGO.tag.Equals("Player"))
        {
            HealthScript healthScript = otherGO.GetComponent<HealthScript>();
            if(healthScript != null)
            {
                healthScript.photonView.RPC("KillPlayer", PhotonTargets.All);
                Debug.Log("Player kill by boundary");
            }
            else
            {
                Debug.Log("Player don't have a HealthScript");
            }
        }
        else
        {
            //Debug.Log("Object out of boundary -> destroy");
            Destroy(otherGO);
        } 
    }
}
