using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour {

    static public Crystal instance;
    public bool isHeld;
    public GameObject playerHolding;
    public Vector3 startingPosition = new Vector3(-5f, 0.5653587f, 5f);

    private void OnTriggerEnter(Collider target)
    {
        

        if (target.transform.root.gameObject.tag == "Player")
        {
            //Debug.Log("crystal_player collider OK!");

            isHeld = true;

            //Debug.Log("player collider ok");

            playerHolding = target.transform.root.gameObject;
        }
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
    }
}
