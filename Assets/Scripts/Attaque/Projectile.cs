using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Photon.PunBehaviour{

    public int impactDamage = 10;
    public int splashDamage = 0;
    public float AOERadius = 0;
    public float speed = 10;
    public int team;
    public bool aoeActivated = false;
    public GameObject AnimBulletExplode;
    private bool animLance = false;

    private Rigidbody myRb;
    private GameObject sender;
    private PiratePassive piratePassive;
    private bool autoAttack = false;
    private bool hasHitEnemies = false;
    

    // Use this for initialization
    void Start ()
    {
        myRb = this.gameObject.GetComponent<Rigidbody>();
        if(myRb != null)
        {
            myRb.velocity = transform.forward * speed;
        }
        else
        {
            Debug.Log("error, no rigidbody on the projectile");
        }
        
        if(sender)
        {
            piratePassive = sender.GetComponent<PiratePassive>();
            if(piratePassive)
            {
                //Debug.Log("The sender of this projectile is a Pirate!");
            }
            else
            {
                //Debug.Log("The sender of this projectile is not a Pirate.");
            }
        }
	}

    public void SetImpactDamage(int newdamage)
    {
        this.impactDamage = newdamage;
    }

    public void SetSplashDamage(int newdamage)
    {
        this.splashDamage = newdamage;
    }

    public void SetDamage(int newImpactdamage, int newSplashdamage)
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

    public void SetTeam(int newTeam)
    {
        this.team = newTeam;
    }

    public void SetSender(GameObject sender)
    {
        this.sender = sender;
    }

    public void SetAutoAttack()
    {
        this.autoAttack = true;
    }

    //Ici other = l'object que l'on a touché
    //Modif a faire, limiter dmg au cible valide -> layer + test
    private void OnTriggerEnter(Collider other)
    {
        //Si l'objet n'est pas controlé localement quand on est connecté, on ne fait rien
        //Donc c'est le possesseur de l'objet qui detectera les collisions et notifira les degats
        if(!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        //on recupère l'object le plus haut de hierachie sur l'objet touché
        GameObject directHitObj = other.transform.root.gameObject;
        //On enlève les collisions pour appliquer des dégâts avec le respawn et la bordure
        if (directHitObj.tag.Equals("Respawn") || directHitObj.tag.Equals("Boundary") || other.tag.Equals("GoalD") || other.tag.Equals("GoalG"))
        {
            return;
        }


        if (!animLance)
        {
            LanceAnim();
            animLance = true; // On ne lance l'animation qu'une fois
        }

        //on test si il a a du friendlyFire ou non
        if (!RoomManager.instance.FriendlyFire)
        {
            if (directHitObj.tag.Equals("Player"))
            {
                PlayerController playerControllerScript = directHitObj.GetComponent<PlayerController>();
                if(playerControllerScript != null)
                {
                    if(playerControllerScript.Team == this.team)
                    {
                        //Debug.Log("Friend hit, not FF, do nothing");
                        return;
                    }
                }
            }
        }
        
        if(autoAttack && piratePassive && piratePassive.PendingCriticalHit())
        {
            impactDamage = Mathf.RoundToInt(impactDamage * Constants.PIRATE_CRITICAL_RATE);
            splashDamage = Mathf.RoundToInt(splashDamage * Constants.PIRATE_CRITICAL_RATE);
        }

        //On applique des dégâts direct avec le projectile
        if (directHitObj.tag.Equals("Player"))
        {
            ApplyDamage(directHitObj, impactDamage);
            hasHitEnemies = true;
        }
        
        //Si le projectile a de l'AOE
        if(aoeActivated)
        {
            //on recupère les colliders dans la zone d'AOE
            Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, AOERadius);
            //On parcours les objets dans l'AOE
            foreach (Collider colliITE in hitColliders)
            {
                GameObject objInAOE = colliITE.transform.root.gameObject;
                //Si c'est un joueur, on lui applique les degats
                if (objInAOE.tag.Equals("Player"))
                {
                    hasHitEnemies = true;
                    //on test sur l'objet dans l'AOE est celui du direct Hit pour ne pas aapliquer
                    //les degats deux fois
                    if (objInAOE != directHitObj)
                    {
                        PlayerController playerControllerScript = objInAOE.GetComponent<PlayerController>();
                        if (playerControllerScript != null)
                        {
                            if (playerControllerScript.Team != this.team)
                            {
                                ApplyDamage(objInAOE, splashDamage);
                            }
                        }
                    }
                }
            }
        }

        if (autoAttack && piratePassive && hasHitEnemies)
        {
            if (piratePassive.PendingCriticalHit())
            {
                piratePassive.CriticalHitApplied();
            }
            else
            {
                piratePassive.IncrementHitStack();
            }
        }

        //On detruit le projectile apres l'impacte
        PhotonNetwork.Destroy(this.gameObject);
    }

    //Fonction pour appliquer les degats sur un player
    private void ApplyDamage(GameObject target, int damage)
    {
        PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
        if(healthScript!=null)
        {
            
            healthScript.photonView.RPC("Damage", PhotonTargets.All, damage);
            //healthScript.Damage(damage)
            //Debug.Log("Damage : " + damage +" deals to : " + target.name);
        }

        PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
        if (healthScript2 != null)
        {
            //healthScript2.Damage2(damage);
            healthScript2.photonView.RPC("Damage2", PhotonTargets.All, damage);
        }
    }

    //activation ou non de l'AOE
    public bool AOEActivated
    {
        get
        {
            return this.aoeActivated;
        }
        set
        {
            this.aoeActivated = value;
        }
    }

    private void LanceAnim()
    {
        GameObject effetExplosion;
        //Pour le local
        if (PhotonNetwork.connected == false)
        {

            effetExplosion = Instantiate(AnimBulletExplode, this.transform.position, AnimBulletExplode.transform.rotation).gameObject as GameObject;
        }
        else
        {
            //Pour le reseau
            effetExplosion = PhotonNetwork.Instantiate(this.AnimBulletExplode.name, this.transform.position, AnimBulletExplode.transform.rotation, 0);
        }
    }

    

}
