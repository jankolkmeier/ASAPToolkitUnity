using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASAPToolkit.Unity.Environment {
    public class AsapAudioClipSource : MonoBehaviour {

        public AudioSource audioSource;

        private string source;
        private string clipId;

        public bool[] partsReceived;

        public void Setup(string source, bool onlyActiveAgentClips, string clipId, AudioClip clip, int nParts) {
            this.source = source;
            this.clipId = clipId;
            this.audioSource = gameObject.AddComponent<AudioSource>();
            this.audioSource.minDistance = 0.25f;
            this.audioSource.maxDistance = 10f;
            this.audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            this.audioSource.clip = clip;
            this.partsReceived = new bool[nParts];
            if (source.Length > 0) {
                ASAPToolkitManager atm = FindObjectOfType<ASAPToolkitManager>();
                ASAPAgent agent = atm.GetAgent(source);
                if (agent != null) {
                    ICharacterSkeleton icskel = agent.GetComponent<ICharacterSkeleton>();
                    if (icskel != null) {
                        transform.position = icskel.GetHeadTransform().position;
                    } 
                } else if (onlyActiveAgentClips) {
                    this.audioSource.mute = true;
                }
            } 
        }

        public void Play(double relTime) {
            if (!audioSource.isPlaying) {
                audioSource.time = Mathf.Min((float) relTime, audioSource.clip.length);
                audioSource.Play();
            }
            float delay = (float) (audioSource.time - relTime);
            if (Mathf.Abs(delay) > 0.05f) {
                audioSource.time = Mathf.Min((float)relTime, audioSource.clip.length);
                audioSource.Play();
            }
        }

        public void Stop() {
            audioSource.Stop();
        }
    }
}