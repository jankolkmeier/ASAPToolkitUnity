using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using System.Linq;
using ASAPToolkit.Unity.Util;

namespace ASAPToolkit.Unity.Retargeting {

    [RequireComponent(typeof(UMADynamicAvatar))]
    public class DynamicUMASkeleton : MonoBehaviour, ICharacterSkeleton {

        public BoneMapAsset boneMap;
        public StoredPoseAsset aPose;
        public Transform rootTranslation;

        private UMADynamicAvatar uma;
        private bool _ready = false;

        private VJoint[] vJoints;
        LocalToCanonical[] ltc;

        public AvatarMask currentAvatarMask;
        public bool exportRestPose;
        public bool T1R;


        // Use this for initialization
        void Start() {
            uma = GetComponent<UMADynamicAvatar>();
            uma.CharacterCreated.AddListener(OnCreated);
            uma.CharacterUpdated.AddListener(OnUpdated);
        }

        void OnCreated(UMAData umaData) {
            OnUpdated(umaData);
        }


        void OnUpdated(UMAData umaData) {
            Debug.Log("Character updated");
            vJoints = null;
            vJoints = GenerateVJoints(rootTranslation);
        }
     
        // Update is called once per frame
        void Update() {
            if (exportRestPose) {
                ExportRestPose();
                exportRestPose = false;
            }
        }

        public void ExportRestPose() {

            List<string> _parts = new List<string>();
            List<Quaternion> _rotations = new List<Quaternion>();
            Vector3 translation = Vector3.zero;

            for (int i = 0; i < currentAvatarMask.transformCount; i++) {
                if (!currentAvatarMask.GetTransformActive(i)) continue;
                string boneName = currentAvatarMask.GetTransformPath(i).Split(new char[] { '/' }).Last();
                if (!CanonicalRepresentation.HAnimBoneNames.Contains(boneName)) continue;
                CanonicalRepresentation.HAnimBones canonicalBone = (CanonicalRepresentation.HAnimBones)System.Enum.Parse(typeof(CanonicalRepresentation.HAnimBones), boneName, false);
                LocalToCanonical _ltc = ltc.FirstOrDefault(m => m.canonicalBoneName == canonicalBone);
                if (_ltc == null) continue;

                if (T1R && _rotations.Count == 0) {
                    if (_ltc.parent != null) {
                        translation = _ltc.parent.canonicalCOSMapping.ToCanonical() * (_ltc.localBone.position - _ltc.parent.localBone.position);
                    } else {
                        translation = _ltc.localBone.position;
                    }
                }

                _rotations.Add(_ltc.canonicalCOSMapping.ToCanonical());
                _parts.Add(boneName);
            }

            string res = "";
            string encoding = "R";
            if (T1R) encoding = "T1R";

            string parts = string.Join(" ", _parts.ToArray());
            res += "<SkeletonPose encoding=\""+encoding+"\" rotationEncoding=\"quaternions\" parts=\""+ parts + "\">\n";
            res += "\t";
            if (T1R) res += translation.ToASAPString() + " ";
            res += string.Join(" ", _rotations.ToArray().Select(q => q.ToASAPString()).ToArray());
            res += "\n</SkeletonPose>";

            Debug.Log(res);
        }

        public void ApplyPose(Quaternion[] poses, Vector3[] rootTransforms) {
            if (ltc == null || poses.Length != ltc.Length) {
                Debug.LogError("Poses not fitting. Got " + poses.Length + " poses, but mapping has: " + ltc.Length);
                return;
            }

            for (int i = 0; i < ltc.Length; i++) {
                CanonicalRepresentation.HAnimBones bone = ltc[i].canonicalBoneName;
                if (bone == CanonicalRepresentation.HAnimBones.NONE) continue;
                ltc[i].canonicalCOSMapping.ApplyFromCanonical(poses[i]);
                if (i == 0) {
                    ltc[i].localBone.position = rootTransforms[0];
                    //ltc[i].localBone.parent.InverseTransformPoint(ltc[i].localBone.parent.position + rootTransforms[0]);
                    //ltc[i].localBone.localPosition = ltc[i].localBone.localPosition * scaleRootPosition;
                } else if (i == 1) {
                    ltc[1].localBone.position = ltc[0].localBone.position + (ltc[0].canonicalCOSMapping.ToCanonical() * rootTransforms[1]);
                }
            }
        }

        public LocalToCanonical[] CanonicalMapping() {
            List<LocalToCanonical> _ltc = new List<LocalToCanonical>();


            if (boneMap.mappings != null) {
                for (int i = 0; i < boneMap.mappings.Length; i++) {
                    Transform localBone = transform.FindChildRecursive(boneMap.mappings[i].src_bone);
                    if (localBone == null) {
                        Debug.LogWarning("Couldn't find bone " + boneMap.mappings[i].src_bone + " in " + transform.name);
                        continue;
                    }
                    _ltc.Add(new LocalToCanonical(localBone, boneMap.mappings[i].hanim_bone));
                }
            }
            LocalToCanonical[] res = _ltc.ToArray();
            foreach (LocalToCanonical m in res) {
                if (m.localBone.parent == null) continue;
                LocalToCanonical parent = res.FirstOrDefault(p => p.localBone == m.localBone.parent);
                m.LinkParent(parent);
            }

            return res;
        }

        public VJoint[] GenerateVJoints(Transform rootTranslation) {
            //if (vJoints != null && vJoints.Length > 0) return vJoints;
            Animator a = GetComponent<Animator>();
            a.enabled = false;
            StoredPoseAsset.ApplyPose(aPose.pose, transform);
            ltc = CanonicalMapping();
            VJoint[] res = new VJoint[ltc.Length];
            Dictionary<string, VJoint> lut = new Dictionary<string, VJoint>();

            for (int b = 0; b < ltc.Length; b++) {
                Transform bone = ltc[b].localBone;
                VJoint parent = null;

                if (b == 0 && ltc[b].canonicalBoneName != CanonicalRepresentation.HAnimBones.HumanoidRoot) {
                    Debug.LogError("First bone in boneMapping needs to be HumanoidRoot");
                    return null;
                } else if (b == 0) {
                    bone.position = rootTranslation.position;
                    if (transform.parent == null) {
                        transform.rotation = rootTranslation.rotation;
                    } else {
                        transform.localRotation = ltc[b].canonicalCOSMapping.FromCanonical(rootTranslation.rotation);
                    }
                }

                if (b > 0) {
                    parent = lut[bone.parent.name];
                    if (parent == null) {
                        Debug.LogError("Parent of " + bone.parent.name + "not added yet");
                        return null;
                    }
                }

                Quaternion rot = ltc[b].canonicalCOSMapping.ToCanonical();// * rootTranslation.rotation;
                //if (b == 0) {
                //    rot = rot * rootTranslation.rotation;
                //}
                Vector3 position = (bone.position - bone.parent.position);
                //if (b == 0)
                //    position = position + rootTranslation.position;
                
                string hAnimName = "";
                if (ltc[b].canonicalBoneName != CanonicalRepresentation.HAnimBones.NONE) {
                    hAnimName = ltc[b].canonicalBoneName.ToString();
                }
                res[b] = new VJoint(bone.name, hAnimName, position, rot, parent);
                lut.Add(bone.name, res[b]);
            }
            vJoints = res;
            _ready = true;
            return vJoints;

        }

        public bool Ready() {
            return _ready;
        }


    }

}