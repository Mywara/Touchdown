using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PirateComp : Photon.PunBehaviour
{
    public GameObject projectilePrefab1 = null;
    public GameObject rhumBottle = null;
    //public GameObject projectilePrefabUlt = null;

    public Transform projectileSpawn1 = null;

    private Animator anim;

    public GameObject HUD;
    public float transparenceCD;

    private float grapLastUse;
    private float bouteilleLastUse;

    public float grapCooldown;
    public float bouteilleCooldown;

    public GameObject grapHUD;
    public GameObject bouteilleHUD;
    private bool compPirateActif = true;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        // On affiche les competence et CD qu'à la personne concernée.
        if (photonView.isMine)
        {
            HUD.SetActive(true);
        }

        grapLastUse = -grapCooldown;
        bouteilleLastUse = -bouteilleCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Competences Update");
        if (!photonView.isMine && PhotonNetwork.connected == true || !compPirateActif)
        {
            return;
        }

        // GrapeShot
        if (Input.GetButtonDown("Skill1") && Time.time > grapLastUse + grapCooldown)
        {
            // animation trigger
            //anim.SetTrigger("AttackGun");
            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(projectilePrefab1, projectileSpawn1.position, projectileSpawn1.rotation).gameObject as GameObject;
                //projectilePrefab1.GetComponent<projectilePrefab1.name>.SetTeam(playerControllerScript.Team);
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(this.projectilePrefab1.name, projectileSpawn1.position, projectileSpawn1.rotation, 0);
                //projectilePrefab1.GetComponent<projectilePrefab1.name>.SetTeam(playerControllerScript.Team);

            }

            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            GrapeShot grapeShotScript = projo.GetComponent<GrapeShot>();
            if (playerControllerScript != null)
            {
                grapeShotScript.SetTeam(playerControllerScript.Team);
                grapeShotScript.SetOwner(this.transform.gameObject);

                grapLastUse = Time.time;

                // Lance l'affichage du CD
                object[] parms = { grapHUD, grapCooldown };
                StartCoroutine("AffichageCooldown", parms);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }

        //Compétence Lancer de bouteille de rhum
        if (Input.GetButtonDown("Skill2") && Time.time > bouteilleLastUse + bouteilleCooldown)
        {
            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(rhumBottle, projectileSpawn1.position, projectileSpawn1.rotation).gameObject as GameObject;
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(rhumBottle.name, projectileSpawn1.position, projectileSpawn1.rotation, 0);
            }

            //Fait de set de la team pour le projectile
            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            RhumSpray rhumSprayScript = projo.GetComponent<RhumSpray>();
            if (playerControllerScript != null)
            {
                rhumSprayScript.SetTeam(playerControllerScript.Team);
                rhumSprayScript.SetOwner(this.transform.gameObject);

                bouteilleLastUse = Time.time;

                // Lance l'affichage du CD
                object[] parms = { bouteilleHUD, bouteilleCooldown };
                StartCoroutine("AffichageCooldown", parms);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }
    }

    // GESTION AFFICHAGE

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

    // AUTRES METHODES

    // Désactive/Active l'utilisation des comps (pour le stun)
    public void SetCompActives(bool b)
    {
        this.compPirateActif = b;
    }

    // Reset le perso (si meurt)
    private void OnDisable()
    {

        // On remet la transparence normale
        Image image = grapHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = 255;
        image.color = c;

        image = bouteilleHUD.GetComponent<Image>();
        image.color = c;

        // On remet l'affichage du cooldown à rien (pas de CD)
        Text t = grapHUD.GetComponentInChildren<Text>();
        t.text = "";
        t = bouteilleHUD.GetComponentInChildren<Text>();
        t.text = "";
    }
}
