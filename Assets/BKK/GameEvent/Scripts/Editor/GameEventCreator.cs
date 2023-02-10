using UnityEditor;
using UnityEngine;
using System.IO;

namespace BKK.GameEventArchitecture.Editor
{
    public class GameEventCreator
    {
        [MenuItem("BKK/게임 이벤트/게임 이벤트 생성기", false, 50)]
        private static void OpenCreateWindow()
        {
            GameEventCreatorWindow window = EditorWindow.GetWindow<GameEventCreatorWindow>(false, "게임 이벤트 생성기", true);
            window.titleContent = new GUIContent("게임 이벤트 생성기");
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
            var nameSpace = type.Substring(0, splitIndex);
            var classType = type.Substring(splitIndex + 1);
            
            var filePath = $"{folderPath}/{classType}GameEvent.cs";
            var existScripts = AssetDatabase.FindAssets($"{classType}GameEvent t:script");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (existScripts.Length == 0)
            {
                using var outfile = new StreamWriter(filePath);
                outfile.WriteLine("// 게임 이벤트 생성 메뉴에 의해 생성되었습니다.");
                outfile.WriteLine("using UnityEngine;");
                if(nameSpace != "UnityEngine") outfile.WriteLine($"using {nameSpace};");
                outfile.WriteLine("");
                outfile.WriteLine("namespace BKK.GameEventArchitecture");
                outfile.WriteLine("{");
                outfile.WriteLine(
                    $"    [CreateAssetMenu(menuName = \"{menuName}/{classType} Game Event\", fileName = \"New {classType} Game Event\", order = 100)]");
                outfile.WriteLine($"    public class {classType}GameEvent : GameEvent<{classType}>");
                outfile.WriteLine("    {");
                outfile.WriteLine("    }");
                outfile.WriteLine("}");
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

                Debug.Log($"{classType} 게임 이벤트 리스너 파일이 이미 존재합니다.: {paths}");
                return;
            }

            if (refresh) AssetDatabase.Refresh();

            Debug.Log($"{classType} 게임 이벤트 클래스 생성: {folderPath}");
        }

        private static void CreateGameEventListener(string type, string folderPath, bool refresh = false)
        {
            var splitIndex = type.LastIndexOf('.');
            var nameSpace = type.Substring(0, splitIndex);
            var classType = type.Substring(splitIndex + 1);
            
            var filePath = $"{folderPath}/{classType}GameEventListener.cs";
            var existScripts = AssetDatabase.FindAssets($"{classType}GameEventListener t:script");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (existScripts.Length == 0)
            {
                using var outfile = new StreamWriter(filePath);
                outfile.WriteLine("// 게임 이벤트 생성 메뉴에 의해 생성되었습니다.");
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using UnityEngine.Events;");
                if(nameSpace != "UnityEngine") outfile.WriteLine($"using {nameSpace};");
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

                Debug.Log($"{classType} 게임 이벤트 파일이 이미 존재합니다.: {paths}");
                return;
            }

            if (refresh) AssetDatabase.Refresh();

            Debug.Log($"{classType} 게임 이벤트 리스너 클래스 생성: {folderPath}");
        }
    }
}
