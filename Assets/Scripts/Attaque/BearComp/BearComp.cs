using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using UnityEngine;

public class BearComp : Photon.PunBehaviour
{
    public GameObject projectilePrefab = null;

    public Transform projectileSpawn1 = null;

    private Animator anim;


    public GameObject HUD;
    public float transparenceCD;
    
    private float shoulderLastUse;
    
    public float shoulderCooldown;
    
    public GameObject shoulderHUD;

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


        shoulderLastUse = -shoulderCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Competences Update");
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        //Compétence Coup d'épaule
        if (Input.GetKeyDown(KeyCode.E)  && Time.time > shoulderLastUse + shoulderCooldown)
        {
            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(projectilePrefab, projectileSpawn1.position, projectileSpawn1.rotation).gameObject as GameObject;
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(projectilePrefab.name, projectileSpawn1.position, projectileSpawn1.rotation, 0);
            }

            //Fait de set de la team pour le projectile
            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
            ShoulderHit shoulderScript = projo.GetComponent<ShoulderHit>();
            if (playerControllerScript != null)
            {
                shoulderScript.SetTeam(playerControllerScript.Team);
                shoulderScript.SetOwner(this.transform.gameObject);

                shoulderLastUse = Time.time;

                // Lance l'affichage du CD
                object[] parms = { shoulderHUD, shoulderCooldown };
                StartCoroutine("AffichageCooldown", parms);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
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

        // On remet la transparence normale
        Image image = shoulderHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = 255;
        image.color = c;

        // On remet l'affichage du cooldown à rien (pas de CD)
        Text t = shoulderHUD.GetComponentInChildren<Text>();
        t.text = "";

    }
}
