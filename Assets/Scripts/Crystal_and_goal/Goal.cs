using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

    public GameObject crys;

    private void OnTriggerEnter(Collider target)
    {


        //Debug.Log(target.transform.root.gameObject.name);

        if (target.transform.root.gameObject.tag == "Player"
            && crys.GetComponent<Crystal>().isHeld == true
            && crys.GetComponent<Crystal>().playerHolding == target.transform.root.gameObject)
        {
            //Debug.Log("Crystal is in goal collider OK!");

            if (crys.GetComponent<Crystal>().isHeld)
            {
                crys.GetComponent<Crystal>().isHeld = false;
                crys.GetComponent<Crystal>().playerHolding = null;
            }

            crys.GetComponent<Crystal>().transform.position = crys.GetComponent<Crystal>().startingPosition;

        }

        
    }

    //// Use this for initialization
    //void Start () {

    //}

    //// Update is called once per frame
    //void Update () {

    //}
}
