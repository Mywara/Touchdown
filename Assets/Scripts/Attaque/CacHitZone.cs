using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CacHitZone : Photon.PunBehaviour/*, IPunObservable*/ {

    public int damage = 10;
    public float fireRate = 1;
    public int team;

    private bool netWorkingDone = false;
    private float nextFire = 0f;
    private List<GameObject> directHitObjs = new List<GameObject>();
    private bool syncTeam = true;

    // Use this for initialization
    void Start()
    {

    }

    public void SetDamage(int newdamage)
    {
        this.damage = newdamage;
    }

    public void SetFireRate(float newFireRate)
    {
        this.fireRate = newFireRate;
    }

    public void SetTeam(int newTeam)
    {
        this.team = newTeam;
    }

    //Ici other = l'object que l'on a touché
    //Modif a faire, limiter dmg au cible valide -> layer + test
    private void OnTriggerStay(Collider other)
    {
        if (!photonView.isMine)
        {
            return;
        }
        GameObject otherGO = other.transform.root.gameObject;
        if (otherGO.tag.Equals("Respawn") || otherGO.tag.Equals("Boundary"))
        {
            return;
        }
        if (!RoomManager.instance.FriendlyFire)
        {
            if (otherGO.tag.Equals("Player"))
            {
                PlayerController playerControllerScript = otherGO.GetComponent<PlayerController>();
                if (playerControllerScript != null)
                {
                    if (playerControllerScript.Team == this.team)
                    {
                        Debug.Log("Friend hit, not FF, do nothing");
                        return;
                    }
                }
            }
        }
        if (!directHitObjs.Contains(otherGO) && otherGO.tag.Equals("Player"))
        {
            directHitObjs.Add(otherGO);
        }
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

    private void ApplyDamage(GameObject target, int damage)
    {
        PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
        if (healthScript != null)
        {
            //healthScript.Damage(damage);
            healthScript.photonView.RPC("Damage", PhotonTargets.All, damage);
            Debug.Log("Damage : " + damage + " deals to : " + target.name);
        }
    }
}
