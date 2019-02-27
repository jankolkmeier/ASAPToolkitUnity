using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASAPToolkit.Unity.Characters;

using System.Linq;

namespace ASAPToolkit.Unity.Retargeting {

    [RequireComponent(typeof(BasicSkeleton))]
    public class ApplyCanonicalPose : MonoBehaviour {

        public BasicSkeleton poseSource;
        private BasicSkeleton poseTarget;

        public AvatarMask boneMask;
        CanonicalRepresentation.HAnimBones[] bones = null;

        void Start() {
            poseTarget = GetComponent<BasicSkeleton>();
            bones = CanonicalRepresentation.AvatarMaskToCanonicalBones(boneMask);
        }

        void LateUpdate() {
            Apply();
        }

        public void Apply() {
            if (poseSource.Ready() && poseTarget.Ready()) {
                if (poseSource != null) {
                    CanonicalPose pose = poseSource.ExportPose();
                    if (pose != null) {
                        poseTarget.ApplyPose(pose, bones);
                    }
                }
            }
        }
    }
}