using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CacHitZone : MonoBehaviour, IPunObservable {

    public float damage = 10;
    public float fireRate = 1;
    public string team;

    private bool netWorkingDone = false;
    private float nextFire = 0f;
    private List<GameObject> directHitObjs = new List<GameObject>();
    // Use this for initialization
    void Start()
    {

    }

    public void SetDamage(float newdamage)
    {
        this.damage = newdamage;
    }

    public void SetFireRate(float newFireRate)
    {
        this.fireRate = newFireRate;
    }

    public void SetTeam(string newTeam)
    {
        this.team = newTeam;
    }

    //Ici other = l'object que l'on a touché
    //Modif a faire, limiter dmg au cible valide -> layer + test
    private void OnTriggerStay(Collider other)
    {
        GameObject otherGO = other.transform.root.gameObject;
        if (!directHitObjs.Contains(otherGO) && otherGO.GetComponent<PUNTutorial.HealthScript>() != null)
        {
            directHitObjs.Add(otherGO);
        }
        /*
        if (Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            
            GameObject directHitObj = other.transform.root.gameObject;
            Debug.Log("Obj in CacHitZone : " + directHitObj.name);
            ApplyDamage(directHitObj, damage);
             
        }
        */
    }

    private void Update()
    {
        if (Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            foreach (GameObject directHitObj in directHitObjs.ToArray())
            {
                Debug.Log("Obj in CacHitZone : " + directHitObj.name);
                ApplyDamage(directHitObj, damage);
                directHitObjs.Remove(directHitObj);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        directHitObjs.Remove(other.transform.root.gameObject);
    }

    private void OnDisable()
    {
        Debug.Log("CacHitZone disable");
        directHitObjs.Clear();
    }

    private void ApplyDamage(GameObject target, float damage)
    {
        PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
        if (healthScript != null)
        {
            healthScript.Damage(damage);
            Debug.Log("Damage : " + damage + " deals to : " + target.name);
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!netWorkingDone)
        {
            if (stream.isWriting)
            {
                stream.SendNext(this.damage);
                stream.SendNext(this.fireRate);
                stream.SendNext(this.team);
            }
            else
            {
                this.damage = (float)stream.ReceiveNext();
                this.fireRate = (float)stream.ReceiveNext();
                this.team = (string)stream.ReceiveNext();
            }
            netWorkingDone = true;
        }
    }
}
