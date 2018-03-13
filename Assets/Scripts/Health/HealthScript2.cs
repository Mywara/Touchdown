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
        private bool invulnerable = false;
        private float shield = 0; // doit être entre 0 et 1 (0 aucun shield / 1 zero degat reçu)

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

            //si l'on est invulnerable, on ne prend pas de degat
            if (invulnerable)
            {
                Debug.Log("invulnerable, can't take damage");
                return;
            }
            CurrentValue = CurrentValue - (int)((float)damage * ( 1-this.shield)); //gere le shield
            HealthSliderUI.value = CurrentValue;
        }

        //fonction pour heal la barre de vie du joueur local, celle en haut a gauche
        [PunRPC]
        public void Heal2(int heal)
        {
            CurrentValue = CurrentValue + heal;
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

        public bool Invulnerable
        {
            get
            {
                return this.invulnerable;
            }
            set
            {
                this.invulnerable = value;
            }
        }

        // setter pour le shield
        [PunRPC]
        public IEnumerator SetShieldTemporaire2(float s, float duree)
        {
            this.shield += s;
            yield return new WaitForSeconds(duree);
            this.shield -= s;
        }
    }
}
