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
        public bool dying = false;
        private Animator anim;
        private bool netWorkingDone = false;
       
        void Awake()
        {
            anim = GetComponent<Animator>();
            
            //Set health depending on the character
            HealthMax = GetComponentInParent<CharacterCaracteristic>().health;
            //initialisation de la vie en fonction du personnage
            HealthSlider.maxValue = HealthMax;
            HealthSlider.value = HealthMax;
            //Debug.Log(this.gameObject.name + " AwakeEnded");
        }

        [PunRPC]
        public void Damage(int damage)
        {
            HealthSlider.value = HealthSlider.value - damage;
            anim.SetTrigger("Damage");
        }

        //fonction pour heal la barre de vie au dessus du joueur
        [PunRPC]
        public void Heal(int heal)
        {
            HealthSlider.value = HealthSlider.value + heal;
        }

        public void Update()
        {
            if (HealthSlider.value <= 0 && !dying)
            {
                StartCoroutine("RespawnAfterDeath");
            }
        }

        IEnumerator RespawnAfterDeath()
        {
            dying = true;

            // laisse le temps à l'animation de mort d'être jouée
            anim.SetTrigger("Die");
            yield return new WaitForSeconds(1.4f);

            // on respawn le joueur
            if (photonView.isMine)
            {
                //Debug.Log("Player died");
                //reset la vie au dessus du perso en reseau
                photonView.RPC("ResetHealth", PhotonTargets.All);
                //reset la vie du client local en haut a gauche
                GetComponent<HealthScript2>().ResetHealth();
                RoomManager.instance.photonView.RPC("RespawnPlayer", PhotonTargets.All, PhotonNetwork.player.ID);
            }

            dying = false;
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

        [PunRPC]
        public void SynchroHealth(int health)
        {
            this.HealthSlider.value = health;
            Debug.Log(this.gameObject.name + " RPC sync Health called : " + health);
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            if(photonView.isMine)
            {
                Debug.Log(this.gameObject.name + " HealthScript : player connected");
                photonView.RPC("SynchroHealth", PhotonTargets.All, (int)this.HealthSlider.value);
            }
        }
    }  
}
