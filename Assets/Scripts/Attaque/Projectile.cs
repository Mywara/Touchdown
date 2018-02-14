using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPunObservable {

    public float impactDamage = 10;
    public float splashDamage = 0;
    public float AOERadius = 0;
    public float speed = 10;
    public string team;

    private Rigidbody myRb;
    private bool netWorkingDone = false;

    // Use this for initialization
    void Start () {
        myRb = this.gameObject.GetComponent<Rigidbody>();
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

        PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
        if (healthScript2 != null)
        {
            healthScript2.Damage2(damage);
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(!netWorkingDone)
        {
            if (stream.isWriting)
            {
                stream.SendNext(this.impactDamage);
                stream.SendNext(this.splashDamage);
                stream.SendNext(this.AOERadius);
                stream.SendNext(this.speed);
                stream.SendNext(this.team);
            }
            else
            {
                this.impactDamage = (float)stream.ReceiveNext();
                this.splashDamage = (float)stream.ReceiveNext();
                this.AOERadius = (float)stream.ReceiveNext();
                this.speed = (float)stream.ReceiveNext();
                this.team = (string)stream.ReceiveNext();

                //Reset the velocity of the projectile
                if (myRb != null)
                {
                    myRb.velocity = transform.forward * speed;
                }
            }
            netWorkingDone = true;
        }
    }
}
