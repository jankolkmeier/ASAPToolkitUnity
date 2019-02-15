using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ASAPToolkit.Unity.Middleware {

    public interface IMiddleware {
        void Send(string data);
        void Register(IMiddlewareListener l);
        void Unregister(IMiddlewareListener l);
    }

    public interface IMiddlewareListener {
        void OnMessage(string msg);
    }

    public abstract class MiddlewareBase : MonoBehaviour, IMiddleware {
        public int maxReadBufferSize = -1;
        public int maxSendBufferSize = -1;

        protected ManualResetEvent send_MRSTE = new ManualResetEvent(false);

        private List<IMiddlewareListener> listeners = new List<IMiddlewareListener>();
        protected Queue<string> _sendQueue = new Queue<string>();
        protected Queue<string> _receiveQueue = new Queue<string>();
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
                string data = "";
                lock (_receiveQueueLock) {
                    while (maxReadBufferSize > 0 && _receiveQueue.Count > maxReadBufferSize) _receiveQueue.Dequeue();
                    if (_receiveQueue.Count > 0) data = _receiveQueue.Dequeue();
                    else haveData = false;
                }

                if (data == null || data.Length == 0) return;

                foreach (IMiddlewareListener l in listeners) {
                    l.OnMessage(data);
                }
            }
        }

        public void Send(string data) {
            lock (_sendQueueLock) {
                while (maxSendBufferSize > 0 && _sendQueue.Count > maxSendBufferSize) _sendQueue.Dequeue();
                _sendQueue.Enqueue(data);
                send_MRSTE.Set();
                send_MRSTE.Reset();
            }
        }
    }

}