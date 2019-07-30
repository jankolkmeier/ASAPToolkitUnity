using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASAPToolkit.Unity.Characters.UMA  {

	public class ToonFace : MonoBehaviour, ICharacterFace {

		public enum FaceTargetType { Expression, Mouth, Other };
		public ToonFaceConfiguration[] faceTargetConfig;

		[System.Serializable]
		public struct ToonFaceConfiguration {
			public string name;
			public Renderer[] slots;
			public Texture[] textures;
			public ToonFace.FaceTargetType type;
		}

		private bool initialized = false;
        IFaceTarget[] faceTargets;

		void Start () {
			Initialize();
		}
		
		private void Initialize() {
			if (initialized) return;
            List<IFaceTarget> _faceTargets = new List<IFaceTarget>();
			foreach (ToonFaceConfiguration ftc in faceTargetConfig) {
				_faceTargets.Add(new ToonFaceTarget(ftc.name, ftc));
            }
			faceTargets =  _faceTargets.ToArray();
			initialized = true;
        }

		public bool Ready() {
            return initialized;
        }

		public void SetFaceTargetValues(float[] targetValues) {

            // Expression Only
			if (targetValues.Length < 1) return;

			int maxId = 0;
			float maxVal = float.NegativeInfinity;

            for (int f = 0; f < targetValues.Length; f++) {
				if (typeof(ToonFaceTarget) == faceTargets[maxId].GetType()) {
					ToonFaceTarget tft = ((ToonFaceTarget) faceTargets[maxId]);
					tft.SetValue(targetValues[f]);
					tft.UnApply();
				}

				if (maxVal < targetValues[f]) {
					maxVal = targetValues[f];
					maxId = f;
				}
            }

			if (typeof(ToonFaceTarget) == faceTargets[maxId].GetType()) {
				ToonFaceTarget tft = ((ToonFaceTarget) faceTargets[maxId]);
				tft.Apply();
			}

        }

        public IFaceTarget[] GetFaceTargets() {
            if (!initialized) Initialize();
            return faceTargets;
        }

		public class ToonFaceTarget : IFaceTarget {
            public string name;
			public float value;
			public ToonFaceConfiguration faceConfiguration;

            public ToonFaceTarget(string name, ToonFaceConfiguration ftc) {
				this.value = 0.0f;
                this.name = name;
				this.faceConfiguration = ftc;
            }

			public void UnApply() {
				for (int i = 0; i < faceConfiguration.slots.Length; i++) {
					faceConfiguration.slots[i].material.mainTexture = null;
				}
			}

			public void Apply() {
				for (int i = 0; i < faceConfiguration.slots.Length; i++) {
					faceConfiguration.slots[i].material.mainTexture = faceConfiguration.textures [i];
				}
			}

			public void SetValue(float v) {
				value = v;
			}

            public float GetMinValue() {
                return 0f;
            }

            public float GetMaxValue() {
                return 1f;
            }

            public string GetName() {
                return name;
            }
        }


	}

}