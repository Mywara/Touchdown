using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiratePassive : Photon.PUNBehaviour
{

    private int hitStack = 0;
    private bool pendingCriticalHit = false;
    public GameObject animPiratePassif;

    private void Start()
    {
        this.photonView.RPC("AnimPassif", PhotonTargets.All, false);
    }

    public void IncrementHitStack()
    {
        hitStack++;

        if (hitStack >= Constants.PIRATE_HITSTACK)
        {
            //Debug.Log("Pending pirate critical hit!");
            pendingCriticalHit = true;
            this.photonView.RPC("AnimPassif", PhotonTargets.All, true);
        }
    }

    public bool PendingCriticalHit()
    {
        return pendingCriticalHit;
    }

    public void CriticalHitApplied()
    {
        hitStack = 0;
        pendingCriticalHit = false;
        this.photonView.RPC("AnimPassif", PhotonTargets.All, false);
    }

    [PunRPC]
    private void AnimPassif(bool b)
    {
        animPiratePassif.SetActive(b);
    }

    public void OnDisable()
    {
        //On reset la classe
        CriticalHitApplied();
    }
}
