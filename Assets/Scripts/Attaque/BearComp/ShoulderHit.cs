using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//attaque large peu profonde
//repousse les ennemis et fait peu de dégâts
public class ShoulderHit : Photon.PunBehaviour
{

    private int damage;
    private float fireRate = 1;
    private int team;
    private float stunTimer;

    private bool netWorkingDone = false;
    private float nextFire = 0f;
    private List<GameObject> directHitObjs = new List<GameObject>();
    private bool syncTeam = true;
    private GameObject owner;

    private GameObject crystal;

    public void Start()
    {
        damage = Constants.SHOULDER_DAMAGE;
        stunTimer = Constants.SHOULDER_STUN_TIMER;
    }

    public void SetTeam(int newTeam)
    {
        this.team = newTeam;
    }

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
        crystal = owner.GetComponent<CrystalDrop>().crys;
    }

    //fonction pour le repoussement des ennemis
    public void repulse(GameObject target)
    {
        //direction de la cible
        Vector3 targetDir = target.transform.position - transform.position;

        if (PhotonNetwork.connected == true)
        {
            target.GetComponent<PlayerController>().photonView.RPC("AddForceTo", PhotonTargets.All, targetDir.normalized * 4000);
        }
        else
        {
            target.GetComponent<Rigidbody>().AddForce(targetDir.normalized * 4000);
        }
    }

    //AJOUTER LE DROP DU FLAG SI PORTEUR !!!
    public IEnumerator StunIfCollide(GameObject target)
    {
        Debug.Log("Entrée dans le StunIfCollide");
        if (target.GetComponent<PlayerController>().onCollision == false)
        {
            Debug.Log("RETOUR ?");
            yield return null;
        }

        Debug.Log("Collision detectée !");
        if (PhotonNetwork.connected == true)
        {
            target.GetComponent<PlayerController>().photonView.RPC("Stun", PhotonTargets.All, stunTimer);

            if (crystal.GetComponent<Crystal>().playerHolding == target.transform.root.gameObject)
            {
                crystal.GetComponent<Crystal>().photonView.RPC("UpdateJustDroppedCrystal", PhotonTargets.All);

                crystal.GetComponent<Crystal>().photonView.RPC("LeaveOnGround", PhotonTargets.All);
            }
        }
        else
        {
            // prive translation, rotation du perso et compétences du perso
            target.GetComponent<PlayerController>().SetMobile(false);
            target.GetComponent<PlayerController>().SetActiveCompetence(false);

            // Attend la duree demandé
            yield return new WaitForSeconds(stunTimer);

            // autorise translation, rotation du perso et compétences du perso
            target.GetComponent<PlayerController>().SetMobile(true);
            target.GetComponent<PlayerController>().SetActiveCompetence(true);
        }
    }

    //Ici other = l'object que l'on a touché
    //Modif a faire, limiter dmg au cible valide -> layer + test
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entrée dans le OnTriggerStay GS");
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            Debug.Log("PhotonView");
            return;
        }
        GameObject otherGO = other.transform.root.gameObject;
        if (otherGO.tag.Equals("Respawn") || otherGO.tag.Equals("Boundary"))
        {
            Debug.Log("In Respawn");
            return;
        }

        if (!RoomManager.instance.FriendlyFire)
        {
            Debug.Log("Room Manager");
            if (otherGO.tag.Equals("Player") && otherGO != owner)
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

        if (!directHitObjs.Contains(otherGO) && otherGO.tag.Equals("Player") && otherGO.GetComponent<PlayerController>().team != team)
        {
            directHitObjs.Add(otherGO);
        }
    }

    private void Update()
    {
        if (directHitObjs.Count == 0) { PhotonNetwork.Destroy(this.gameObject); return; }

        foreach (GameObject directHitObj in directHitObjs.ToArray())
        {
            ApplyDamage(directHitObj, damage);
            repulse(directHitObj);
            //Méthode pour le stun et drop de flag en cas de collisions
            Debug.Log("Avant le stun");
            StartCoroutine(StunIfCollide(directHitObj));
            Debug.Log("Après le stun");
            directHitObjs.Remove(directHitObj);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        directHitObjs.Remove(other.transform.root.gameObject);
        PhotonNetwork.Destroy(this.gameObject);
    }

    private void ApplyDamage(GameObject target, int damage)
    {
        if (PhotonNetwork.connected == true)
        {
            PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
            if (healthScript != null)
            {
                //healthScript.Damage(damage);
                healthScript.photonView.RPC("Damage", PhotonTargets.All, damage);

            }

            PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
            if (healthScript2 != null)
            {
                //healthScript2.Damage2(damage);
                healthScript2.photonView.RPC("Damage2", PhotonTargets.All, damage);
            }
        }
        else
        {
            target.GetComponent<PUNTutorial.HealthScript>().Damage(damage);
        }
    }
}
