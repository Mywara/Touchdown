using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndeadComp : Photon.PunBehaviour
{
    public GameObject projectilePrefab1 = null;
    public GameObject projectilePrefab2 = null;
    public GameObject projectilePrefabUlt = null;

    public Transform projectileSpawn1 = null;

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
            // animation trigger
            //anim.SetTrigger("AttackGun");

            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(projectilePrefab1, projectileSpawn1.position, Quaternion.identity).gameObject as GameObject;
                //projectilePrefab1.GetComponent<projectilePrefab1.name>.SetTeam(playerControllerScript.Team);
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(this.projectilePrefab1.name, projectileSpawn1.position, Quaternion.identity, 0);
                //projectilePrefab1.GetComponent<projectilePrefab1.name>.SetTeam(playerControllerScript.Team);

            }

            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            CurseDoT curseDoTScript = projo.GetComponent<CurseDoT>();
            if (playerControllerScript != null)
            {
                curseDoTScript.SetTeam(playerControllerScript.Team);
                //curseDoTScript.SetOwner(this.transform.gameObject);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }
    }
}
