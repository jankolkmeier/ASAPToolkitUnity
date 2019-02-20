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

        protected virtual void Awake() {
            rootRotation = transform.rotation;
            rootPosition = transform.position;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
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

        public VJoint[] GenerateVJoints() {
            if (vJoints != null && vJoints.Length > 0) return vJoints;
            Animator a = GetComponent<Animator>();
            if (a != null) a.enabled = false;
            StoredPoseAsset.ApplyPose(aPose.pose, transform);
            rig = new CanonicalRig(transform, boneMap.mappings);

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