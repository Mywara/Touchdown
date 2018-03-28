using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlay : MonoBehaviour {

    public Canvas howToPlayCanva;
	// Use this for initialization
	void Start () {
        howToPlayCanva.enabled = false;
	}
	
    public void PrintCanva()
    {
        howToPlayCanva.enabled = true;
    }

    public void EraseCanva()
    {
        howToPlayCanva.enabled = false;
    }

}
