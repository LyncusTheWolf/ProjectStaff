using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic{
	public class CharacterAnimSetBool : StateMachineBehaviour {

        public string boolOnActive;
        public bool stateOnActive;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
            base.OnStateEnter(animator, animatorStateInfo, layerIndex);
            animator.SetBool(boolOnActive, stateOnActive);
            Debug.Log("Entered state");
        }
    }
}
