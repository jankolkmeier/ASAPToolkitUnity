using System.Collections;
using System.Collections.Generic;
using ASAPToolkit.Unity.Retargeting;
using UMA;
using UnityEngine;

namespace ASAPToolkit.Unity.Characters.UMA {

    public class UMASkeleton : SkeletonConfiguration {


        UMAData umaData;
        private bool uma_initialized;

        // TODO: instead, this should bind to uma generated/ready etc events...


        public void Update() {
            if (uma_initialized) return;
            if (!umaData) umaData = GetComponent<UMAData>();
            if (!umaData) umaData = GetComponentInChildren<UMAData>();
            if (!umaData) umaData = GetComponentInParent<UMAData>();
            if (umaData && umaData.skeleton != null) {
                uma_initialized = true;
            }
        }

        public override bool Ready() {
            return uma_initialized;
        }

    }

}