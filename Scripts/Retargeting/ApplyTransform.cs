/*
   Copyright 2020 Jan Kolkmeier <jankolkmeier@gmail.com>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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