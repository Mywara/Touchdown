using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttaqueRanged : MonoBehaviour {

    public float impactDamage = 10;
    public float splashDamage = 0;
    public float AOERadius = 0;
    public float fireRate = 1;
    public float projectileSpeed = 10;
    public GameObject projectilePrefab;
    public Transform projectileSpawn;

    private float nextFire;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            GameObject projo = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation).gameObject as GameObject;
            Projectile projectileScript = projo.GetComponent<Projectile>();
            if(projectileScript != null)
            {
                projectileScript.SetDamage(impactDamage, splashDamage);
                projectileScript.SetSpeed(projectileSpeed);
                //dire de quelle equipe vient le projectile pour ne pas TK
                //projectileScript.SetTeam();
                projectileScript.SetAOERadius(AOERadius);
            }
            else
            {
                Debug.Log("Projectile missing 'Projectile' script");
            }
        }
    }
}
