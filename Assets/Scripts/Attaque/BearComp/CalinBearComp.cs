using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalinBearComp : Photon.PunBehaviour
{


    private Rigidbody rb;
    private Animator anim;
    public AudioSource audioSource;
    private PlayerController playerControllerScript;
    public float transparenceCD;
    public GameObject HUD;
    private Camera cam;
    private RaycastHit hit;
    

    /////////// CALIN STUFF


    public GameObject calinHUD; // UI pour la comp Calin
    public float calinCooldown; // temps du cooldown du calin en seconde
    public float calinDuree; // duree de la competence
    private GameObject calinCdMask;
    private float calinLastUse; // temps (en seconde) de derniere utilisation
    public float maxRange;
    public float calinShield = 0; // doit être entre 0 et 1 (0 aucun shield / 1 zero degat reçu)

    public GameObject effetArmor; // L'effet visuel

    public AudioClip SFXCalinDash; // L'audio

    private bool CalinActif = true;


    // Use this for initialization
    void Start()
    {
        if (photonView.isMine)
        {
            HUD.SetActive(true);
            calinCdMask = calinHUD.transform.Find("CooldownGreyMask").gameObject;
            calinCdMask.SetActive(false);
        }

        playerControllerScript = this.gameObject.GetComponent<PlayerController>();

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;


        ///////////// CALIN STUFF
        calinLastUse = -calinCooldown;

        /////////////

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true || !CalinActif)
        {
            return;
        }

        // Input du Calin
        if (Input.GetButtonDown("Skill3") && (Time.time > (calinLastUse + calinCooldown)))
        {
            // Si on cible sur quelqu'un d'assez près
            if (Physics.Raycast(cam.transform.position + cam.transform.forward * 2.5f, cam.transform.forward, out hit, 500))
            {
                
                if (!RoomManager.instance.FriendlyFire)
                {
                    if (hit.transform.tag.Equals("Player") && hit.distance < maxRange)
                    {
                        //hitPlayerControllerScript = script de la cible
                        //playerControllerScript = script de nous-même
                        PlayerController hitPlayerControllerScript = hit.transform.GetComponent<PlayerController>();

                        if (hitPlayerControllerScript != null)
                        {
                            if (hitPlayerControllerScript.Team != playerControllerScript.team )
                            {
                                
                                Debug.Log("Calin Competence");
                                DashCalin(hitPlayerControllerScript); // On lance le dash du calin
                                calinLastUse = Time.time;
                            }
                        }
                        else
                        {
                            Debug.Log("pas trouvé de script PlayerController sur la cible");
                        }
                    }
                }
            }
        }
        // fin Input calin
    }

    // Lance le dash
    private void DashCalin(PlayerController ennemiPlayerControllerScript)
    {

        //Joue le son de dash
        this.photonView.RPC("PlaySFXCalinDash", PhotonTargets.All);

        // Lance le CD sur l'affichage
        StartCoroutine("CalinAffichageCooldown");

        // Animation

        this.photonView.RPC("LanceAnimArmor", PhotonTargets.All);

        // On stun le lanceur et la cible
        playerControllerScript.photonView.RPC("Stun", PhotonTargets.All, calinDuree,false);
        ennemiPlayerControllerScript.photonView.RPC("Stun", PhotonTargets.All, calinDuree,true);

        // On donne le shield
        PUNTutorial.HealthScript healthScript = this.GetComponent<PUNTutorial.HealthScript>();
        PUNTutorial.HealthScript2 healthScript2 = this.GetComponent<PUNTutorial.HealthScript2>();

        healthScript.photonView.RPC("SetShieldTemporaire1", PhotonTargets.All, calinShield, calinDuree);
        healthScript2.photonView.RPC("SetShieldTemporaire2", PhotonTargets.All, calinShield, calinDuree);

        // Animation du dash
        StartCoroutine("DashTranslation");
    }

    [PunRPC]
    private void LanceAnimArmor()
    {
        GameObject effetA;
        effetA = Instantiate(effetArmor, this.transform.position, effetArmor.transform.rotation).gameObject as GameObject;
        if (effetA != null)
        {
            //On met en enfant du joueur l'effet, donc si le joueur bouge, l'effet le suit
            effetA.transform.SetParent(this.transform);
        }
    }

    // Animation du dash
    private IEnumerator DashTranslation()
    {
        float distance = 0.0f;
        distance = Vector3.Distance(this.transform.position, hit.transform.position);
        while (1.2 < distance && distance < maxRange+1 )
        {
            transform.Translate(10 * 1/maxRange * Time.deltaTime, 0, 10 * Time.deltaTime);
            yield return null;
            distance = Vector3.Distance(this.transform.position, hit.transform.position);
        }
    }

    // Sons

    [PunRPC]
    public void PlaySFXCalinDash()
    {
        //audioRPC.minDistance = 1;
        audioSource.maxDistance = 10;
        audioSource.clip = SFXCalinDash;
        audioSource.Play();
    }

    /////////////////////// Affichage Competences


    private IEnumerator CalinAffichageCooldown()
    {
        float dureeCD = calinCooldown;
        /*
        // Modifie la transparence
        Image image = calinHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = transparenceCD;
        image.color = c;
        */
        calinCdMask.SetActive(true);

        // Pour modifier le text
        Text t = calinHUD.GetComponentInChildren<Text>();
        
        while (dureeCD > 0)
        {
            dureeCD -= Time.deltaTime;
            //if (dureeCD <= 0)
            //{
            //    // TODO ?
            //}
            yield return new WaitForFixedUpdate();
            t.text = (Mathf.Floor(dureeCD) + 1).ToString();
        }
        /*
        // On remet la transparence normale
        c.a = 255;
        image.color = c;
        */
        calinCdMask.SetActive(false);
        t.text = "";
    }


    /////////////////// Autres méthodes


    public void SetCalinActif(bool b)
    {
        this.CalinActif = b;
    }

    private void OnDisable()
    {
        /*
        // On remet la transparence normale
        Image image = calinHUD.GetComponent<Image>();
        Color c = image.color;
        c.a = 255;
        image.color = c;
        */
        if (photonView.isMine)
        {
            if (calinCdMask)
            {
                calinCdMask.SetActive(false);
            }

            // On remet l'affichage du cooldown à rien (pas de CD)
            Text t = calinHUD.GetComponentInChildren<Text>();
            t.text = "";
        }

        // on reset le CD
        calinLastUse = 0;
    }
}
