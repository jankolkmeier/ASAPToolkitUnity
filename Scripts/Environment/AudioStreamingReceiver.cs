using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASAPToolkit.Unity.Middleware;
using System.IO;
using System;

namespace ASAPToolkit.Unity.Environment {

    [RequireComponent(typeof(IMiddleware))]
    public class AudioStreamingReceiver : MonoBehaviour, IMiddlewareListener {


        Dictionary<string, AsapAudioClipSource> clipLib;

        private MiddlewareBase middleware;

        public void OnMessage(MSG msg) {
            StreamingClipJSON audioStreamMsg;
            try {
                audioStreamMsg = JsonUtility.FromJson<StreamingClipJSON>(msg.data);
            } catch (System.Exception e) {
                Debug.LogWarning("Failed to parse incomming StreamingClipJSON to JSON: " + msg.data + "\n\n" + e);
                return;
            }

            if (audioStreamMsg.msgType == "DATA") {
                StreamingClipDataJSON audioStreamDataMsg;
                try {
                    audioStreamDataMsg = JsonUtility.FromJson<StreamingClipDataJSON>(msg.data);
                } catch (System.Exception e) {
                    Debug.LogWarning("Failed to parse incomming StreamingClipDataJSON to JSON: " + msg.data + "\n\n" + e);
                    return;
                }

                HandleClipData(audioStreamDataMsg);
            }

            if (audioStreamMsg.msgType == "CTRL") {

                StreamingClipCtrlJSON audioStreamControlMsg;
                try {
                    audioStreamControlMsg = JsonUtility.FromJson<StreamingClipCtrlJSON>(msg.data);
                } catch (System.Exception e) {
                    Debug.LogWarning("Failed to parse incomming StreamingClipCtrlJSON to JSON: " + msg.data + "\n\n" + e);
                    return;
                }

                HandleClipControl(audioStreamControlMsg);

                
            }
        }

        private void HandleClipControl(StreamingClipCtrlJSON msg) {

            if (!clipLib.ContainsKey(msg.clipId)) {
                Debug.LogWarning("ASAP wants to "+msg.cmd+" clip " + msg.clipId + " which we haven't received (yet)");
                return;
            }


            if (msg.cmd == "play") {
                clipLib[msg.clipId].Play(msg.floatParam);
            } else if (msg.cmd == "stop") {
                clipLib[msg.clipId].Stop();
                Destroy(clipLib[msg.clipId].audioSource.gameObject);
                clipLib.Remove(msg.clipId);
            } else {
                Debug.LogWarning("Command unknown: " + msg.cmd);
            }
        }


        void HandleClipData(StreamingClipDataJSON msg) {
            if (!clipLib.ContainsKey(msg.clipId)) {
                int lengthSamples = (int) msg.frameLength;
                int channels = msg.audioFormat.channels;
                float frequency = msg.audioFormat.sampleRate;
                AsapAudioClipSource asco = (new GameObject(msg.clipId)).AddComponent<AsapAudioClipSource>();
                asco.transform.SetParent(transform);
                asco.Setup(msg.source, msg.clipId, AudioClip.Create(msg.clipId, lengthSamples, channels, (int)frequency, false));
                clipLib.Add(msg.clipId, asco);
            }

            byte[] audioBuffer = System.Convert.FromBase64String(msg.data);
            float[] samples = new float[audioBuffer.Length/2];
            using (MemoryStream stream = new MemoryStream(audioBuffer)) {
                using (BinaryReader reader = new BinaryReader(stream)) {
                    for (int i = 0; i < samples.Length; i++) {
                        samples[i] = reader.ReadInt16() / 32767f;
                    }
                }
            }
            clipLib[msg.clipId].audioSource.clip.SetData(samples, msg.partOffsets[msg.thisPartIdx] / 2);
        }

        void InstantiateAudioSource(AudioClip c, string id) {

        }

        void Awake() {
            clipLib = new Dictionary<string, AsapAudioClipSource>();
            middleware = GetComponent<MiddlewareBase>();
            if (middleware != null) middleware.Register(this);
        }

        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }

    [System.Serializable]
    class StreamingClipJSON {
        public string msgType;
        public string source;
        public string clipId;
    }

    [System.Serializable]
    class StreamingClipDataJSON : StreamingClipJSON {
        public long frameLength;
        public StreamingClipDataFormatJSON audioFormat;
        public long totalSize;
        public int thisPartIdx;
        public int[] partOffsets;
        public string data;

    }

    [System.Serializable]
    class StreamingClipDataFormatJSON {
        public string encoding;
        public int channels;
        public int frameSize;
        public float sampleRate;
    }


    [System.Serializable]
    class StreamingClipCtrlJSON : StreamingClipJSON {
        public string cmd;
        public double floatParam;
    }

}