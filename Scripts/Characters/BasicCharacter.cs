using UnityEngine;
using ASAPToolkit.Unity.Retargeting;

namespace ASAPToolkit.Unity.Characters {

    public class BasicCharacter : ASAPAgent {

        public ICharacterSkeleton skeleton;
        public ICharacterFace face;
        private bool initialized = false;

        void Awake() {
        }

        void Start() {
            if (skeleton == null) skeleton = GetComponent<ICharacterSkeleton>();
            if (face == null) face = GetComponent<ICharacterFace>();
        }

        void Update() {
            if (!initialized) {
                Initialize();
                return;
            }
        }

        public override void Initialize() {

            if (face != null && !face.Ready()) return;
            if (skeleton != null && !skeleton.Ready()) return;

            // TODO: asap needs some default joints...
            VJoint[] vJoints = new VJoint[0];
            if (skeleton != null) vJoints = skeleton.GenerateVJoints();

            IFaceTarget[] faceTargets = new IFaceTarget[0];
            if (face != null) faceTargets = face.GetFaceTargets();

            agentSpec = new AgentSpec(agentId, vJoints, faceTargets);
            FindObjectOfType<ASAPToolkitManager>().OnAgentInitialized(this);
            initialized = true;
        }

        public override void ApplyAgentState() {
            if (agentState.boneValues.Length >= 2 && skeleton != null) {
                Quaternion[] pose = new Quaternion[agentState.boneValues.Length];
                Vector3[] rootTransforms = new Vector3[2];
                for (int b = 0; b < agentState.boneValues.Length; b++) {
                    if (b < 2) {
                        rootTransforms[b] = new Vector3(
                            -agentState.boneTranslations[b].t[0], // Minus x value b/c of different COS in ASAP
                             agentState.boneTranslations[b].t[1],
                             agentState.boneTranslations[b].t[2]);
                    }
                    pose[b] = new Quaternion(
                        -agentState.boneValues[b].r[0], // Same with order and sign of quat values
                         agentState.boneValues[b].r[1],
                         agentState.boneValues[b].r[2],
                        -agentState.boneValues[b].r[3]);
                }

                skeleton.ApplyPose(pose, rootTransforms);
            }

            if (face != null && agentState.faceTargetValues != null && agentState.faceTargetValues.Length > 0) { 
                face.SetFaceTargetValues(agentState.faceTargetValues);
            }
        }
    }

}