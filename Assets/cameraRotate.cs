using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraRotate : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 angle = transform.eulerAngles;
        angle.z += 0.2f;
        transform.eulerAngles = angle;
    }


}
