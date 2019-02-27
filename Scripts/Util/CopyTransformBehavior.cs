using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTransformBehavior : MonoBehaviour {

	// TODO: set relative cos..

	public bool copyRotation = true;
	public bool copyPosition = true;
	public Transform src;
	public Transform target;

	void Awake() {
		if (target == null) target = transform;
		if (src == null) src = transform;
	}

	void Update () {
		Apply();
	}

	public void Apply() {
		if (copyRotation) target.transform.rotation = src.rotation;
		if (copyPosition) target.transform.position = src.position;
	}
}
