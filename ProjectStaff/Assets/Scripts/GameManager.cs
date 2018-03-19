/* Kenneth Mitten
 * 1/18/2018
 * 
 * Contains all the controls pertaining to managing the game state as well as the master
 * controls to all the prefabs and persistent data within the scene
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic{
	public class GameManager : MonoBehaviour {

        public struct LoadData {

        }

        private static GameManager instance;

        public delegate void LevelLoadEvent(LoadData ld);

        public static LevelLoadEvent onLevelLoad;

        public Player playerRef;

        public static GameManager Instance {
            get { return instance; }
        }
		
		void Awake(){
			if(instance != null) {
                GameObject.Destroy(gameObject);
                return;
            }

            instance = this;
		}
	}
}
