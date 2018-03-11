using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateComp : Photon.PunBehaviour
{
    public GameObject projectilePrefab1 = null;
    public GameObject rhumBottle = null;
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
        if (Input.GetButtonDown("A"))
        {
            // animation trigger
            //anim.SetTrigger("AttackGun");
            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(projectilePrefab1, projectileSpawn1.position, projectileSpawn1.rotation).gameObject as GameObject;
                //projectilePrefab1.GetComponent<projectilePrefab1.name>.SetTeam(playerControllerScript.Team);
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(this.projectilePrefab1.name, projectileSpawn1.position, projectileSpawn1.rotation, 0);
                //projectilePrefab1.GetComponent<projectilePrefab1.name>.SetTeam(playerControllerScript.Team);

            }

            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            GrapeShot grapeShotScript = projo.GetComponent<GrapeShot>();
            if (playerControllerScript != null)
            {
                grapeShotScript.SetTeam(playerControllerScript.Team);
                grapeShotScript.SetOwner(this.transform.gameObject);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }
        //Compétence Lancer de bouteille de rhum
        if (Input.GetButtonDown("E"))
        {
            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(rhumBottle, projectileSpawn1.position, projectileSpawn1.rotation).gameObject as GameObject;
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(rhumBottle.name, projectileSpawn1.position, projectileSpawn1.rotation, 0);
            }

            //Fait de set de la team pour le projectile
            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            RhumSpray rhumSprayScript = projo.GetComponent<RhumSpray>();
            if (playerControllerScript != null)
            {
                rhumSprayScript.SetTeam(playerControllerScript.Team);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }
    }
}
