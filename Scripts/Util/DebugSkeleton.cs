using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASAPToolkit.Unity.Util { 
    [ExecuteInEditMode]
    public class DebugSkeleton : MonoBehaviour {
        public float cosScale = 0.05f;
        public Color color = new Color(1.0f, 0.0f, 0.0f, 1.0f);

        private Animator animator;

        public string ignore = "Adjust";
        public bool startAtHip = false;

        // Use this for initialization
        void Start() {
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update() {
            if (startAtHip && animator != null && animator.GetBoneTransform(HumanBodyBones.Hips)) {
                DrawToChilds(animator.GetBoneTransform(HumanBodyBones.Hips));
                DrawLocalCOS(animator.GetBoneTransform(HumanBodyBones.Hips));
            } else {
                DrawToChilds(transform);
                DrawLocalCOS(transform);
            }
        }

        void DrawToChilds(Transform t) {
            if (t == null) return;
            foreach (Transform child in t) {
                //if (child.name[0] == '_') continue;
                if (!child.name.Contains(ignore)) {
                    Debug.DrawLine(t.position, child.position, color);
                    DrawToChilds(child);
                    DrawLocalCOS(child);
                }
            }
        }

        void DrawLocalCOS(Transform t) {
            Debug.DrawLine(t.position, t.position + t.right * cosScale, Color.red);
            Debug.DrawLine(t.position, t.position + t.up * cosScale, Color.green);
            Debug.DrawLine(t.position, t.position + t.forward * cosScale, Color.blue);
        }
    }
}