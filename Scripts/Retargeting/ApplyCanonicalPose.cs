using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASAPToolkit.Unity.Characters;

namespace ASAPToolkit.Unity.Retargeting {

    [RequireComponent(typeof(BasicSkeleton))]
    public class ApplyCanonicalPose : MonoBehaviour {

        public BasicSkeleton poseSource;
        private BasicSkeleton poseTarget;
        public float scaleRoot = 1.0f;

        void Start() {
            poseTarget = GetComponent<BasicSkeleton>();
        }

        void LateUpdate() {
            if (poseSource.Ready() && poseTarget.Ready()) {
                if (poseSource != null) {
                    CanonicalPose pose = poseSource.ExportPose();
                    if (pose != null) {
                        poseTarget.ApplyPose(pose);
                    }
                }
            }
        }
    }
}