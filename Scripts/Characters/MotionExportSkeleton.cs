using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ASAPToolkit.Unity.Retargeting;

namespace ASAPToolkit.Unity.Characters {
	public class MotionExportSkeleton : BasicSkeleton {

		public Transform animationRootBone;

        public void Sync_start() { }
        public void Sync_ready() { }
        public void Sync_strokeStart() { }
        public void Sync_stroke() { }
        public void Sync_strokeEnd() { }
        public void Sync_relax() { }
        public void Sync_end() { } 
		public void Sync_custom(string id) {}

        public CanonicalPoseClip ExportClip(AnimationClip clip, int fps) {
			List<CanonicalPose> _frames = new List<CanonicalPose>();
			float delta = 1.0f / (float)fps;
			for (int frame = 0; frame < Mathf.Max(1f, clip.length * fps); frame++) {
				float t = delta * frame;
				clip.SampleAnimation(animationRootBone.gameObject, t);
				CanonicalPose p = ExportPose();
				p.timestamp = t;
				_frames.Add(p);
			}
			return new CanonicalPoseClip(_frames.ToArray());
        }


        public CanonicalRepresentation.HAnimBones BoneNameMapped(string boneName) {
            try {
                return boneMap.mappings.First(m => m.src_bone == boneName).hanim_bone;
            } catch { }
            return CanonicalRepresentation.HAnimBones.NONE;
        }

	}
}