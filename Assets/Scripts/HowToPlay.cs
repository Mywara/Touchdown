using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlay : MonoBehaviour {

    public Canvas howToPlayCanvas;


	// Use this for initialization
	void Start () {
        howToPlayCanvas.enabled = false;
	}
	
    public void PrintCanvas()
    {
        howToPlayCanvas.enabled = true;
    }

    public void EraseCanvas()
    {
        howToPlayCanvas.enabled = false;
    }
}
