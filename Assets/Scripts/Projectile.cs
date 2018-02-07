using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public float impactDamage = 10;
    public float splashDamage = 0;
    public float AOERadius = 0;
    public float speed = 10;
    public string team;

	// Use this for initialization
	void Start () {
        Rigidbody myRb = this.gameObject.GetComponent<Rigidbody>();
        if(myRb != null)
        {
            myRb.velocity = transform.forward * speed;
        }
        else
        {
            Debug.Log("error, no rigidbody on the projectile");
        }
	}

    public void SetImpactDamage(float newdamage)
    {
        this.impactDamage = newdamage;
    }

    public void SetSplashDamage(float newdamage)
    {
        this.speed = newdamage;
    }

    public void SetDamage(float newImpactdamage, float newSplashdamage)
    {
        this.impactDamage = newImpactdamage;
        this.splashDamage = newSplashdamage;
    }

    public void SetSpeed(float newSpeed)
    {
        this.speed = newSpeed;
    }

    public void SetAOERadius(float newRadius)
    {
        this.AOERadius = newRadius;
    }

    public void SetTeam(string newTeam)
    {
        this.team = newTeam;
    }

    //Ici other = l'object que l'on a touché
    //Modif a faire, limiter dmg au cible valide -> layer + test
    private void OnTriggerEnter(Collider other)
    {
        GameObject directHitObj = other.transform.root.gameObject;
        Debug.Log("Direct hit on object : " + directHitObj.name);
        ApplyDamage(directHitObj, impactDamage);
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, AOERadius);
        foreach(Collider colliITE in hitColliders)
        {
            GameObject objInAOE = colliITE.transform.root.gameObject;
            Debug.Log("AOE hits object : " + objInAOE.name);
            if (objInAOE != directHitObj)
            {
                ApplyDamage(objInAOE, splashDamage);
            }
        }
        Destroy(this.gameObject);
    }

    private void ApplyDamage(GameObject target, float damage)
    {
        PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
        if(healthScript!=null)
        {
            healthScript.Damage(damage);
            Debug.Log("Damage : " + damage +" deals to : " + target.name);
        }
    }
}
