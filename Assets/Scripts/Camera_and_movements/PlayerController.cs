using UnityEngine;
using System.Collections;

public class PlayerController : Photon.PunBehaviour
{
    public float originaleMovementSpeed = 10;
    private float movementSpeed;

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
    public GameObject AnimStun;

    private bool isCursed = false;
    private float lastCurseHit; // sert à savoir quand la cible a été maudit pour la dernière fois
    public GameObject AnimCurse;

    public GameObject AnimConfu;

    public AudioSource audioSource;
    public AudioClip jumpSnd;



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

        movementSpeed = originaleMovementSpeed;
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
                    {
                        photonView.RPC("JumpAnimation", PhotonTargets.All);
                    }



                    else
                    {
                        JumpAnimation();
                    }
                    this.photonView.RPC("JumpSFX", PhotonTargets.All);

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
		//Si le personnage est maudit
		if(isCursed){
			//Si on a atteint la fin du temps de malédiction
			//Alors supprimer la malédiction sur le personnage
			if (Time.time > lastCurseHit + Constants.CURSEDOT_DURATION)
			{
				photonView.RPC("FinCurse", PhotonTargets.All);
			}
			//dans le cas contraire propager la malédiction aux alliés sains proche
			else
			{
				//on crée ici la zone de propagation correspondant à une sphere autour du personnage
				Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, 2f);
				for(int i=0; i<hitColliders.Length; i++)
				{
					GameObject objInAOE = hitColliders[i].transform.root.gameObject;
					//Touche uniquement les alliés présents dans la zone
					if (objInAOE.tag.Equals("Player") && objInAOE.GetComponent<PlayerController>().team == team)
					{
						//Si l'allié n'est pas maudit lui appliquer la malédiction
						if (!objInAOE.GetComponent<PlayerController>().Cursed())
						{
							objInAOE.GetComponent<PlayerController>().Curse();
							Debug.Log("L'allié "+objInAOE.name+" a été maudit par propagation.");
						}
					}
				}
			}
		}


        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        /*
        if (Input.GetKeyDown(KeyCode.L))
        {
            inGame = !inGame;
        }
        */
        if (inGame)
        {

            // gestion du stun (on utilise mobile comme condition 
            if (isStun && Time.time > timeStun)
            {
                this.photonView.RPC("FinStun",PhotonTargets.All);
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
        if (pourcentageVitesse < 0)
        {
            // Début animation confusion
            AnimConfu.SetActive(true);
        }
        movementSpeed = movementSpeed * (pourcentageVitesse / 100);
        yield return new WaitForSeconds(duree);
        movementSpeed = movementSpeed / (pourcentageVitesse / 100);

        // Fin animation confusion
        AnimConfu.SetActive(false);
    }

    [PunRPC]
    public void DebutModifVitesse(float pourcentageVitesse)
    {
        if (pourcentageVitesse < 0)
        {
            //Début animation confusion
            AnimConfu.SetActive(true);
        }

        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        movementSpeed = movementSpeed * (pourcentageVitesse / 100);
    }

    [PunRPC]
    public void FinModifVitesse(float pourcentageVitesse)
    {
        // Fin animation confusion
        AnimConfu.SetActive(false);

        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

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
    public void Stun(float duree, bool withAnim)
    {

        // On met l'animation du stun s'il la faut
        if (withAnim)
        {
            AnimStun.SetActive(true);
        }

        // Pour éviter un accès à la caméra des autres joueurs (inaccessibles)
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

    [PunRPC]
    private void FinStun()
    {
        // On arrete l'animation
        AnimStun.SetActive(false);

        // Pour éviter un accès à la caméra des autres joueurs (inaccessibles)
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }

        // autorise translation, rotation du perso et compétences du perso
        SetMobile(true);
        cameraFollowScript.ActiveCamera();
        SetActiveCompetence(true);
        SetActiveAutoAtt(true);

        isStun = false;
    }


    // Applique la malédiction au personnage
    [PunRPC]
    public void Curse()
    {
        AnimCurse.SetActive(true);
        isCursed = true;
        lastCurseHit = Time.time;
    }

    // Enlève la malédiction au personnage
    [PunRPC]
    public void FinCurse()
    {
        AnimCurse.SetActive(false);
        isCursed = false;
    }

    public void ResetCurseTimer()
    {
        lastCurseHit = Time.time;
    }

    public bool Cursed()
    {
        return isCursed;
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

                BearComp bc = GetComponent<BearComp>();
                bc.SetCompBearActives(b);
                break;

            case "Undeath":
                UndeadComp udc = GetComponent<UndeadComp>();
                udc.SetCompActives(b);
                break;

            case "Pirate":
                TirClochePirateComp tcpc = GetComponent<TirClochePirateComp>();
                tcpc.SetTirClocheActif(b);

                PirateComp pc = GetComponent<PirateComp>();
                pc.SetCompActives(b);
                break;

            default:
                Debug.Log("le name : \"" + name + "\" utilisé pour le stun ne correspond pas à WarBear / Undeath / Pirate " +
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

        if (isStun)
        {
            this.photonView.RPC("FinStun", PhotonTargets.All);
        }

        if (isCursed)
        {
            this.photonView.RPC("FinCurse", PhotonTargets.All);
        }

        movementSpeed = originaleMovementSpeed;
        AnimConfu.SetActive(false);
        AnimStun.SetActive(false);
        AnimCurse.SetActive(false);
    }

    public void SwitchPlayerMode()
    {
        inGame = !inGame;
    }

    [PunRPC]
    public void JumpSFX()
    {
        //audioRPC.minDistance = 1;
        audioSource.maxDistance = 7;
        audioSource.clip = jumpSnd;
        audioSource.Play();
    }
}
