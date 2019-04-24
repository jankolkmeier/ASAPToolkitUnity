using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ASAPToolkit.Unity.Characters {
    public class CharacterSubtitles : MonoBehaviour {

        public Text text;

        public void ShowSubtitles(string content) {
            if (text == null) return;
            text.gameObject.SetActive(true);
            text.text = content;
        }

        public void HideSubtitles() {
            if (text == null) return;
            text.gameObject.SetActive(false);
        }

    }
}