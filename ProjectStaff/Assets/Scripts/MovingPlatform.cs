using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    [SerializeField]
    private BezierCurve platformPath;
    [SerializeField]
    private float movementTime;
    [SerializeField]
    private float endPointWaitTime;

    private float time;

    private bool ascending;
    private float speed;

    private float currentWaitTime;

	// Use this for initialization
	void Start () {
        time = 0;
        speed = 1 / movementTime;
    }
	
	// Update is called once per frame
	void Update () {
        currentWaitTime -= Time.deltaTime;
        if(currentWaitTime > 0.0f) {
            return;
        }

        if (ascending) {
            time = Mathf.Clamp(time + Time.deltaTime * speed, 0.0f, 1.0f);
            if(time == 1.0f) {
                ascending = false;
                currentWaitTime = endPointWaitTime;
            }
        } else {
            time = Mathf.Clamp(time - Time.deltaTime * speed, 0.0f, 1.0f);
            if (time == 0.0f) {
                ascending = true;
                currentWaitTime = endPointWaitTime;
            }
        }

        transform.position = platformPath.GetPoint(time);	
	}
}
