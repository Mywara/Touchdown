using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JumpBearComp : Photon.PunBehaviour
{


    private Rigidbody rb;
    private Animator anim;
    public AudioSource audioSource;
    private PlayerController playerControllerScript;
    public float transparenceCD;
    public GameObject HUD;
    

    /////////// JUMP STUFF


    public GameObject jumpHUD; // UI pour la comp Jump
    public float jumpCooldown; // temps du cooldown du jump en seconde
    private float jumpLastUse; // temps (en seconde) de derniere utilisation
    public float jumpHeight; // La hauteur de saut de la competence
    private bool jumping; // Détermine si on est en saut après l'utilisation du Jump pour savoir si on applique les dégats à l'atterissage
    public GameObject jumpAOEZone;
    public int jumpDamage;
    public float BoostVitesseEnSaut; // Pourcentage
    private JumpAOE jumpAOEScript; // Utile pour communiquer avec la hitbox de la competence

    public GameObject effetDecollage; // Il s'agit des effets visuels
    public GameObject effetAtterrissage;

    public AudioClip audioDecollage;
    public AudioClip audioAtterrissage;

    private bool JumpActif = true;

    



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

        /////////// JUMP STUFF

        jumpAOEScript = jumpAOEZone.GetComponent<JumpAOE>();
        if (jumpAOEScript != null)
        {
            jumpAOEScript.SetDamage(this.jumpDamage);
            jumpAOEScript.SetApply(false);

            //dire de quelle equipe vient le coup pour ne pas TK
            if (playerControllerScript != null)
            {
                jumpAOEScript.SetTeam(playerControllerScript.Team);
                //Debug.Log("cachitzone team set to : " + playerControllerScript.Team);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }
        else
        {
            Debug.Log("The jumpAOEZone does not have a jumpAOEZone Script");
        }
        jumpAOEZone.SetActive(false);

        jumpLastUse = -jumpCooldown; // Ainsi on peut utiliser la compétence dès le début
        jumping = false;
        

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true || !JumpActif)
        {
            return;
        }

        /////////// JUMP STUFF


        // Si on est en saut depuis plus de 0.2 sec (pour éviter que le sherecast ne touche au décollage)
        if (jumping && Time.time > jumpLastUse + 0.2)
        {
            // on utilise un raycast pour connaitre la distance vis a vis du sol
            RaycastHit hit;
            if (Physics.SphereCast(rb.transform.position + Vector3.up * 0.35f, 0.25f, -rb.transform.up, out hit, 10))
            {
                //test
                //print(hit.distance);

                // Si on atterri
                if (hit.distance <= 0.3)
                {
                    // On diminue la vitesse de déplacement en vol
                    //playerControllerScript.photonView.RPC("FinModifVitesse", PhotonTargets.All, BoostVitesseEnSaut);
                    playerControllerScript.photonView.RPC("FinModifVitesse", PhotonTargets.All, BoostVitesseEnSaut);
                    //Joue le son de l'atterrissage
                    audioSource.clip = audioAtterrissage;
                    audioSource.Play();

                    jumping = false;
                    LaunchJumpAOE();
                }
            }

            // N'arrive normalement jamais mais au cas où
            if (Time.time > (jumpLastUse + jumpCooldown))
            {
                jumping = false;
            }
        }

        // Input du Jump
        if (Input.GetButtonDown("Skill2") && (Time.time > (jumpLastUse + jumpCooldown)))
        {
            Debug.Log("Jump Competence");
            compJump(); // On lance le saut

            // On accelere la vitesse de déplacement en vol
            //playerControllerScript.photonView.RPC("DebutModifVitesse", PhotonTargets.All, BoostVitesseEnSaut);
            playerControllerScript.photonView.RPC("DebutModifVitesse", PhotonTargets.All, BoostVitesseEnSaut);

            jumpLastUse = Time.time;
            jumping = true; // On considère qu'on est en train de sauter
        }
        
    }
    

    // Lance le saut
    void compJump()
    {

        // Set jump animation trigger
        anim.SetTrigger("Jump");

        //Joue le son de décollages
        audioSource.clip = audioDecollage;
        audioSource.Play();

        // Lance le CD sur l'affichage
        StartCoroutine("JumpAffichageCooldown");



        GameObject effetDecol;
        //Pour le local
        if (PhotonNetwork.connected == false)
        {
            effetDecol = Instantiate(effetDecollage, this.transform.position, effetDecollage.transform.rotation).gameObject as GameObject;
        }
        else
        {
            //Pour le reseau
            effetDecol = PhotonNetwork.Instantiate(this.effetDecollage.name, this.transform.position, effetDecollage.transform.rotation, 0);
        }

        

        // Modifie la velocité verticale
        Vector2 velocity = rb.velocity;
        velocity.y = CalculateJumpVerticalSpeed(jumpHeight);
        rb.velocity = velocity;

        // On active la zone de l'AOE
        jumpAOEZone.SetActive(true);
        jumpAOEScript.SetTeam(playerControllerScript.team);
    }

    // Méthode pour calculer la velocité à donner pour atteindre la hauteur donnée
    public static float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2f * targetJumpHeight * -Physics.gravity.y);
    }

    // Applique l'animation et les effets de l'aterrissage
    void LaunchJumpAOE()
    {
        // animations


        GameObject effetAtterr;
        //Pour le local
        if (PhotonNetwork.connected == false)
        {
            effetAtterr = Instantiate(effetAtterrissage, this.transform.position, effetAtterrissage.transform.rotation).gameObject as GameObject;
        }
        else
        {
            //Pour le reseau
            effetAtterr = PhotonNetwork.Instantiate(this.effetAtterrissage.name, this.transform.position, effetAtterrissage.transform.rotation, 0);
        }

        // applique les dégats et effets
        jumpAOEScript.SetApply(true);
    }

    /////////////////////// Affichage Competences


    private IEnumerator JumpAffichageCooldown()
    {
        float dureeCD = jumpCooldown;
        // Modifi la transparence
        Image image = jumpHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = transparenceCD;
        image.color = c;

        // Pour modifier le text
        Text t = jumpHUD.GetComponentInChildren<Text>();


        while (dureeCD > 0)
        {
            dureeCD -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
             t.text = (Mathf.Floor(dureeCD)+1).ToString();
        }

        // On remet la transparence normale
        c.a = 255;
        image.color = c;

        t.text = "";
    }

    /////////////////// Autres méthodes

    public void SetJumpActif(bool b)
    {
        this.JumpActif = b;
    }

    private void OnDisable()
    {

        // On remet la transparence normale
        Image image = jumpHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = 255;
        image.color = c;

        // On remet l'affichage du cooldown à rien (pas de CD)
        Text t = jumpHUD.GetComponentInChildren<Text>();
        t.text = "";

        jumping = false;

        if(jumpAOEScript != null)
        {
            jumpAOEScript.SetApply(false);
            jumpAOEZone.SetActive(false);
        }

        // On reset le CD
        jumpLastUse = 0;

    }
}
