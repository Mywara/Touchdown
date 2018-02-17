using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : Photon.PUNBehaviour
{
    public int CountdownTime = 600;
    private Coroutine CountdownTCourou = null;
    // Use this for initialization
    void Start () {
        //lance la coroutine une fois depuis le masterClient (possible depuis le serveur ?)
        photonView.RPC("StartCountdownTime", PhotonTargets.All);
        
    }	

    IEnumerator CountdownT(float CountdownTimeL)
    {
        while (CountdownTimeL > 0)
        {
            CountdownTimeL -= Time.deltaTime;
            if (CountdownTimeL <= 0)
            {
                PhotonNetwork.LoadLevel("EndScene");
            }
            yield return new WaitForFixedUpdate();
            this.transform.GetChild(1).GetComponent<Text>().text = "Remaining Time : \n" + string.Format("{0:0}:{1:00}", Mathf.Floor(CountdownTimeL / 60), CountdownTimeL % 60);
        }
    }

    [PunRPC]
    private void StartCountdownTime()
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
}
