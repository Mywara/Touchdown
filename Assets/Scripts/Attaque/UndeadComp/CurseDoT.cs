using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//attaque large peu profonde
//repousse les ennemis et fait peu de dégâts
public class CurseDoT : Photon.PunBehaviour
{

    private int damage;
    private float fireRate = 1f;
    private int team;
    private GameObject owner;

    public int duration = 5;
    
    public int splashDamage = 0;
    public float AOERadius = 3f;
    public float speed = 10;

    private bool netWorkingDone = false;
    private float nextFire = 0f;
    private List<GameObject> directHitObjs = new List<GameObject>();
    private List<int> dotNumber = new List<int>();
    private bool syncTeam = true;

    private int count = 0;
    //indique que la propagation doit s'arrêter car il n'y a pas eu lieu
    //au tic précédent la chaîne est brisée
    private bool stopPropag = false;
    //indique que tous les dot ont été appliqués
    private bool end = false;

    //the radius of the circular zone = depth of the spell
    private float radius;

    private Rigidbody myRb;

    public void Start()
    {
        damage = Constants.CURSEDOT_DAMAGE;
        radius = Constants.CURSEDOT_RADIUS;

        myRb = this.gameObject.GetComponent<Rigidbody>();
        if (myRb != null)
        {
            myRb.velocity = transform.forward * speed;
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

    //Dans le OnTriggerStay on applique le dégat au premier ennemi touché la
    //propagation de la malédiction se fera dans l'update
    private void OnTriggerStay(Collider other)
    {
        if (directHitObjs != null) return;

        Debug.Log("Entrée dans le OnTriggerStay CURSE");
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        GameObject directHitObj = other.transform.root.gameObject;
        if (directHitObj.tag.Equals("Respawn") || directHitObj.tag.Equals("Boundary"))
        {
            //Debug.Log("hit Respawn");
            return;
        }
        
        if (!RoomManager.instance.FriendlyFire)
        {
            if (directHitObj.tag.Equals("Player") && directHitObj != owner)
            {
                PlayerController playerControllerScript = directHitObj.GetComponent<PlayerController>();
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
        
        Debug.Log("Direct hit on object : " + directHitObj.name);
        if (directHitObj.tag.Equals("Player") && directHitObj.GetComponent<PlayerController>().team != team)
        {
            Debug.Log("Ajout dans la liste");
            directHitObjs.Add(directHitObj);
            ApplyDamage(directHitObj, damage);
            //On maudit ici l'ennemi
            directHitObj.GetComponent<PlayerController>().Curse();
            dotNumber.Add(1);
            count++;
        }
    }

    private void Update()
    {

        //la destruction de tous les objects se fait lorsque tous les dégats sont appliqués
        end = true;
        foreach (int num in dotNumber)
        {
            if (num < Constants.CURSEDOT_TIC) { end = false; }
        }

        if (Time.time > nextFire && !end)
        {
            nextFire = Time.time + fireRate;
            Debug.Log("Entrée Dans l'update de CURSE DOT");
            //pour chaque personnage touché appliquer la dot,
            //si le nombre maximum de tic est atteint => supprimer l'élément de la liste
            for (int i = 0; i < directHitObjs.Count; i++)
            {
                Debug.Log("Obj in CURSEDOT : " + directHitObjs[i].name);
                if (dotNumber[i] < Constants.CURSEDOT_TIC)
                {
                    ApplyDamage(directHitObjs[i], damage);

                    //on reset le timer de la malédiction afin de faire durer la malédiction plus longtemps
                    directHitObjs[i].GetComponent<PlayerController>().ResetCurseTimer();

                    dotNumber[i]++;
                }
            }

            bool change = false;
            //Propagation de la malédiction aux ennemis proches du dernier ennemi touché
            if (count < Constants.CURSEDOT_TARGETS || !stopPropag)
            {
                Debug.Log("Tentative de propagation");
                //on se positionne au niveau du dernier élément de la liste pour pouvoir appliquer la propagation aux autres ennemis
                Collider[] hitColliders = Physics.OverlapSphere(directHitObjs[directHitObjs.Count - 1].transform.position, AOERadius);
                for (int i = 0; i < hitColliders.Length; i++)
                {
                    GameObject objInAOE = hitColliders[i].transform.root.gameObject;
                    if (objInAOE.tag.Equals("Player") && objInAOE.GetComponent<PlayerController>().team != team)
                    {
                        Debug.Log("AOE hits object : " + objInAOE.name);
                        if (!directHitObjs.Contains(objInAOE))
                        {
                            Debug.Log(objInAOE + " N'appartient pas à la liste, team : " + this.team);
                            PlayerController playerControllerScript = objInAOE.GetComponent<PlayerController>();
                            if (playerControllerScript != null)
                            {
                                Debug.Log("Not Script");
                                if (playerControllerScript.Team != this.team)
                                {
                                    Debug.Log("Propagation à " + objInAOE.name);
                                    ApplyDamage(objInAOE, damage);
                                    //On maudit ici l'ennemi
                                    playerControllerScript.Curse();
                                    directHitObjs.Add(objInAOE);
                                    dotNumber.Add(1);
                                    count++;
                                    change = true;
                                }
                            }
                        }
                    }
                    if (change) { i = hitColliders.Length; }
                }
            }
            if (!change) { stopPropag = true; }
        }
        /*
        if (end)
        {
            Debug.Log("before destroy");
            PhotonNetwork.Destroy(this.gameObject);
            Debug.Log("Destroy");
        }
        */
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
                Debug.Log("Damage : " + damage + " deals to : " + target.name);
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
