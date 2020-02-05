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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASAPToolkit.Unity.Middleware;

namespace ASAPToolkit.Unity.BML {

    [RequireComponent(typeof(IMiddleware))]
    [RequireComponent(typeof(BMLFeedback))]
    public class BMLManager : MonoBehaviour, IMiddlewareListener {
        
        private BMLFeedback feedback;
        private MiddlewareBase middleware;

        void Start() {
            feedback = GetComponent<BMLFeedback>();
            middleware = GetComponent<MiddlewareBase>();
            if (middleware != null) middleware.Register(this);
        }

        public void SendBML(string bml) {
            Send(JsonUtility.ToJson(new BMLMiddlewareMessage {
                bml = new MiddlewareContent { content = System.Uri.EscapeDataString(bml) }
            }));
        }

        public void SendBML_noEscape(string escapedBML) {
            Send(JsonUtility.ToJson(new BMLMiddlewareMessage {
                bml = new MiddlewareContent { content = escapedBML }
            }));
        }

        public void Send(string data) {
            middleware.Send(new MSG(data));
        }

        public void OnMessage(MSG msg) {
            if (msg.data.Length == 0) return;
            try {
                FeedbackMiddlewareMessage fmsg = JsonUtility.FromJson<FeedbackMiddlewareMessage>(msg.data);
                feedback.HandleFeedback(System.Uri.UnescapeDataString(fmsg.feedback.content).Replace('+', ' '));
            } catch (System.ArgumentException ae) {
                Debug.Log("Message not valid JSON:\n" + msg.data + "\n\n" + ae);
            }
        }

        void OnApplicationQuit() {
        }
    }

    [System.Serializable]
    public class FeedbackMiddlewareMessage {
        public MiddlewareContent feedback;
    }

    [System.Serializable]
    public class BMLMiddlewareMessage {
        public MiddlewareContent bml;
    }

    [System.Serializable]
    public class MiddlewareContent {
        public string content;
    }
}