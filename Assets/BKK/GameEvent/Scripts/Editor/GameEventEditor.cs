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

        // private GameEventDescriptionOption descriptionOption;

        private Vector2 scroll;

        private bool locked;
        private void OnEnable()
        {
            gameEvent = (GameEvent) target;
            
            // FindDescriptionOption();
        }

        public override void OnInspectorGUI()
        {
            if (EditorApplication.isPlaying)
            {
                EditorPrefs.SetBool(GameEventDescriptionOption.locked, true);
            }

            EditorGUILayout.LabelField("[ 디버그 기능 ]", EditorStyles.boldLabel);
            if (!EditorApplication.isPlaying) GUI.enabled = false;
            if (GUILayout.Button("실행"))
            {
                gameEvent.Raise();
            }

            if (GUILayout.Button("취소"))
            {
                gameEvent.Cancel();
            }

            if (!EditorApplication.isPlaying) GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[ 이벤트 설명 ]", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll,
                GUILayout.MaxHeight(EditorPrefs.GetInt(GameEventDescriptionOption.typingAreaHeight) *
                                    EditorGUIUtility.singleLineHeight));
            
            if (EditorPrefs.GetBool(GameEventDescriptionOption.locked)) GUI.enabled = false;

            gameEvent.description = WithoutSelectAll(() => EditorGUILayout.TextArea(gameEvent.description,
                EditorStyles.textArea,
                GUILayout.MaxHeight(EditorPrefs.GetInt(GameEventDescriptionOption.typingAreaHeight) *
                                    EditorGUIUtility.singleLineHeight)));

            if (EditorPrefs.GetBool(GameEventDescriptionOption.locked)) GUI.enabled = true;

            EditorStyles.textArea.fontSize = EditorPrefs.GetInt(GameEventDescriptionOption.fontSize);
            EditorGUILayout.EndScrollView();

            Undo.RecordObject(gameEvent, "Game Event Description");

            var fontSizeValue = EditorGUILayout.IntField("폰트 사이즈", EditorPrefs.GetInt(GameEventDescriptionOption.fontSize));
            if (fontSizeValue < 1) fontSizeValue = 1;
            EditorPrefs.SetInt(GameEventDescriptionOption.fontSize, fontSizeValue);
            
            var typingAreaHeightValue = EditorGUILayout.IntField("필드 높이", EditorPrefs.GetInt(GameEventDescriptionOption.typingAreaHeight));
            if (typingAreaHeightValue < 1) typingAreaHeightValue = 1;
            EditorPrefs.SetInt(GameEventDescriptionOption.typingAreaHeight, typingAreaHeightValue);
            
            if (EditorApplication.isPlaying) GUI.enabled = false;
            if (EditorPrefs.GetBool(GameEventDescriptionOption.locked))
            {
                if (GUILayout.Button("편집"))
                {
                    EditorPrefs.SetBool(GameEventDescriptionOption.locked, false);
                }
            }
            else
            {
                if (GUILayout.Button("저장"))
                {
                    EditorPrefs.SetBool(GameEventDescriptionOption.locked, true);
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

        private GUIContent debugValueContent = new GUIContent("디버그 값");
        
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
            if (EditorApplication.isPlaying) EditorPrefs.SetBool(GameEventDescriptionOption.locked, true);

            EditorGUILayout.LabelField("[ 디버그 기능 ]", EditorStyles.boldLabel);
            if (!EditorApplication.isPlaying) GUI.enabled = false;

            EditorGUILayout.PropertyField(_debugValue, debugValueContent, true);

            if (GUILayout.Button("실행"))
            {
                object debugValue = EditorExtension.GetFieldValue(_debugValue, debugValuePropertyName);
                MethodExtension.ExecuteMethod(target, _raiseMethod, debugValue);
            }

            if (GUILayout.Button("취소"))
            {
                object debugValue = EditorExtension.GetFieldValue(_debugValue, debugValuePropertyName);
                MethodExtension.ExecuteMethod(target, _cancelMethod, debugValue);
            }

            if (!EditorApplication.isPlaying) GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[ 이벤트 설명 ]", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll,
                GUILayout.MaxHeight(EditorPrefs.GetInt(GameEventDescriptionOption.typingAreaHeight) * EditorGUIUtility.singleLineHeight));

            if (EditorPrefs.GetBool(GameEventDescriptionOption.locked)) GUI.enabled = false;

            _description.stringValue = WithoutSelectAll(() => EditorGUILayout.TextArea(_description.stringValue,
                EditorStyles.textArea,
                GUILayout.MaxHeight(EditorPrefs.GetInt(GameEventDescriptionOption.typingAreaHeight) * EditorGUIUtility.singleLineHeight)));

            if (EditorPrefs.GetBool(GameEventDescriptionOption.locked)) GUI.enabled = true;

            EditorStyles.textArea.fontSize = EditorPrefs.GetInt(GameEventDescriptionOption.fontSize);
            EditorGUILayout.EndScrollView();

            Undo.RecordObject(target, "Game Event Description");
            
            var fontSizeValue = EditorGUILayout.IntField("폰트 사이즈", EditorPrefs.GetInt(GameEventDescriptionOption.fontSize));
            if (fontSizeValue < 1) fontSizeValue = 1;
            EditorPrefs.SetInt(GameEventDescriptionOption.fontSize, fontSizeValue);
            
            var typingAreaHeightValue = EditorGUILayout.IntField("필드 높이", EditorPrefs.GetInt(GameEventDescriptionOption.typingAreaHeight));
            if (typingAreaHeightValue < 1) typingAreaHeightValue = 1;
            EditorPrefs.SetInt(GameEventDescriptionOption.typingAreaHeight, typingAreaHeightValue);

            if (EditorApplication.isPlaying) GUI.enabled = false;
            if (EditorPrefs.GetBool(GameEventDescriptionOption.locked))
            {
                if (GUILayout.Button("편집"))
                {
                    EditorPrefs.SetBool(GameEventDescriptionOption.locked, false);
                }
            }
            else
            {
                if (GUILayout.Button("저장"))
                {
                    EditorPrefs.SetBool(GameEventDescriptionOption.locked, true);
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

