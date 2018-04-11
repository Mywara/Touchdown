using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCrystalRotation : MonoBehaviour {

    public float rotation = 1f;
    
	
	// Update is called once per frame
	void Update () {
        Quaternion newRot = transform.rotation;
        Vector3 euler = newRot.eulerAngles;
        euler.y += rotation;
        newRot.eulerAngles = euler;
        transform.rotation = newRot;
    }
}
