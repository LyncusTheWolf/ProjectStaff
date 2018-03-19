using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic {
	public class CopterSkill : Skill {

        [Header("Copter Controls")]
        public float copterUpdraftForce;                            //The initial force applied to the motor at the start of a copter manuever
        public float copterPassiveForce;                            //The passive force applied to the motor during a copter manuever
        public float copterInitialStaminaCost = 5.0f;               //The stamina cost to start a copter manuever
        public float copterSustainStaminaCost = 3.0f;               //The stamina cost per second to maintain a copter manuever

        private float copterPower;
        private bool inCopter = false;

        public override void Init() {

        }

        public void Update() {
            if (copterPower > 0.0f) {
                if (player.UseStamina(copterSustainStaminaCost * Time.deltaTime) == false) {
                    copterPower = 0.0f;
                    inCopter = false;
                }
            }

            if (copterPower <= 0.0f) {
                inCopter = false;
            }

            copterPower -= Time.deltaTime;
        }

        public override bool Evaluate(CharacterAnimator charAnimator, Rigidbody rigidBody) {
            if (motor.IsGrounded) {
                copterPower = 0.0f;
            } else {
                if (Input.GetButtonDown("Y Button")) {
                    if (inCopter == false && player.UseStamina(copterInitialStaminaCost)) {
                        charAnimator.PushCopter();
                        copterPower = 1.5f;
                        charAnimator.Anim.SetFloat("CopterPower", copterPower);
                        inCopter = true;
                        //motor.CurrentFrameInput.isEquipped = true;
                        motor.PushNewEquipState(true);
                        Vector3 copterVel = rigidBody.velocity;
                        copterVel.y = copterUpdraftForce;
                        rigidBody.velocity = copterVel;
                        return true;
                    }
                }
            }

            return false;
        }

        public override void InUse(CharacterAnimator charAnimator, Rigidbody rigidBody) {
            if (Input.GetButtonDown("Y Button")) {
                if (inCopter == true) {
                    copterPower += 0.5f;
                }
            }

            charAnimator.Anim.SetFloat("CopterPower", copterPower);
        }

        public override void PhysicsEffects(Rigidbody rigidBody, float delta) {
            rigidBody.velocity += Vector3.up * delta * copterPassiveForce * Mathf.Clamp(copterPower, 0.0f, 2.0f);
        }
    }
}
