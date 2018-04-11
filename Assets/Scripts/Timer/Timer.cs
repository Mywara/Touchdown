using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : Photon.PUNBehaviour
{
    static public Timer instance;
    public float CountdownTime = 600F;

    private ScoreUpdate scoreUpdate;
    private Coroutine CountdownTCourou = null;
    private float gameTimerValueSaved;
    private Text timerText;


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
        scoreUpdate = gameObject.GetComponent<ScoreUpdate>();
        timerText = this.transform.Find("TopPanel/TimerPanel/Timer").GetComponent<Text>();
    }	

    IEnumerator CountdownT(float CountdownTimeL)
    {
        gameTimerValueSaved = CountdownTimeL;

        while (gameTimerValueSaved > 0)
        {
            gameTimerValueSaved -= Time.deltaTime;
            if (gameTimerValueSaved <= 0 && RoomManager.instance.IsInPlayPhase() && PhotonNetwork.isMasterClient)
            {
                Debug.Log("Partie terminée par le temps");
                photonView.RPC("GoToEndScene", PhotonTargets.AllViaServer);
            }
            
            if(gameTimerValueSaved <0)
            {
                gameTimerValueSaved = 0;
            }
            timerText.text = string.Format("{0:0}:{1:00}", Mathf.Floor(gameTimerValueSaved / 60), gameTimerValueSaved % 60);
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

    [PunRPC]
    private void GoToEndScene()
    {
        scoreUpdate.SetWinner();
        PhotonNetwork.LoadLevel("EndScene");
    }
}
