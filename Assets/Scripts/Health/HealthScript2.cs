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
        public int HealthMax;
        private int CurrentValue;
        private bool invulnerable = false;
        private float shield = 0; // doit être entre 0 et 1 (0 aucun shield / 1 zero degat reçu)
        public RawImage hurtScreen; // L'image qui apparait à l'ecran pour indiquer qu'on est touché
        public RawImage healScreen;
        public float transparenceMaxImage;
        public float dureeAppEffet = 0.2f;
        public float dureeDispEffet = 0.4f;


        void Awake()
        {

            if (!photonView.isMine)
            {
                HealthSliderUI.transform.parent.gameObject.SetActive(false);
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
                //Debug.Log("invulnerable, can't take damage");
                return;
            }
            // Lance l'effet hurt sur l'ecran (bords rouge)
            StartCoroutine("AnimeBordScreen",hurtScreen);

            CurrentValue = CurrentValue - (int)((float)damage * ( 1-this.shield)); //gere le shield
            HealthSliderUI.value = CurrentValue;
        }

        //fonction pour heal la barre de vie du joueur local, celle en haut a gauche
        [PunRPC]
        public void Heal2(int heal)
        {
            // Lance l'effet hurt sur l'ecran (bords blanc)
            StartCoroutine("AnimeBordScreen", healScreen);



            CurrentValue = CurrentValue + heal;
            // Pour éviter d'avoir plus de vie que le max
            if (CurrentValue > HealthMax)
            {
                CurrentValue = HealthMax;
            }
            HealthSliderUI.value = CurrentValue;

            
        }

        /*  [PunRPC]
          public void UpdateBar()
          {

              HealthSliderUI.value = CurrentValue;
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

        public void EmptyBar()
        {
            HealthSliderUI.value = 0;
            CurrentValue = 0;
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

        private IEnumerator AnimeBordScreen(RawImage img)
        {
            float tempsApp = dureeAppEffet;
            float tempsDisp = dureeDispEffet;

            Color c = img.color;
            

            while (tempsApp > 0)
            {
                // On change la transparence de l'image
                c.a = transparenceMaxImage * (1- (tempsApp/dureeAppEffet));
                img.color = c;
                yield return null;
                tempsApp -= Time.deltaTime;
            }

            while (tempsDisp > 0)
            {
                // On change la transparence de l'image
                c.a = transparenceMaxImage * (tempsDisp / dureeDispEffet);
                img.color = c;
                yield return null;
                tempsDisp -= Time.deltaTime;
            }
            c.a = 0;
            img.color = c;
        }



        private void OnDisable()
        {
            // reset du shield
            this.shield = 0;

            // reset de la transparence des effets sur les bords
            // pour le heal
            Color c = this.healScreen.color;
            c.a = 0;
            this.healScreen.color = c;
            // pour le hurt
            c = this.hurtScreen.color;
            c.a = 0;
            this.hurtScreen.color = c;
        }
    }
}
