using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : Photon.PUNBehaviour
{

    static public Crystal instance;
    public bool isHeld;
    //player currently holding the crystal
    public GameObject playerHolding;
    //player who just dropped the crystal
    public GameObject justDroppedCrystal;
    public Vector3 startingPosition = new Vector3(0f, 1.5f, 0f);
    public int pickupCooldown = 3;
    // lumière
    private Light volumetricLight;
    private Color couleurRouge;
    private Color couleurBleue;
    private Color couleurOrange;
    public int rougeIntensite;
    public int bleuIntensite;
    public int orangeIntensite;



    [PunRPC]
    public void SetStartingPosition(Vector3 pos)
    {
        startingPosition = pos;
    }


    [PunRPC]
    public void PickupCrystal(int playerViewID)
    {
        //Debug.Log("crystal picked up");
        GameObject o = PhotonView.Find(playerViewID).gameObject;

        isHeld = true;
        playerHolding = o;
        
        PlayerController playerHoldingScript = playerHolding.GetComponent<PlayerController>();
        if (playerHoldingScript.team == PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team)
        {
            SetCouleurLight(couleurBleue, bleuIntensite);
        }
        else
        {
            SetCouleurLight(couleurRouge, rougeIntensite);
        }

        //tests
        //SetCouleurLight(couleurBleue, bleuIntensite);
        //fin test
    }

    [PunRPC]
    public void UpdateJustDroppedCrystal()
    {
        justDroppedCrystal = playerHolding;
    }


    [PunRPC]
    public void ResetCrystalPosition()
    {
        Vector3 pos = this.transform.position;
        pos = startingPosition;
        this.transform.position = pos;
    }


    [PunRPC]
    public void LeaveOnGround()
    {
        Vector3 pos = this.transform.position;
        pos.y = 0.8f;

        //resets the crystal without reinitializing it at its starting point
        playerHolding = null;
        isHeld = false;

        //reset la couleur du halo
        SetCouleurLight(couleurOrange, orangeIntensite);

        this.transform.position = pos;
    }



    private void ResetPreviousPlayer()
    {
        //resets the player name kept in memory in order to settle a cooldown on picking up the crystal again
        justDroppedCrystal = null;
    }

    void Awake()
    {

        if (instance != null && instance != this)
        {
            Debug.Log("There is already a Crystal");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        //Debug.Log("crystal initialized");


        isHeld = false;

        // On set les couleurs
        couleurRouge = new Vector4(1, 0, 0, 1);
        couleurBleue = new Vector4(0, 5f / 255f, 1, 1);
        couleurOrange = new Vector4(246f / 255f, 211f / 255f, 20f / 255f, 1);


        //On récupère le halo lumineux
        volumetricLight = GetComponentInChildren<Light>();
        // On lui donne la couleur qu'on veut
        SetCouleurLight(couleurOrange, orangeIntensite);

        //tests
        //SetCouleurLight(couleurBleue, bleuIntensite);
        //SetCouleurLight(couleurRouge, rougeIntensite);
        // fin tests
    }

    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        if (isHeld == true)
        {
            //Debug.Log("following playerHolding ok");
            Vector3 pos = playerHolding.gameObject.transform.position;
            pos.y += 1f;

            this.transform.position = pos;
        }

        if (justDroppedCrystal != null)
        {

            // 3 seconds cooldown on picking up the crystal
            Invoke("ResetPreviousPlayer", pickupCooldown);

        }

    }

    private void SetCouleurLight(Color c, int intensite)
    {
        volumetricLight.color = c;
        volumetricLight.intensity = intensite;
    }
}
