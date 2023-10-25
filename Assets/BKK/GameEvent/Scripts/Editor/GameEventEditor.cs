using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace BKK.GameEventArchitecture.Editor
{
    [CustomEditor(typeof(GameEvent))]
    public class GameEventEditor : UnityEditor.Editor
    {
        private GameEvent gameEvent;

        private Vector2 scroll;

        private bool locked;
        private void OnEnable()
        {
            gameEvent = (GameEvent) target;
        }

        public override void OnInspectorGUI()
        {
            if (EditorApplication.isPlaying)
            {
                EditorPrefs.SetBool(GameEventGlobalOption.locked, true);
            }

            EditorGUILayout.LabelField("[ Debug ]", EditorStyles.boldLabel);

            if (!EditorApplication.isPlaying) GUI.enabled = false;
            
            if (GUILayout.Button("Invoke"))
            {
                gameEvent.Raise();
            }

            if (GUILayout.Button("Cancel"))
            {
                gameEvent.Cancel();
            }

            if (!EditorApplication.isPlaying) GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[ Description ]", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll,
                GUILayout.MaxHeight(EditorPrefs.GetInt(GameEventGlobalOption.typingAreaHeight) *
                                    EditorGUIUtility.singleLineHeight));
            
            if (EditorPrefs.GetBool(GameEventGlobalOption.locked)) GUI.enabled = false;

            gameEvent.description = WithoutSelectAll(() => EditorGUILayout.TextArea(gameEvent.description,
                EditorStyles.textArea,
                GUILayout.MaxHeight(EditorPrefs.GetInt(GameEventGlobalOption.typingAreaHeight) *
                                    EditorGUIUtility.singleLineHeight)));

            if (EditorPrefs.GetBool(GameEventGlobalOption.locked)) GUI.enabled = true;

            EditorStyles.textArea.fontSize = EditorPrefs.GetInt(GameEventGlobalOption.fontSize);
            EditorGUILayout.EndScrollView();

            Undo.RecordObject(gameEvent, "Game Event Description");

            var fontSizeValue = EditorGUILayout.IntField("Font Size", EditorPrefs.GetInt(GameEventGlobalOption.fontSize));
            if (fontSizeValue < 1) fontSizeValue = 1;
            EditorPrefs.SetInt(GameEventGlobalOption.fontSize, fontSizeValue);
            
            var typingAreaHeightValue = EditorGUILayout.IntField("Input Field Height", EditorPrefs.GetInt(GameEventGlobalOption.typingAreaHeight));
            if (typingAreaHeightValue < 1) typingAreaHeightValue = 1;
            EditorPrefs.SetInt(GameEventGlobalOption.typingAreaHeight, typingAreaHeightValue);
            
            if (EditorApplication.isPlaying) GUI.enabled = false;
            if (EditorPrefs.GetBool(GameEventGlobalOption.locked))
            {
                if (GUILayout.Button("Edit"))
                {
                    EditorPrefs.SetBool(GameEventGlobalOption.locked, false);
                }
            }
            else
            {
                if (GUILayout.Button("Save"))
                {
                    EditorPrefs.SetBool(GameEventGlobalOption.locked, true);
                    EditorUtility.SetDirty(gameEvent);
                }
            }


            if (EditorApplication.isPlaying) GUI.enabled = true;

            EditorUtility.SetDirty(gameEvent);
        }

        /// <summary>
        /// 텍스트 에이리어 선택시 텍스트가 전부 선택되는 문제를 방지한다.
        /// </summary>
        /// <param name="guiCall">레이아웃 메서드</param>
        /// <typeparam name="T">리턴 타입</typeparam>
        /// <returns></returns>
        private T WithoutSelectAll<T>(System.Func<T> guiCall)
        {
            bool preventSelection = (Event.current.type == EventType.MouseDown);

            Color oldCursorColor = GUI.skin.settings.cursorColor;

            if (preventSelection)
                GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);

            T value = guiCall();

            if (preventSelection)
                GUI.skin.settings.cursorColor = oldCursorColor;

            return value;
        }
    }
    
    [CustomEditor(typeof(GameEvent<>), true)]
    public class CustomTypeGameEventEditor : UnityEditor.Editor
    {
        private Vector2 scroll;
    
        private bool locked;

        private SerializedProperty _debugValue;
        private SerializedProperty _description;
        private MethodInfo _raiseMethod;
        private MethodInfo _cancelMethod;
        
        private readonly string debugValuePropertyName = "debugValue";
        private readonly string descriptionPropertyName = "description";

        private GUIContent debugValueContent = new GUIContent("Deug Value");
        
        private void OnEnable()
        {
            _debugValue = serializedObject.FindProperty(debugValuePropertyName);
            _description = serializedObject.FindProperty(descriptionPropertyName);
            _raiseMethod = target.GetType().BaseType.GetMethod("Raise",
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            _cancelMethod = target.GetType().BaseType.GetMethod("Cancel",
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        public override void OnInspectorGUI()
        {
            if (EditorApplication.isPlaying) EditorPrefs.SetBool(GameEventGlobalOption.locked, true);

            EditorGUILayout.LabelField("[ Debug ]", EditorStyles.boldLabel);

            if (!EditorApplication.isPlaying) GUI.enabled = false;

            EditorGUILayout.PropertyField(_debugValue, debugValueContent, true);

            if (GUILayout.Button("Invoke"))
            {
                // debug 값은 어떤 타입의 값이든 될 수 있기 때문에 System.Reflection 기능을 활용하여 가져온다.
                object debugValue = EditorExtension.GetFieldValue(_debugValue, debugValuePropertyName);
                MethodExtension.ExecuteMethod(target, _raiseMethod, debugValue);
            }

            if (GUILayout.Button("Cancel"))
            {
                object debugValue = EditorExtension.GetFieldValue(_debugValue, debugValuePropertyName);
                MethodExtension.ExecuteMethod(target, _cancelMethod, debugValue);
            }

            if (!EditorApplication.isPlaying) GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[ Description ]", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll,
                GUILayout.MaxHeight(EditorPrefs.GetInt(GameEventGlobalOption.typingAreaHeight) * EditorGUIUtility.singleLineHeight));

            if (EditorPrefs.GetBool(GameEventGlobalOption.locked)) GUI.enabled = false;

            _description.stringValue = WithoutSelectAll(() => EditorGUILayout.TextArea(_description.stringValue,
                EditorStyles.textArea,
                GUILayout.MaxHeight(EditorPrefs.GetInt(GameEventGlobalOption.typingAreaHeight) * EditorGUIUtility.singleLineHeight)));

            if (EditorPrefs.GetBool(GameEventGlobalOption.locked)) GUI.enabled = true;

            EditorStyles.textArea.fontSize = EditorPrefs.GetInt(GameEventGlobalOption.fontSize);
            EditorGUILayout.EndScrollView();

            Undo.RecordObject(target, "Game Event Description");
            
            var fontSizeValue = EditorGUILayout.IntField("Font Size", EditorPrefs.GetInt(GameEventGlobalOption.fontSize));
            if (fontSizeValue < 1) fontSizeValue = 1;
            EditorPrefs.SetInt(GameEventGlobalOption.fontSize, fontSizeValue);
            
            var typingAreaHeightValue = EditorGUILayout.IntField("Input Field Height", EditorPrefs.GetInt(GameEventGlobalOption.typingAreaHeight));
            if (typingAreaHeightValue < 1) typingAreaHeightValue = 1;
            EditorPrefs.SetInt(GameEventGlobalOption.typingAreaHeight, typingAreaHeightValue);

            if (EditorApplication.isPlaying) GUI.enabled = false;
            if (EditorPrefs.GetBool(GameEventGlobalOption.locked))
            {
                if (GUILayout.Button("Edit"))
                {
                    EditorPrefs.SetBool(GameEventGlobalOption.locked, false);
                }
            }
            else
            {
                if (GUILayout.Button("Save"))
                {
                    EditorPrefs.SetBool(GameEventGlobalOption.locked, true);
                    EditorUtility.SetDirty(target);
                }
            }
            
            if (EditorApplication.isPlaying) GUI.enabled = true;

            EditorUtility.SetDirty(target);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 텍스트 에이리어 선택시 텍스트가 전부 선택되는 문제를 방지한다.
        /// </summary>
        /// <param name="guiCall">레이아웃 메서드</param>
        /// <typeparam name="T">리턴 타입</typeparam>
        /// <returns></returns>
        private T WithoutSelectAll<T>(System.Func<T> guiCall)
        {
            bool preventSelection = (Event.current.type == EventType.MouseDown);
    
            Color oldCursorColor = GUI.skin.settings.cursorColor;
    
            if (preventSelection)
                GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);
    
            T value = guiCall();
    
            if (preventSelection)
                GUI.skin.settings.cursorColor = oldCursorColor;
    
            return value;
        }
    }
}

