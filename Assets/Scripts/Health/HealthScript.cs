using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PUNTutorial
{
    public class HealthScript : Photon.PunBehaviour
    {
        public Slider HealthSlider;
        public int HealthMax;
        public GameObject animHeal;

        private bool dying = false;
        private Animator anim;
        private bool netWorkingDone = false;
        private bool invulnerable = false;
        private float shield = 0;       // doit être entre 0 et 1 (0 aucun shield / 1 zero degat reçu)
        private bool cursed = false;    // affecté ou non par le Curse DoT du Undead

        void Awake()
        {
            anim = GetComponent<Animator>();

            if(photonView.isMine)
            {
                HealthSlider.transform.parent.gameObject.SetActive(false);
            }
            
            //Set health depending on the character
            HealthMax = GetComponentInParent<CharacterCaracteristic>().health;
            //initialisation de la vie en fonction du personnage
            HealthSlider.maxValue = HealthMax;
            HealthSlider.value = HealthMax;
            //Debug.Log(this.gameObject.name + " AwakeEnded");
        }

        void Start()
        {
            PlayerController localPlayerController = PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>();
            PlayerController thisPlayerController = gameObject.GetComponent<PlayerController>();
            Image healthUIFillImage = HealthSlider.transform.Find("Fill Area").gameObject.GetComponentInChildren<Image>();

            // if this player is in the same team as our local player
            if(localPlayerController.Team == thisPlayerController.Team)
            {
                healthUIFillImage.color = new Color(.2f, .6f, 1, .7f); // the slider will be light blue
            }
            else
            {
                healthUIFillImage.color = new Color(1, .2f, .2f, .7f); // the slider will be light red
            }
        }

        [PunRPC]
        public void Damage(int damage)
        {
            //si l'on est invulnerable, on ne prend pas de degat
            if(invulnerable)
            {
                Debug.Log("invulnerable, can't take damage");
                return;
            }

            HealthSlider.value = HealthSlider.value - (int)((float)damage * (1 - this.shield)); //gere le shield
            anim.SetTrigger("Damage");
        }

        //fonction pour heal la barre de vie au dessus du joueur
        [PunRPC]
        public void Heal(int heal)
        {
            HealthSlider.value = HealthSlider.value + heal;

            // Pour éviter d'avoir plus de vie que le max
            if (HealthSlider.value > HealthMax)
            {
                HealthSlider.value = HealthMax;
            }

            //Lance l'animation de soin
            GameObject effetA;
            effetA = Instantiate(animHeal, this.transform.position + animHeal.transform.position, animHeal.transform.rotation).gameObject as GameObject;
            if (effetA != null)
            {
                //On met en enfant du joueur l'effet, donc si le joueur bouge, l'effet le suit
                effetA.transform.SetParent(this.transform);
            }
        }

        void Update()
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
                //Cette partie est maintenant faite dans la méthode respawn
                /*
                //reset la vie au dessus du perso en reseau
                photonView.RPC("ResetHealth", PhotonTargets.All);
                //reset la vie du client local en haut a gauche
                GetComponent<HealthScript2>().ResetHealth();
                */
                dying = false;
                RoomManager.instance.photonView.RPC("RespawnPlayer", PhotonTargets.All, PhotonNetwork.player.ID);
                //lache le crystal s'il était tenu par le joueur
                if (Crystal.instance.playerHolding == gameObject)
                {
                    Crystal.instance.photonView.RPC("LeaveOnGround", PhotonTargets.All);
                }
            }
            else
            {
                dying = false;
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
        public IEnumerator SetShieldTemporaire1(float s, float duree)
        {
            this.shield += s;
            yield return new WaitForSeconds(duree);
            this.shield -= s;
        }

        // Curse DoT de l'Undead
        [PunRPC]
        public void Curse()
        {
            cursed = true;
        }

        [PunRPC]
        public void EndCurse()
        {
            cursed = false;
        }

        public bool isCursed()
        {
            return cursed;
        }
    }  
}
