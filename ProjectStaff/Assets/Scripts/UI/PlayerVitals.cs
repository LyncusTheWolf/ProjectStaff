using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic{
	public class PlayerVitals : MonoBehaviour {

        public RectTransform staminaFill;

		// Use this for initialization
		void Start(){
			
		}

		// Update is called once per frame
		void Update(){
            Player playerRef = GameManager.Instance.playerRef;

            staminaFill.localScale = new Vector3(playerRef.currentStamina / playerRef.maxStamina, 1.0f, 0.0f);

        }
	}
}
