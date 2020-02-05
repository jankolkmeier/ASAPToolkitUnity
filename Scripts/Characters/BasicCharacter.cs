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