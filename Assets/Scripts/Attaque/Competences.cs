﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Competences : Photon.PunBehaviour
{
    public GameObject projectilePrefab1 = null;
    public GameObject projectilePrefab2 = null;
    public GameObject projectilePrefabUlt = null;

    public Transform projectileSpawn1 = null;
    public Transform projectileSpawn2 = null;
    public Transform projectileSpawnUlt = null;

    // test
    public GameObject sourceObject;
    //
    
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Competences Update");
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        if (Input.GetButton("A"))
        {
            Debug.Log("Enter in A");
            // animation trigger
            //anim.SetTrigger("AttackGun");

            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                Debug.Log("En local");
                projo = Instantiate(projectilePrefab1, new Vector3(projectileSpawn1.position.x, projectileSpawn1.position.y + 0.5f, projectileSpawn1.position.z), Quaternion.identity).gameObject as GameObject;
                //projectilePrefab1.GetComponent<projectilePrefab1.name>.SetTeam(playerControllerScript.Team);
                Debug.Log("Instantiate ok");
            }
            else
            {
                Debug.Log("En réseau");
                //Pour le reseau
                projo = PhotonNetwork.Instantiate(this.projectilePrefab1.name, new Vector3(projectileSpawn1.position.x, projectileSpawn1.position.y + 0.5f, projectileSpawn1.position.z), Quaternion.identity, 0);
                //projectilePrefab1.GetComponent<projectilePrefab1.name>.SetTeam(playerControllerScript.Team);
                Debug.Log("Instantiate ok");
            }
        }
    }
}
