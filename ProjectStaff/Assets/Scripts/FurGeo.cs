using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Default{
	public class FurGeo : MonoBehaviour {

        //public Shader furGeoShader;

        public Material material;

        public void Awake() {
            //material = new Material(furGeoShader);
        }

        private void OnRenderObject() {
            Graphics.DrawProcedural(MeshTopology.Points, 12);           
        }
    }
}
