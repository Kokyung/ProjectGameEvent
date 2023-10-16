using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BKK.GameEventArchitecture
{
    public class GameEventGlobalOption
    {
        public static string fontSize = "GameEventDescription_FontSize";

        public static string locked = "GameEventDescription_Locked";

        public static string typingAreaHeight = "GameEventDescription_TypingAreaHeight";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (!EditorPrefs.HasKey(fontSize))
            {
                EditorPrefs.SetInt(fontSize, 15);
            }
            if (!EditorPrefs.HasKey(locked))
            {
                EditorPrefs.SetBool(locked, true);
            }
            if (!EditorPrefs.HasKey(typingAreaHeight))
            {
                EditorPrefs.SetInt(typingAreaHeight, 12);
            }
        }
    }
}
