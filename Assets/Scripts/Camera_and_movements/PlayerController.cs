using UnityEngine;
using System.Collections;

public class PlayerController : Photon.PunBehaviour{

    public float movementSpeed = 10;
    public float turningSpeed = 60;
    public float rotY = 0.0f;
    public float mouseX;
    public float finalInputX;
    public float inputSensitivity = 150.0f;
    public GameObject cameraFollow;
    //public float jumpForce = 10.0f;
    public float myJumpHeight = 5.0f;
    public int team = 0;

    private Rigidbody rb;
    private Animator anim;
    private bool netWorkingDone = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }


    void Start ()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;

        if (photonView.isMine)
        {
            GameObject cameraPrefab = Camera.main.transform.root.gameObject;
            if (cameraPrefab != null)
            {
                CameraFollow cameraFollowScript = cameraPrefab.GetComponent<CameraFollow>();
                if (cameraFollowScript != null)
                {
                    cameraFollowScript.SetObjectToFollow(cameraFollow);
                }
                else
                {
                    Debug.Log("Main camera found, but don't have CameraFollow script");
                }
            }
            else
            {
                Debug.Log("No main Camera / no CameraPrefab");
            }
        }
    }


    void FixedUpdate()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        // saut perso
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // on utilise un raycast pour connaitre la distance vis a vis du sol
            RaycastHit hit;

            // On appelle le Raycast dans un if car s'il ne touche rien il renvoit false (c'est qu'on est dans le vide et on peut pas sauter)
            if (Physics.Raycast(rb.transform.position + Vector3.up * 0.1f, -rb.transform.up, out hit, 10))
            {
                
                //test
                //print(hit.distance);

                // On vérifie si on est assez prêt du sol poour pouvoir sauter
                if (hit.distance <= 0.2)
                {
                    // Set jump animation trigger
                    anim.SetTrigger("Jump");


                    // soit on utilise une force soit on modifie la velocité verticale
                    Vector2 velocity = rb.velocity;
                    velocity.y = CalculateJumpVerticalSpeed(myJumpHeight);
                    rb.velocity = velocity;
                    //rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
                }

            }
        }
    }

    // Méthode pour calculer la velocité à donner pour atteindre la hauteur donnée
    public static float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2f * targetJumpHeight * -Physics.gravity.y);
    }


    void Update()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        // translation perso
        float horizontal = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        transform.Translate(horizontal, 0, 0);

        float vertical = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        transform.Translate(0, 0, vertical);

        // animation de déplacement en fonction des inputs horizontaux et verticaux (flèches directionnelles)
        Animate(horizontal, vertical);
    }

    void Animate(float h, float v)
    {
        // s'il y a déplacement latéral
        if(h != 0f)
        {
            anim.SetBool("RunningForward", false);

            if(h < 0f)
            {
                anim.SetBool("RunningLeft", true);
                anim.SetBool("RunningRight", false);
            }
            else if(h > 0f)
            {
                anim.SetBool("RunningLeft", false);
                anim.SetBool("RunningRight", true);
            }
        }
        // s'il n'y a pas de déplacement latéral
        else
        {
            anim.SetBool("RunningForward", (v != 0f));

            anim.SetBool("RunningLeft", false);
            anim.SetBool("RunningRight", false);
        }
    }


    public int Team
    {
        get
        {
            return this.team;
        }
        set
        {
            if(this.team != value)
            {
                this.team = value;
                AutoAttaqueCac autoCacScript = this.gameObject.GetComponent<AutoAttaqueCac>();
                if (autoCacScript != null)
                {
                    CacHitZone cacHitZoneScript = autoCacScript.cacHitZone.GetComponent<CacHitZone>();
                    if(cacHitZoneScript != null)
                    {
                        cacHitZoneScript.SetTeam(this.Team);
                    }
                    else
                    {
                        Debug.Log("AutoAttaqueCac does not have CacHitZone script");
                    }
                }
            }  
        }
    }
}
