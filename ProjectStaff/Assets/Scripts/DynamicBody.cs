using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Default{
	public class DynamicBody : MonoBehaviour {

        public float forceMultiplier;

        private Transform parent;
        private Rigidbody rigid;

        private Vector3 posLastFrame;
		
		void Awake(){
            parent = transform.parent;
            posLastFrame = parent.position;
            rigid = transform.GetComponent<Rigidbody>();
            //Prevent parent objects from colliding with their children
            Collider parentCol = parent.GetComponent<Collider>();
            if (parentCol != null) {
                Physics.IgnoreCollision(rigid.GetComponent<Collider>(), parent.GetComponent<Collider>());
            }          
            //posLastFrame = transform.position;
		}

		// Use this for initialization
		void Start(){
			
		}

		// Update is called once per frame
		void FixedUpdate(){

            /*Vector3 deltaPos = posLastFrame - transform.position;
            Vector3 crossAxis = Vector3.Cross(deltaPos, transform.up);
            Debug.DrawRay(transform.position, transform.up, Color.red);
            Debug.DrawRay(transform.position, deltaPos * 5.0f, Color.green);
            Debug.DrawRay(transform.position, crossAxis * 5.0f, Color.blue);

            rigid.AddTorque(crossAxis * forceMultiplier, ForceMode.Impulse);

            posLastFrame = transform.position;*/

            /*if (rigid.velocity.sqrMagnitude > (forceMultiplier * forceMultiplier)) {
                rigid.velocity = rigid.velocity.normalized * forceMultiplier;
            }*/

            Vector3 deltaForce = posLastFrame - parent.position;

            Debug.DrawRay(transform.position, deltaForce * 100.0f, Color.blue);

            rigid.AddForce((deltaForce) * forceMultiplier * (1 - Mathf.Abs(Vector3.Dot(transform.up, deltaForce.normalized))), ForceMode.Impulse);

            posLastFrame = parent.position;
        }
	}
}
