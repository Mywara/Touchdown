using UnityEngine;
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
        if (Input.GetKeyDown(KeyCode.Space) && !immobilization)
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

        // TODO modifier cette condition (appel de la fonction SetImmobilization au bon moment (destruction du piege ?))
        //Le probleme etant que s'il n'y a pas de piege on peut bouger (peut importe si une competence veut immobiliser ou non)
        // En l'occurence une competence ne peut pas immobiliser s'il n'y a pas de piege

        //permet de bouger a nouveau lorsque le piege immobilisant est détruit
        //if (!activeTrap && immobilization)
        //{
        //    immobilization = false;
        //}

        if (!immobilization)
        {
            // translation perso
            float horizontal = Input.GetAxis("Horizontal");
            transform.Translate(horizontal * movementSpeed * Time.deltaTime, 0, 0);

            float vertical = Input.GetAxis("Vertical");
            transform.Translate(0, 0, vertical * movementSpeed * Time.deltaTime);

            // animation de déplacement en fonction des inputs horizontaux et verticaux (flèches directionnelles)
            if (PhotonNetwork.connected)
                photonView.RPC("Animate", PhotonTargets.All, horizontal, vertical);
            else
                Animate(horizontal, vertical);
        }
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
    public IEnumerator Stun(float duree)
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            yield return null;
        }
        else
        {
            // prive translation, rotation du perso et compétences du perso
            SetImmobilization(true);
            cameraFollowScript.StopCamera();
            SetActiveCompetence(false);

            // Attend la duree demandé
            yield return new WaitForSeconds(duree);

            // autorise translation, rotation du perso et compétences du perso
            SetImmobilization(false);
            cameraFollowScript.ActiveCamera();
            SetActiveCompetence(true);

        }
        

    }

    // Stun le perso
    public void SetImmobilization(bool immo)
    {
        this.immobilization = immo;
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
                // TODO les 3 comps
                break;

            default:
                Debug.Log("name : \"" + name + "\" ne correspond pas à WarBear / Undeath / Pirate " +
                    "\n voir Cname de CharacterCaractistique du prefab");
                break;
        }

    }

    [PunRPC]
    public void AddForceTo(Vector3 forceVector)
    {
        this.GetComponent<Rigidbody>().AddForce(forceVector);
    }
}
