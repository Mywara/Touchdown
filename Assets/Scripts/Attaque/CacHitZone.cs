using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CacHitZone : Photon.PunBehaviour {

    public int damage = 10;
    public float fireRate = 1;
    public int team;

    private bool netWorkingDone = false;
    private float nextFire = 0f;
    private List<GameObject> directHitObjs = new List<GameObject>();
    private bool syncTeam = true;

    // Passives
    private BearPassive bearPassive;
    private UndeadPassive undeadPassive;


    void Awake()
    {
        bearPassive = gameObject.GetComponentInParent<BearPassive>();
        undeadPassive = gameObject.GetComponentInParent<UndeadPassive>();

        if(bearPassive)
        {
            Debug.Log("The owner of this CaC hit zone is a Warbear!");
        }
        else
        {
            Debug.Log("The owner of this CaC hit zone is not a Warbear.");
        }

        if (undeadPassive)
        {
            Debug.Log("The owner of this CaC hit zone is an Undead!");
        }
        else
        {
            Debug.Log("The owner of this CaC hit zone is not an Undead.");
        }
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
        if (!photonView.isMine && PhotonNetwork.connected == true)
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

            if(bearPassive && directHitObjs.Count > 0)
            {
                bearPassive.IncrementHitStack(directHitObjs);
            }

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

        PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
        if (healthScript2 != null)
        {
            //healthScript2.Damage2(damage);
            healthScript2.photonView.RPC("Damage2", PhotonTargets.All, damage);
        }
    }
}
