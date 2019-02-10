#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ASAPToolkit.Unity.Editor {

    public class BMLEditorWindow : EditorWindow {

        private static string defaultBml = "<bml id=\"bml1\" characterId=\"\" xmlns=\"http://www.bml-initiative.org/bml/bml-1.0\" \n  xmlns:bmlt=\"http://hmi.ewi.utwente.nl/bmlt\">\n  <speech id=\"s1\">\n    <text>Hello World</text>\n  </speech>\n</bml>";

        private Vector2 scroll;

        private BMLMessageAsset testBmlAsset;

        private SerializedObject serializedObj;

        private string[] guids;
        private string[] labels;
        private int selection;

        void Populate() {
            string currentGuid = null;
            if (guids != null && guids.Length > selection && selection != -1) {
                currentGuid = guids[selection];
            }

            guids = AssetDatabase.FindAssets("t:BMLMessageAsset");
            List<string> _labels = new List<string>();

            if (currentGuid != null) {
                int idxCurrentGuid = Array.IndexOf(guids, currentGuid);
                if (idxCurrentGuid >= 0) selection = idxCurrentGuid;
            }

            for (int i = 0; i < guids.Length; i++) {
                BMLMessageAsset bmlta = AssetDatabase.LoadAssetAtPath<BMLMessageAsset>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (bmlta != null) _labels.Add(bmlta.name);
            }

            labels = _labels.ToArray();

            if (guids.Length > 0) {
                if (selection >= guids.Length || selection < 0) {
                    selection = 0;
                }
                testBmlAsset = AssetDatabase.LoadAssetAtPath<BMLMessageAsset>(AssetDatabase.GUIDToAssetPath(guids[selection]));
                serializedObj = new SerializedObject(testBmlAsset);
            } else {
                selection = -1;
            }
        }

        private void OnGUI() {

            GUIStyle delStyle = new GUIStyle(GUI.skin.button);
            GUIStyle addStyle = new GUIStyle(GUI.skin.button);
            GUIStyle sndStyle = new GUIStyle(GUI.skin.button);
            delStyle.normal.textColor = new Color(0.35f, 0.1f, 0.1f);
            addStyle.normal.textColor = new Color(0.1f, 0.35f, 0.1f);
            sndStyle.normal.textColor = new Color(0.1f, 0.1f, 0.35f);
            delStyle.stretchWidth = false;
            addStyle.stretchWidth = false;
            sndStyle.stretchWidth = false;


            GUIStyle popStyle = new GUIStyle(EditorStyles.popup);

            popStyle.margin = new RectOffset(5, 5, 5, 5);

            GUILayout.BeginHorizontal();
            if (testBmlAsset == null || serializedObj == null) {
                return;
            }

            int curr = selection;
            selection = EditorGUILayout.Popup(selection, labels, popStyle);
            if (curr != selection) Populate();

            serializedObj.Update();
            SerializedProperty bmls = serializedObj.FindProperty("bmls");

            if (GUILayout.Button("SEND ALL", sndStyle, GUILayout.Height(19.0f))) {
                for (int i = 0; i < testBmlAsset.bmls.Length; i++) {
                    FindObjectOfType<BML.BMLRequests>().SendBML(testBmlAsset.bmls[i]);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(EditorStyles.helpBox);

            scroll = GUILayout.BeginScrollView(scroll, false, false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", addStyle, GUILayout.Width(20.0f))) {
                bmls.InsertArrayElementAtIndex(0);
                bmls.GetArrayElementAtIndex(0).stringValue = defaultBml;
            }
            GUILayout.EndHorizontal();

            for (int i = 0; i < serializedObj.FindProperty("bmls").arraySize; i++) {
                SerializedProperty bmlProp = bmls.GetArrayElementAtIndex(i);

                GUILayout.BeginHorizontal();
                //bmlProp.stringValue = GUILayout.TextArea(bmlProp.stringValue, GUILayout.ExpandHeight(true));
                bmlProp.stringValue = EditorGUILayout.TextArea(bmlProp.stringValue, GUILayout.ExpandHeight(true));
                if (GUILayout.Button("X", delStyle, GUILayout.Width(20.0f), GUILayout.ExpandHeight(true))) {
                    bmls.DeleteArrayElementAtIndex(i);
                    break;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", addStyle, GUILayout.Width(20.0f))) {
                    bmls.InsertArrayElementAtIndex(i + 1);
                    bmls.GetArrayElementAtIndex(i + 1)
                        .stringValue = defaultBml;
                    break;
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.Space(10.0f);
            serializedObj.ApplyModifiedProperties();



        }

        void OnSelectionChange() {
            Populate();
            Repaint();
        }

        void OnEnable() {
            Populate();
        }

        void OnFocus() {
            Populate();
        }
    }

}
#endif