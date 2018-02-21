﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttaqueCac : Photon.PUNBehaviour, IPunObservable {

    public int damage = 10;
    public float fireRate = 1;
    public GameObject cacHitZone;
    public float hclamp;

    private bool isAttacking = false;
    private Animator anim;
    private Camera cam;

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(this.cacHitZone.GetActive());
        }
        else
        {
            this.cacHitZone.SetActive((bool)stream.ReceiveNext());
        }
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        CacHitZone cacHitZoneScript = cacHitZone.GetComponent<CacHitZone>();
        if(cacHitZoneScript != null)
        {
            cacHitZoneScript.SetDamage(this.damage);
            cacHitZoneScript.SetFireRate(this.fireRate);
            //dire de quelle equipe vient le projectile pour ne pas TK
            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            if (playerControllerScript != null)
            {
                cacHitZoneScript.SetTeam(playerControllerScript.Team);
                //Debug.Log("cachitzone team set to : " + playerControllerScript.Team);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }
        else
        {
            Debug.Log("The CaCHitZone does not have a CacHitZone Script");
        }
        cacHitZone.SetActive(false);
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        if(Input.GetButtonDown("Fire1"))
        {
            // animation trigger
            anim.SetBool("AttackCac", true);

            cacHitZone.SetActive(true);
        }
        if (Input.GetButtonUp("Fire1"))
        {
            // animation trigger
            anim.SetBool("AttackCac", false);

            cacHitZone.SetActive(false);
        }
        // on oriente la CacHitZone (sa hauteur)
        cacHitZone.transform.position = calculHauteur();

    }

    private Vector3 calculHauteur()
    {
        Vector3 vect = cacHitZone.transform.position;
        var rotationVector = cam.transform.rotation.eulerAngles;
        float rx = rotationVector.x;

        // si on regarde vers le haut les valeurs partent de 359 et diminu plus on regarde haut
        // sinon elle partent de 0 et augmentent 
        if (rx > 180)
        {
            rx = rx - 360;
        }
        vect.y = this.transform.position.y - rx / hclamp;

        return vect;
    }
}
