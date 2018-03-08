using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearCompetences : Photon.PunBehaviour
{


    private Rigidbody rb;
    private Animator anim;

    /////////// JUMP STUFF

    public float cooldownJump; // temps du cooldown du jump en seconde
    private float lastUseJump; // temps (en seconde) de derniere utilisation
    public float JumpHeight; // La hauteur de saut de la competence
    private bool jumping; // Détermine si on est en saut après l'utilisation du Jump pour savoir si on applique les dégats à l'atterissage
    public GameObject JumpAOEZone;
    public int jumpDamage;
    private JumpAOE JumpAOEScript; // Utile pour communiquer avec la hitbox de la competence

    public GameObject effetDecollage; // Il s'agit des effets visuels
    public GameObject effetAtterrissage;

    public AudioSource audioSource;
    public AudioClip audioDecollage;
    public AudioClip audioAtterrissage;

    private PlayerController playerControllerScript;



    // Use this for initialization
    void Start()
    {
        playerControllerScript = this.gameObject.GetComponent<PlayerController>();

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        /////////// JUMP STUFF

        JumpAOEScript = JumpAOEZone.GetComponent<JumpAOE>();
        if (JumpAOEScript != null)
        {
            JumpAOEScript.SetDamage(this.jumpDamage);
            JumpAOEScript.SetApply(false);

            //dire de quelle equipe vient le coup pour ne pas TK
            if (playerControllerScript != null)
            {
                JumpAOEScript.SetTeam(playerControllerScript.Team);
                //Debug.Log("cachitzone team set to : " + playerControllerScript.Team);
            }
            else
            {
                Debug.Log("player have no PlayerController script");
            }
        }
        else
        {
            Debug.Log("The JumpAOEZone does not have a JumpAOEZone Script");
        }
        JumpAOEZone.SetActive(false);

        lastUseJump = -cooldownJump; // Ainsi on peut utiliser la compétence dès le début
        jumping = false;


        /////////////

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        /////////// JUMP STUFF


        // Si on est en saut depuis plus de 0.2 sec (pour éviter que le sherecast ne touche au décollage)
        if (jumping && Time.time > lastUseJump + 0.2)
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

                    Debug.Log("Launch JumpAOE");

                    //Joue le son de l'atterrissage
                    audioSource.clip = audioAtterrissage;
                    audioSource.Play();

                    jumping = false;
                    LaunchJumpAOE();
                }
            }

            // N'arrive normalement jamais mais au cas où
            if (Time.time > (lastUseJump + cooldownJump))
            {
                jumping = false;
            }
        }

        // Input du Jump
        if (Input.GetKeyDown(KeyCode.LeftShift) && (Time.time > (lastUseJump + cooldownJump)))
        {
            Debug.Log("Jump Competence");
            compJump(); // On lance le saut

            playerControllerScript.photonView.RPC("ModificationVitesse", PhotonTargets.All, 250, 2.5f);
            lastUseJump = Time.time;
            jumping = true; // On considère qu'on est en train de sauter
        }

        ///////////////////// 



    }


    /////////// JUMP STUFF

    // Lance le saut
    void compJump()
    {
        // Set jump animation trigger
        anim.SetTrigger("Jump");

        //Joue le son de décollages
        audioSource.clip = audioDecollage;
        audioSource.Play();

        //effetDecollage.transform.position = this.transform.position;

        //effetDecollage.transform.SetPositionAndRotation(new Vector3(0,0,0), this.transform.rotation);
        //effetDecollage.SetActive(true);
        //finEffetDecollage = Time.time + 1.5f;


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

        Destroy(effetDecol, 1.5f);

        // Modifie la velocité verticale
        Vector2 velocity = rb.velocity;
        velocity.y = CalculateJumpVerticalSpeed(JumpHeight);
        rb.velocity = velocity;
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

        //effetAtterrissage.SetActive(true);

        //effetAtterrissage.transform.SetPositionAndRotation(this.transform.position + Vector3.up * 0.1f, this.transform.rotation);


        GameObject effetAtterr;
        //Pour le local
        if (PhotonNetwork.connected == false)
        {
            effetAtterr = Instantiate(effetAtterrissage, this.transform.position + Vector3.up * 0.1f, effetAtterrissage.transform.rotation).gameObject as GameObject;
        }
        else
        {
            //Pour le reseau
            effetAtterr = PhotonNetwork.Instantiate(this.effetAtterrissage.name, this.transform.position, effetAtterrissage.transform.rotation, 0);
        }
        Destroy(effetAtterr, 1.5f);

        // applique les dégats et effets
        JumpAOEZone.SetActive(true);
        JumpAOEScript.SetApply(true);
    }

    ///////////////////////
}
