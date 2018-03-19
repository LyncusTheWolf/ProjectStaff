using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic {
	public class CharacterAnimState : StateMachineBehaviour {

        public const string IN_ACTION_ID = "InAction";

        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
            base.OnStateEnter(animator, animatorStateInfo, layerIndex);
            animator.SetBool(IN_ACTION_ID, false);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            animator.SetBool(IN_ACTION_ID, true);
        }
    }
}
