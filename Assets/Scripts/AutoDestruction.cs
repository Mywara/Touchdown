using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestruction : Photon.PunBehaviour
{

    public float delaiAvantDestruction; // timer avant auto-destruction
    private float tempsDeCreation; // moment où créé

    // Use this for initialization
    void Start()
    {
        tempsDeCreation = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > tempsDeCreation + delaiAvantDestruction)
        {
            if (PhotonNetwork.connected == false)
            {
                Destroy(this.gameObject);
            }
            else
            {
                PhotonNetwork.Destroy(this.gameObject);
            }

        }
    }

    private void OnDisable()
    {
        if (PhotonNetwork.connected == false)
        {
            Destroy(this.gameObject);
        }
        else
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}
