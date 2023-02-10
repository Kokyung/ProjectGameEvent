using System;
using BKK.GameEventArchitecture.Editor;
using UnityEditor;
using UnityEngine;

namespace BKK.GameEventArchitecture.Editor
{
    public class GameEventCreatorWindow : EditorWindow
    {
        private string className;
        private string creationPath = "Assets/CustomGameEvent";
        private string createMenuPath = "BKK/Game Event Architecture";
        private MonoScript script = null;
        
        private readonly GUIContent scriptLabel = new GUIContent("타겟 클래스 지정", "파라미터로 받을 타겟 클래스를 지정합니다.");
        private readonly GUIContent classNameLabel =
            new GUIContent("클래스 이름", "파라미터로 받을 클래스 이름. 네임스페이스도 포함되어야합니다.\n예) UnityEngine.Quaternion");
        private readonly GUIContent creationPathLabel = new GUIContent("폴더 경로", "생성된 게임 이벤트가 저장될 폴더 경로");
        private readonly GUIContent createMenuPathLabel = new GUIContent("생성 메뉴 경로", "Project 탭에서 해당 게임 이벤트 생성 메뉴가 표시될 경로를 설정합니다.");
        private readonly GUIContent folderButtonLabel = new GUIContent("폴더 경로 지정", "게임 이벤트가 생성될 폴더를 지정합니다.");
        private readonly GUIContent generateButtonLabel = new GUIContent("생성", "게임 이벤트를 생성합니다.");
        private readonly GUIContent resetPathButtonLabel = new GUIContent("폴더 경로 초기화", "기본 경로로 초기화합니다.");

        private readonly string classImportDropDownLabel = "클래스 가져오기 방식";
        private readonly string folderMenuTitle = "게임 이벤트 폴더 지정";
        private readonly string scriptAccessErrorMessage = "접근 할 수 없는 클래스입니다. Assets 폴더 내에 있는지 확인해주세요";
        private readonly string defaultCreationPath = "Assets/CustomGameEvent";
        private readonly string defaultCreateMenuPath = "BKK/Game Event Architecture";
        private readonly string selectedKeyName = "GameEventCreatorWindow_ClassImportType";
        
        private readonly string[] options = new string[]
        {
            "클래스 선택", "직접 입력",
        };
        
        private int selected = 1;
        
        private readonly Vector2 minWindowSize = new Vector2(416, 200);

        private void OnEnable()
        {
            selected = EditorPrefs.GetBool(selectedKeyName, selected != 0) ? 1 : 0;
        }

        private void OnGUI()
        {
            Draw();
        }

        private void OnDestroy()
        {
            ResetProperties();
        }

        private void Draw()
        {
            minSize = minWindowSize;
            
            EditorGUILayout.Separator();

            selected = EditorGUILayout.Popup(classImportDropDownLabel, selected, options);
            
            EditorGUILayout.Separator();

            if (selected == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(scriptLabel);
                script = EditorGUILayout.ObjectField(script, typeof(MonoScript), false) as MonoScript;

                if (script != null)
                {
                    if (script.GetClass() != null)
                    {
                        className = script.GetClass().FullName;
                    }
                    else
                    {
                        Debug.LogError(scriptAccessErrorMessage);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                className = EditorGUILayout.TextField(classNameLabel, className);
            }
            
            creationPath = EditorGUILayout.TextField(creationPathLabel, creationPath);
            createMenuPath = EditorGUILayout.TextField(createMenuPathLabel, createMenuPath);
            
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            
            FileBrowserButton(ref creationPath, folderButtonLabel, folderMenuTitle);

            if (GUILayout.Button(resetPathButtonLabel))
            {
                ResetPath();
            }
            
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(generateButtonLabel))
            {
                GameEventCreator.CreateAll(className, creationPath, createMenuPath);
            }

            EditorPrefs.SetBool(selectedKeyName, selected != 0);
            
            EditorUtility.SetDirty(this);
        }

        private void ResetProperties(bool withPath = false)
        {
            if (withPath) ResetPath();
            
            className = string.Empty;
            script = null;
        }

        private void ResetPath()
        {
            creationPath = defaultCreationPath;
            createMenuPath = defaultCreateMenuPath;
        }
        
        public void FileBrowserButton(ref string currentPath, GUIContent guiContent, string panelTitle)
        {
            if (GUILayout.Button(guiContent))
            {
                FileBrowser(ref currentPath, panelTitle);
            }
        }

        public void FileBrowser(ref string currentPath, string panelTitle)
        {
            string folderPath = EditorUtility.SaveFolderPanel("게임 이벤트 폴더 지정", Application.dataPath, "");

            if (string.IsNullOrEmpty(folderPath)) return;

            currentPath = folderPath;
        }
    }
}
