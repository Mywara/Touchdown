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

    public GameObject AnimSlash;

    // Passives
    private BearPassive bearPassive;
    private UndeadPassive undeadPassive;


    void Awake()
    {
        bearPassive = gameObject.GetComponentInParent<BearPassive>();
        undeadPassive = gameObject.GetComponentInParent<UndeadPassive>();

        if(bearPassive)
        {
            //Debug.Log("The owner of this CaC hit zone is a Warbear!");
        }
        else
        {
            //Debug.Log("The owner of this CaC hit zone is not a Warbear.");
        }

        if (undeadPassive)
        {
            //Debug.Log("The owner of this CaC hit zone is an Undead!");
        }
        else
        {
            //Debug.Log("The owner of this CaC hit zone is not an Undead.");
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
                        //Debug.Log("Friend hit, not FF, do nothing");
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
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        if (Time.time > nextFire)
        {
            // Lance l'animation slash
            GameObject effetSlash;

            // On cherche l'orientation de l'effet
            var rotationVector = Camera.main.transform.rotation.eulerAngles;
            float rx = rotationVector.x -15f;

            // si on regarde vers le haut les valeurs partent de 359 et diminu plus on regarde haut
            // sinon elle partent de 0 et augmentent 
            if (rx > 180)
            {
                rx = rx - 360;
            }
            rotationVector = transform.parent.rotation.eulerAngles;
            rotationVector.x += rx - 90;

            // On cherche la position de l'effet
            var positionVector = this.transform.parent.position + this.transform.parent.up.normalized * 0.4f + this.transform.parent.forward.normalized * 0.4f;


            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                effetSlash = Instantiate(AnimSlash, positionVector, Quaternion.Euler(rotationVector)).gameObject as GameObject;
            }
            else
            {
                //Pour le reseau
                effetSlash = PhotonNetwork.Instantiate(this.AnimSlash.name, positionVector, Quaternion.Euler(rotationVector), 0);
            }

            effetSlash.transform.parent = this.transform.parent;

            nextFire = Time.time + fireRate;

            if(bearPassive && directHitObjs.Count > 0)
            {
                bearPassive.IncrementHitStack(directHitObjs);
            }

            foreach (GameObject directHitObj in directHitObjs.ToArray())
            {
                
                ApplyDamage(directHitObj, damage);

                if (undeadPassive)
                {
                    PlayerController hitObjController = directHitObj.GetComponent<PlayerController>();
                    if (hitObjController && hitObjController.Cursed())
                    {
                        //Debug.Log("Undead passive : retrieving " + Mathf.RoundToInt(damage * Constants.UNDEAD_LEECHLIFE_RATE) + "HP from " + directHitObj.name);
                        LeechLife(Mathf.RoundToInt(damage * Constants.UNDEAD_LEECHLIFE_RATE));
                    }
                }

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
        directHitObjs.Clear();
    }

    private void ApplyDamage(GameObject target, int damage)
    {
        PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
        if (healthScript)
        {
            //healthScript.Damage(damage);
            healthScript.photonView.RPC("Damage", PhotonTargets.All, damage);
            //Debug.Log("Damage : " + damage + " deals to : " + target.name);
        }

        PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
        if (healthScript2)
        {
            //healthScript2.Damage2(damage);
            healthScript2.photonView.RPC("Damage2", PhotonTargets.All, damage);
        }
    }

    private void LeechLife(int life)
    {
        PUNTutorial.HealthScript healthScript = transform.root.gameObject.GetComponent<PUNTutorial.HealthScript>();
        if (healthScript)
        {
            //healthScript.Damage(damage);
            healthScript.photonView.RPC("Heal", PhotonTargets.All, life);
            //Debug.Log("Leech life : " + life + " deals to : " + gameObject.name);
        }

        PUNTutorial.HealthScript2 healthScript2 = transform.root.gameObject.GetComponent<PUNTutorial.HealthScript2>();
        if (healthScript2)
        {
            //healthScript2.Damage2(damage);
            healthScript2.photonView.RPC("Heal2", PhotonTargets.All, life);
        }
    }
}
