using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic{
	public abstract class CharacterBase : Damagable {


        public float currentStamina;
        public float maxStamina = 15.0f;
        public float staminaRegen = 1.0f;

        protected override void Awake(){
            base.Awake();

            currentStamina = maxStamina;
        }

        public void Update() {
            float delta = Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina + delta, 0.0f, maxStamina);
            Tick(delta);
        }

        public float StaminaRatio() {
            return currentStamina / maxStamina;
        }

        public bool UseStamina(float delta) {
            if (delta > currentStamina) {
                return false;
            }

            currentStamina = Mathf.Clamp(currentStamina - delta, 0.0f, maxStamina);
            return true;
        }

        public abstract void Tick(float delta);
    }
}
