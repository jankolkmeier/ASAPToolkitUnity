using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ASAPToolkit.Unity.Util {

    public static class Extensions {

        public static Transform FindChildRecursive(this Transform parent, string name) {
            if (parent.name == name) return parent;
            foreach (Transform child in parent) {
                Transform childResult = child.FindChildRecursive(name);
                if (childResult != null) return childResult;
            }
            return null;
        }


        public static float[] ToASAP(this Vector3 v) {
            return new float[] { -v.x, v.y, v.z };
        }

        public static string ToASAPString(this Vector3 v) {
            return string.Join(" ", v.ToASAP().Select(f => f.ToString("0.0##")).ToArray());
        }

        public static float[] ToASAP(this Quaternion q) {
            return new float[] { -q.w, -q.x, q.y, q.z };
        }

        public static string ToASAPString(this Quaternion q) {
            return string.Join(" ", q.ToASAP().Select(f => f.ToString("0.0##")).ToArray());
        }

    }
}