using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PGlaceMovement : MonoBehaviour {

    public int distanceMaxForTrap = 10;
    public bool inGame = true;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (inGame)
        {
            //permet de déplacer le gameObject de previsualisation a une distance max: distanceMaxForTrap 
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit);
            if (hit.transform != null)
            {
                if (hit.transform.tag == "Floor")
                {
                    if (hit.distance < distanceMaxForTrap)
                    {
                        this.transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + 0.5f, hit.transform.position.z);
                    }
                }
            }
        }
        else
        {

            //le gameobject suit la souris en restant sur le sol
            Vector3 pos = Input.mousePosition;
            RaycastHit[] hits;
            //pose les pieges case par case depuis le centre camera
           // Ray ray = new Ray(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)), Vector3.down);
           Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray, Mathf.Infinity);
            foreach(RaycastHit hit in hits)
            {
                if (hit.collider.transform.tag == "Floor")
                {
                    this.transform.position = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y + 0.5f, hit.collider.transform.position.z);
                }

              
            }

        }

    }
}
