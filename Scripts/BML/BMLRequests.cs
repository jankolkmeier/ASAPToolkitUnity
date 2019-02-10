using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASAPToolkit.Unity.BML {

    [RequireComponent(typeof(BMLManager))]
    public class BMLRequests : MonoBehaviour {
        BMLManager manager;

        void Start() {
            manager = GetComponent<BMLManager>();
        }

        public void SendBML(string bml) {
            manager.SendBML(bml);
        }
    }

}