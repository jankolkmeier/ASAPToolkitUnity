using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ASAPToolkit.Unity.Retargeting {


    public class CanonicalRepresentation {
        // root, legs, feet
        public static readonly string HumanoidRoot = "HumanoidRoot";
        public static readonly string sacroiliac = "sacroiliac";
        public static readonly string l_hip = "l_hip";
        public static readonly string l_knee = "l_knee";
        public static readonly string l_ankle = "l_ankle";
        public static readonly string l_subtalar = "l_subtalar";
        public static readonly string l_midtarsal = "l_midtarsal";
        public static readonly string l_metatarsal = "l_metatarsal";
        public static readonly string r_hip = "r_hip";
        public static readonly string r_knee = "r_knee";
        public static readonly string r_ankle = "r_ankle";
        public static readonly string r_subtalar = "r_subtalar";
        public static readonly string r_midtarsal = "r_midtarsal";
        public static readonly string r_metatarsal = "r_metatarsal";

        // spine
        public static readonly string vl5 = "vl5";
        public static readonly string vl4 = "vl4";
        public static readonly string vl3 = "vl3";
        public static readonly string vl2 = "vl2";
        public static readonly string vl1 = "vl1";
        public static readonly string vt12 = "vt12";
        public static readonly string vt11 = "vt11";
        public static readonly string vt10 = "vt10";
        public static readonly string vt9 = "vt9";
        public static readonly string vt8 = "vt8";
        public static readonly string vt7 = "vt7";
        public static readonly string vt6 = "vt6";
        public static readonly string vt5 = "vt5";
        public static readonly string vt4 = "vt4";
        public static readonly string vt3 = "vt3";
        public static readonly string vt2 = "vt2";
        public static readonly string vt1 = "vt1";
        public static readonly string vc7 = "vc7";
        public static readonly string vc6 = "vc6";
        public static readonly string vc5 = "vc5";
        public static readonly string vc4 = "vc4";
        public static readonly string vc3 = "vc3";
        public static readonly string vc2 = "vc2";
        public static readonly string vc1 = "vc1";
        public static readonly string skullbase = "skullbase";

        // face joints
        public static readonly string l_eyelid_joint = "l_eyelid_joint";
        public static readonly string r_eyelid_joint = "r_eyelid_joint";
        public static readonly string l_eyeball_joint = "l_eyeball_joint";
        public static readonly string r_eyeball_joint = "r_eyeball_joint";
        public static readonly string l_eyebrow_joint = "l_eyebrow_joint";
        public static readonly string r_eyebrow_joint = "r_eyebrow_joint";
        public static readonly string temporomandibular = "temporomandibular";

        // shoulder, arm, hand, fingers
        public static readonly string l_sternoclavicular = "l_sternoclavicular";
        public static readonly string l_acromioclavicular = "l_acromioclavicular";
        public static readonly string l_shoulder = "l_shoulder";
        public static readonly string l_elbow = "l_elbow";
        public static readonly string l_wrist = "l_wrist";
        public static readonly string l_thumb1 = "l_thumb1";
        public static readonly string l_thumb2 = "l_thumb2";
        public static readonly string l_thumb3 = "l_thumb3";
        public static readonly string l_thumb_distal_tip = "l_thumb_distal_tip";
        public static readonly string l_index0 = "l_index0";
        public static readonly string l_index1 = "l_index1";
        public static readonly string l_index2 = "l_index2";
        public static readonly string l_index3 = "l_index3";
        public static readonly string l_index_distal_tip = "l_index_distal_tip";
        public static readonly string l_middle0 = "l_middle0";
        public static readonly string l_middle1 = "l_middle1";
        public static readonly string l_middle2 = "l_middle2";
        public static readonly string l_middle3 = "l_middle3";
        public static readonly string l_middle_distal_tip = "l_middle_distal_tip";
        public static readonly string l_ring0 = "l_ring0";
        public static readonly string l_ring1 = "l_ring1";
        public static readonly string l_ring2 = "l_ring2";
        public static readonly string l_ring3 = "l_ring3";
        public static readonly string l_ring_distal_tip = "l_ring_distal_tip";
        public static readonly string l_pinky0 = "l_pinky0";
        public static readonly string l_pinky1 = "l_pinky1";
        public static readonly string l_pinky2 = "l_pinky2";
        public static readonly string l_pinky3 = "l_pinky3";
        public static readonly string l_pinky_distal_tip = "l_pinky_distal_tip";

        public static readonly string r_sternoclavicular = "r_sternoclavicular";
        public static readonly string r_acromioclavicular = "r_acromioclavicular";
        public static readonly string r_shoulder = "r_shoulder";
        public static readonly string r_elbow = "r_elbow";
        public static readonly string r_wrist = "r_wrist";
        public static readonly string r_thumb1 = "r_thumb1";
        public static readonly string r_thumb2 = "r_thumb2";
        public static readonly string r_thumb3 = "r_thumb3";
        public static readonly string r_thumb_distal_tip = "r_thumb_distal_tip";
        public static readonly string r_index0 = "r_index0";
        public static readonly string r_index1 = "r_index1";
        public static readonly string r_index2 = "r_index2";
        public static readonly string r_index3 = "r_index3";
        public static readonly string r_index_distal_tip = "r_index_distal_tip";
        public static readonly string r_middle0 = "r_middle0";
        public static readonly string r_middle1 = "r_middle1";
        public static readonly string r_middle2 = "r_middle2";
        public static readonly string r_middle3 = "r_middle3";
        public static readonly string r_middle_distal_tip = "r_middle_distal_tip";
        public static readonly string r_ring0 = "r_ring0";
        public static readonly string r_ring1 = "r_ring1";
        public static readonly string r_ring2 = "r_ring2";
        public static readonly string r_ring3 = "r_ring3";
        public static readonly string r_ring_distal_tip = "r_ring_distal_tip";
        public static readonly string r_pinky0 = "r_pinky0";
        public static readonly string r_pinky1 = "r_pinky1";
        public static readonly string r_pinky2 = "r_pinky2";
        public static readonly string r_pinky3 = "r_pinky3";
        public static readonly string r_pinky_distal_tip = "r_pinky_distal_tip";

        // non-standard joints, but available in AutoDesk skeletons
        public static readonly string l_upperarm_roll = "l_upperarm_roll";
        public static readonly string l_forearm_roll = "l_forearm_roll";
        public static readonly string r_upperarm_roll = "r_upperarm_roll";
        public static readonly string r_forearm_roll = "r_forearm_roll";
        public static readonly string l_thigh_roll = "l_thigh_roll";
        public static readonly string l_calf_roll = "l_calf_roll";
        public static readonly string r_thigh_roll = "r_thigh_roll";
        public static readonly string r_calf_roll = "r_calf_roll";

        // non-standard, physics related joints
        public static readonly string skulltop = "skulltop";
        public static readonly string l_forefoot_tip = "l_forefoot_tip";
        public static readonly string r_forefoot_tip = "r_forefoot_tip";

        public enum HAnimBones {
            HumanoidRoot = 0, sacroiliac, l_hip, l_knee, l_ankle, l_subtalar, l_midtarsal, l_metatarsal, r_hip, r_knee, r_ankle, r_subtalar, r_midtarsal,
            r_metatarsal, vl5, vl4, vl3, vl2, vl1, vt12, vt11, vt10, vt9, vt8, vt7, vt6, vt5, vt4, vt3, vt2, vt1, vc7, vc6, vc5, vc4, vc3, vc2, vc1, skullbase, l_eyelid_joint, r_eyelid_joint,
            l_eyeball_joint, r_eyeball_joint, l_eyebrow_joint, r_eyebrow_joint, temporomandibular, l_sternoclavicular, l_acromioclavicular, l_shoulder, l_elbow, l_wrist, l_thumb1, l_thumb2,
            l_thumb3, l_thumb_distal_tip, l_index0, l_index1, l_index2, l_index3, l_index_distal_tip, l_middle0, l_middle1, l_middle2, l_middle3, l_middle_distal_tip, l_ring0, l_ring1, l_ring2,
            l_ring3, l_ring_distal_tip, l_pinky0, l_pinky1, l_pinky2, l_pinky3, l_pinky_distal_tip, r_sternoclavicular, r_acromioclavicular, r_shoulder, r_elbow, r_wrist, r_thumb1, r_thumb2,
            r_thumb3, r_thumb_distal_tip, r_index0, r_index1, r_index2, r_index3, r_index_distal_tip, r_middle0, r_middle1, r_middle2, r_middle3, r_middle_distal_tip, r_ring0, r_ring1, r_ring2,
            r_ring3, r_ring_distal_tip, r_pinky0, r_pinky1, r_pinky2, r_pinky3, r_pinky_distal_tip, l_upperarm_roll, l_forearm_roll, r_upperarm_roll, r_forearm_roll, l_thigh_roll, l_calf_roll,
            r_thigh_roll, r_calf_roll, skulltop, l_forefoot_tip, r_forefoot_tip, NONE
        };

        public static readonly string[] HAnimBoneNames = {
            HumanoidRoot, sacroiliac, l_hip, l_knee, l_ankle, l_subtalar, l_midtarsal, l_metatarsal, r_hip, r_knee, r_ankle, r_subtalar, r_midtarsal,
            r_metatarsal, vl5, vl4, vl3, vl2, vl1, vt12, vt11, vt10, vt9, vt8, vt7, vt6, vt5, vt4, vt3, vt2, vt1, vc7, vc6, vc5, vc4, vc3, vc2, vc1, skullbase, l_eyelid_joint, r_eyelid_joint,
            l_eyeball_joint, r_eyeball_joint, l_eyebrow_joint, r_eyebrow_joint, temporomandibular, l_sternoclavicular, l_acromioclavicular, l_shoulder, l_elbow, l_wrist, l_thumb1, l_thumb2,
            l_thumb3, l_thumb_distal_tip, l_index0, l_index1, l_index2, l_index3, l_index_distal_tip, l_middle0, l_middle1, l_middle2, l_middle3, l_middle_distal_tip, l_ring0, l_ring1, l_ring2,
            l_ring3, l_ring_distal_tip, l_pinky0, l_pinky1, l_pinky2, l_pinky3, l_pinky_distal_tip, r_sternoclavicular, r_acromioclavicular, r_shoulder, r_elbow, r_wrist, r_thumb1, r_thumb2,
            r_thumb3, r_thumb_distal_tip, r_index0, r_index1, r_index2, r_index3, r_index_distal_tip, r_middle0, r_middle1, r_middle2, r_middle3, r_middle_distal_tip, r_ring0, r_ring1, r_ring2,
            r_ring3, r_ring_distal_tip, r_pinky0, r_pinky1, r_pinky2, r_pinky3, r_pinky_distal_tip, l_upperarm_roll, l_forearm_roll, r_upperarm_roll, r_forearm_roll, l_thigh_roll, l_calf_roll,
            r_thigh_roll, r_calf_roll, skulltop, l_forefoot_tip, r_forefoot_tip
        };

        public static readonly Dictionary<HumanBodyBones, string> MecanimToHAnimMap = new Dictionary<HumanBodyBones, string> {
            {HumanBodyBones.Hips, vl5},
            {HumanBodyBones.LeftUpperLeg, l_hip},
            {HumanBodyBones.RightUpperLeg, r_hip},
            {HumanBodyBones.LeftLowerLeg, l_knee},
            {HumanBodyBones.RightLowerLeg, r_knee},
            {HumanBodyBones.LeftFoot, l_ankle},
            {HumanBodyBones.RightFoot, r_ankle},
            {HumanBodyBones.Spine, vt10},
            {HumanBodyBones.Chest, vt6},
            {HumanBodyBones.Neck, vc4},
            {HumanBodyBones.Head, skullbase},
            {HumanBodyBones.LeftShoulder, l_sternoclavicular},
            {HumanBodyBones.RightShoulder, r_sternoclavicular},
            {HumanBodyBones.LeftUpperArm, l_shoulder},
            {HumanBodyBones.RightUpperArm, r_shoulder},
            {HumanBodyBones.LeftLowerArm, l_elbow},
            {HumanBodyBones.RightLowerArm, r_elbow},
            {HumanBodyBones.LeftHand, l_wrist},
            {HumanBodyBones.RightHand, r_wrist},
            {HumanBodyBones.LeftToes, l_metatarsal},
            {HumanBodyBones.RightToes, r_metatarsal},
            {HumanBodyBones.LeftEye, l_eyeball_joint},
            {HumanBodyBones.RightEye, r_eyeball_joint},
            {HumanBodyBones.Jaw, temporomandibular},
            {HumanBodyBones.LeftThumbProximal, l_thumb1},
            {HumanBodyBones.LeftThumbIntermediate, l_thumb2},
            {HumanBodyBones.LeftThumbDistal, l_thumb3},
            {HumanBodyBones.LeftIndexProximal, l_index1},
            {HumanBodyBones.LeftIndexIntermediate, l_index2},
            {HumanBodyBones.LeftIndexDistal, l_index3},
            {HumanBodyBones.LeftMiddleProximal, l_middle1},
            {HumanBodyBones.LeftMiddleIntermediate, l_middle2},
            {HumanBodyBones.LeftMiddleDistal, l_middle3},
            {HumanBodyBones.LeftRingProximal, l_ring1},
            {HumanBodyBones.LeftRingIntermediate, l_ring2},
            {HumanBodyBones.LeftRingDistal, l_ring3},
            {HumanBodyBones.LeftLittleProximal, l_pinky1},
            {HumanBodyBones.LeftLittleIntermediate, l_pinky2},
            {HumanBodyBones.LeftLittleDistal, l_pinky3},
            {HumanBodyBones.RightThumbProximal, r_thumb1},
            {HumanBodyBones.RightThumbIntermediate, r_thumb2},
            {HumanBodyBones.RightThumbDistal, r_thumb3},
            {HumanBodyBones.RightIndexProximal, r_index1},
            {HumanBodyBones.RightIndexIntermediate, r_index2},
            {HumanBodyBones.RightIndexDistal, r_index3},
            {HumanBodyBones.RightMiddleProximal, r_middle1},
            {HumanBodyBones.RightMiddleIntermediate, r_middle2},
            {HumanBodyBones.RightMiddleDistal, r_middle3},
            {HumanBodyBones.RightRingProximal, r_ring1},
            {HumanBodyBones.RightRingIntermediate, r_ring2},
            {HumanBodyBones.RightRingDistal, r_ring3},
            {HumanBodyBones.RightLittleProximal, r_pinky1},
            {HumanBodyBones.RightLittleIntermediate, r_pinky2},
            {HumanBodyBones.RightLittleDistal, r_pinky3}
        };


        public static Transform[] Bones(Transform t) {
            List<Transform> res = new List<Transform>();
            res.Add(t);

            foreach (Transform child in t) {
                Transform[] childBones = Bones(child);
                res.AddRange(childBones);
            }
            return res.ToArray();
        }
    }


    public struct CanonicalCOSMapping {
        private Transform localBone;
        private Quaternion qInit;
        private Quaternion qInit_i;
        private Quaternion RG;
        private Quaternion RG_i;

        public CanonicalCOSMapping(Transform localBone) {
            this.localBone = localBone;
            this.qInit = Quaternion.identity;
            this.qInit_i = Quaternion.identity;
            this.RG = Quaternion.identity;
            this.RG_i = Quaternion.identity;
            MapCOS(localBone);
        }

        public Quaternion ToCanonical() {
            return ToCanonical(localBone.localRotation);
        }

        public Quaternion ToCanonical(Quaternion localRotation) {
            return RG * qInit_i * localRotation * RG_i;
        }

        public Vector3 ToCanonicalTranslation() {
            //Quaternion rotationC = ToCanonical(localBone.localRotation);
            Vector3 world = localBone.parent.TransformPoint(localBone.position);
            Vector3 canonicalOffset = world - localBone.parent.position;
            return //ToCanonical() *
                canonicalOffset;
        }

        public void ApplyFromCanonical(Vector3 canonicalTranslation) {
            //Debug.Log("Hello World "+canonicalTranslation.x +" "+canonicalTranslation.y+ " "+canonicalTranslation.z);
            localBone.position = canonicalTranslation;// localBone.parent.InverseTransformPoint(canonicalTranslation - );
            //Debug.Log("Hello World " + localBone.position.x + " " + localBone.position.y + " " + localBone.position.z);
            // localBone.localRotation *
            //canonicalTranslation;
        }

        public void ApplyFromCanonical(Quaternion canonicalRotation) {
            localBone.localRotation = FromCanonical(canonicalRotation);
        }


        public Quaternion FromCanonical(Quaternion canonicalRotation) {
            return qInit * RG_i * canonicalRotation * RG;
        }


        // Requires bone to be aligned with canonical bone
        public void MapCOS(Transform bone) {
            localBone = bone;
            qInit = bone.localRotation;
            qInit_i = Quaternion.Inverse(qInit);
            RG = bone.rotation;
            RG_i = Quaternion.Inverse(RG);
        }

        // TODO: Allow custom canonical skeletons

    }

    public class LocalToCanonical {
        public Transform localBone;
        public CanonicalRepresentation.HAnimBones canonicalBoneName;
        public CanonicalCOSMapping canonicalCOSMapping;
        public LocalToCanonical parent;

        public LocalToCanonical(Transform localBone, CanonicalRepresentation.HAnimBones canonicalBoneName) {
            this.localBone = localBone;
            this.canonicalBoneName = canonicalBoneName;
            this.canonicalCOSMapping = new CanonicalCOSMapping(localBone);
            this.parent = null;
        }

        public void LinkParent(LocalToCanonical parent) {
            this.parent = parent;
        }
    }

    // Pose of local skeleton in canonical representation
    public struct CanonicalSkeletonPose {
        public CanonicalRepresentation.HAnimBones[] canonicalBoneNames;
        public CanonicalRepresentation.HAnimBones rootTranslationBone;
        public Quaternion[] bonePoses;
        public Vector3[] boneTranslations;
        public float timestamp;

        public CanonicalSkeletonPose(LocalToCanonical[] ltc, float t, CanonicalRepresentation.HAnimBones rootTranslationBone) {
            this.canonicalBoneNames = new CanonicalRepresentation.HAnimBones[ltc.Length];
            this.bonePoses = new Quaternion[ltc.Length];
            this.boneTranslations = new Vector3[ltc.Length];
            this.timestamp = t;
            this.rootTranslationBone = rootTranslationBone;
            for (int i = 0; i < ltc.Length; i++) {
                this.canonicalBoneNames[i] = ltc[i].canonicalBoneName;
            }
            UpdateCanonicalSkeletonPose(ltc);
        }

        public void UpdateCanonicalSkeletonPose(LocalToCanonical[] ltc) {
            if (this.bonePoses == null || this.bonePoses.Length != ltc.Length)
                throw new System.Exception("This mapping is not compatible with CanonicalSkeletonPose");

            for (int i = 0; i < ltc.Length; i++) {
                if (this.canonicalBoneNames[i] == CanonicalRepresentation.HAnimBones.NONE)
                    continue;
                this.bonePoses[i] = ltc[i].canonicalCOSMapping.ToCanonical();
                //if (this.canonicalBoneNames[i] == CanonicalMapping.HAnimBones.HumanoidRoot) {
                if (ltc[i].localBone.parent != null)
                    //ltc[i].localBone.localPosition
                    //ltc[i].canonicalCOSMapping.
                    //this.boneTranslations[i] = ltc[i].localBone.parent.TransformPoint(ltc[i].localBone.position);
                    this.boneTranslations[i] = ltc[i].canonicalCOSMapping.ToCanonicalTranslation();
                else
                    this.boneTranslations[i] = ltc[i].localBone.position;
                /*
                if (this.canonicalBoneNames[i] == rootTranslationBone) {

                } else {
                    this.boneTranslations[i] = ltc[i].localBone.position - ltc[i].localBone.parent.position;
                }*/
            }
        }

    }


    public interface ICanonicalPoseProvider {
        void ExportPose();
    }

    public interface ICanonicalPoseClipConverter {
        void ExportClip(AnimationClip clip);
        Animator GetAnimator();
    }

    public interface ICanonicalPoseTarget {
        void ApplyPose();
    }

}