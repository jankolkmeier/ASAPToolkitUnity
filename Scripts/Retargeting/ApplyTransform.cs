using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASAPToolkit.Unity.Retargeting {
	public class ApplyTransform : MonoBehaviour, IPriorityApply {

		// TODO: set relative cos..

		public bool copyRotation = true;
		public bool copyPosition = true;
		public Transform src;
		public Transform target;

		public int priority = 0;

		public int GetPriority() {
			return priority;
		}

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
}