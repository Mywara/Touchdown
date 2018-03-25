using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSceneManager : MonoBehaviour {

    public float endSceneTimer = 10F;

    private float startTime;
	// Use this for initialization
	void Start () {
        startTime = Time.time;
        Debug.Log("endScene");
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time > startTime + endSceneTimer)
        {
            PhotonNetwork.LeaveRoom();
        }
	}
}
