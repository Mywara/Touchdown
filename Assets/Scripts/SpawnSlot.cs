using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSlot : MonoBehaviour {

    public SpawnPoint spawnPoint;
    private bool available = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {
        if(available)
        {
            available = false;
            spawnPoint.RemoveFromAvailiable(this.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        available = true;
        spawnPoint.AddToAvailiable(this.gameObject);
    }
}
