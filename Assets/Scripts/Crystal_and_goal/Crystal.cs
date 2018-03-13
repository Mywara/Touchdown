using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{

    static public Crystal instance;
    public bool isHeld;
    public GameObject playerHolding;
    public GameObject justDroppedCrystal;
    public Vector3 startingPosition = new Vector3(-5f, 0.5653587f, 5f);
    public int pickupCooldown = 3;

    private void OnTriggerEnter(Collider target)
    {


        if (target.transform.root.gameObject.tag == "Player" && target.transform.root.gameObject != justDroppedCrystal)
        {
            //Debug.Log("crystal_player collider OK!");

            isHeld = true;

            //Debug.Log("player collider ok");

            playerHolding = target.transform.root.gameObject;
        }
    }

    public void LeaveOnGround()
    {
        //resets the crystal without reinitializing it at its starting point
        playerHolding = null;
        isHeld = false;
    }

    private void ResetPreviousPlayer()
    {
        //resets the player name kept in memory in order to settle a cooldown on picking up the crystal again
        justDroppedCrystal = null;
    }

    // Use this for initialization
    void Start()
    {
        //Debug.Log("crystal initialized");


        isHeld = false;
        this.transform.position = startingPosition;
    }

    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        if (isHeld == true)
        {
            //Debug.Log("following playerHolding ok");

            this.transform.position = playerHolding.gameObject.transform.position;
        }

        if (justDroppedCrystal != null)
        {

            // 3 seconds cooldown on picking up the crystal
            Invoke("ResetPreviousPlayer", pickupCooldown);

        }

    }
}
