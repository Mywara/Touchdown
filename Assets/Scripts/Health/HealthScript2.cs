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
        private int HealthMax;
        private int CurrentValue;


        void Awake()
        {

            if (!photonView.isMine)
            {
                HealthSliderUI.transform.parent.gameObject.SetActive(false);
                //Debug.Log(this.gameObject.name +" : Not local player disable health bar");
                return;
            }
            HealthMax = GetComponentInParent<CharacterCaracteristic>().health;
            HealthSliderUI.maxValue = HealthMax;
            HealthSliderUI.value = HealthMax;
            CurrentValue = HealthMax;
        }

        [PunRPC]
        public void Damage2(int damage)
        {
            /*
            if (!photonView.isMine)
            {
                return;
            }
            */
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

        public void ResetHealth()
        {
            HealthSliderUI.value = HealthMax;
            CurrentValue = HealthMax;
        }
    }
}
