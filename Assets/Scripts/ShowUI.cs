using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PUNTutorial
{
    public class ShowUI : Photon.PunBehaviour
    {

        public Slider slider;
        // Use this for initialization
        void Start()
        {
            if (!photonView.isMine)
            {
                slider.enabled = false;
            }
        }


    }
}
