using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PUNTutorial
{
    public class HealthScript2 : Photon.PunBehaviour
    {

        // Use this for initialization
        private GameObject PlayerHealth;
        public Slider HealthSliderUI;
        private float HealthMax;
        private float CurrentValue;


        void Start()
        {

            if (!photonView.isMine)
            {
                return;
            }
            HealthMax = GetComponentInParent<CharacterCaracteristic>().health;
            HealthSliderUI.maxValue = HealthMax;
            HealthSliderUI.value = HealthMax;
            CurrentValue = HealthMax;
        }

        [PunRPC]
        public void Damage2(float damage)
        {
            if (!photonView.isMine)
            {
                return;
            }
           // HealthSliderUI.value = HealthSliderUI.value - damage;
            CurrentValue = CurrentValue - damage;
            HealthSliderUI.value = CurrentValue;
        }

      /*  [PunRPC]
        public void UpdateBar()
        {

            HealthSliderUI.value = CurrentValue;
            Debug.Log("current value " + CurrentValue);
        }
        // Update is called once per frame*/
        void Update()
        {
            if (!photonView.isMine)
            {
                return;
            }
           // photonView.RPC("Dama", RPCMode.AllBuffered, HealthSliderUI, HealthSliderUI.value);

        }
    }
}
