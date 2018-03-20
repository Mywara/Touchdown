﻿using UnityEngine;
using System.Collections;

public class PlayerController : Photon.PunBehaviour
{

    public float movementSpeed = 10;


    public float inputSensitivity = 150.0f;
    public GameObject cameraFollow;
    public float myJumpHeight = 5.0f;

    public int team = 0;
    private Rigidbody rb;
    private Animator anim;
    private bool netWorkingDone = false;
    public bool immobilization = false;
    public Transform activeTrap;
    public bool inGame = true;


    public bool onCollision = false;
    private bool mobile = true; // sert à savoir si on a le droit de bouger
    private float timeStun = 0; // sert à savoir jusqu'à quand on est stun
    private bool isStun = false; // sert à savoir si on stun

    // Script pour controler l'orientation de la camera
    private CameraFollow cameraFollowScript;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }


    void Start()
    {
        if (photonView.isMine)
        {
            GameObject cameraPrefab = Camera.main.transform.root.gameObject;
            if (cameraPrefab != null)
            {
                cameraFollowScript = cameraPrefab.GetComponent<CameraFollow>();
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

        if (Input.GetKeyDown(KeyCode.Space) && !immobilization && mobile)
        {
            // on utilise un raycast pour connaitre la distance vis a vis du sol
            RaycastHit hit;

            // On appelle le SphereCast dans un if car s'il ne touche rien il renvoit false (c'est qu'on est dans le vide et on peut pas sauter)
            if (Physics.SphereCast(rb.transform.position + Vector3.up * 0.35f, 0.25f, -rb.transform.up, out hit, 10))
            {

                // On vérifie si on est assez prêt du sol poour pouvoir sauter
                if (hit.distance <= 0.2)
                {
                    // Set jump animation trigger
                    if (PhotonNetwork.connected)
                        photonView.RPC("JumpAnimation", PhotonTargets.All);
                    else
                        JumpAnimation();

                    Vector2 velocity = rb.velocity;
                    velocity.y = CalculateJumpVerticalSpeed(myJumpHeight);
                    rb.velocity = velocity;
                }

            }
        }
    }

    [PunRPC]
    private void JumpAnimation()
    {
        anim.SetTrigger("Jump");
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
        if (Input.GetKeyDown(KeyCode.L))
        {
            inGame = !inGame;
        }
        if (inGame)
        {

            // gestion du stun (on utilise mobile comme condition 
            if (isStun && Time.time > timeStun)
            {
                FinStun();
                isStun = false;
            }

            //permet de bouger a nouveau lorsque le piege immobilisant est détruit
            if (!activeTrap && immobilization)
            {
                immobilization = false;
            }

            if (!immobilization && mobile)
            {
                // translation perso
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");

                // Ancienne version sans la normalisation
                //transform.Translate(horizontal * movementSpeed * Time.deltaTime, 0, 0);
                //transform.Translate(0, 0, vertical * movementSpeed * Time.deltaTime);


                // deplacement normalisé (pour diagonale)
                Vector3 movement = (new Vector3(horizontal, 0, vertical).normalized) * Mathf.Max(Mathf.Abs(horizontal), Mathf.Abs(vertical));
                transform.Translate(movement * movementSpeed * Time.deltaTime);



                // animation de déplacement en fonction des inputs horizontaux et verticaux (flèches directionnelles)
                if (PhotonNetwork.connected)
                    photonView.RPC("Animate", PhotonTargets.All, horizontal, vertical);
                else
                    Animate(horizontal, vertical);
            }
        }


    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            onCollision = true;
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        onCollision = false;
    }

    [PunRPC]
    private void Animate(float h, float v)
    {
        anim.SetFloat("VelY", v);
        anim.SetFloat("VelX", h);
    }


    public int Team
    {
        get
        {
            return this.team;
        }
        set
        {
            if (this.team != value)
            {
                this.team = value;
                AutoAttaqueCac autoCacScript = this.gameObject.GetComponent<AutoAttaqueCac>();
                if (autoCacScript != null)
                {
                    CacHitZone cacHitZoneScript = autoCacScript.cacHitZone.GetComponent<CacHitZone>();
                    if (cacHitZoneScript != null)
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
    ///////////////// Fonctions qui ont des effets sur le joueur


    // Modifie la vitesse de déplacement en prenant un pourcentage de la vitesse actuelle sur une certaine duree
    [PunRPC]
    public IEnumerator ModificationVitesse(float pourcentageVitesse, float duree)
    {
        movementSpeed = movementSpeed * (pourcentageVitesse / 100);
        yield return new WaitForSeconds(duree);
        movementSpeed = movementSpeed / (pourcentageVitesse / 100);
    }


    // Fait faire un saut de la hauteur voulue
    [PunRPC]
    public void PetitSaut(float hauteurSaut)
    {
        Vector2 velocity = rb.velocity;
        velocity.y = CalculateJumpVerticalSpeed(hauteurSaut);
        rb.velocity = velocity;
    }

    // Stun le perso
    [PunRPC]
    public void Stun(float duree)
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        timeStun = Time.time + duree;
        isStun = true;

        // prive translation, rotation du perso et compétences du perso
        SetMobile(false);
        cameraFollowScript.StopCamera();
        SetActiveCompetence(false);
        SetActiveAutoAtt(false);

    }

    private void FinStun()
    {
        // autorise translation, rotation du perso et compétences du perso
        SetMobile(true);
        cameraFollowScript.ActiveCamera();
        SetActiveCompetence(true);
        SetActiveAutoAtt(true);
    }

    // Stun le perso
    public void SetMobile(bool mob)
    {
        this.mobile = mob;
    }

    // Set les competences a active ou non
    public void SetActiveCompetence(bool b)
    {

        string name = GetComponent<CharacterCaracteristic>().Cname;


        switch (name)
        {
            case "WarBear":
                JumpBearComp jbc = GetComponent<JumpBearComp>();
                jbc.SetJumpActif(b);

                CalinBearComp cbc = GetComponent<CalinBearComp>();
                cbc.SetCalinActif(b);

                // TODO le coup d'épaule
                break;

            case "Undeath":
                // TODO les 3 comps
                break;

            case "Pirate":
                TirClochePirateComp tcpc = GetComponent<TirClochePirateComp>();
                tcpc.SetTirClocheActif(b);
                // TODO pirateComp
                break;

            default:
                Debug.Log("name : \"" + name + "\" ne correspond pas à WarBear / Undeath / Pirate " +
                    "\n voir Cname de CharacterCaractistique du prefab");
                break;
        }

    }


    // Set les auto-attaques a active ou non
    public void SetActiveAutoAtt(bool b)
    {

        string name = GetComponent<CharacterCaracteristic>().Cname;


        switch (name)
        {
            case "WarBear":
                AutoAttaqueCac aacwb = GetComponent<AutoAttaqueCac>();
                aacwb.SetCACActif(b);
                break;

            case "Undeath":
                AutoAttaqueCac aacud = GetComponent<AutoAttaqueCac>();
                aacud.SetCACActif(b);

                break;

            case "Pirate":
                AutoAttaqueRanged aarp = GetComponent<AutoAttaqueRanged>();
                aarp.SetTirActif(b);
                break;

            default:
                Debug.Log("name : \"" + name + "\" ne correspond pas à WarBear / Undeath / Pirate " +
                    "\n voir Cname de CharacterCaractistique du prefab");
                break;
        }

    }

    //Applique une force "forceVector" au gameObject associe
    [PunRPC]
    public void AddForceTo(Vector3 forceVector)
    {
        this.GetComponent<Rigidbody>().AddForce(forceVector);
    }

    //Positionne le gameObject à la position "pos"
    [PunRPC]
    public void SetPosition(Vector3 pos)
    {
        this.transform.position = pos;
    }

    //Change la direction du gameObject dans le sens du vecteur "forward"
    [PunRPC]
    public void LookAt(Vector3 forward)
    {
        this.transform.forward = forward;
    }

    private void OnDisable()
    {

    }
}
