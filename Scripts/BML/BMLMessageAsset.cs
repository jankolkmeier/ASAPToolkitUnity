using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "BMLMessage", menuName = "ASAP/BMLMessageAsset", order = 1)]
public class BMLMessageAsset : ScriptableObject {
    [SerializeField]
    public string[] bmls;
}