using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

[Serializable]
[CreateAssetMenu(fileName = "BonePoseTargetSet", menuName = "ASAP/BonePoseTargetSet", order = 1)]
public class BonePoseTargetSet : ScriptableObject {
    [SerializeField]
    public BonePoseTarget[] bonePoseTargets;

    public string[] BonePoseTargetNames() {
        return bonePoseTargets.Select(a => a.name).ToArray();
    }
}