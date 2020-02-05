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
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UMA.PoseTools;
using UMA;
using ASAPToolkit.Unity.Characters.UMA;

namespace ASAPToolkit.Unity.Editor {

    [CustomEditor(typeof(UMABonePoseFace), true)]
    public class UMABonePoseFaceEditor : UnityEditor.Editor {


        UMABonePoseFace editor;


        private int curIdx;

        public void OnEnable() {
            editor = target as UMABonePoseFace;
            editor.editing = false;
        }

        private void Init() {
            lastEditName = "";
            editor.editing = true;
        }

        private void UnInit() {
            editor.editing = false;
        }

        private string lastEditName;
        private float[] values;
        private bool[] currentUseControl;
        private void EnsureLoaded() {
            if (editor == null || !editor.editing || !editor.Ready()) return;
            if (lastEditName != editor.targetSet.bonePoseTargets[curIdx].name) {
                editor.values = new float[editor.values.Length];
                currentUseControl = new bool[editor.values.Length];
                foreach (BonePoseTarget.BonePoseSetting bpts in editor.targetSet.bonePoseTargets[curIdx].bonePoseSettings) {
                    int poseIdx = System.Array.IndexOf(ExpressionPlayer.PoseNames, bpts.poseName);
                    if (poseIdx < 0 || poseIdx > editor.values.Length) {
                        Debug.LogWarning("Can't find BonePose: "+bpts.poseName);
                        continue;
                    }
                    editor.values[poseIdx] = bpts.poseValue;
                    currentUseControl[poseIdx] = true;
                }
                lastEditName = editor.targetSet.bonePoseTargets[curIdx].name;
            }
        }

        private void SaveCurrent() {
            List<BonePoseTarget.BonePoseSetting> newSettings = new List<BonePoseTarget.BonePoseSetting>();
            for (int i = 0; i < ExpressionPlayer.PoseCount; i++) {
                if (!currentUseControl[i]) continue;
                BonePoseTarget.BonePoseSetting bpts;
                bpts.poseName = ExpressionPlayer.PoseNames[i];
                bpts.poseValue = editor.values[i];
                newSettings.Add(bpts);
            }

            editor.targetSet.bonePoseTargets[curIdx].bonePoseSettings = newSettings.ToArray();
            EditorUtility.SetDirty(editor.targetSet.bonePoseTargets[curIdx]);
        }

        public override void OnInspectorGUI() {
            if (!Application.isPlaying) {
                EditorGUILayout.HelpBox("You can cutsomize face targets in play mode.", UnityEditor.MessageType.Info);
            } else {
                if (editor != null && editor.Ready() && !editor.editing && GUILayout.Button("Start Editing")) {
                    Init();
                } else if (editor.editing && GUILayout.Button("Stop Editing")) {
                    UnInit();
                }
            }

            DrawDefaultInspector();

            if (!editor.editing) return;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            if (editor.targetSet == null || editor.targetSet.bonePoseTargets == null || editor.targetSet.bonePoseTargets.Length == 0) {
                EditorGUILayout.HelpBox("Please configure the targetSet property first (with at least 1 BonePoseTarget added)", UnityEditor.MessageType.Info);
                return;
            }

            //editor.writeValues = GUILayout.Toggle(editor.writeValues, "Write?");
            string[] targetNames = editor.targetSet.BonePoseTargetNames();
            if (curIdx < 0) curIdx = targetNames.Length - 1;
            if (curIdx >= targetNames.Length) curIdx = 0;
            curIdx = EditorGUILayout.Popup(curIdx, targetNames);
            EditorGUILayout.HelpBox(targetNames[curIdx], UnityEditor.MessageType.Info);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev")) {
                curIdx = curIdx - 1;
                return;
            }
            if (GUILayout.Button("Next")) {
                curIdx = curIdx + 1;
                return;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            EnsureLoaded();

            GUILayout.Space(5.0f);

            GUILayout.BeginVertical();
            for (int i = 0; i < ExpressionPlayer.PoseCount; i++) {
                GUILayout.BeginHorizontal();
                currentUseControl[i] = GUILayout.Toggle(currentUseControl[i], ExpressionPlayer.PoseNames[i], GUILayout.Width(130));
                float minVal = -1.0f;
                if (editor.expressionSet.posePairs[i].inverse == null) {
                    minVal = 0.0f;
                }
                float currentTarget = GUILayout.HorizontalSlider(editor.values[i], minVal, 1.0f);
                if (!Mathf.Approximately(currentTarget, editor.values[i])) {
                    editor.values[i] = currentTarget;
                    currentUseControl[i] = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(5.0f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset")) {
                lastEditName = "";
                EnsureLoaded();
            }
            if (GUILayout.Button("Save")) {
                SaveCurrent();
            }
            GUILayout.EndHorizontal();

        }
    }

}
#endif