using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASAPToolkit.Unity.Characters;

using System.Linq;

namespace ASAPToolkit.Unity.Retargeting {

    [RequireComponent(typeof(BasicSkeleton))]
    public class ApplyCanonicalPose : MonoBehaviour, IPriorityApply {

        public BasicSkeleton poseSource;
        private BasicSkeleton poseTarget;

        public int priority = 0;

        public AvatarMask boneMask;
        CanonicalRepresentation.HAnimBones[] bones = null;

        public int GetPriority() {
            return priority;
        }

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