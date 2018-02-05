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

        public void Damage(int damage)
        {
            HealthSlider.value = HealthSlider.value - damage;
        }

        public void Update()
        {

            if (Input.GetKeyDown(KeyCode.I))
            {
                Damage(90);
            }

            if(HealthSlider.value <= 0)
            {
                //mort
            }
        }

    }  
}
