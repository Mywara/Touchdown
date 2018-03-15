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
}
