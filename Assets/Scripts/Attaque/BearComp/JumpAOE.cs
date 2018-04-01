using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAOE : Photon.PunBehaviour
{

    public int damage = 10;
    public int team;
    public float ennemiesJump = 0.2f; // fait décoller les ennemis du sol à l'atterrissage

    public float pourcentageRalenti;
    public float dureeRalenti;

    private List<GameObject> directHitObjs = new List<GameObject>(); // Pour lister les joueurs touchés
    private bool apply; // Détermine si on peut appliquer les dommages ou pas (on ne les appliquent qu'une fois par utilisation de la competence)

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (apply)
        {
            
            foreach (GameObject directHitObj in directHitObjs.ToArray())
            {
                ApplyDamage(directHitObj, damage);
                directHitObjs.Remove(directHitObj);
            }

            apply = false;
            directHitObjs.Clear();
            this.transform.gameObject.SetActive(false);
            Debug.Log("end Jump skill");
        }

        

    }

    public void SetDamage(int newdamage)
    {
        this.damage = newdamage;
    }

    public void SetTeam(int newTeam)
    {
        this.team = newTeam;
    }

    public void SetApply(bool newApply)
    {
        this.apply = newApply;
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

    private void OnTriggerExit(Collider other)
    {
        directHitObjs.Remove(other.transform.root.gameObject);
    }


    // Applique les dommages
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



        PlayerController playerControllerScript = target.GetComponent<PlayerController>();

        if (playerControllerScript != null)
        {
            // Fait sauter les ennemis touchés
            playerControllerScript.photonView.RPC("PetitSaut", PhotonTargets.All, ennemiesJump);

            // Ralenti les ennemis touchés
            playerControllerScript.photonView.RPC("ModificationVitesse", PhotonTargets.All, pourcentageRalenti, dureeRalenti);
        }
    }

    // Méthode pour calculer la velocité à donner pour atteindre la hauteur donnée
    public static float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2f * targetJumpHeight * -Physics.gravity.y);
    }

    private void OnDisable()
    {
        directHitObjs.Clear();
    }

}
