using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Winner : MonoBehaviour {
    
    public string winner = "I don't know who";
    public bool hasWon = false;
    public bool tie = false;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
