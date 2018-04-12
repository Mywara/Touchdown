using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhumSpray : Photon.PUNBehaviour {

    public int splashHeal = 30;
    public float AOERadius = 2;
    public float projectileSpeed = 10;
    public float dureeConfusion = 3F;
    public float pourcentageConfusion = -100;
    public int team;
    public float maxAnglularVelocity = 10;
    public float verticalVelocity = 10;
    public GameObject animBouteille;
    private GameObject owner;

    private Rigidbody myRb;


    // Use this for initialization
    void Start()
    {
        myRb = this.gameObject.GetComponent<Rigidbody>();
        if (myRb != null)
        {
            //On lance l'object vers l'avant (vers où on vise) avec une certaine vitesse
            myRb.velocity = transform.forward * projectileSpeed + transform.up * verticalVelocity;
            myRb.angularVelocity = Random.insideUnitSphere * maxAnglularVelocity;
        }
        else
        {
            Debug.Log("error, no rigidbody on the projectile");
        }
    }

    public void SetTeam(int newTeam)
    {
        this.team = newTeam;
    }
    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }

    //Ici other = l'object que l'on a touché
    //Modif a faire, limiter dmg au cible valide -> layer + test
    private void OnTriggerEnter(Collider other)
    {
        //Si l'objet n'est pas controlé localement quand on est connecté, on ne fait rien
        //Donc c'est le possesseur de l'objet qui detectera les collisions et notifira les degats
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        //on recupère l'object le plus haut de hierachie sur l'objet touché
        GameObject directHitObj = other.transform.root.gameObject;
        //On enlève les collisions pour appliquer des dégâts avec le respawn et la bordure
        if (directHitObj.tag.Equals("Respawn") || directHitObj.tag.Equals("Boundary") || directHitObj == owner)
        {
            return;
        }

        // On lance l'animation
        this.photonView.RPC("LanceAnimRhum", PhotonTargets.All);

        //on recupère les colliders dans la zone d'AOE
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, AOERadius);
        //On parcours les objets dans l'AOE
        foreach (Collider colliITE in hitColliders)
        {
            GameObject objInAOE = colliITE.transform.root.gameObject;
            //Si c'est un joueur, on peut continuer
            if (objInAOE.tag.Equals("Player"))
            {
                PlayerController playerControllerScript = objInAOE.GetComponent<PlayerController>();
                if (playerControllerScript != null)
                {
                    //On test si la cible est de notre cible ou non
                    if (playerControllerScript.Team == this.team)
                    {
                        //Debug.Log("Friend hit, apply heal");
                        ApplyHeal(objInAOE, splashHeal);
                    }
                    //On applique l'inversion des touches de directions
                    ApplyConfusion(objInAOE, pourcentageConfusion, dureeConfusion);
                }
            }
        }
        //On detruit le projectile apres l'impacte
        PhotonNetwork.Destroy(this.gameObject);
    }

    //Fonction pour appliquer les heals sur un player
    private void ApplyHeal(GameObject target, int heal)
    {
        PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
        if (healthScript != null)
        {

            healthScript.photonView.RPC("Heal", PhotonTargets.All, heal);
            //Debug.Log("Heal : " + heal + " done to : " + target.name);
        }

        PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
        if (healthScript2 != null)
        {
            healthScript2.photonView.RPC("Heal2", PhotonTargets.All, heal);
        }
    }

    //Applique la confusion (inversion des touches z et s / q et d sur la cible
    private void ApplyConfusion(GameObject target, float pourcentageRalenti, float dureeRalenti)
    {
        PlayerController playerControllerScript = target.GetComponent<PlayerController>();
        if (playerControllerScript != null)
        {
            //On donne une vitesse de -100% donc ca inverse les directions
            playerControllerScript.photonView.RPC("ModificationVitesse", PhotonTargets.All, pourcentageRalenti, dureeRalenti);
        }
        
    }

    [PunRPC]
    private void LanceAnimRhum()
    {
        GameObject effetA;
        effetA = Instantiate(animBouteille, this.transform.position, animBouteille.transform.rotation).gameObject as GameObject;
    }
}
