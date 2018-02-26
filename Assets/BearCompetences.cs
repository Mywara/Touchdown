using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearCompetences :  Photon.PunBehaviour {


    private Rigidbody rb;
    private Animator anim;

    /////////// JUMP STUFF

    public float cooldownJump; // temps du cooldown du jump en seconde
    private float lastUseJump ; // temps (en seconde) de derniere utilisation
    public float JumpHeight; // La hauteur de saut de la competence
    private bool jumping; // Détermine si on est en saut après l'utilisation du Jump pour savoir si on applique les dégats à l'atterissage
    public GameObject JumpAOEZone;
    public int jumpDamage;
    private JumpAOE JumpAOEScript; // Utile pour communiquer avec la hitbox de la competence


    // Use this for initialization
    void Start () {


        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        /////////// JUMP STUFF

        JumpAOEScript = JumpAOEZone.GetComponent<JumpAOE>();
        if (JumpAOEScript != null)
        {
            JumpAOEScript.SetDamage(this.jumpDamage);
            JumpAOEScript.SetApply(false);

            //dire de quelle equipe vient le coup pour ne pas TK
            PlayerController playerControllerScript = this.gameObject.GetComponent<PlayerController>();
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
        if (jumping && Time.time  > lastUseJump + 0.2)
        {
            // on utilise un raycast pour connaitre la distance vis a vis du sol
            RaycastHit hit;
            if (Physics.SphereCast(rb.transform.position + Vector3.up * 0.35f, 0.25f, -rb.transform.up, out hit, 10))
            {
                //test
                //print(hit.distance);

                // Si on atterri
                if (hit.distance <= 0.2)
                {
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

            compJump(); // On lance le saut
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

    // Applique les effets de l'aterrissage
    void LaunchJumpAOE()
    {
        JumpAOEZone.SetActive(true);
        JumpAOEScript.SetApply(true);
    }

    ///////////////////////
}
