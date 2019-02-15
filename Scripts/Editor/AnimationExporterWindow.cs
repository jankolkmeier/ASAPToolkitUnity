#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Xml;
using System.IO;

using ASAPToolkit.Unity.Retargeting;

namespace ASAPToolkit.Unity.Editor {

    public class AnimationExporterWindow : EditorWindow {

        public enum ExportMode { ProcAnimationGesture, Keyframes, GestureBindingRestPose, GestureBindingKeyFrames, GestureBindingProcAnim };

        private SkeletonConfiguration currentRig;
        private AvatarMask currentAvatarMask;

        private int selectedRig;
        private int selectedClip;
        private int selectedMask;

        private string restposeName = "";

        private string asapHumanRootPath = "";
        private string gestureFilesPath = "";
        private string restposeFilesPath = "";
        private string keyframeFilesPath = "";

        bool ignoreRootTranslation = true;
        bool batchKeyframe = false;
        bool batchProc = false;

        private string fileSuffix = "";

        private enum MASK_MODE {
            ALL = 0,
            ALL_ANIMATED = 1,
            MASK_MAPPED_ALL = 2,
            MASK_MAPPED_ANIMATED = 3
        }

        private MASK_MODE maskMode;

        void WriteXMLFile(string path, string name, string xml) {
            string filePath = System.IO.Path.Combine(path, name + fileSuffix + ".xml");
            Debug.Log("Wrote file: " + filePath);
            System.IO.File.WriteAllText(filePath, xml);
        }

        void OnGUI() {
            GUIStyle popStyle = new GUIStyle(EditorStyles.popup);
            popStyle.margin = new RectOffset(5, 5, 5, 5);

            GUILayout.Label("Output Paths", EditorStyles.boldLabel);

            if (GUILayout.Button("Set Human Root Path")) {
                asapHumanRootPath = EditorUtility.OpenFolderPanel("ASAP Humans Root Folder", asapHumanRootPath, "");
                //sampleBMLPath = System.IO.Path.Combine(asapHumanRootPath, "");
                gestureFilesPath = System.IO.Path.Combine(asapHumanRootPath, "procanimation");
                restposeFilesPath = System.IO.Path.Combine(asapHumanRootPath, "restposes");
                keyframeFilesPath = System.IO.Path.Combine(asapHumanRootPath, "keyframe");
            }
            GUILayout.Label(ShortPath(asapHumanRootPath));

            fileSuffix = EditorGUILayout.TextField("File Suffix: ", fileSuffix);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Pose/Animiation Source", EditorStyles.boldLabel);
            SkeletonConfiguration[] rigs = FindObjectsOfType<SkeletonConfiguration>();
            string[] rigNames = rigs.Select(rig => rig.transform.name).ToArray();
            if (rigNames.Length > 0) {
                if (selectedRig >= rigNames.Length) selectedRig = rigNames.Length - 1;
                if (selectedRig < 0) selectedRig = 0;
                int _selectedRig = EditorGUILayout.Popup(selectedRig, rigNames, popStyle);
                if (_selectedRig != selectedRig) {
                    selectedRig = _selectedRig;
                    selectedClip = 0;
                }

                currentRig = rigs[selectedRig];
            } else {
                currentRig = null;
            }

            if (currentRig == null) return;
            Animator animator = currentRig.GetComponent<Animator>();
            if (animator == null) return;
            //UnityEditor.Selection.objects = new UnityEngine.Object[] { currentRig };

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Pose/Clip", EditorStyles.boldLabel);

            string assetPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            UnityEditor.Animations.AnimatorController controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(assetPath);
            if (controller == null) {
                return;
            }
            AnimationClip[] _c = controller.animationClips;
            List<string> clipNames = _c.Select(clip => clip.name).ToList();
            clipNames.Insert(0, "Current Pose");

            if (selectedClip >= clipNames.Count) selectedClip = clipNames.Count - 1;
            if (selectedClip < 0) selectedClip = 0;
            int _selectedClip = EditorGUILayout.Popup(selectedClip, clipNames.ToArray(), popStyle);

            selectedClip = _selectedClip;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            if (selectedClip > 0) GUILayout.Label("Length: " + _c[_selectedClip - 1].length);

            GUILayout.Label("Avatar Mask", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            currentAvatarMask = (AvatarMask)EditorGUILayout.ObjectField(currentAvatarMask, typeof(AvatarMask), true);
            maskMode = (MASK_MODE)EditorGUILayout.EnumPopup("Mask Mode: ", maskMode);
            ignoreRootTranslation = EditorGUILayout.Toggle("Ignore Root Translation", ignoreRootTranslation);
            EditorGUILayout.EndVertical();

            AnimationClip ac = null;
            if (selectedClip > 0) ac = _c[selectedClip - 1];

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Export:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.BeginVertical();
            batchKeyframe = EditorGUILayout.Toggle("All Keyframe", batchKeyframe);
            batchProc = EditorGUILayout.Toggle("All ProcAnim", batchProc);

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            bool _batch = GUILayout.Button("Batch Export");
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (_batch) {
                foreach (AnimationClip _ac in _c) {
                    HandleExport(_ac, true);
                }
            } else {
                HandleExport(ac, false);
            }
        }

        void HandleExport(AnimationClip clip, bool batch) {

            CanonicalRepresentation.HAnimBones[] boneUnion = null;
            //List<CanonicalRepresentation.HAnimBones> boneUnion = new List<CanonicalRepresentation.HAnimBones>();

            //List<CanonicalRepresentation.HAnimBones> skeletonUnion = new List<CanonicalRepresentation.HAnimBones>();
            //List<CanonicalRepresentation.HAnimBones> animatedUnion = new List<CanonicalRepresentation.HAnimBones>();

            if (maskMode == MASK_MODE.ALL) {
                boneUnion = currentRig.ExportPose().frames.First().canonicalBoneNames;
            } else if (maskMode == MASK_MODE.ALL_ANIMATED && selectedClip > 0) {
                boneUnion = GetAnimatedCanonicalBones(currentRig, clip);
            } else if ((maskMode == MASK_MODE.MASK_MAPPED_ALL || maskMode == MASK_MODE.MASK_MAPPED_ANIMATED) && currentAvatarMask != null) {
                List<CanonicalRepresentation.HAnimBones> _boneUnion = new List<CanonicalRepresentation.HAnimBones>();
                CanonicalRepresentation.HAnimBones[] skeletonBones = currentRig.ExportPose().frames.First().canonicalBoneNames;
                CanonicalRepresentation.HAnimBones[] animatedBones = new CanonicalRepresentation.HAnimBones[0];
                if (selectedClip > 0)
                    animatedBones = GetAnimatedCanonicalBones(currentRig, clip);
                for (int i = 0; i < currentAvatarMask.transformCount; i++) {
                    if (!currentAvatarMask.GetTransformActive(i)) continue;
                    string boneName = currentAvatarMask.GetTransformPath(i).Split(new char[] { '/' }).Last();
                    if (CanonicalRepresentation.HAnimBoneNames.Contains(boneName)) {
                        CanonicalRepresentation.HAnimBones canonicalBone = (CanonicalRepresentation.HAnimBones)System.Enum.Parse(typeof(CanonicalRepresentation.HAnimBones), boneName, false);
                        if (maskMode == MASK_MODE.MASK_MAPPED_ALL && skeletonBones.Contains(canonicalBone)) {
                            _boneUnion.Add(canonicalBone);
                        }

                        if (maskMode == MASK_MODE.MASK_MAPPED_ANIMATED && animatedBones.Contains(canonicalBone)) {
                            _boneUnion.Add(canonicalBone);
                        }
                    }
                }
                /*
                if (skeletonUnion.Count > 0)
                    GUILayout.Label("Mask Union w/ skeleton: "+skeletonUnion.Count);//+" / "+skeletonUnion.Aggregate((i,j) => i+" "+j));
                if (animatedUnion.Count > 0)
                    GUILayout.Label("Mask Union w/ animated: "+animatedUnion.Count);//+" / "+animatedUnion.Aggregate((i,j) => i+" "+j));
                 */
                boneUnion = _boneUnion.ToArray();
            }



            if (!batch) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                if (boneUnion != null && boneUnion.Length > 0) {
                    string descr = "";
                    switch (maskMode) {
                        case MASK_MODE.ALL:
                            descr = "Writing animation on all skeleton bones";
                            break;
                        case MASK_MODE.ALL_ANIMATED:
                            descr = "Writing animation only for bones with animation data";
                            break;
                        case MASK_MODE.MASK_MAPPED_ALL:
                            descr = "Writing animation on all skeleton bones that are enabled in Mask";
                            break;
                        case MASK_MODE.MASK_MAPPED_ANIMATED:
                            descr = "Writing animation on all skeleton bones that are enabled in Mask and have animation data";
                            break;
                    }
                    GUILayout.Label(descr + " (" + boneUnion.Length + ").");
                } else {
                    GUILayout.Label("Error with mask/bone mapping setup");
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }

            if (!batch) EditorGUILayout.BeginHorizontal();
            if (!batch) EditorGUILayout.BeginVertical();
            if (selectedClip == 0) { // POSE
                restposeName = EditorGUILayout.TextField("Restpose name: ", restposeName);
                if (!batch && GUILayout.Button("Pose as RestPose")) {
                    SkeletonConfiguration.ASAPClip c = currentRig.ExportPose();
                    string xml = WriteXML(c, ExportMode.GestureBindingRestPose, boneUnion, null);
                    if (restposeFilesPath.Length > 0 && restposeName.Length > 0) {
                        WriteXMLFile(restposeFilesPath, restposeName, xml);
                    }
                    // TODO: 
                    //  - preview name
                    //  - write file (set folder up top)
                    //  - preview gesture binding entry (allow some settings?)
                }

            } else { // CLIP
                if (!batch && GUILayout.Button("Live Test (low fps)")) {
                    SkeletonConfiguration.ASAPClip c = currentRig.ExportClip(clip, 5);
                    SkeletonConfiguration.SyncPoint[] syncPoints = GetSyncPoints(clip);
                    string xml = WriteXML(c, ExportMode.ProcAnimationGesture, boneUnion, syncPoints);
                    Debug.Log(xml);

                    if (Application.isPlaying) {
                        Transform.FindObjectOfType<BML.BMLRequests>().SendBML(xml);
                    }

                }

                if ((batch && batchProc) || GUILayout.Button("Export Proc. Anim (Binding)")) {
                    SkeletonConfiguration.ASAPClip c = currentRig.ExportClip(clip, 30);
                    SkeletonConfiguration.SyncPoint[] syncPoints = GetSyncPoints(clip);
                    string xml = WriteXML(c, ExportMode.GestureBindingProcAnim, boneUnion, syncPoints);
                    if (gestureFilesPath.Length > 0) {
                        WriteXMLFile(gestureFilesPath, clip.name, xml);
                    }
                }

                if ((batch && batchKeyframe) || GUILayout.Button("Export Keyframes (Binding)")) {
                    SkeletonConfiguration.ASAPClip c = currentRig.ExportClip(clip, 30);
                    SkeletonConfiguration.SyncPoint[] syncPoints = GetSyncPoints(clip);
                    string xml = WriteXML(c, ExportMode.GestureBindingKeyFrames, boneUnion, syncPoints);
                    if (keyframeFilesPath.Length > 0) {
                        WriteXMLFile(keyframeFilesPath, clip.name, xml);
                    }
                }
            }

            if (!batch) EditorGUILayout.EndVertical();
            if (!batch) EditorGUILayout.EndHorizontal();

        }

        private string ShortPath(string path) {
            string prefix = "Path: ";
            if (path.Length <= 30) return prefix + path;
            return prefix + ".." + path.Substring(path.Length - 32, 32);
        }

        private string WriteXML(SkeletonConfiguration.ASAPClip asapClip, ExportMode mode, CanonicalRepresentation.HAnimBones[] boneSet, SkeletonConfiguration.SyncPoint[] syncPoints) {
            string characterId = "COUCH_M_2";

            string encoding = "R";
            if (!ignoreRootTranslation && boneSet.Contains(CanonicalRepresentation.HAnimBones.HumanoidRoot)) encoding = "T1R";
            // TODO: should probably check if there is actual translation data in clip?
            string rotationEncoding = "quaternions";
            string parts = "";
            foreach (CanonicalRepresentation.HAnimBones canonicalBone in System.Enum.GetValues(typeof(CanonicalRepresentation.HAnimBones))) {
                if (!boneSet.Contains(canonicalBone)) continue;
                if (parts.Length > 0) parts += " ";
                parts += canonicalBone.ToString();
            }

            MemoryStream ms = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.OmitXmlDeclaration = true;

            using (XmlWriter w = XmlWriter.Create(ms, settings)) {
                w.WriteStartDocument();

                if (mode == ExportMode.Keyframes || mode == ExportMode.ProcAnimationGesture) {
                    w.WriteStartElement("bml", "http://www.bml-initiative.org/bml/bml-1.0"); // <bml ...>
                    w.WriteAttributeString("id", "exportedBml1");
                    w.WriteAttributeString("characterId", characterId);
                    w.WriteAttributeString("xmlns", "bmlt", null, "http://hmi.ewi.utwente.nl/bmlt");
                }

                if (mode == ExportMode.Keyframes) {
                    //w.WriteStartElement("bmlt:keyframe"); // <bmlt:keyframe ...>
                    w.WriteStartElement("bmlt", "keyframe", null); // <bmlt:keyframe ...>
                    w.WriteAttributeString("id", "keyframes1");

                    w.WriteStartElement("SkeletonInterpolator"); // <SkeletonInterpolator ...>
                    w.WriteAttributeString("encoding", encoding); // ... xmlns=""
                    w.WriteAttributeString("rotationEncoding", rotationEncoding);
                    w.WriteAttributeString("parts", parts);

                    WriteFrames(w, asapClip.frames, boneSet, encoding == "T1R", true, false);

                    w.WriteEndElement(); // </SkeletonInterpolator>
                    w.WriteEndElement(); // </bmlt:keyframe>
                } else if (mode == ExportMode.ProcAnimationGesture) {
                    // w.WriteStartElement("bmlt:procanimationgesture"); // <bmlt:procanimationgesture ...>
                    w.WriteStartElement("bmlt", "procanimationgesture", null); // <bmlt:procanimationgesture ...>
                    w.WriteAttributeString("id", "procgesture1");

                    w.WriteStartElement("ProcAnimation");  // < ProcAnimation ...>
                    float clipDuration = asapClip.frames.Max(frame => frame.timestamp);

                    SkeletonConfiguration.SyncPoint start = syncPoints.FirstOrDefault(s => s.name == "start");
                    SkeletonConfiguration.SyncPoint ready = syncPoints.FirstOrDefault(s => s.name == "ready");
                    SkeletonConfiguration.SyncPoint strokeStart = syncPoints.FirstOrDefault(s => s.name == "strokeStart");
                    SkeletonConfiguration.SyncPoint stroke = syncPoints.FirstOrDefault(s => s.name == "stroke");
                    SkeletonConfiguration.SyncPoint strokeEnd = syncPoints.FirstOrDefault(s => s.name == "strokeEnd");
                    SkeletonConfiguration.SyncPoint relax = syncPoints.FirstOrDefault(s => s.name == "relax");
                    SkeletonConfiguration.SyncPoint end = syncPoints.FirstOrDefault(s => s.name == "end");

                    if (System.Array.IndexOf(null, new SkeletonConfiguration.SyncPoint[] { start, ready, strokeStart, strokeEnd, relax, end }) >= 0) {
                        Debug.LogWarning("Please define all syncpoints/animation events.");
                        return null;
                    }

                    float frameMin = asapClip.frames.Select(f => f.timestamp).Last(t => t <= start.relativeTime * clipDuration);
                    float frameMax = asapClip.frames.Select(f => f.timestamp).First(t => t >= end.relativeTime * clipDuration);
                    float gestureDuration = (1 - (strokeStart.relativeTime + (1 - strokeEnd.relativeTime))) * clipDuration;
                    List<SkeletonConfiguration.SyncPoint> _syncPoints = new List<SkeletonConfiguration.SyncPoint>();

                    _syncPoints.Add(new SkeletonConfiguration.SyncPoint("start", Remap(start.relativeTime, start.relativeTime, end.relativeTime, 0f, 1f)));
                    _syncPoints.Add(new SkeletonConfiguration.SyncPoint("ready", Remap(ready.relativeTime, start.relativeTime, end.relativeTime, 0f, 1f)));
                    _syncPoints.Add(new SkeletonConfiguration.SyncPoint("strokeStart", Remap(strokeStart.relativeTime, start.relativeTime, end.relativeTime, 0f, 1f)));
                    _syncPoints.Add(new SkeletonConfiguration.SyncPoint("stroke", Remap(stroke.relativeTime, start.relativeTime, end.relativeTime, 0f, 1f)));
                    _syncPoints.Add(new SkeletonConfiguration.SyncPoint("strokeEnd", Remap(strokeEnd.relativeTime, start.relativeTime, end.relativeTime, 0f, 1f)));
                    _syncPoints.Add(new SkeletonConfiguration.SyncPoint("relax", Remap(relax.relativeTime, start.relativeTime, end.relativeTime, 0f, 1f)));
                    _syncPoints.Add(new SkeletonConfiguration.SyncPoint("end", Remap(end.relativeTime, start.relativeTime, end.relativeTime, 0f, 1f)));
                    syncPoints = _syncPoints.ToArray();

                    w.WriteAttributeString("prefDuration", gestureDuration.ToString("0.0##"));
                    w.WriteAttributeString("minDuration", (gestureDuration/2).ToString("0.0##"));
                    w.WriteAttributeString("maxDuration", (gestureDuration*2).ToString("0.0##"));
                    w.WriteStartElement("SkeletonInterpolator"); // <SkeletonInterpolator ...>
                    w.WriteAttributeString("encoding", encoding); // ...
                    w.WriteAttributeString("rotationEncoding", rotationEncoding);
                    w.WriteAttributeString("parts", parts);

                    WriteFrames(w, asapClip.frames, boneSet, encoding == "T1R", true, true, frameMin, frameMax);

                    w.WriteEndElement(); // </SkeletonInterpolator>
                    
                    WriteSyncPoints(w, syncPoints);

                    w.WriteEndElement(); // </ProcAnimation>
                    w.WriteEndElement(); // </bmlt:procanimationgesture>
                } else if (mode == ExportMode.GestureBindingRestPose) {
                    w.WriteStartElement("SkeletonPose"); // <SkeletonPose ...>
                    w.WriteAttributeString("encoding", encoding); // ... encoding=""
                    w.WriteAttributeString("rotationEncoding", rotationEncoding);
                    w.WriteAttributeString("parts", parts);

                    WriteFrames(w, asapClip.frames, boneSet, encoding == "T1R", false);

                    w.WriteEndElement(); // </SkeletonPose>
                } else if (mode == ExportMode.GestureBindingKeyFrames) {
                    w.WriteStartElement("SkeletonInterpolator"); // <SkeletonInterpolator ...>
                    w.WriteAttributeString("encoding", encoding); // ... xmlns=""
                    w.WriteAttributeString("rotationEncoding", rotationEncoding);
                    w.WriteAttributeString("parts", parts);
                    WriteFrames(w, asapClip.frames, boneSet, encoding == "T1R", true, false);
                    w.WriteEndElement(); // </SkeletonInterpolator>
                } else if (mode == ExportMode.GestureBindingProcAnim) {
                    w.WriteStartElement("ProcAnimation");  // < ProcAnimation ...>
                    float gestureDuration = asapClip.frames.Max(frame => frame.timestamp);
                    SkeletonConfiguration.SyncPoint strokeStart = syncPoints.FirstOrDefault(s => s.name == "strokeStart");
                    SkeletonConfiguration.SyncPoint strokeEnd = syncPoints.FirstOrDefault(s => s.name == "strokeEnd");
                    if (strokeStart != null && strokeEnd != null && strokeEnd.relativeTime > strokeStart.relativeTime) {
                        gestureDuration = (1 - (strokeStart.relativeTime + (1 - strokeEnd.relativeTime))) * gestureDuration;
                    }
                    w.WriteAttributeString("prefDuration", gestureDuration.ToString("0.0##"));
                    w.WriteAttributeString("minDuration", (gestureDuration/4).ToString("0.0##"));
                    w.WriteAttributeString("maxDuration", (gestureDuration*4).ToString("0.0##"));
                    w.WriteStartElement("SkeletonInterpolator"); // <SkeletonInterpolator ...>
                    w.WriteAttributeString("encoding", encoding); // ... 
                    w.WriteAttributeString("rotationEncoding", rotationEncoding);
                    w.WriteAttributeString("parts", parts);

                    WriteFrames(w, asapClip.frames, boneSet, encoding == "T1R", true);

                    w.WriteEndElement(); // </SkeletonInterpolator>

                    WriteSyncPoints(w, syncPoints);

                    w.WriteEndElement(); // </ProcAnimation>
                }

                if (mode == ExportMode.Keyframes || mode == ExportMode.ProcAnimationGesture) {
                    w.WriteEndElement(); // </bml>
                }
                w.WriteEndDocument();
            }

            StreamReader sr = new StreamReader(ms);
            ms.Seek(0, SeekOrigin.Begin);
            string xml = sr.ReadToEnd();

            sr.Dispose();
            return xml;
        }

        public CanonicalRepresentation.HAnimBones[] GetAnimatedCanonicalBones(SkeletonConfiguration p, AnimationClip clip) {
            List<CanonicalRepresentation.HAnimBones> res = new List<CanonicalRepresentation.HAnimBones>();
            foreach (UnityEditor.EditorCurveBinding binding in UnityEditor.AnimationUtility.GetCurveBindings(clip)) {
                //AnimationCurve curve = AnimationUtility.GetEditorCurve (clip, binding);
                string[] pathElems = binding.path.Split('/');
                string boneName = pathElems[pathElems.Length - 1];
                CanonicalRepresentation.HAnimBones hAnimName = p.BoneNameMapped(boneName);
                if ((binding.propertyName.StartsWith("m_LocalRotation") || binding.propertyName.StartsWith("m_LocalPosition")) &&
                    hAnimName != CanonicalRepresentation.HAnimBones.NONE &&
                    !res.Contains(hAnimName)) {
                    res.Add(hAnimName);
                }

                //if (binding.propertyName.StartsWith("m_LocalPosition")) {
                // Translation bones...
                //}
            }
            return res.ToArray();
        }

        public SkeletonConfiguration.SyncPoint[] GetSyncPoints(AnimationClip clip) {
            List<SkeletonConfiguration.SyncPoint> res = new List<SkeletonConfiguration.SyncPoint>();
            foreach (AnimationEvent ae in UnityEditor.AnimationUtility.GetAnimationEvents(clip)) {
                if (ae.functionName.StartsWith("Sync_")) {
                    string syncName = ae.functionName.Substring(5);
                    if (syncName == "custom") syncName = ae.stringParameter;
                    res.Add(new SkeletonConfiguration.SyncPoint(syncName, ae.time / clip.length));
                }
            }
            return res.ToArray();
        }

        private void WriteSyncPoints(XmlWriter w, SkeletonConfiguration.SyncPoint[] syncPoints) {
            foreach (SkeletonConfiguration.SyncPoint syncPoint in syncPoints.OrderBy(syncPoint => syncPoint.relativeTime)) {
                w.WriteStartElement("KeyPosition");
                w.WriteAttributeString("id", syncPoint.name);
                w.WriteAttributeString("weight", "1");
                w.WriteAttributeString("time", syncPoint.relativeTime.ToString("0.0##"));
                w.WriteEndElement();
            }
        }

        private void WriteFrames(XmlWriter w, CanonicalSkeletonPose[] frames, CanonicalRepresentation.HAnimBones[] boneSet) {
            WriteFrames(w, frames, boneSet, true, true, true);
        }

        private void WriteFrames(XmlWriter w, CanonicalSkeletonPose[] frames, CanonicalRepresentation.HAnimBones[] boneSet, bool writeTranslation) {
            WriteFrames(w, frames, boneSet, writeTranslation, true, true);
        }

        private void WriteFrames(XmlWriter w, CanonicalSkeletonPose[] frames, CanonicalRepresentation.HAnimBones[] boneSet, bool writeTranslation, bool writeTimestamp) {
            WriteFrames(w, frames, boneSet, writeTranslation, writeTimestamp, true);
        }
        private void WriteFrames(XmlWriter w, CanonicalSkeletonPose[] frames, CanonicalRepresentation.HAnimBones[] boneSet, bool writeTranslation, bool writeTimestamp, bool normalizeTime) {
            float frameMax = frames.Max(frame => frame.timestamp);
            float frameMin = frames.Min(frame => frame.timestamp);
            WriteFrames(w, frames, boneSet, writeTranslation, writeTimestamp, normalizeTime, frameMin, frameMax);
        }


        private float Remap(float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        private void WriteFrames(XmlWriter w, CanonicalSkeletonPose[] frames, CanonicalRepresentation.HAnimBones[] boneSet, bool writeTranslation, bool writeTimestamp, bool normalizeTime, float frameMin, float frameMax) {
            float duration = frameMax-frameMin;
            w.WriteString("\n");
            int framesWritten = 0;
            int totalFrames = frames.Count(f => f.timestamp >= frameMin && f.timestamp <= frameMax);
            foreach (CanonicalSkeletonPose frame in frames) {
                if (frame.timestamp < frameMin || frame.timestamp > frameMax) continue;
                w.WriteString("      ");

                if (writeTimestamp && normalizeTime) {
                    float time = (frame.timestamp - frameMin)/duration;
                    if (framesWritten == 0) time = 0f;
                    if (framesWritten == totalFrames-1) time = 1f;
                    w.WriteString(time.ToString("0.0##"));
                }

                if (writeTimestamp && !normalizeTime) {
                    float time = frame.timestamp - frameMin;
                    w.WriteString(time.ToString("0.0##"));
                }
                foreach (CanonicalRepresentation.HAnimBones canonicalBone in System.Enum.GetValues(typeof(CanonicalRepresentation.HAnimBones))) {
                    if (canonicalBone == CanonicalRepresentation.HAnimBones.NONE || !boneSet.Contains(canonicalBone)) continue;
                    w.WriteString(" ");
                    if (writeTranslation && canonicalBone == CanonicalRepresentation.HAnimBones.HumanoidRoot) {
                        // TODO: check which one is actually root translation.
                        int rootIdx = System.Array.IndexOf(frame.canonicalBoneNames, CanonicalRepresentation.HAnimBones.HumanoidRoot);
                        WriteVector(w, frame.boneTranslations[rootIdx]);
                        w.WriteString(" ");
                    }
                    WriteQuaternion(w, frame.bonePoses[System.Array.IndexOf(frame.canonicalBoneNames, canonicalBone)]);
                }

                framesWritten++;
                w.WriteString("\n");
            }
            w.WriteString("\n");
        }

        private void WriteVector(XmlWriter w, Vector3 v) {
            w.WriteString(string.Join(" ", ExtractAsapTranslation(v).Select(f => f.ToString("0.0##")).ToArray()));
        }

        private void WriteQuaternion(XmlWriter w, Quaternion q) {
            w.WriteString(string.Join(" ", ExtractAsapQuaternionRotation(q).Select(f => f.ToString("0.0##")).ToArray()));
        }

        private static float[] ExtractAsapQuaternionRotation(Transform t) {
            return ExtractAsapQuaternionRotation(t.localRotation);
        }

        private static float[] ExtractAsapQuaternionRotation(Quaternion q) {
            return new[] {
                    -q.w,
                    -q.x,
                     q.y,
                     q.z
                };
        }

        private static float[] ExtractAsapTranslation(Transform t) {
            return ExtractAsapTranslation(t.position);
        }

        private static float[] ExtractAsapTranslation(Vector3 v) {
            return new[] {
                    -v.x,
                     v.y,
                     v.z
                };
        }
    }

}
#endif