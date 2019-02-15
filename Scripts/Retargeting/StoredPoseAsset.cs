using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ASAPToolkit.Unity.Retargeting {
    [CreateAssetMenu(fileName = "New StoredPoseAsset", menuName = "ASAP/StoredPoseAsset", order = 1)]
    public class StoredPoseAsset : ScriptableObject {

        [System.Serializable]
        public class StoredPose {
            public string src_bone;
            public Quaternion rotation;
            public Vector3 position;
        }

        public StoredPose[] pose;

        public static StoredPose[] ApplyPose(StoredPose[] pose, Transform root) {
            if (root == null) {
                Debug.LogError("Cannot apply pose to null root");
                return null;
            }

            StoredPose[] _save = StoredPoseAsset.StorePose(root);
            Transform[] bones = CanonicalRepresentation.Bones(root);
            foreach (Transform bone in bones) {
                StoredPose sp = pose.FirstOrDefault(p => p.src_bone == bone.name);
                if (sp == null) continue;
                if (bone.parent == null) {
                    bone.rotation = sp.rotation;
                    //if (bone == root) bone.position =   sp.position;
                } else {
                    bone.localRotation = sp.rotation;
                    //if (bone == root) bone.localPosition = sp.position;
                }
            }
            return _save;
        }

        public static StoredPose[] StorePose(Transform root) {
            Transform[] bones = CanonicalRepresentation.Bones(root);
            StoredPose[] res = new StoredPoseAsset.StoredPose[bones.Length];
            for (int t = 0; t < bones.Length; t++) {
                res[t] = new StoredPose();
                res[t].src_bone = bones[t].name;
                if (bones[t].parent == null) {
                    res[t].rotation = bones[t].rotation;
                    res[t].position = bones[t].position;
                } else {
                    res[t].rotation = bones[t].localRotation;
                    res[t].position = bones[t].localPosition;
                }
            }
            return res;
        }

    }
}