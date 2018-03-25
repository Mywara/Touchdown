using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : Photon.PUNBehaviour
{
    static public Timer instance;
    public float CountdownTime = 600F;
    private Coroutine CountdownTCourou = null;
    private float gameTimerValueSaved;
    // Use this for initialization

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("there is already a timer script");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    void Start () {
        //lance la coroutine une fois depuis le masterClient (possible depuis le serveur ?)
        //photonView.RPC("StartCountdownTime", PhotonTargets.All);
        
    }	

    IEnumerator CountdownT(float CountdownTimeL)
    {
        gameTimerValueSaved = CountdownTimeL;

        while (gameTimerValueSaved > 0)
        {
            gameTimerValueSaved -= Time.deltaTime;
            if (gameTimerValueSaved <= 0 && RoomManager.instance.IsInPlayPhase())
            {
                PhotonNetwork.LoadLevel("EndScene");
            }
            
            if(gameTimerValueSaved <0)
            {
                gameTimerValueSaved = 0;
            }
            this.transform.GetChild(1).GetComponent<Text>().text = "Remaining Time : \n" + string.Format("{0:0}:{1:00}", Mathf.Floor(gameTimerValueSaved / 60), gameTimerValueSaved % 60);
            yield return new WaitForFixedUpdate();
        }
    }

    [PunRPC]
    public void StartCountdownTime()
    {
        if (CountdownTCourou == null) { 
       CountdownTCourou =  StartCoroutine(CountdownT(CountdownTime));
        }
        else //un nouveau joueur est connecté, on stop la coroutine en cours et on la redémare
        {
            StopCoroutine(CountdownTCourou);
            CountdownTCourou = StartCoroutine(CountdownT(CountdownTime));
        }
    }

    [PunRPC]
    public void StartCustomCountdownTime(float cdTime)
    {
        if (CountdownTCourou == null)
        {
            CountdownTCourou = StartCoroutine(CountdownT(cdTime));
        }
        else //un nouveau joueur est connecté, on stop la coroutine en cours et on la redémare
        {
            StopCoroutine(CountdownTCourou);
            CountdownTCourou = StartCoroutine(CountdownT(cdTime));
        }
    }

    public float GameTimerSaved
    {
        get
        {
            return this.gameTimerValueSaved;
        }
    }
}
