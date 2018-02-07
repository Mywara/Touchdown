﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttaqueCac : Photon.MonoBehaviour, IPunObservable {

    public float damage = 10;
    public float fireRate = 1;
    public GameObject cacHitZone;

    private bool isAttacking = false;

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

    // Use this for initialization
    void Start()
    {
        CacHitZone cacHitZoneScript = cacHitZone.GetComponent<CacHitZone>();
        if(cacHitZoneScript != null)
        {
            cacHitZoneScript.SetDamage(this.damage);
            cacHitZoneScript.SetFireRate(this.fireRate);
            //dire de quelle equipe vient le projectile pour ne pas TK
            //cacHitZoneScript.SetTeam();
        }
        else
        {
            Debug.Log("The CaCHitZone does not have a CacHitZone Script");
        }
        cacHitZone.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        /*
        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            cacHitZone.SetActive(true);
            cacHitZone.SetActive(false);
        }
        */
        if(Input.GetButtonDown("Fire1"))
        {
            cacHitZone.SetActive(true);
        }
        if (Input.GetButtonUp("Fire1"))
        {
            cacHitZone.SetActive(false);
        }
    }
}
