/*
   Copyright 2020 Jan Kolkmeier <jankolkmeier@gmail.com>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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