using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Photon.PunBehaviour, IPunObservable {

    public int impactDamage = 10;
    public int splashDamage = 0;
    public float AOERadius = 0;
    public float speed = 10;
    public int team;
    public bool aoeActivated = false;

    private Rigidbody myRb;
    private bool netWorkingDone = false;
    

    // Use this for initialization
    void Start () {
        myRb = this.gameObject.GetComponent<Rigidbody>();
        if(myRb != null)
        {
            myRb.velocity = transform.forward * speed;
        }
        else
        {
            Debug.Log("error, no rigidbody on the projectile");
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
        if(directHitObj.tag.Equals("Respawn") || directHitObj.tag.Equals("Boundary"))
        {
            //Debug.Log("hit Respawn");
            return;
        }

        //on test si il a a du friendlyFire ou non
        if(!RoomManager.instance.FriendlyFire)
        {
            if (directHitObj.tag.Equals("Player"))
            {
                PlayerController playerControllerScript = directHitObj.GetComponent<PlayerController>();
                if(playerControllerScript != null)
                {
                    if(playerControllerScript.Team == this.team)
                    {
                        Debug.Log("Friend hit, not FF, do nothing");
                        return;
                    }
                }
            }
        }    

        //On applique des dégâts direct avec le projectile
        if (directHitObj.tag.Equals("Player"))
        {
            ApplyDamage(directHitObj, impactDamage);
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
                    Debug.Log("Projectile : AOE hits object : " + objInAOE.name);
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
            Debug.Log("Damage : " + damage +" deals to : " + target.name);
        }

        PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
        if (healthScript2 != null)
        {
            //healthScript2.Damage2(damage);
            healthScript2.photonView.RPC("Damage2", PhotonTargets.All, damage);
        }
    }
    
    //On set une fois en reseaux les valeurs de l'instance
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(!netWorkingDone)
        {
            if (stream.isWriting)
            {
                stream.SendNext(this.impactDamage);
                stream.SendNext(this.splashDamage);
                stream.SendNext(this.AOERadius);
                stream.SendNext(this.speed);
                stream.SendNext(this.team);
            }
            else
            {
                this.impactDamage = (int)stream.ReceiveNext();
                this.splashDamage = (int)stream.ReceiveNext();
                this.AOERadius = (float)stream.ReceiveNext();
                this.speed = (float)stream.ReceiveNext();
                this.team = (int)stream.ReceiveNext();
                netWorkingDone = true;
                //Debug.Log("Networking Done for projectiles");
                //Reset the velocity of the projectile
                if (myRb != null)
                {
                    myRb.velocity = transform.forward * speed;
                }
            }
        }
    }

    //si quelqu'un join la room en cours, on resynchronise les valeurs en réseaux
    public override void OnJoinedRoom()
    {
        netWorkingDone = false;
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
}
