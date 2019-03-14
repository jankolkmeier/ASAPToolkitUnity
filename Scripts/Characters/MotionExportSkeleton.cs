using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ASAPToolkit.Unity.Retargeting;

namespace ASAPToolkit.Unity.Characters {
	public class MotionExportSkeleton : BasicSkeleton {

		public Animator ikDrivingAnimator;


        public void Sync_start() { }
        public void Sync_ready() { }
        public void Sync_strokeStart() { }
        public void Sync_stroke() { }
        public void Sync_strokeEnd() { }
        public void Sync_relax() { }
        public void Sync_end() { } 
		public void Sync_custom(string id) {}

		public Animator GetAnimator() {
			if (ikDrivingAnimator == null) return GetComponent<Animator>();
			return ikDrivingAnimator;
		}

		public MotionExportSkeleton GetAnimationRig() {
			if (ikDrivingAnimator == null) return this;
			MotionExportSkeleton altRig = ikDrivingAnimator.GetComponent<MotionExportSkeleton>();
			if (altRig == null) return this;
			return altRig;
		}

		public Transform GetAnimationRoot() {
			if (ikDrivingAnimator == null) return transform;
			else return ikDrivingAnimator.transform;
		}

        public CanonicalPoseClip ExportClip(AnimationClip clip, int fps) {
			List<CanonicalPose> _frames = new List<CanonicalPose>();
			float delta = 1.0f / (float)fps;
			for (int frame = 0; frame < Mathf.Max(1f, clip.length * fps); frame++) {
				float t = delta * frame;
				if (ikDrivingAnimator == null) {
					clip.SampleAnimation(transform.gameObject, t);
				} else {
					//if (fbbik_head != null) fbbik_head.;
					clip.SampleAnimation(ikDrivingAnimator.gameObject, t);
				}
				SendMessage("Apply");
				CanonicalPose p = ExportPose();
				p.timestamp = t;
				_frames.Add(p);
			}
			return new CanonicalPoseClip(_frames.ToArray());
        }

        public CanonicalRepresentation.HAnimBones[] GetAnimatedCanonicalBones(AnimationClip clip) {
            List<CanonicalRepresentation.HAnimBones> res = new List<CanonicalRepresentation.HAnimBones>();
#if UNITY_EDITOR
            foreach (UnityEditor.EditorCurveBinding binding in UnityEditor.AnimationUtility.GetCurveBindings(clip)) {
                //AnimationCurve curve = AnimationUtility.GetEditorCurve (clip, binding);
                string[] pathElems = binding.path.Split('/');
                string boneName = pathElems[pathElems.Length - 1];
                CanonicalRepresentation.HAnimBones hAnimName = BoneNameMapped(boneName);
                if ((binding.propertyName.StartsWith("m_LocalRotation") || binding.propertyName.StartsWith("m_LocalPosition")) &&
                    hAnimName != CanonicalRepresentation.HAnimBones.NONE &&
                    !res.Contains(hAnimName)) {
                    res.Add(hAnimName);
                }

                //if (binding.propertyName.StartsWith("m_LocalPosition")) {
                // Translation bones...
                //}
            }
#endif
            return res.ToArray();
        }

		public CanonicalRepresentation.HAnimBones[] GetExportBones(CanonicalRepresentation.MASK_MODE maskMode, AvatarMask exportMask, AnimationClip clip) {
            CanonicalRepresentation.HAnimBones[] skeletonBones = this.ExportPose().parts;
			CanonicalRepresentation.HAnimBones[] animatedBones;
			if (ikDrivingAnimator == null && clip != null) animatedBones = GetAnimatedCanonicalBones(clip);
			else animatedBones = ExportPose().parts;
			
            if (maskMode == CanonicalRepresentation.MASK_MODE.ALL) {
                return skeletonBones;
            } else if (maskMode == CanonicalRepresentation.MASK_MODE.ALL_ANIMATED) {
				return animatedBones;
				// TODO: if we are IK rig, this should return all bones on this rig.
            } else if ((maskMode == CanonicalRepresentation.MASK_MODE.MASK_MAPPED_ALL || maskMode == CanonicalRepresentation.MASK_MODE.MASK_MAPPED_ANIMATED) && exportMask != null) {
                List<CanonicalRepresentation.HAnimBones> _boneUnion = new List<CanonicalRepresentation.HAnimBones>();
                for (int i = 0; i < exportMask.transformCount; i++) {
                    if (!exportMask.GetTransformActive(i)) continue;
                    string boneName = exportMask.GetTransformPath(i).Split(new char[] { '/' }).Last();
                    if (CanonicalRepresentation.HAnimBoneNames.Contains(boneName)) {
                        CanonicalRepresentation.HAnimBones canonicalBone = (CanonicalRepresentation.HAnimBones)System.Enum.Parse(typeof(CanonicalRepresentation.HAnimBones), boneName, false);
                        if (maskMode == CanonicalRepresentation.MASK_MODE.MASK_MAPPED_ALL && skeletonBones.Contains(canonicalBone)) {
                            _boneUnion.Add(canonicalBone);
                        }

                        if (maskMode == CanonicalRepresentation.MASK_MODE.MASK_MAPPED_ANIMATED && animatedBones.Contains(canonicalBone)) {
                            _boneUnion.Add(canonicalBone);
                        }
                    }
                }
                return _boneUnion.ToArray();
            }

			return skeletonBones;
		}

        public CanonicalRepresentation.HAnimBones BoneNameMapped(string boneName) {
            try {
                return boneMap.mappings.First(m => m.src_bone == boneName).hanim_bone;
            } catch { }
            return CanonicalRepresentation.HAnimBones.NONE;
        }

	}
}