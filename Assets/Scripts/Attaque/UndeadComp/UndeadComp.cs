﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UndeadComp : Photon.PunBehaviour
{
    public GameObject projectilePrefab1 = null;
    //public GameObject projectilePrefab2 = null;

    public Transform projectileSpawn1 = null;
    //public Transform projectileSpawn2 = null;

    private Animator anim;

    public GameObject HUD;
    public float transparenceCD;

    private float invulnerableLastUse;
    private float dotLastUse;
    private float tpLastUse;

    public float invulnerableCooldown;
    public float dotCooldown;
    public float tpCooldown;

    public GameObject invulnerableHUD;
    public GameObject dotHUD;
    public GameObject tpHUD;

    private bool compUndeathActif = true;

    private bool isInInvunerabilityMode = false;

    // Utile pour les competences de vise (TP stuff)
    private Camera cam;
    private RaycastHit hit;
    public float tpMaxRange;
    public GameObject animTP;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        // Pour pouvoir utiliser les compétences dès le début
        invulnerableLastUse = -invulnerableCooldown;
        dotLastUse = -dotCooldown;
        tpLastUse = -tpCooldown;

        // On affiche les compétences et CD qu'à la personne concernée.
        if (photonView.isMine)
        {
            HUD.SetActive(true);
            invulnerableHUD.transform.Find("CooldownGreyMask").gameObject.SetActive(false);
            tpHUD.transform.Find("CooldownGreyMask").gameObject.SetActive(false);
            dotHUD.transform.Find("CooldownGreyMask").gameObject.SetActive(false);
        }
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsInInvunerabilityMode && this.gameObject.GetComponent<CrystalDrop>().crys.GetComponent<Crystal>().playerHolding == this.gameObject)
        {
            return;
        }
        if (!photonView.isMine && PhotonNetwork.connected == true || !compUndeathActif)
        {
            return;
        }

        // Lance le dot 
        if (Input.GetButtonDown("Skill1") && Time.time > dotLastUse + dotCooldown)
        {
            // animation trigger
            //anim.SetTrigger("AttackGun");

            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(projectilePrefab1, projectileSpawn1.position, projectileSpawn1.rotation).gameObject as GameObject;
            }
            else
            {
                //Pour le reseau
                projo = PhotonNetwork.Instantiate(this.projectilePrefab1.name, projectileSpawn1.position, projectileSpawn1.rotation, 0);
            }

            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            CurseDoT curseDoTScript = projo.GetComponent<CurseDoT>();
            if (playerControllerScript != null)
            {
                curseDoTScript.SetTeam(playerControllerScript.Team);
                curseDoTScript.SetOwner(this.transform.gameObject);

                dotLastUse = Time.time;

                // Lance l'affichage du CD
                object[] parms = { dotHUD, dotCooldown };
                StartCoroutine("AffichageCooldown", parms);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }

        // Lance la TP
        else if (Input.GetButtonDown("Skill2") && Time.time > tpLastUse + tpCooldown)
        {
            // animation trigger
            //anim.SetTrigger("AttackGun");

            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();

            // Si on cible sur quelqu'un d'assez près
            if (Physics.Raycast(cam.transform.position + cam.transform.forward * 2.5f, cam.transform.forward, out hit, 500))
            {

                if (!RoomManager.instance.FriendlyFire)
                {
                    if (hit.transform.tag.Equals("Player") && hit.transform != this.transform && hit.distance < tpMaxRange)
                    {
                        
                        //playerControllerScript = script de nous-même

                        //Lance l'animation de disparition
                        this.photonView.RPC("LanceAnimTP", PhotonTargets.All);

                        tpLastUse = Time.time;
                        //Lance l'affichage du CD
                        object[] parms = { tpHUD, tpCooldown };
                        StartCoroutine("AffichageCooldown", parms);

                        Tp(hit.transform.gameObject, playerControllerScript.team);


                    }
                }
            }

            /////////////////////////////////////////////

            //GameObject projo;
            ////Pour le local
            //if (PhotonNetwork.connected == false)
            //{
            //    projo = Instantiate(projectilePrefab2, projectileSpawn2.position, Quaternion.identity).gameObject as GameObject;
            //}
            //else
            //{

            //    //Pour le reseau
            //    projo = PhotonNetwork.Instantiate(this.projectilePrefab2.name, projectileSpawn2.position, Quaternion.identity, 0);

            //}


            //Teleportation tpScript = projo.GetComponent<Teleportation>();
            //if (playerControllerScript != null)
            //{
            //    tpScript.SetTeam(playerControllerScript.Team);
            //    tpScript.SetOwner(this.transform.gameObject);




            //}
            //else
            //{
            //    Debug.Log("player have no PlayerController script");
            //}
            ///////////////////////////////////////////////////////////////

        }

        //Compétence pour l'invulnerabilité
        if (Input.GetButtonDown("Skill3") && Time.time > invulnerableLastUse + invulnerableCooldown)
        {
            Invulnerability invulScript = GetComponent<Invulnerability>();
            if (invulScript != null)
            {
                invulScript.photonView.RPC("BecameInvulnerable", PhotonTargets.All);

                invulnerableLastUse = Time.time;

                // Lance l'affichage du cooldown
                object[] parms = { invulnerableHUD, invulnerableCooldown };
                StartCoroutine("AffichageCooldown", parms);
            }
            else
            {
                Debug.Log("missinf Invulnerability script");
            }
        }
    }

    // GESTION AFFICHAGE

    // parms : arg0 = hud , arg1 = duree
    private IEnumerator AffichageCooldown(object[] parms)
    {
        float dureeCD = (float)parms[1];
        GameObject cdMask = ((GameObject)parms[0]).transform.Find("CooldownGreyMask").gameObject;

        /*
        // Modifie la transparence
        Image image = ((GameObject)parms[0]).GetComponent<Image>();
        Color c = image.color;
        c.a = transparenceCD;
        image.color = c;
        */
        cdMask.SetActive(true);

        // Pour modifier le text
        Text t = ((GameObject)parms[0]).GetComponentInChildren<Text>();

        // Affiche le décompte
        while (dureeCD > 0)
        {
            dureeCD -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
            t.text = (Mathf.Floor(dureeCD) + 1).ToString();
        }
        /*
        // On remet la transparence normale
        c.a = 255;
        image.color = c;
        */
        cdMask.SetActive(false);
        t.text = "";
    }


    // AUTRES METHODES

    // TP méthode
    private void Tp(GameObject target, int ourTeam)
    {
        if (PhotonNetwork.connected == true)
        {
            this.GetComponent<PlayerController>().photonView.RPC("SetPosition", PhotonTargets.All, target.transform.position - target.transform.forward);
        }
        else
        {
            this.transform.position = target.transform.position - target.transform.forward;
            this.transform.forward = target.transform.forward;
        }
        //On maudit la cible si c'est un ennemi
        //hitPlayerControllerScript = script de la cible
        PlayerController hitPlayerControllerScript =target.GetComponent<PlayerController>();
        if (hitPlayerControllerScript.team != ourTeam)
        {
            hitPlayerControllerScript.photonView.RPC("Curse",PhotonTargets.All);
        }

        // Lance l'animation d'apparition
        this.photonView.RPC("LanceAnimTP", PhotonTargets.All);
    }


    //Obliger de faire plusieurs méthodes pour lancer les animations car on ne peut pas passer de GameObject en argument d'un RPC

    // Lance l'animation de disparition
    [PunRPC]
    private void LanceAnimTP()
    {
        GameObject effetA;
        effetA = Instantiate(animTP, this.transform.position + animTP.transform.position, animTP.transform.rotation).gameObject as GameObject;
    }


    // Désactive/Active l'utilisation des comps (pour le stun)
    public void SetCompActives(bool b)
    {
        this.compUndeathActif = b;
    }

    public bool IsInInvunerabilityMode
    {
        get
        {
            return this.isInInvunerabilityMode;
        }
        set
        {
            this.isInInvunerabilityMode = value;
        }
    }

    // Reset le perso (si meurt)
    private void OnDisable()
    {
        /*
        // On remet la transparence normale de l'affichage
        Image image = invulnerableHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = 255;
        image.color = c;

        image = dotHUD.GetComponent<Image>();
        image.color = c;
        image = tpHUD.GetComponent<Image>();
        image.color = c;
        */
        if (photonView.isMine)
        {
            invulnerableHUD.transform.Find("CooldownGreyMask").gameObject.SetActive(false);
            tpHUD.transform.Find("CooldownGreyMask").gameObject.SetActive(false);
            dotHUD.transform.Find("CooldownGreyMask").gameObject.SetActive(false);

            // On remet l'affichage du cooldown à rien (pas de CD)
            Text t = invulnerableHUD.GetComponentInChildren<Text>();
            t.text = "";
            t = dotHUD.GetComponentInChildren<Text>();
            t.text = "";
            t = tpHUD.GetComponentInChildren<Text>();
            t.text = "";
        }

        // On reset les CD
        dotLastUse = 0;
        invulnerableLastUse = 0;
        tpLastUse = 0;

    }

}
