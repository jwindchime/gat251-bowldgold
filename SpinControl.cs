using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinControl : MonoBehaviour {

    public GameObject spinPointer;
    public GameObject spinBall;
    public float spinSpeed;

    float defaultSpeed = 5.0f;
    float spinRange = 0.6f;

	// Use this for initialization
	void Start () {
        Reset();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //Debug.Log(spinPointer.transform.rotation.z);
        if (Mathf.Abs(spinPointer.transform.rotation.z) > spinRange) { spinSpeed *= -1; }
        spinPointer.transform.Rotate(0.0f, 0.0f, spinSpeed);

        spinBall.transform.Rotate(0.0f, 0.0f, 1.5f * defaultSpeed);
    }

    public void Reset()
    {
        spinPointer.transform.localRotation = Quaternion.identity;
        spinSpeed = defaultSpeed;
    }
}
