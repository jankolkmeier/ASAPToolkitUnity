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
using System.Linq;
using ASAPToolkit.Unity.Retargeting;
using ASAPToolkit.Unity.Util;

namespace ASAPToolkit.Unity.Characters {

	public class BasicSkeleton : MonoBehaviour, ICharacterSkeleton {

        public CanonicalRig rig { get; protected set; }
		protected VJoint[] vJoints;
        protected bool _ready = false;

		[SerializeField]
        protected BoneMapAsset boneMap;
		public BoneMapAsset BoneMap { get { return boneMap; } private set { boneMap = value; } }

		[SerializeField]
        protected StoredPoseAsset aPose;
		public StoredPoseAsset APose { get { return aPose; } private set { aPose = value; } }

        protected Quaternion rootRotation;
        protected Vector3 rootPosition;

        private Transform head;

        protected virtual void Awake() {
            rootRotation = transform.rotation;
            rootPosition = transform.position;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            head = transform; 
            vJoints = null;
        }

		protected virtual void Start () {
		}
		
		protected virtual void Update () {
		}

		public virtual bool Ready() {
			if (_ready) return _ready;
			GenerateVJoints();
			return _ready;
		}

        public CanonicalPose ExportPose() {
            if (rig != null) {
				rig.UpdatePose();
				return rig.ExportPose();
			}
            return null;
        }

        public void ApplyPose(CanonicalPose pose, CanonicalRepresentation.HAnimBones[] bones) {
            if (pose != null) rig.ApplyPose(pose, bones);
        }

        public void ApplyPose(CanonicalPose pose) {
            if (pose != null) rig.ApplyPose(pose);
        }

        public void ApplyPose(Quaternion[] poses, Vector3[] rootTransforms) {
            if (rig == null || rig.boneMap == null || poses.Length != rig.boneMap.Length) {
				string rigState = "not set";
				if (rig != null && rig.boneMap != null) rigState = rig.boneMap.Length.ToString();
                Debug.LogError("Poses not fitting. Got " + poses.Length + " poses, but mapping has: " + rigState);
                return;
            }

            for (int i = 0; i < rig.boneMap.Length; i++) {
                //CanonicalRepresentation.HAnimBones bone = rig.boneMap[i].canonicalBoneName;
                //if (bone == CanonicalRepresentation.HAnimBones.NONE) continue;
                rig.boneMap[i].ApplyRotationFromCanonical(poses[i]);
                if (i < 2) rig.boneMap[i].ApplyLocalPositionFromCanonical(rootTransforms[i]);
            }
        }

        public Transform GetHeadTransform() {
            return head;
        }

        public VJoint[] GenerateVJoints() {
            if (vJoints != null && vJoints.Length > 0) return vJoints;
            Animator a = GetComponent<Animator>();
            if (a != null) a.enabled = false;
            StoredPoseAsset.ApplyPose(aPose.pose, transform);
            List<BoneMapAsset.HAnimBoneMapping> mergedMap = new List<BoneMapAsset.HAnimBoneMapping>();
            foreach (BoneMapAsset.HAnimBoneMapping m in boneMap.mappings) {
                mergedMap.Add(m);
            }
            foreach (BoneMapAsset.ExtraBone eb in boneMap.extraBones) {
                string bone_name = eb.hanim_bone.ToString();
                if (eb.hanim_bone == CanonicalRepresentation.HAnimBones.NONE) {
                    bone_name = eb.bone_name;
                }
                Transform ebTransform = transform.FindChildRecursive(bone_name);
                Transform ebParentTransform = transform.FindChildRecursive(eb.parent_src_bone);
                if (ebTransform == null && ebParentTransform != null) {
                    ebTransform = (new GameObject(bone_name)).transform;
                    ebTransform.SetParent(ebParentTransform);
                    ebTransform.position = ebParentTransform.position+eb.localPosition;
                    ebTransform.localRotation = Quaternion.Euler(eb.localEulerRotation.x, eb.localEulerRotation.y, eb.localEulerRotation.z);
                }
                BoneMapAsset.HAnimBoneMapping newMap;
                newMap.hanim_bone = eb.hanim_bone;
                newMap.src_bone = bone_name;
                mergedMap.Add(newMap);
            }
            rig = new CanonicalRig(transform, mergedMap.ToArray());

            if (rig.boneMap == null) {
                _ready = false;
                rig = null;
                Debug.LogError("Failed to create CanonicalRig");
                return null;
            }

            VJoint[] res = new VJoint[rig.boneMap.Length];
            Dictionary<string, VJoint> lut = new Dictionary<string, VJoint>();
            for (int b = 0; b < rig.boneMap.Length; b++) {
                Transform bone = rig.boneMap[b].bone;
                if (rig.boneMap[b].canonicalBoneName == CanonicalRepresentation.HAnimBones.skullbase) {
                    head = bone;
                }
                VJoint parent = null;
                Vector3 pos = Vector3.zero;
                Quaternion rot = rig.boneMap[b].CurrentRotationInCanonical();
                if (b == 0) {
                    pos = rootPosition;
                    rot = rot * rootRotation;
                } else {
                    pos = (bone.position - bone.parent.position);
                    parent = lut[bone.parent.name];
                    if (parent == null) {
                        Debug.LogError("Parent of " + bone.parent.name + " not added yet");
                        return null;
                    }
                }
                string hAnimName = "";
                if (rig.boneMap[b].canonicalBoneName != CanonicalRepresentation.HAnimBones.NONE) {
                    hAnimName = rig.boneMap[b].canonicalBoneName.ToString();
                }
                res[b] = new VJoint(bone.name, hAnimName, pos, rot, parent);
                lut.Add(bone.name, res[b]);
            }
            vJoints = res;
            _ready = true;
            return vJoints;
        }
	}

}