using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ASAPToolkit.Unity.Characters {
    public class CharacterSubtitles : MonoBehaviour {

        public Text text;

        public void ShowSubtitles(string content, int pos) {
            if (text == null) return;
            text.gameObject.SetActive(true);
            text.text = "<color=#22ff55>" + content.Substring(0, pos).TrimStart()+"</color>"+ content.Substring(pos).TrimEnd();
        }

        public void HideSubtitles() {
            if (text == null) return;
            text.gameObject.SetActive(false);
        }

    }
}