using UnityEditor;
using UnityEngine;
using System.IO;

namespace BKK.GameEventArchitecture.Editor
{
    public class GameEventCreator
    {
        [MenuItem("BKK/Game Event/Game Event Generator", false, 50)]
        private static void OpenCreateWindow()
        {
            GameEventCreatorWindow window = EditorWindow.GetWindow<GameEventCreatorWindow>(false, "게임 이벤트 생성기", true);
            window.titleContent = new GUIContent("Game Event Generator");
        }

        public static void CreateAll(string type, string path, string menuName)
        {
            if (!path.EndsWith("/")) path += "/";

            CreateGameEvent(type, path + "GameEvent", menuName);
            CreateGameEventListener(type, path + "GameEventListener");

            AssetDatabase.Refresh();
        }

        private static void CreateGameEvent(string type, string folderPath,
            string menuName = "BKK/Game Event Architecture", bool refresh = false)
        {
            var splitIndex = type.LastIndexOf('.');
            var nameSpace = "";
            var classType = type.Substring(splitIndex + 1);

            if (splitIndex != -1)
            {
                nameSpace = type.Substring(0, splitIndex);
            }

            var filePath = $"{folderPath}/{classType}GameEvent.cs";
            var existScripts = AssetDatabase.FindAssets($"{classType}GameEvent t:script");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (existScripts.Length == 0)
            {
                using var outfile = new StreamWriter(filePath);
                outfile.WriteLine("// Created by Game Event Generator.");
                outfile.WriteLine("using UnityEngine;");
                
                if (nameSpace != "UnityEngine" && !string.IsNullOrEmpty(nameSpace))
                    outfile.WriteLine($"using {nameSpace};");
                
                outfile.WriteLine("");
                outfile.WriteLine("namespace BKK.GameEventArchitecture");
                outfile.WriteLine("{");
                outfile.WriteLine(
                    $"    [CreateAssetMenu(menuName = \"{menuName}/{classType} Game Event\", fileName = \"New {classType} Game Event\", order = 100)]");
                outfile.WriteLine($"    public class {classType}GameEvent : GameEvent<{classType}>");
                outfile.WriteLine("    {");
                outfile.WriteLine("    }");
                outfile.WriteLine("}");
                outfile.Close();
            }
            else
            {
                var paths = "\n";

                foreach (var script in existScripts)
                {
                    var path = AssetDatabase.GUIDToAssetPath(script);
                    if (path.Contains("Listener")) continue;
                    paths += $"{path}\n";
                }

                Debug.Log($"{classType} Game Event Listener already exists.: {paths}");
                return;
            }

            if (refresh) AssetDatabase.Refresh();

            Debug.Log($"{classType} Game Event class generated.: {folderPath}");
        }

        private static void CreateGameEventListener(string type, string folderPath, bool refresh = false)
        {
            var splitIndex = type.LastIndexOf('.');
            var nameSpace = "";
            var classType = type.Substring(splitIndex + 1);

            if (splitIndex != -1)
            {
                nameSpace = type.Substring(0, splitIndex);
            }
            
            var filePath = $"{folderPath}/{classType}GameEventListener.cs";
            var existScripts = AssetDatabase.FindAssets($"{classType}GameEventListener t:script");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (existScripts.Length == 0)
            {
                using var outfile = new StreamWriter(filePath);
                outfile.WriteLine("// Created by Game Event Generator.");
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using UnityEngine.Events;");

                if (nameSpace != "UnityEngine" && !string.IsNullOrEmpty(nameSpace))
                    outfile.WriteLine($"using {nameSpace};");
                
                outfile.WriteLine("");
                outfile.WriteLine("namespace BKK.GameEventArchitecture");
                outfile.WriteLine("{");
                outfile.WriteLine(
                    $"    public class {classType}GameEventListener : GameEventListener<{classType}, {classType}GameEvent, UnityEvent<{classType}>>");
                outfile.WriteLine("    {");
                outfile.WriteLine("");
                outfile.WriteLine("    }");
                outfile.WriteLine("}");
            }
            else
            {
                var paths = "\n";

                foreach (var script in existScripts)
                {
                    paths += $"{AssetDatabase.GUIDToAssetPath(script)}\n";
                }

                Debug.Log($"{classType} Game Event class already exists.: {paths}");
                return;
            }

            if (refresh) AssetDatabase.Refresh();

            Debug.Log($"{classType} Game Event Listener class generated.: {folderPath}");
        }
    }
}
