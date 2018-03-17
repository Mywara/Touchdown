using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TirClochePirateComp : Photon.PunBehaviour
{


    private Rigidbody rb;
    private Animator anim;
    public AudioSource audioSource;
    private PlayerController playerControllerScript;
    public float transparenceCD;
    public GameObject HUD;
    public Transform projectileSpawn;


    /////////// TirCloche STUFF


    public GameObject tirClocheHUD; // UI pour la comp 
    public float tirClocheCooldown; // temps du cooldown du calin en seconde
    private float tirClocheLastUse; // temps (en seconde) de derniere utilisation

    public GameObject tirClocheProjectile; // L'effet visuel

    public AudioClip audioTirCloche; // L'audio

    private bool tirClocheActif = true;


    // Use this for initialization
    void Start()
    {
        if (photonView.isMine)
        {
            HUD.SetActive(true);
        }

        playerControllerScript = this.gameObject.GetComponent<PlayerController>();

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();


        ///////////// tirCloche STUFF
        tirClocheLastUse = -tirClocheCooldown;

        /////////////

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true || !tirClocheActif)
        {
            return;
        }


        // Input 
        if (Input.GetKeyDown(KeyCode.R) && (Time.time > (tirClocheLastUse + tirClocheCooldown)))
        {

            GameObject projo;
            //Pour le local
            if (PhotonNetwork.connected == false)
            {
                projo = Instantiate(tirClocheProjectile, projectileSpawn.position + Vector3.up * 0.2f, projectileSpawn.rotation).gameObject as GameObject;
            }
            else
            {

                //Pour le reseau
                projo = PhotonNetwork.Instantiate(tirClocheProjectile.name, projectileSpawn.position + Vector3.up * 0.2f, projectileSpawn.rotation, 0);
            }

            //Fait de set de la team pour le projectile
            Bomb BombScript = projo.GetComponent<Bomb>();
            if (playerControllerScript != null)
            {
                BombScript.SetTeam(playerControllerScript.Team);
                BombScript.SetOwner(this.transform.gameObject);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }

            tirClocheLastUse = Time.time;
            StartCoroutine("TirClocheAffichageCooldown");
        }
        // fin Input 
    }




    /////////////////////// Affichage Competences


    private IEnumerator TirClocheAffichageCooldown()
    {
        float dureeCD = tirClocheCooldown;
        // Modifi la transparence
        Image image = tirClocheHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = transparenceCD;
        image.color = c;

        // Pour modifier le text
        Text t = tirClocheHUD.GetComponentInChildren<Text>();


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

    

    public void SetTirClocheActif(bool b)
    {
        this.tirClocheActif = b;
    }

    private void OnDisable()
    {

        // On remet la transparence normale
        Image image = tirClocheHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = 255;
        image.color = c;

        // On remet l'affichage du cooldown à rien (pas de CD)
        Text t = tirClocheHUD.GetComponentInChildren<Text>();
        t.text = "";
    }
}
