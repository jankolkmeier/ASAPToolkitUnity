// Based on (UMA)ExpressionPlayer.cs by Eli Curtz

using UnityEngine;
using UMA;
using UMA.PoseTools;
using System.Collections;
using System.Collections.Generic;
using static UMA.PoseTools.ExpressionPlayer;

namespace ASAPToolkit.Unity.Characters.UMA  {

    public class UMABonePoseFace : MonoBehaviour, ICharacterFace {

        public BonePoseTargetSet targetSet;
        public UMAExpressionSet expressionSet;
        private FaceTargetControlMapping[] expressionControls;

        private bool overrideMecanimEyes = false;
        private bool overrideMecanimJaw = true;
        private bool overrideMecanimNeck = false;
        private bool overrideMecanimHead = false;

        [System.NonSerialized]
        public UMAData umaData;

        private int jawHash = 0;
        private int neckHash = 0;
        private int headHash = 0;

        private bool initialized = false;


        [System.NonSerialized]
        public bool editing = false;

        public float[] values;

        // Use this for initialization
        void Start() {
        }

        public void Initialize() {
            if (umaData == null) umaData = gameObject.GetComponent<UMAData>();
            if (umaData == null) umaData = gameObject.GetComponentInChildren<UMAData>();
            if (umaData == null) umaData = gameObject.GetComponentInParent<UMAData>();

            if (umaData != null && umaData.skeleton != null) {

                expressionSet = umaData.umaRecipe.raceData.expressionSet;

                if (umaData.animator != null) {
                    Transform jaw = umaData.animator.GetBoneTransform(HumanBodyBones.Jaw);
                    if (jaw != null)
                        jawHash = UMAUtils.StringToHash(jaw.name);

                    Transform neck = umaData.animator.GetBoneTransform(HumanBodyBones.Neck);
                    if (neck != null)
                        neckHash = UMAUtils.StringToHash(neck.name);

                    Transform head = umaData.animator.GetBoneTransform(HumanBodyBones.Head);
                    if (head != null)
                        headHash = UMAUtils.StringToHash(head.name);
                }

                if (expressionSet != null) {
                    values = new float[expressionSet.posePairs.Length];
                    initialized = true;
                }
            }
        }

        public bool Ready() {
            return initialized;
        }

        void Update() {
            if (!initialized) {
                Initialize();
                return;
            }

            // Fix for animation systems which require consistent values frame to frame
            Quaternion headRotation = umaData.skeleton.GetRotation(headHash);
            Quaternion neckRotation = umaData.skeleton.GetRotation(neckHash);

            // Need to reset bones here if we want Mecanim animation
            expressionSet.RestoreBones(umaData.skeleton, false);

            if (!overrideMecanimNeck)
                umaData.skeleton.SetRotation(neckHash, neckRotation);
            if (!overrideMecanimHead)
                umaData.skeleton.SetRotation(headHash, headRotation);
        }

        void LateUpdate() {
            if (!initialized) return;
            MecanimJoint mecanimMask = MecanimJoint.None;
            if (!overrideMecanimNeck)
                mecanimMask |= MecanimJoint.Neck;
            if (!overrideMecanimHead)
                mecanimMask |= MecanimJoint.Head;
            if (!overrideMecanimJaw)
                mecanimMask |= MecanimJoint.Jaw;
            if (!overrideMecanimEyes)
                mecanimMask |= MecanimJoint.Eye;
            if (overrideMecanimJaw)
                umaData.skeleton.Restore(jawHash);

            for (int i = 0; i < values.Length; i++) {
                if (Mathf.Approximately(values[i], 0f) || (MecanimAlternate[i] & mecanimMask) != MecanimJoint.None)
                    continue;
                if (expressionSet.posePairs[i].inverse == null) 
                     values[i] = Mathf.Clamp(values[i],    0f, 1.0f);
                else values[i] = Mathf.Clamp(values[i], -1.0f, 1.0f);

                if (values[i] >= 0) {
                    expressionSet.posePairs[i].primary.ApplyPose(umaData.skeleton,  values[i]);
                } else {
                    expressionSet.posePairs[i].inverse.ApplyPose(umaData.skeleton, -values[i]);
                }
            }
        }


        public void SetFaceTargetValues(float[] targetValues) {
            if (editing || targetValues.Length != expressionControls.Length) return;
            float[] newValues = new float[values.Length];

            for (int f = 0; f < targetValues.Length; f++) {
                if (Mathf.Approximately(targetValues[f], 0.0f)) continue;
                for (int c = 0; c < expressionControls[f].indexes.Length; c++) {
                    newValues[expressionControls[f].indexes[c]] +=
                        expressionControls[f].values[c] * targetValues[f];
                }
            }
            values = newValues;
        }


        public IFaceTarget[] GetFaceTargets() {
            if (!initialized) return null;

            List<IFaceTarget> faceTargets = new List<IFaceTarget>();
            List<FaceTargetControlMapping> controlMappings = new List<FaceTargetControlMapping>();

            foreach (BonePoseTarget bpt in targetSet.bonePoseTargets) {
                faceTargets.Add(new ExpressionPlayerFaceTarget(bpt.name));
                controlMappings.Add(new FaceTargetControlMapping(bpt.GetTargetBonePoseNames(), bpt.GetTargetBonePoseValues()));
            }
            expressionControls = controlMappings.ToArray();
            return faceTargets.ToArray();
        }

        public class FaceTargetControlMapping {
            public float[] values;
            public int[] indexes;

            public FaceTargetControlMapping(string[] controls, float[] values) {
                this.values = values;
                this.indexes = new int[values.Length];
                for (int i = 0; i < values.Length; i++) {
                    indexes[i] = System.Array.IndexOf(ExpressionPlayer.PoseNames, controls[i]);
                }
            }
        }


        public class ExpressionPlayerFaceTarget : IFaceTarget {
            public string name;

            public ExpressionPlayerFaceTarget(string name) {
                this.name = name;
            }

            public float GetMinValue() {
                return 0f;
            }

            public float GetMaxValue() {
                return 1f;
            }

            public string GetName() {
                return name;
            }
        }
    }
}
