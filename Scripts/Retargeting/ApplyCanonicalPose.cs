using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASAPToolkit.Unity.Retargeting {

    [ExecuteInEditMode, RequireComponent(typeof(SkeletonConfiguration))]
    public class ApplyCanonicalPose : MonoBehaviour {

        // TODO: interface...
        public SkeletonConfiguration poseSource;
        public float scaleRoot = 1.0f;
        private SkeletonConfiguration poseTarget;


        void Start() {
            poseTarget = GetComponent<SkeletonConfiguration>();
        }

        void LateUpdate() {
            if (poseSource != null) {
                SkeletonConfiguration.ASAPClip ac = poseSource.ExportPose();
                if (ac != null && ac.frames.Length == 1) {
                    poseTarget.ApplyPose(ac.frames[0], scaleRoot);
                }
            }
        }
    }
}