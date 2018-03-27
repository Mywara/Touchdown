using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UndeadComp : Photon.PunBehaviour
{
    public GameObject projectilePrefab1 = null;
    public GameObject projectilePrefab2 = null;

    public Transform projectileSpawn1 = null;
    public Transform projectileSpawn2 = null;

    private Animator anim;

    public GameObject HUD;
    public float transparenceCD;

    private float invulnerableLastUse;
    private float dotLastUse;
    private float tpLastUse;

    public float invulnerableCooldown;
    public float dotCooldown;
    public float tpCooldown;

    public GameObject invulnerableHUD;
    public GameObject dotHUD;
    public GameObject tpHUD;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        // Pour pouvoir utiliser les compétences dès le début
        invulnerableLastUse = -invulnerableCooldown;
        dotLastUse = -dotCooldown;
        tpLastUse = -tpCooldown;


        // On affiche les competence et CD qu'à la personne concernée.
        if (photonView.isMine)
        {
            HUD.SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Competences Update");
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        // Lance le dot 
        if (Input.GetButton("A") && Time.time > dotLastUse + dotCooldown)
        {
            // animation trigger
            //anim.SetTrigger("AttackGun");

            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(projectilePrefab1, projectileSpawn1.position, projectileSpawn1.rotation).gameObject as GameObject;
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(this.projectilePrefab1.name, projectileSpawn1.position, projectileSpawn1.rotation, 0);

            }

            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            CurseDoT curseDoTScript = projo.GetComponent<CurseDoT>();
            if (playerControllerScript != null)
            {
                curseDoTScript.SetTeam(playerControllerScript.Team);
                curseDoTScript.SetOwner(this.transform.gameObject);

                dotLastUse = Time.time;

                // Lance l'affichage du CD
                object[] parms = { dotHUD, dotCooldown };
                StartCoroutine("AffichageCooldown", parms);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }

        // Lance la TP
        else if (Input.GetButton("E") && Time.time > tpLastUse + tpCooldown)
        {
            // animation trigger
            //anim.SetTrigger("AttackGun");

            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(projectilePrefab2, projectileSpawn2.position, Quaternion.identity).gameObject as GameObject;
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(this.projectilePrefab2.name, projectileSpawn2.position, Quaternion.identity, 0);

            }

            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            Teleportation tpScript = projo.GetComponent<Teleportation>();
            if (playerControllerScript != null)
            {
                tpScript.SetTeam(playerControllerScript.Team);
                tpScript.SetOwner(this.transform.gameObject);

                tpLastUse = Time.time;

                // Lance l'affichage du CD
                object[] parms = { tpHUD, tpCooldown };
                StartCoroutine("AffichageCooldown", parms);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }

        //Compétence pour l'invulnerabilité
        if (Input.GetKeyDown(KeyCode.R) && Time.time > invulnerableLastUse + invulnerableCooldown)
        {
            Invulnerability invulScript = GetComponent<Invulnerability>();
            if (invulScript != null)
            {
                invulScript.photonView.RPC("BecameInvulnerable", PhotonTargets.All);

                invulnerableLastUse = Time.time;

                // Lance l'affichage du cooldown
                object[] parms = { invulnerableHUD, invulnerableCooldown };
                StartCoroutine("AffichageCooldown", parms);
            }
            else
            {
                Debug.Log("missinf Invulnerability script");
            }
        }
    }

    // parms : arg0 = hud , arg1 = duree
    private IEnumerator AffichageCooldown(object[] parms)
    {
        Debug.Log(((Object)parms[0]).name);
        float dureeCD = (float)parms[1];
        // Modifi la transparence
        Image image = ((GameObject)parms[0]).GetComponent<Image>();
        Color c = image.color;
        c.a = transparenceCD;
        image.color = c;

        // Pour modifier le text
        Text t = ((GameObject)parms[0]).GetComponentInChildren<Text>();

        // Affiche le décompte
        while (dureeCD > 0)
        {
            dureeCD -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
            t.text = (Mathf.Floor(dureeCD) + 1).ToString();
        }

        // On remet la transparence normale
        c.a = 255;
        image.color = c;

        t.text = "";
    }

    // Reset le perso (si meurt)
    private void OnDisable()
    {

        // On remet la transparence normale de l'affichage
        Image image = invulnerableHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = 255;
        image.color = c;

        image = dotHUD.GetComponent<Image>();
        image.color = c;
        image = tpHUD.GetComponent<Image>();
        image.color = c;


        // On remet l'affichage du cooldown à rien (pas de CD)
        Text t = invulnerableHUD.GetComponentInChildren<Text>();
        t.text = "";
        t = dotHUD.GetComponentInChildren<Text>();
        t.text = "";
        t = tpHUD.GetComponentInChildren<Text>();
        t.text = "";

    }
}
