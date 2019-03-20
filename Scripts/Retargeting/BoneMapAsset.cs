using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASAPToolkit.Unity.Retargeting { 

    [CreateAssetMenu(fileName = "New BoneMapAsset", menuName = "ASAP/BoneMapAsset", order = 1)]
    public class BoneMapAsset : ScriptableObject {

        [System.Serializable]
        public struct HAnimBoneMapping {
            public string src_bone;
            public CanonicalRepresentation.HAnimBones hanim_bone;
        }

        [System.Serializable]
        public struct ExtraBone {
            public CanonicalRepresentation.HAnimBones hanim_bone;
            public string parent_src_bone;
            public string bone_name;
            public Vector3 localPosition;
            public Vector3 localEulerRotation;
        }

        public HAnimBoneMapping[] mappings;
        public ExtraBone[] extraBones;
    }

}