using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttaqueRanged : Photon.PunBehaviour {

    public int impactDamage = 10;
    public int splashDamage = 0;
    public float AOERadius = 0;
    public float fireRate = 1;
    public float projectileSpeed = 10;
    public GameObject projectilePrefab;
    public Transform projectileSpawn;

    public GameObject AnimMuzzle;
    /*
    public float offset_tir_vertical = 12;
    public float offset_tir_horizontal = 23;
    */
    public bool inModePlacing = false;
    public bool hasAnAOE = false;
    // test
    public GameObject sourceObject;
    //

    private float nextFire = 0f;
    private Animator anim;
    private bool tirActif = true;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true || !tirActif)
        {
            return;
        }
        if (Input.GetButton("Fire1") && Time.time > nextFire && !inModePlacing)
        {
            nextFire = Time.time + fireRate;

            //animation trigger
            if (PhotonNetwork.connected)
                photonView.RPC("ShotgunAnimation", PhotonTargets.All);
            else
                ShotgunAnimation();

            // Lance le muzzle du fusil
            this.photonView.RPC("LanceAnimMuzzle", PhotonTargets.All);

            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                //projo = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.Euler(rotationVector)).gameObject as GameObject;
                projo = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation).gameObject as GameObject;
            }
            else
            {
                //Pour le reseau
                //projo = PhotonNetwork.Instantiate(this.projectilePrefab.name, projectileSpawn.position, Quaternion.Euler(rotationVector), 0);
                projo = PhotonNetwork.Instantiate(this.projectilePrefab.name, projectileSpawn.position, projectileSpawn.rotation, 0);
            }

            Projectile projectileScript = projo.GetComponent<Projectile>();
            if (projectileScript)
            {
                projectileScript.SetAutoAttack();
                projectileScript.SetDamage(impactDamage, splashDamage);
                projectileScript.SetSender(gameObject);
                projectileScript.AOEActivated = hasAnAOE;
                projectileScript.SetSpeed(projectileSpeed);
                //dire de quelle equipe vient le projectile pour ne pas TK
                PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
                if (playerControllerScript != null)
                {
                    projectileScript.SetTeam(playerControllerScript.Team);
                }
                else
                {
                    Debug.Log("player have no PlayerController script");
                }

                projectileScript.SetAOERadius(AOERadius);
            }
            else
            {
                Debug.Log("Projectile missing 'Projectile' script");
            }
        }
    }

    [PunRPC]
    private void ShotgunAnimation()
    {
        anim.SetTrigger("AttackGun");
    }

    IEnumerator DelayedGunShot()
    {
        yield return new WaitForSeconds(0f);
        
        GameObject projo;
        //Pour le local
        if (PhotonNetwork.connected == false)
        {
            //projo = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.Euler(rotationVector)).gameObject as GameObject;
            projo = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation).gameObject as GameObject;
        }
        else
        {
            //Pour le reseau
            //projo = PhotonNetwork.Instantiate(this.projectilePrefab.name, projectileSpawn.position, Quaternion.Euler(rotationVector), 0);
            projo = PhotonNetwork.Instantiate(this.projectilePrefab.name, projectileSpawn.position, projectileSpawn.rotation, 0);
        }

        Projectile projectileScript = projo.GetComponent<Projectile>();
        if (projectileScript)
        {
            projectileScript.SetAutoAttack();
            projectileScript.SetDamage(impactDamage, splashDamage);
            projectileScript.SetSender(gameObject);
            projectileScript.AOEActivated = hasAnAOE;
            projectileScript.SetSpeed(projectileSpeed);
            //dire de quelle equipe vient le projectile pour ne pas TK
            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            if (playerControllerScript != null)
            {
                projectileScript.SetTeam(playerControllerScript.Team);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }

            projectileScript.SetAOERadius(AOERadius);
        }
        else
        {
            Debug.Log("Projectile missing 'Projectile' script");
        }
    }

    public void SetTirActif(bool b)
    {
        this.tirActif = b;
    }

    [PunRPC]
    private void LanceAnimMuzzle()
    {
        GameObject effetMuzzle;
        //Pour le local
        if (PhotonNetwork.connected == false)
        {

            effetMuzzle = Instantiate(AnimMuzzle, projectileSpawn.transform.position, AnimMuzzle.transform.rotation).gameObject as GameObject;
        }
        else
        {
            //Pour le reseau
            effetMuzzle = PhotonNetwork.Instantiate(this.AnimMuzzle.name, projectileSpawn.transform.position, AnimMuzzle.transform.rotation, 0);
        }
    }
}
