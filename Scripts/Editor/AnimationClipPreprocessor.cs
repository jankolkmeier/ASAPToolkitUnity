using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityAsapIntegration.ASAP.Editor {
	public class AnimationClipPreprocessor : AssetPostprocessor {

		void OnPreprocessModel() {
			AnimationClipPreprocessorSettings ais = AnimationClipPreprocessorSettings.FindParentSettings(assetPath);
			if (ais == null) return;
			
			ModelImporter modelImporter = assetImporter as ModelImporter;
			modelImporter.importAnimation = true;
			modelImporter.resampleCurves = true;
			modelImporter.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
			modelImporter.animationRotationError = ais.rotationError;
			modelImporter.animationPositionError = ais.positionError;
			modelImporter.animationScaleError = ais.scaleError;
			modelImporter.animationType = ModelImporterAnimationType.Generic;
			if (ais.defaultAvatar != null) {
				modelImporter.sourceAvatar = ais.defaultAvatar;
			}

			modelImporter.importMaterials = ais.importMaterials;
		}

		void OnPreprocessAnimation() {
		}

		void OnPostprocessModel(GameObject go) {
		}

		AnimationClip CreateCopyWithDefaultEvents (AnimationClip sourceClip, AnimationClipPreprocessorSettings ais) {
			if (sourceClip != null) {
				string path = AssetDatabase.GetAssetPath(sourceClip);
				path = System.IO.Path.Combine(ais.GetOutputPath(), sourceClip.name) + ".anim";
				AnimationClip existingClip = (AnimationClip) AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));
				if (existingClip != null) {
					Debug.LogWarning("Overwriting existing clip, shoud copy existing animation events(?)");
				}
				//string newPath = AssetDatabase.GenerateUniqueAssetPath(path);
				AnimationClip newClip = new AnimationClip();
				List<AnimationEvent> _events = new List<AnimationEvent>();
				foreach (AnimationClipPreprocessorSettings.DefaultAnimationEvent dae in ais.defaultAnimationEvents) {
					AnimationEvent ev = new AnimationEvent();
					ev.functionName = dae.ParseEventName();
					if (ev.functionName.Contains("custom")) {
						ev.stringParameter = dae.event_name;
					}

					ev.time = dae.relative_time*sourceClip.length;
					_events.Add(ev);
				}	
				EditorUtility.CopySerialized(sourceClip, newClip);
				AssetDatabase.CreateAsset(newClip, path);
				AnimationUtility.SetAnimationEvents(newClip, _events.ToArray());
				return newClip;
			}
			return null;
		}

		void OnPostprocessAnimation(GameObject go, AnimationClip clip) {
			Debug.Log("Post anim: "+clip.name+" in "+assetPath);
			AnimationClipPreprocessorSettings ais = AnimationClipPreprocessorSettings.FindParentSettings(assetPath);
			if (ais == null) return;

			//AnimationClip clipCopy =
			CreateCopyWithDefaultEvents(clip, ais);
		}
	}

}