using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAOE :  Photon.PunBehaviour {

    public int damage = 10;
    public int team;
    public float ennemiesJump = 0.2f; // fait décoller les ennemis du sol à l'atterrissage

    private List<GameObject> directHitObjs = new List<GameObject>(); // Pour lister les joueurs touchés
    private bool apply; // Détermine si on peut appliquer les dommages ou pas (on ne les appliquent qu'une fois par utilisation de la competence)

    // Use this for initialization
    void Start () {
        apply = false;
	}
	
	// Update is called once per frame
	void Update () {
        if(apply){
            foreach (GameObject directHitObj in directHitObjs.ToArray())
            {
                Debug.Log("Obj in CacHitZone : " + directHitObj.name);
                ApplyDamage(directHitObj, damage);
                directHitObjs.Remove(directHitObj);
            }

            apply = false;
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

        // Fait sauter les ennemis touchés
        Rigidbody rb;
        rb = target.GetComponent<Rigidbody>();
        Vector2 velocity = rb.velocity;
        velocity.y = CalculateJumpVerticalSpeed(ennemiesJump);
        rb.velocity = velocity;
    }

    // Méthode pour calculer la velocité à donner pour atteindre la hauteur donnée
    public static float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2f * targetJumpHeight * -Physics.gravity.y);
    }

}
