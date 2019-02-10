using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace ASAPToolkit.Unity.BML {

    public delegate void BlockProgressCallback(BlockProgress blockProgress);
    public delegate void PredictionFeedbackCallback(PredictionFeedback predictionFeedback);
    public delegate void SyncPointProgressCallback(SyncPointProgress syncPointProgress);
    public delegate void WarningFeedbackCallback(WarningFeedback warningFeedback);

    [RequireComponent(typeof(BMLManager))]
    public class BMLFeedback : MonoBehaviour {
        public event BlockProgressCallback BlockProgressEventHandler;
        public event PredictionFeedbackCallback PredictionFeedbackEventHandler;
        public event SyncPointProgressCallback SyncPointProgressEventHandler;
        public event WarningFeedbackCallback WarningFeedbackEventHandler;

        void Start() { }

        public T ParseFeedbackBlock<T>(string block) {
            T res = default(T);
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StringReader reader = new StringReader(block)) {
                    res = (T)(serializer.Deserialize(reader));
                }
            } catch (System.Exception xmle) {
                Debug.LogWarning("Exception while parsing feedback: " + xmle + "\n\n" + block);
            }
            return res;
        }

        public void HandleFeedback(string feedback) {
            if (feedback.StartsWith("<blockProgress")) {
                BlockProgress blockProgress = ParseFeedbackBlock<BlockProgress>(feedback);
                blockProgress.raw = HtmlDecode(feedback);
                if (BlockProgressEventHandler != null) BlockProgressEventHandler.Invoke(blockProgress);
            } else if (feedback.StartsWith("<predictionFeedback")) {
                PredictionFeedback predictionFeedback = ParseFeedbackBlock<PredictionFeedback>(feedback);
                predictionFeedback.raw = HtmlDecode(feedback);
                if (PredictionFeedbackEventHandler != null) PredictionFeedbackEventHandler.Invoke(predictionFeedback);
            } else if (feedback.StartsWith("<syncPointProgress")) {
                SyncPointProgress syncPointProgress = ParseFeedbackBlock<SyncPointProgress>(feedback);
                syncPointProgress.raw = HtmlDecode(feedback);
                if (SyncPointProgressEventHandler != null) SyncPointProgressEventHandler.Invoke(syncPointProgress);
            } else if (feedback.StartsWith("<warningFeedback")) {
                WarningFeedback warningFeedback = ParseFeedbackBlock<WarningFeedback>(feedback);
                warningFeedback.raw = HtmlDecode(feedback);
                if (WarningFeedbackEventHandler != null) WarningFeedbackEventHandler.Invoke(warningFeedback);
            }
        }

        public string HtmlDecode(string unescaped) {
            return unescaped.Replace("&lt;", "<")
                .Replace("&amp;", "&")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'");
        }

    }

    /* For some of the time properties we use "string" instead of decimal, because the parser
     * seems to have difficulties with parsing numers in exponential notation....
     */

    [XmlRoot("blockProgress", Namespace = "http://www.bml-initiative.org/bml/bml-1.0")]
    public class BlockProgress {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("characterId")]
        public string characterId { get; set; }

        [XmlAttribute("globalTime")]
        public string globalTime { get; set; }

        [XmlAttribute("posixTime", Namespace = "http://www.asap-project.org/bmla")]
        public long posixTime { get; set; }

        [XmlAttribute("status", Namespace = "http://www.asap-project.org/bmla")]
        public string status { get; set; }

        public string raw { get; set; }
    }

    [XmlRoot("syncPointProgress", Namespace = "http://www.bml-initiative.org/bml/bml-1.0")]
    public class SyncPointProgress {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("characterId")]
        public string characterId { get; set; }

        [XmlAttribute("globalTime")]
        public string globalTime { get; set; }

        [XmlAttribute("posixTime", Namespace = "http://www.asap-project.org/bmla")]
        public long posixTime { get; set; }

        [XmlAttribute("time")]
        public string time { get; set; }

        public string raw { get; set; }
    }

    [XmlRoot("warningFeedback", Namespace = "http://www.bml-initiative.org/bml/bml-1.0")]
    public class WarningFeedback {

        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("characterId")]
        public string characterId { get; set; }

        [XmlAttribute("type")]
        public string type { get; set; }

        [XmlText]
        public string Value { get; set; }

        public string raw { get; set; }
    }

    [XmlRoot("predictionFeedback", Namespace = "http://www.bml-initiative.org/bml/bml-1.0")]
    public class PredictionFeedback {
        [XmlAttribute("characterId")]
        public string characterId { get; set; }

        [XmlElement]
        public BmlBlock bml;

        public string raw { get; set; }
    }

    [XmlRoot("bml")]
    public class BmlBlock {
        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlAttribute("status", Namespace = "http://www.asap-project.org/bmla")]
        public string status { get; set; }

        [XmlAttribute("globalStart")]
        public string globalStart { get; set; }

        [XmlAttribute("globalEnd")]
        public string globalEnd { get; set; }

        [XmlAttribute("posixStartTime", Namespace = "http://www.asap-project.org/bmla")]
        public long posixStartTime { get; set; }

        [XmlAttribute("posixEndTime", Namespace = "http://www.asap-project.org/bmla")]
        public long posixEndTime { get; set; }

        [XmlText]
        public string Value { get; set; }

        public string raw { get; set; }
    }


    /*
    [XmlElement("TestElement")]
    public TestElement TestElement { get; set; }

    [XmlText]
    public int Value { get; set; }

    [XmlAttribute]
    public string attr1 { get; set; }
    */
}
