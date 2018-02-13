using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PUNTutorial
{
    public class HealthScript : Photon.PunBehaviour
    {
        public Slider HealthSlider;
        private int HealthMax;

        public void Start()
        {
            //Set health depending on the character
            HealthMax = GetComponentInParent<CharacterCaracteristic>().health;
            //initialisation de la vie en fonction du personnage
            HealthSlider.maxValue = HealthMax;
            HealthSlider.value = HealthMax;
        }
        [PunRPC]
        public void Damage(int damage)
        {
            HealthSlider.value = HealthSlider.value - damage;
        }

        public void Update()
        {

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (photonView.isMine)
                {
                    photonView.RPC("Damage", PhotonTargets.All, 90);
                }
                //Damage(90);
            }

            if(HealthSlider.value <= 0)
            {
                //mort
                if (photonView.isMine)
                {
                    Debug.Log("Player died");
                    //ResetHealth();
                    //RoomManager.instance.RespawnPlayer(this.gameObject);
                    photonView.RPC("ResetHealth", PhotonTargets.All);
                    RoomManager.instance.photonView.RPC("RespawnPlayer", PhotonTargets.All, this.gameObject.GetPhotonView().viewID);
                }
                
            }
        }

        [PunRPC]
        public void ResetHealth()
        {
            HealthSlider.value = HealthMax;
        }

        [PunRPC]
        public void KillPlayer()
        {
            HealthSlider.value = 0;
        }
    }  
}
