using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

    public List<GameObject> spawnSlots;

    private List<GameObject> spawnSlotsAvailiable;

    private void Awake()
    {
        spawnSlotsAvailiable = spawnSlots;
    }
    // Use this for initialization
    void Start () {     
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void AddToAvailiable(GameObject spawnslot)
    {
        spawnSlotsAvailiable.Add(spawnslot);
    }

    public void RemoveFromAvailiable(GameObject spawnslot)
    {
        spawnSlotsAvailiable.Remove(spawnslot);
    }

    public GameObject[] SpawnSlotsAvailiable
    {
        get
        {
            return this.spawnSlotsAvailiable.ToArray();
        }
    }
}
