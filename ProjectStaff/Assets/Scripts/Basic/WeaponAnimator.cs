using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic {
    [RequireComponent(typeof(Animator))]
    public class WeaponAnimator : MonoBehaviour {

        [System.Serializable]
        public struct WeaponStats {
            public uint damageBase;
        }

        public struct WeaponAction {
            public WeaponFilter filterEvent;
            public WeaponColliderEvent weaponEvent;
        }

        public delegate bool WeaponFilter(Collider other);
        public delegate void WeaponColliderEvent(Collider other, WeaponStats stats);

        public float startingWeaponLength;

        public WeaponStats weaponStats;
        public WeaponCollider weaponCollider;

        private float weaponLength;
        private Animator anim;

        private int weaponLengthHash;

        private WeaponAction currentAction;

        public float WeaponLength {
            get { return weaponLength; }
            set {
                weaponLength = (value > 0.5f) ? value : startingWeaponLength;
            }
        }

        public WeaponAction CurrentAction {
            get { return currentAction; }
        }

        public void Awake() {
            anim = GetComponent<Animator>();
            weaponLength = startingWeaponLength;

            weaponCollider.RegisterParent(this);

            weaponLengthHash = Animator.StringToHash("Length");
        }

        // Update is called once per frame
        void Update() {
            anim.SetFloat(weaponLengthHash, weaponLength);
        }

        public void SetAction(WeaponAction actionData) {
            currentAction = actionData;
        }

        public void OpenWeaponCollider() {
            weaponCollider.gameObject.SetActive(true);
        }

        public void CloseWeaponCollider() {
            weaponCollider.gameObject.SetActive(false);
        }
    }
}
