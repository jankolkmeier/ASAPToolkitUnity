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
using UnityEngine;
using System.Collections;

namespace ASAPToolkit.Unity.Environment {

    public class WorldObject : MonoBehaviour {

        private VJoint vjoint;
        public Transform cosAnchor;
        public bool randomNameSuffix = false;

        // Use this for initialization
        void Start() {
            if (randomNameSuffix) {
                transform.name = transform.name + "_" + System.Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
            }
            vjoint = new VJoint(transform.name, transform.position, transform.rotation);
            ASAPToolkitManager manager = FindObjectOfType<ASAPToolkitManager>();
            if (manager != null) manager.OnWorldObjectInitialized(vjoint);
        }

        // Update is called once per frame
        void Update() {
            //if (vjoint.position.Equals (transform.position)) {
            // Todo: only transmit if changed? use a flag for "moved" ?
            //}
            if (cosAnchor == null) { 
                vjoint.position = transform.position;
                vjoint.rotation = transform.rotation;
            } else {
                vjoint.position = cosAnchor.InverseTransformPoint(transform.position);
                vjoint.rotation = Quaternion.Inverse(transform.rotation) * cosAnchor.transform.rotation; // Uhm, untested...
            }
        }
    }

}