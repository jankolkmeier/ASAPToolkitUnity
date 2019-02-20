using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using System.Linq;
using ASAPToolkit.Unity.Util;
using ASAPToolkit.Unity.Retargeting;

namespace ASAPToolkit.Unity.Characters {

    [RequireComponent(typeof(UMADynamicAvatar))]
    public class DynamicUMASkeleton : BasicSkeleton {

        private UMADynamicAvatar uma;

/*
        public AvatarMask currentAvatarMask;
        public bool exportRestPose;
        public bool T1R;
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
                MappedBone _ltc = rig.boneMap.FirstOrDefault(m => m.canonicalBoneName == canonicalBone);
                if (_ltc == null) continue;
                if (T1R && _rotations.Count == 0) {
                    translation = _ltc.CurrentPositionInCanonical();
                }

                _rotations.Add(_ltc.CurrentRotationInCanonical());
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
 */

        protected override void Awake() {
            base.Awake();
            uma = GetComponent<UMADynamicAvatar>();
            uma.CharacterCreated.AddListener(OnCreated);
            uma.CharacterUpdated.AddListener(OnUpdated);
        }

        void OnCreated(UMAData umaData) {
            OnUpdated(umaData);
        }

        void OnUpdated(UMAData umaData) {
            vJoints = null;
            vJoints = GenerateVJoints();
            AddDefaultTargets(umaData);
            _ready = vJoints != null;
        }

        public void AddDefaultTargets(UMAData umaData) {
            ASAPAgent agent = GetComponent<ASAPAgent>();
            if (agent == null) return;
            Transform head = umaData.GetBoneGameObject("Head").transform;
            Transform headcenter = head.Find("head_" + agent.agentId);
            if (headcenter == null) {
                headcenter = new GameObject("head_" + agent.agentId).transform;
                headcenter.parent = head;
                headcenter.localPosition = new Vector3(-0.115f, 0.0f, 0.0f);
                headcenter.localRotation = Quaternion.identity;
                headcenter.gameObject.AddComponent<Environment.WorldObject>();
            }
        }
     
        public override bool Ready() {
            return _ready;
        }

    }

}