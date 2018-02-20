 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//attaque large peu profonde
//repousse les ennemis et fait peu de dégâts
public class GrapeShot : Photon.PunBehaviour
{

    private int damage;
    private float fireRate = 1;
    public int team;

    private bool netWorkingDone = false;
    private float nextFire = 0f;
    private List<GameObject> directHitObjs = new List<GameObject>();
    private bool syncTeam = true;

    //the radius of the circular zone
    //= depth of the spell
    private float radius;
    //the angle of the circular zone
    private float angle;

    public void Start()
    {
        damage = Constants.GRAPE_DAMAGE;
        radius = Constants.GRAPE_RADIUS;
        angle  = Constants.GRAPE_ANGLE;
    }

    //fonction pour le repoussement des ennemis
    public void repulse(GameObject target)
    {
        //direction de la cible
        Vector3 targetDir = target.transform.position - transform.position;
        //angle entre la cible et le point de visée du joueur
        float angle = Vector3.Angle(targetDir, transform.forward);

        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

        target.transform.SetPositionAndRotation(transform.position + q * Vector3.right * radius, q);
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
                repulse(directHitObj);
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
