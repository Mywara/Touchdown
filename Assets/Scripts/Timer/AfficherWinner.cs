using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AfficherWinner : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.transform.GetChild(1).GetComponent<Text>().text = "winner is" + GameObject.Find("WinnerManager").GetComponent<Winner>().winner;
    }
	

}
