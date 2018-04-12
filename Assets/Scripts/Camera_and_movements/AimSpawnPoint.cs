using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimSpawnPoint : MonoBehaviour {

    public float offset_tir_vertical = 28;
    public float offset_tir_horizontal = 10;
    Camera cam;
    RaycastHit hit;
    // Use this for initialization
    void Start () {
        cam = Camera.main;
    }
	
	// Update is called once per frame
	void Update () {
        

        float distance; // la distance d'un point près du personnage jusqu'à la cible

        if (Physics.Raycast(cam.transform.position + cam.transform.forward * 2.5f, cam.transform.forward, out hit, 500))
        {
            //Instantiate(sourceObject, hit.point, Quaternion.identity);

            distance = hit.distance;
        }
        else
        {
            distance = 30;
        }

        Vector3 rotationVector = cam.transform.rotation.eulerAngles;
        rotationVector.y += (1 / distance) * offset_tir_horizontal; // axe horizontal de visée 
        rotationVector.x -= (1 / distance) * offset_tir_vertical; // axe vertical de visée

        this.transform.rotation = Quaternion.Euler(rotationVector);
    }
}
