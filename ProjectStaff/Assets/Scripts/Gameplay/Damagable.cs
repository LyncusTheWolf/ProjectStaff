using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic {
    [RequireComponent(typeof(Collider))]
	public abstract class Damagable : MonoBehaviour {

        public uint startingHealth = 3;

        protected uint currentHealth;
        protected uint maxHealth;

        protected bool isDead;
        protected Collider objCollider;

        //Use this at start up
        protected virtual void Awake () {
            maxHealth = startingHealth;
            currentHealth = maxHealth;
            isDead = false;
            objCollider = GetComponent<Collider>();
            Init();
		}

        public void Damage(uint amt) {
            if (isDead) {
                return;
            }

            if(amt > currentHealth) {
                currentHealth = 0;
            } else {
                currentHealth -= amt;
            }

            if(currentHealth == 0) {
                isDead = true;
                OnDeath();
            }
        }

        public abstract void Init();
        public abstract void OnDeath();
    }
}
