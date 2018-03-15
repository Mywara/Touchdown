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
                        this.transform.position = hit.transform.position;
                    }
                }
            }
        }
        else
        {
            //le gameobject suit la souris en restant sur le sol
            Vector3 pos = Input.mousePosition;
            /*RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)), out hit);
            pos.z = hit.point.y;
            Debug.Log("distance" + hit.point.y);
            Debug.DrawRay(ray.origin, ray.direction, Color.green);*/
            pos.z = 11.8f;
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
            {
                pos.z -= 0.1f; 
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
            {
                pos.z += 0.1f;
            }
            this.transform.position = Camera.main.ScreenToWorldPoint(pos);

        }
            
    }
}
