using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
[CreateAssetMenu(fileName = "BonePoseTarget", menuName = "ASAP/BonePoseTarget", order = 1)]
public class BonePoseTarget : ScriptableObject {

    [System.Serializable]
    public struct BonePoseSetting {
        public string poseName;
        public float poseValue;
    }

    [SerializeField]
    public BonePoseSetting[] bonePoseSettings;

    public string[] GetTargetBonePoseNames() {
        return bonePoseSettings.Select(a => a.poseName).ToArray();
    }

    public float[] GetTargetBonePoseValues() {
        return bonePoseSettings.Select(a => a.poseValue).ToArray();
    }
}
