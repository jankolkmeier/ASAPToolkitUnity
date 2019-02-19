using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASAPToolkit.Unity.Characters;

namespace  ASAPToolkit.Unity.Retargeting {
	
    [RequireComponent(typeof(BasicSkeleton))]
	public class ClipExporter : MonoBehaviour {
		public Transform animationRootBone;
		private BasicSkeleton skeleton;

		void Awake() {
			if (animationRootBone == null) animationRootBone = transform;
			skeleton = GetComponent<BasicSkeleton>();
		}

        public CanonicalPoseSequence ExportClip(AnimationClip clip, int fps) {
			CanonicalPoseSequence res = new CanonicalPoseSequence(clip.name, skeleton.rig);
			List<CanonicalPose> _frames = new List<CanonicalPose>();
			float delta = 1.0f / (float)fps;
			for (int frame = 0; frame < Mathf.Max(1f, clip.length * fps); frame++) {
				float t = delta * frame;
				clip.SampleAnimation(animationRootBone.gameObject, t);
				res.AddFrame(t);
			}
			return res;
        }
	}
}