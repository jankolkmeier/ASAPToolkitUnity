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