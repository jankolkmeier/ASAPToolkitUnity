using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ASAPToolkit.Unity.Util {
    [ExecuteInEditMode]
    public class LogASAPTransform : MonoBehaviour {

        public string rotation = "";
        public string position = "";
        public string T1R = "";
        public string localRotation = "";
        public string localPosition = "";
        public string localT1R = "";

        void Update() {
            rotation = transform.rotation.ToASAPString();
            position = transform.position.ToASAPString();

            localRotation = transform.localRotation.ToASAPString();
            localPosition = transform.localPosition.ToASAPString();
            T1R = position + " " + rotation;
            localT1R = localPosition + " " + localRotation;
        }
        
    }

}