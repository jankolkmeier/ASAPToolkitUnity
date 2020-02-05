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