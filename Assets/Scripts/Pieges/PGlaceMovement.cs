using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PGlaceMovement : MonoBehaviour {

    public int distanceMaxForTrap = 10;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
       
        //permet de déplacer le gameObject de previsualisation a une distance max: distanceMaxForTrap 
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit);
        if (hit.transform != null) {
            if (hit.transform.tag == "Floor")
            {
                if (hit.distance < distanceMaxForTrap)
                {
                this.transform.position = hit.transform.position;
                }
            }
        }
    }
}
