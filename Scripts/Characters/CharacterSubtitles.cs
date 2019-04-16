using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ASAPToolkit.Unity.Characters {
    public class CharacterSubtitles : MonoBehaviour {

        public Text text;

        public void ShowSubtitles(string content) {
            text.gameObject.SetActive(true);
            text.text = content;
        }

        public void HideSubtitles() {
            text.gameObject.SetActive(false);
        }

    }
}