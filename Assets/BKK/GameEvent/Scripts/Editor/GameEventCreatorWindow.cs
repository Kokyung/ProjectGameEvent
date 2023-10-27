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
        
        private readonly GUIContent scriptLabel = new GUIContent("Target Class", "Select the target class to use as the type parameter.");
        private readonly GUIContent classNameLabel =
            new GUIContent("Class Name", "Class name to use as parameter. Namespace must be included.\nex) UnityEngine.Quaternion");
        private readonly GUIContent creationPathLabel = new GUIContent("Folder Path", "Folder path for game event generated.");
        private readonly GUIContent createMenuPathLabel = new GUIContent("Asset Creation Menu Path", "Game event asset creation menu path in Project tab.");
        private readonly GUIContent folderButtonLabel = new GUIContent("Choose Folder Path", "Choose Folder Path which game event will generated.");
        private readonly GUIContent generateButtonLabel = new GUIContent("Generate", "Generate Custom Game Event.");
        private readonly GUIContent resetPathButtonLabel = new GUIContent("Reset Folder Path", "Reset Folder Path to default.");

        private readonly string classImportDropDownLabel = "Class Import By";
        private readonly string folderMenuTitle = "Choose Game Event Folder";

        private readonly string scriptAccessErrorMessage = "Can't access target class. Please check target class is in Asset Folder.";
        private readonly string defaultCreationPath = "Assets/CustomGameEvent";
        private readonly string defaultCreateMenuPath = "BKK/Game Event Architecture";
        private readonly string selectedKeyName = "GameEventCreatorWindow_ClassImportType";
        
        private readonly string[] options = new string[]
        {
            "Select class", "Typing directly",
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
            string folderPath = EditorUtility.SaveFolderPanel(panelTitle, Application.dataPath, "");

            if (string.IsNullOrEmpty(folderPath)) return;

            currentPath = folderPath;
        }
    }
}
