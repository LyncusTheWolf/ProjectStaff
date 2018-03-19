using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic {

    [RequireComponent(typeof(BoxCollider))]
    public class WeaponCollider : MonoBehaviour {

        private WeaponAnimator parentWeaponAnimator;

        public void RegisterParent(WeaponAnimator wpa) {
            parentWeaponAnimator = wpa;
        }

        public void OnTriggerEnter(Collider other) {
            if(parentWeaponAnimator == null || parentWeaponAnimator.CurrentAction.filterEvent == null) {
                return;
            }

            if (parentWeaponAnimator.CurrentAction.filterEvent(other)) {
                parentWeaponAnimator.CurrentAction.weaponEvent(other, parentWeaponAnimator.weaponStats);
            }
        }
    }
}
