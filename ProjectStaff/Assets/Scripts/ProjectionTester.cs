using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectionTester : MonoBehaviour {

    public Transform objA;
    public Transform objB;
    public Transform objPoint;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update () {
        Vector3 cPoint = objPoint.position.ClosestPoint(objA.position, objB.position);
        Debug.DrawRay(objA.position, objB.position - objA.position, Color.red);
        Debug.DrawRay(cPoint, objPoint.position - cPoint, Color.yellow);
    }
}
