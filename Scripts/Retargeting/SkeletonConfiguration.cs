using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static ASAPToolkit.Unity.Retargeting.CanonicalRepresentation;

namespace ASAPToolkit.Unity.Retargeting {

    [ExecuteInEditMode]
    public class SkeletonConfiguration : MonoBehaviour, ICharacterSkeleton {

        private LocalToCanonical[] ltc;
        private CanonicalSkeletonPose lastPose;

        [SerializeField]
        private BoneMapAsset boneMapping;

        [SerializeField]
        private StoredPoseAsset aPose;

        public Transform rootBone;
        public HAnimBones rootTranslationBone = HAnimBones.vl5;

        private bool initialized = false;

        public void Init() {
            if (rootBone == null) rootBone = transform;
            if (boneMapping == null) {
                return;
            }

            if (aPose == null) {
                return;
            }



            List<LocalToCanonical> _ltc = new List<LocalToCanonical>();


            if (boneMapping.mappings != null) {
                for (int i = 0; i < boneMapping.mappings.Length; i++) {
                    Transform localBone = FindBone(boneMapping.mappings[i].src_bone, rootBone);
                    if (i == 0 && localBone == null) localBone = rootBone;

                    if (i == 0) {
                        rootBone = localBone;
                        StoredPoseAsset.ApplyPose(aPose.pose, rootBone);
                    }
                    //if (localBone == null || boneMapping.mappings[i].hanim_bone == CanonicalMapping.HAnimBones.NONE) continue;
                    if (localBone == null) continue;
                    _ltc.Add(new LocalToCanonical(localBone, boneMapping.mappings[i].hanim_bone));
                }
            }

            if (_ltc.Count <= 1) return; // Empty skeleton...

            if (boneMapping.extraBones != null) { 
                foreach (BoneMapAsset.ExtraBone extraBone in boneMapping.extraBones) {
                    Transform parentBone = FindBone(extraBone.parent_src_bone, rootBone);
                    if (parentBone == null) {
                        Debug.LogWarning("Cannot find parent for extra bone: " + extraBone.parent_src_bone);
                        continue;
                    }

                    string boneName = extraBone.hanim_bone.ToString();
                    Transform localBone = FindBone(boneName, rootBone);
                    if (localBone == null) {
                        localBone = new GameObject(boneName).transform;
                    }

                    localBone.rotation = Quaternion.identity;
                    localBone.parent = parentBone;
                    localBone.localPosition = extraBone.localPosition;
                    _ltc.Add(new LocalToCanonical(localBone, extraBone.hanim_bone));
                }
            }

            ltc = _ltc.ToArray();
            lastPose = new CanonicalSkeletonPose(ltc, 0, rootTranslationBone);
            initialized = true;
            //Debug.Log("Initialzed PoseProvider: " + transform.name + " (" + ltc.Length + " mapped bones)");
            // TODO: store (assumed) t/a-pose?
        }

        public virtual bool Ready() {
            return initialized;
        }

        public HAnimBones BoneNameMapped(string boneName) {
            try {
                return boneMapping.mappings.First(m => m.src_bone == boneName).hanim_bone;
            } catch { }
            return HAnimBones.NONE;
        }

        public VJoint[] GenerateVJoints() {
            Init();
            if (ltc == null || ltc.Length < 1) return null;
            VJoint[] res = new VJoint[ltc.Length];
            Dictionary<string, VJoint> lut = new Dictionary<string, VJoint>();

            for (int b = 0; b < ltc.Length; b++) {
                Transform bone = ltc[b].localBone;
                VJoint parent = null;

                if (b == 0 && ltc[b].canonicalBoneName != HAnimBones.HumanoidRoot) {
                    Debug.LogError("First bone in boneMapping needs to be HumanoidRoot");
                    return null;
                }

                if (b > 0) {
                    parent = lut[bone.parent.name];
                    if (parent == null) {
                        Debug.LogError("Parent of " + bone.parent.name + "not added yet");
                        return null;
                    }
                }

                // Default HAnim skeleton has rotation that aligns with global Zero COS
                Quaternion rot = Quaternion.identity;
                Vector3 position = bone.position;
                if (ltc[b].canonicalBoneName == rootTranslationBone && b > 0) {
                    position = Vector3.zero;
                    res[0].position = bone.position - res[0].position;
                } else if (b > 0) {
                    position = Quaternion.Inverse(Quaternion.identity) * (bone.position - bone.parent.position);
                }

                string hAnimName = "";
                if (ltc[b].canonicalBoneName != HAnimBones.NONE) {
                    hAnimName = ltc[b].canonicalBoneName.ToString();
                }
                res[b] = new VJoint(bone.name, hAnimName, position, rot, parent);
                lut.Add(bone.name, res[b]);
            }
            return res;
        }

        void Start() {
        }

        void Update() {
            if (ltc == null) Init();
        }

        private Transform FindBone(string name, Transform parent) {
            Transform result = parent;
            if (result != null && result.name == name) return result;
            result = parent.Find(name);
            if (result != null) return result;
            foreach (Transform child in parent) {
                result = FindBone(name, child);
                if (result != null) return result;
            }
            return null;
        }

        public ASAPClip ExportClip(AnimationClip clip, int fps) {
            if (boneMapping != null) {
                List<CanonicalSkeletonPose> _frames = new List<CanonicalSkeletonPose>();
                float delta = 1.0f / (float)fps;
                for (int frame = 0; frame < Mathf.Max(1f, clip.length * fps); frame++) {
                    float t = delta * frame;
                    clip.SampleAnimation(rootBone.gameObject, t);
                    _frames.Add(new CanonicalSkeletonPose(ltc, t, rootTranslationBone));
                }
                return new ASAPClip(clip.name, _frames.ToArray(), _frames.First().canonicalBoneNames);
            }
            return null;
        }

        public void ApplyPose(CanonicalSkeletonPose pose) {
            ApplyPose(pose, 1.0f);
        }

        public void ApplyPose(CanonicalSkeletonPose pose, float scaleRootPosition) {
            if (ltc == null || pose.canonicalBoneNames == null) return;
            for (int i = 0; i < pose.canonicalBoneNames.Length; i++) {
                HAnimBones bone = pose.canonicalBoneNames[i];
                if (bone == HAnimBones.NONE) continue;
                LocalToCanonical map = ltc.FirstOrDefault(m => m.canonicalBoneName == bone);
                if (map != null) {
                    map.canonicalCOSMapping.ApplyFromCanonical(pose.bonePoses[i]);
                    if (map.canonicalBoneName == rootTranslationBone) {
                        map.localBone.localPosition = map.localBone.parent.InverseTransformPoint(map.localBone.parent.position + pose.rootTransform);
                        map.localBone.position = map.localBone.localPosition * scaleRootPosition;
                    }


                    /*
                    if (bone == CanonicalMapping.HAnimBones.HumanoidRoot) {
                        //map.localBone.localPosition = pose.rootTransform;
                        //rootTranslationBone.localPosition = pose.rootTransform;
                    }
                     */
                }
            }
        }

        public void ApplyPose(Quaternion[] poses, Vector3[] rootTransforms) {
            ApplyPose(poses, rootTransforms, 1.0f);
        }

        public void ApplyPose(Quaternion[] poses, Vector3[] rootTransforms, float scaleRootPosition) {
            if (ltc == null || poses.Length != ltc.Length) {
                Debug.LogError("Poses not fitting. Got "+poses.Length+" poses, but mapping has: "+ltc.Length);
                return;
            }

            for (int i = 0; i < ltc.Length; i++) {
                HAnimBones bone = ltc[i].canonicalBoneName;
                if (bone == HAnimBones.NONE) continue;
                ltc[i].canonicalCOSMapping.ApplyFromCanonical(poses[i]);
                if (bone == rootTranslationBone) {
                    Debug.Log("HAAA "+ ltc[i].localBone.name);
                    ltc[i].localBone.localPosition = ltc[i].localBone.parent.InverseTransformPoint(ltc[i].localBone.parent.position + rootTransforms[0]);
                    ltc[i].localBone.localPosition = ltc[i].localBone.localPosition * scaleRootPosition;
                }
            }
        }

        // Current pose, disregard animation clip.
        public ASAPClip ExportPose() {
            if (boneMapping != null && ltc != null) {
                List<CanonicalSkeletonPose> _frames = new List<CanonicalSkeletonPose>();
                _frames.Add(new CanonicalSkeletonPose(ltc, 0f, rootTranslationBone));
                lastPose.UpdateCanonicalSkeletonPose(ltc);
                return new ASAPClip("restpose", _frames.ToArray(), _frames.First().canonicalBoneNames);
            }
            return null;
        }

        public CanonicalSkeletonPose ExportPose(float t) {
            return new CanonicalSkeletonPose();
        }

        public CanonicalSkeletonPose ExportPose(int frame) {
            return new CanonicalSkeletonPose();
        }

        public void Sync_start() { }
        public void Sync_ready() { }
        public void Sync_strokeStart() { }
        public void Sync_stroke() { }
        public void Sync_strokeEnd() { }
        public void Sync_relax() { }
        public void Sync_end() { }

        public class SyncPoint {
            public string name;
            public float relativeTime;

            public SyncPoint(string name, float time) {
                this.name = name;
                this.relativeTime = time;
            }
        }

        public class ASAPClip {
            public string name;
            public CanonicalSkeletonPose[] frames;
            public HAnimBones[] bones;

            public ASAPClip(string name, CanonicalSkeletonPose[] frames, HAnimBones[] bones) {
                this.name = name;
                this.frames = frames;
                this.bones = bones;
            }
        }
    }

}