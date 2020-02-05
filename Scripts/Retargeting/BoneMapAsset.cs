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