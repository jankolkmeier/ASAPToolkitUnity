using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityAsapIntegration.ASAP.Editor {

	[CreateAssetMenu(fileName="NewAnimationClipPreprocessorSettings", menuName="ASAP/Animation Preprocessor Settings", order=2)]
	public class AnimationClipPreprocessorSettings : ScriptableObject {

		[MenuItem("ASAP/Create Animation Preprocessor Settings")]
		public static void CreateAsset() {
			AnimationClipPreprocessorSettings ais = ScriptableObject.CreateInstance<AnimationClipPreprocessorSettings>();
			AssetDatabase.CreateAsset(ais, "Assets/"+ais.name+".asset");
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = ais;
		}

		[System.Serializable]
		public struct DefaultAnimationEvent {
			public string event_name;
			public float relative_time;

			// TODO fix this
			public string ParseEventName() {
				string[] default_names = new string[] {
					"start", "ready", "strokeStart", "stroke", "strokeEnd", "relax", "end"
				};
				if (System.Array.IndexOf(default_names, event_name) >= 0) {
					return "Sync_"+event_name;
				} else {
					return "Sync_custom";
				}
			}
		}

		public static AnimationClipPreprocessorSettings FindParentSettings(string assetPath) {
			string search = "t:"+typeof(AnimationClipPreprocessorSettings).Name;
			Debug.Log("Type: "+search);
			string[] ais_guids = AssetDatabase.FindAssets(search);
			string assetDir = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(Application.dataPath, assetPath));

			AnimationClipPreprocessorSettings res = null;
			int pathSize = int.MinValue;

			foreach (string ais_guid in ais_guids) {
				string ais_path = AssetDatabase.GUIDToAssetPath(ais_guid);

				string settingAssetDir = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(Application.dataPath, ais_path));
				if (settingAssetDir.Length >= pathSize && assetDir.StartsWith(settingAssetDir)) {
					pathSize = settingAssetDir.Length;
					res = (AnimationClipPreprocessorSettings) AssetDatabase.LoadAssetAtPath(ais_path, typeof(AnimationClipPreprocessorSettings));
				}
			}

			return res;
		}

		public DefaultAnimationEvent[] defaultAnimationEvents;
		public Avatar defaultAvatar;
		public float rotationError = 0.1f;
		public float positionError = 0.1f;
		public float scaleError = 0.5f;
		public bool importMaterials = false;

		public string outputFolder;

		public string GetOutputPath() {
			return outputFolder;//System.IO.Path.Combine(System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)), outputFolder);
		}
	}

}
