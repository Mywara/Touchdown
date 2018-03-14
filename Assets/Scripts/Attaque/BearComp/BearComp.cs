using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearComp : Photon.PunBehaviour
{
    public GameObject projectilePrefab = null;

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
        //Compétence Coup d'épaule
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(projectilePrefab, projectileSpawn1.position, projectileSpawn1.rotation).gameObject as GameObject;
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(projectilePrefab.name, projectileSpawn1.position, projectileSpawn1.rotation, 0);
            }

            //Fait de set de la team pour le projectile
            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            ShoulderHit shoulderScript = projo.GetComponent<ShoulderHit>();
            if (playerControllerScript != null)
            {
                shoulderScript.SetTeam(playerControllerScript.Team);
                shoulderScript.SetOwner(this.transform.gameObject);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }
    }
}
