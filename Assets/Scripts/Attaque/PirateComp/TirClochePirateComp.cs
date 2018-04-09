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
    public GameObject animMuzzleBoulet;


    /////////// TirCloche STUFF


    public GameObject tirClocheHUD; // UI pour la comp
    public float tirClocheCooldown; // temps du cooldown du calin en seconde
    private float tirClocheLastUse; // temps (en seconde) de derniere utilisation
    private GameObject tirClocheCdMask;

    public GameObject tirClocheProjectile; // L'effet visuel

    public AudioClip audioTirCloche; // L'audio

    private bool tirClocheActif = true;


    // Use this for initialization
    void Start()
    {
        if (photonView.isMine)
        {
            HUD.SetActive(true);
            tirClocheCdMask = tirClocheHUD.transform.Find("CooldownGreyMask").gameObject;
            tirClocheCdMask.SetActive(false);
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
        if (Input.GetButtonDown("Skill3") && (Time.time > (tirClocheLastUse + tirClocheCooldown)))
        {
            // Lance le muzzle du fusil
            this.photonView.RPC("LanceAnimMuzzleBoulet", PhotonTargets.All);

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

        /*
        // Modifie la transparence
        Image image = tirClocheHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = transparenceCD;
        image.color = c;
        */
        tirClocheCdMask.SetActive(true);

        // Pour modifier le text
        Text t = tirClocheHUD.GetComponentInChildren<Text>();

        while (dureeCD > 0)
        {
            dureeCD -= Time.deltaTime;
            
            yield return new WaitForFixedUpdate();
            t.text = (Mathf.Floor(dureeCD) + 1).ToString();
        }

        /*
        // On remet la transparence normale
        c.a = 255;
        image.color = c;
        */
        tirClocheCdMask.SetActive(false);
        t.text = "";
    }

    

    public void SetTirClocheActif(bool b)
    {
        this.tirClocheActif = b;
    }

    [PunRPC]
    private void LanceAnimMuzzleBoulet()
    {
        GameObject effetA;
        effetA = Instantiate(animMuzzleBoulet, projectileSpawn.position, animMuzzleBoulet.transform.rotation).gameObject as GameObject;
        if (effetA != null)
        {
            //On met en enfant du joueur l'effet, donc si le joueur bouge, l'effet le suit
            effetA.transform.SetParent(this.transform);
        }
    }

    private void OnDisable()
    {
        /*
        // On remet la transparence normale
        Image image = tirClocheHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = 255;
        image.color = c;
        */
        if (photonView.isMine)
        {
            tirClocheCdMask.SetActive(false);

            // On remet l'affichage du cooldown à rien (pas de CD)
            Text t = tirClocheHUD.GetComponentInChildren<Text>();
            t.text = "";
        }

        // On reset le CD
        tirClocheLastUse = 0;

    }
}
