using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailedHeroDescription : MonoBehaviour {

    public Canvas DetailedDescriptionCanvas;
    public GameObject PirateDescription;
    public GameObject UndeadDescription;
    public GameObject WarbearDescription;


	// Use this for initialization
	void Start ()
    {
        PirateDescription.SetActive(false);
        UndeadDescription.SetActive(false);
        WarbearDescription.SetActive(false);
        DetailedDescriptionCanvas.enabled = false;
	}
	
	public void SeePirateDetails()
    {
        DetailedDescriptionCanvas.enabled = true;
        PirateDescription.SetActive(true);
    }

    public void SeeUndeadDetails()
    {
        DetailedDescriptionCanvas.enabled = true;
        UndeadDescription.SetActive(true);
    }

    public void SeeWarbearDetails()
    {
        DetailedDescriptionCanvas.enabled = true;
        WarbearDescription.SetActive(true);
    }

    public void CloseDetails()
    {
        PirateDescription.SetActive(false);
        UndeadDescription.SetActive(false);
        WarbearDescription.SetActive(false);
        DetailedDescriptionCanvas.enabled = false;
    }
}
