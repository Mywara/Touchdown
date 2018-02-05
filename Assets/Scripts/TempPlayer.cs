using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUNTutorial
{
    public class TempPlayer : Photon.PunBehaviour
    {

        Camera playerCam;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            playerCam = GetComponentInChildren<Camera>();
        }
    }
}
