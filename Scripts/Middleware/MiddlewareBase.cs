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
using System.Threading;
using UnityEngine;

namespace ASAPToolkit.Unity.Middleware {

    public struct MSG {
        public string data;
        public string src;

        public MSG(string d) {
            this.data = d;
            this.src = "";
        }

        public MSG(string d, string s) {
            this.data = d;
            this.src = s;
        }
    }

    public interface IMiddleware {
        void Send(MSG msg);
        void Register(IMiddlewareListener l);
        void Unregister(IMiddlewareListener l);
    }

    public interface IMiddlewareListener {
        void OnMessage(MSG msg);
    }

    public abstract class MiddlewareBase : MonoBehaviour, IMiddleware {
        public int maxReadBufferSize = -1;
        public int maxSendBufferSize = -1;

        protected ManualResetEvent send_MRSTE = new ManualResetEvent(false);

        private List<IMiddlewareListener> listeners = new List<IMiddlewareListener>();
        protected Queue<MSG> _sendQueue = new Queue<MSG>();
        protected Queue<MSG> _receiveQueue = new Queue<MSG>();
        protected System.Object _sendQueueLock = new System.Object();
        protected System.Object _receiveQueueLock = new System.Object();

        public void Register(IMiddlewareListener l) {
            listeners.Add(l);
        }

        public void Unregister(IMiddlewareListener l) {
            listeners.Remove(l);
        }

        private void Update() {
            bool haveData = true;
            while (haveData) {
                MSG msg = new MSG("");
                lock (_receiveQueueLock) {
                    while (maxReadBufferSize > 0 && _receiveQueue.Count > maxReadBufferSize) _receiveQueue.Dequeue();
                    if (_receiveQueue.Count > 0) msg = _receiveQueue.Dequeue();
                    else haveData = false;
                }
                if (msg.data == null || msg.data.Length == 0) return;

                foreach (IMiddlewareListener l in listeners) {
                    l.OnMessage(msg);
                }
            }
        }

        public void Send(string data) {
            lock (_sendQueueLock) {
                while (maxSendBufferSize > 0 && _sendQueue.Count > maxSendBufferSize) _sendQueue.Dequeue();
                _sendQueue.Enqueue(new MSG(data));
                send_MRSTE.Set();
                send_MRSTE.Reset();
            }
        }

        public void Send(MSG msg) {
            lock (_sendQueueLock) {
                while (maxSendBufferSize > 0 && _sendQueue.Count > maxSendBufferSize) _sendQueue.Dequeue();
                _sendQueue.Enqueue(msg);
                send_MRSTE.Set();
                send_MRSTE.Reset();
            }
        }
    }

}