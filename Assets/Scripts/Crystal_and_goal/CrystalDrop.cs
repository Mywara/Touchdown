using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalDrop : MonoBehaviour
{


    public GameObject crys;

    //// Use this for initialization
    //void Start () {

    //}

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Drop")
            && crys.GetComponent<Crystal>().playerHolding == this.transform.root.gameObject
            && crys.GetComponent<Crystal>().isHeld == true)
        {

            Debug.Log("Le boutton Drop est pressé");

            crys.GetComponent<Crystal>().justDroppedCrystal = crys.GetComponent<Crystal>().playerHolding;

            crys.GetComponent<Crystal>().LeaveOnGround();

        }

    }
}
