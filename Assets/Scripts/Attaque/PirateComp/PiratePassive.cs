﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiratePassive : MonoBehaviour {

    private int hitStack = 0;
    private bool pendingCriticalHit = false;

    public void IncrementHitStack()
    {
        hitStack++;
        Debug.Log("Current hit stack : " + hitStack);

        if (hitStack >= Constants.PIRATE_HITSTACK)
        {
            Debug.Log("Pending pirate critical hit!");
            pendingCriticalHit = true;
        }
    }

    public bool PendingCriticalHit()
    {
        return pendingCriticalHit;
    }

    public void CriticalHitApplied()
    {
        Debug.Log("Pirate hit stack reset!");
        hitStack = 0;
        pendingCriticalHit = false;
    }
}
