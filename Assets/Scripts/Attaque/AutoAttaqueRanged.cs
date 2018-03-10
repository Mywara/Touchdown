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
    public float offset_tir_vertical = 12;
    public float offset_tir_horizontal = 23;
    public float gunShotDelay = .5f;
    public bool inModePlacing = false;
    public bool hasAnAOE = false;
    // test
    public GameObject sourceObject;
    //

    private float nextFire = 0f;
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        if (Input.GetButton("Fire1") && Time.time > nextFire && !inModePlacing)
        {
            // animation trigger
            anim.SetTrigger("AttackGun");

            nextFire = Time.time + fireRate;

            StartCoroutine("DelayedGunShot");
        }
    }

    IEnumerator DelayedGunShot()
    {
        yield return new WaitForSeconds(gunShotDelay);

        Camera cam = Camera.main;
        RaycastHit hit;
        GameObject projo;
        float distance; // la distance d'un point près du personnage jusqu'à la cible

        if (Physics.Raycast(cam.transform.position + cam.transform.forward * 2.5f, cam.transform.forward, out hit, 500))
        {
            //Debug.Log(hit.collider.name);
            //Debug.Log(hit.distance);
            //Instantiate(sourceObject, hit.point, Quaternion.identity);

            distance = hit.distance;
        }
        else
        {
            distance = 30;
        }

        Vector3 rotationVector = cam.transform.rotation.eulerAngles;
        rotationVector.y += (1 / distance) * offset_tir_horizontal; // axe horizontal de visée 
        rotationVector.x -= (1 / distance) * offset_tir_vertical; // axe vertical de visée

        //Pour le local
        if (PhotonNetwork.connected == false)
        {
            projo = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.Euler(rotationVector)).gameObject as GameObject;
        }
        else
        {
            //Pour le reseau
            projo = PhotonNetwork.Instantiate(this.projectilePrefab.name, projectileSpawn.position, Quaternion.Euler(rotationVector), 0);
        }

        Projectile projectileScript = projo.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.AOEActivated = hasAnAOE;
            projectileScript.SetDamage(impactDamage, splashDamage);
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
