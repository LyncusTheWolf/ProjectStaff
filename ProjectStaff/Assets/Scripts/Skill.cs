using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic{
    [RequireComponent(typeof(CharacterMotor))]
	public abstract class Skill: MonoBehaviour {

        public bool isKnown;

        protected CharacterMotor motor;
        protected Player player;

        public void Awake() {
            motor = GetComponent<CharacterMotor>();
            motor.RegisterSkill(this);
            player = GetComponent<Player>();
            Init();
        }

        public abstract void Init();
        public abstract bool Evaluate(CharacterAnimator charAnimator, Rigidbody rigidBody);
        public abstract void InUse(CharacterAnimator charAnimator, Rigidbody rigidBody);
        public abstract void PhysicsEffects(Rigidbody rigidBody, float delta);
	}
}
