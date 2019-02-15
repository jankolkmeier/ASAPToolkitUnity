using UnityEngine;
using ASAPToolkit.Unity.Retargeting;

using UnityEditor;

namespace ASAPToolkit.Unity.Characters {

    public class BasicCharacter : ASAPAgent {

        public ICharacterSkeleton skeleton;
        public ICharacterFace face;
        private bool initialized = false;

        public Transform rootBonePose;

        public Transform[] rootDebug;


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
            if (skeleton != null) vJoints = skeleton.GenerateVJoints(rootBonePose);

            IFaceTarget[] faceTargets = new IFaceTarget[0];
            if (face != null) faceTargets = face.GetFaceTargets();

            agentSpec = new AgentSpec(agentId, vJoints, faceTargets);
            ASAPToolkitManager atkm = FindObjectOfType<ASAPToolkitManager>();
            if (atkm != null) atkm.OnAgentInitialized(this);
            initialized = true;
        }

        public override void ApplyAgentState() {
            if (agentState.boneRotationsParsed.Length >= 2 && skeleton != null) {
                skeleton.ApplyPose(agentState.boneRotationsParsed, agentState.boneTranslationsParsed);
            }

            if (face != null && agentState.faceTargetValues != null && agentState.faceTargetValues.Length > 0) {
                face.SetFaceTargetValues(agentState.faceTargetValues);
            }

        }
    }

}

/*
public class MyScriptGizmoDrawer {
    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    static void DrawGizmoForMyScript(ASAPToolkit.Unity.Characters.BasicCharacter c, GizmoType gizmoType) {

        if (c.agentState.boneValues.Length >= 2 && c.skeleton != null) {
            Quaternion[] pose = new Quaternion[c.agentState.boneValues.Length];
            Vector3[] rootTransforms = new Vector3[c.agentState.boneValues.Length];
            for (int b = 0; b < c.agentState.boneValues.Length; b++) {
                pose[b] = new Quaternion(
                    -c.agentState.boneValues[b].r[0], // Same with order and sign of quat values
                     c.agentState.boneValues[b].r[1],
                     c.agentState.boneValues[b].r[2],
                    -c.agentState.boneValues[b].r[3]);
                
                rootTransforms[b] = new Vector3(
                    -c.agentState.boneTranslations[b].t[0], // Minus x value b/c of different COS in ASAP
                     c.agentState.boneTranslations[b].t[1],
                     c.agentState.boneTranslations[b].t[2]);


                Gizmos.DrawSphere(rootTransforms[b], 0.05f);
            }
        }
    }
}*/