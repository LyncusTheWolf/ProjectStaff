using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic{
	public class BreakableObject : Damagable {

        public GameObject Visual;

        public override void Init() {
            //throw new NotImplementedException();
        }

        public override void OnDeath() {
            Visual.SetActive(false);
            objCollider.enabled = false;
        }
    }
}
