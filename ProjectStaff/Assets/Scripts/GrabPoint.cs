using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic {
    [RequireComponent(typeof(BoxCollider))]
	public class GrabPoint : MonoBehaviour {

        public string actionName;
        public float wallPointOffset;
        public Vector3 offset;
        public float maxHeightSnapDistance = 0.2f;
        public float minHeightSnapDistance = 0.2f;
        public float clampAngle;
        public bool autoSnap;

        private bool isActive;
		
		void Awake(){
			
		}

		// Use this for initialization
		void Start(){
            isActive = true;
        }

		// Update is called once per frame
		void Update(){
			
		}

        public void OnTriggerStay(Collider other) {
            if(isActive && other.tag == "Player") {
                //Check to see if the player character is close enough height wise in order to use this grab point
                float deltaHeight =  other.transform.position.y - (transform.position + offset).y;
                if (deltaHeight > maxHeightSnapDistance || deltaHeight < -minHeightSnapDistance) {
                    return;
                }

                CharacterMotor cm = other.GetComponent<CharacterMotor>();

                //Debug.Log("Pushing grab action");

                //If we don't expect to automatically snap to the target regardless of input then check to see if the stick is being pushed towards the grab point
                if (!autoSnap) {
                    if (cm.CurrentFrameInput.moveDir.sqrMagnitude < 0.05f) {
                        return;
                    }
                }

                //If the player is between the clamp angle and their input is between the clamp angle then proceed
                if(Vector3.Angle(transform.forward, cm.CurrentFrameInput.moveDir) > clampAngle && Vector3.Angle(transform.forward, other.transform.forward) > clampAngle) {
                    return;
                }

                isActive = false;

                Vector3 xzOffset = other.transform.position - transform.position;
                xzOffset.y = 0.0f;
                xzOffset = Vector3.ProjectOnPlane(xzOffset, transform.forward);
                cm.PushNewEquipState(false);
                cm.PushAction(actionName, transform.position + xzOffset + offset + transform.forward * wallPointOffset, transform.rotation);
            }
        }

        public void OnTriggerExit(Collider other) {
            if (other.tag == "Player") {
                isActive = true;
            }
        }

        public void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0.0f, 0.0f, 1.0f) * wallPointOffset + offset + new Vector3(0.0f, 1.0f, 0.0f) * ((maxHeightSnapDistance - minHeightSnapDistance)/2.0f), 
                new Vector3(1.0f, maxHeightSnapDistance + minHeightSnapDistance, 1.0f));
        }
    }
}
