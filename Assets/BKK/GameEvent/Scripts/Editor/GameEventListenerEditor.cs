using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace BKK.GameEventArchitecture.Editor
{
    [CustomEditor(typeof(GameEventListener))]
    public class GameEventListenerEditor : BaseGameEventListenerEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            
            // Game Event Listener의 GameEvent 에셋 타입을 System.Reflection 기능으로 찾는다.
            gameEventType = target.GetType().GetProperty("GameEvent")?.PropertyType;

            if (gameEventType == null)
            {
                EditorGUILayout.PropertyField(mGameEvent);
            }
            else
            {
                mGameEvent.objectReferenceValue =
                    EditorGUILayout.ObjectField("Game Event", mGameEvent.objectReferenceValue, gameEventType, false);
            }
            
            if (mGameEvent.objectReferenceValue == null)
            {
                if (GUILayout.Button(createButtonText))
                {
                    string folderPath = EditorUtility.SaveFilePanel(savePanelTitle, Application.dataPath, defaultCreateAssetName, "asset");

                    if (string.IsNullOrEmpty(folderPath)) return;

                    string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);
                    
                    ScriptableObject instance = CreateInstance(typeof(GameEvent));

                    AssetDatabase.CreateAsset(instance, relativePath);

                    GameEvent asset = AssetDatabase.LoadAssetAtPath<GameEvent>(relativePath);

                    mGameEvent.objectReferenceValue = asset;

                    Debug.Log($"Game Event Asset Created!: {relativePath}");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(mOnStart);
            EditorGUILayout.PropertyField(mOnEnd);
            EditorGUILayout.PropertyField(mOnCancel);
            EditorGUILayout.PropertyField(mRestartDelay);
            EditorGUILayout.PropertyField(mStartTiming);
            EditorGUILayout.PropertyField(mEndTiming);
            
            serializedObject.ApplyModifiedProperties();

            // base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            var listener = target as GameEventListener; 
            
            var listenerGameEventValue = target.GetType().GetField("gameEvent").GetValue(listener) as ScriptableObject;

            if (listenerGameEventValue == null) return;
            
            var listenerGameEventType = listenerGameEventValue.GetType();

            var listenerGameEventName = listenerGameEventValue.ToString();
            
            var targetType = target.GetType();
            
            var behaviours = FindObjectsOfType<MonoBehaviour>();
            
            Handles.Label(listener.transform.position,
                $"{listenerGameEventValue.name} ({listenerGameEventType.Name})\n{listener.name} ({targetType.Name})",
                GameEventEditorUtility.ListenerStyle);

            foreach (var behaviour in behaviours)
            {
                var behaviourType = behaviour.GetType();
                
                if (behaviour == target || behaviour is GameEventListener) continue;
                
                var fields = behaviourType.GetRuntimeFields();

                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;

                    if (fieldType.IsAssignableFrom(typeof(GameEvent)))
                    {
                        var gameEventName = field.GetValue(behaviour).ToString();
                        // var gameEventName = GetValueRecursively(field, behaviour).ToString();

                        if(gameEventName != listenerGameEventName) break;
                        
                        Handles.color = GameEventEditorUtility.lineColor;
                        Handles.DrawDottedLine(listener.transform.position, behaviour.transform.position, 3);
                        
                        DrawEventDot(listener.transform.position, behaviour.transform.position);

                        Handles.Label(behaviour.transform.position,
                            $"{listenerGameEventValue.name} ({fieldType.Name})\n{behaviour.name} ({behaviour.GetType().Name})",
                            GameEventEditorUtility.GameEventStyle);
                        break;
                    }
                }
            }
        }
    }
    
    [CustomEditor(typeof(GameEventListener<,,>),true)]
    public class CustomTypeGameEventListenerEditor : BaseGameEventListenerEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            
            // Game Event Listener의 GameEvent 에셋 타입을 System.Reflection 기능으로 찾는다.
            gameEventType = target.GetType().GetProperty("GameEvent")?.PropertyType;

            if (gameEventType == null)
            {
                EditorGUILayout.PropertyField(mGameEvent);
            }
            else
            {
                mGameEvent.objectReferenceValue =
                    EditorGUILayout.ObjectField("Game Event", mGameEvent.objectReferenceValue, gameEventType, false);
            }

            if (mGameEvent.objectReferenceValue == null)
            {
                if (GUILayout.Button(createButtonText))
                {
                    string folderPath = EditorUtility.SaveFilePanel(savePanelTitle, Application.dataPath, defaultCreateAssetName, "asset");

                    if (string.IsNullOrEmpty(folderPath)) return;

                    string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);

                    ScriptableObject instance = CreateInstance(gameEventType);
                    
                    AssetDatabase.CreateAsset(instance, relativePath);
                    
                    Object asset = AssetDatabase.LoadAssetAtPath(relativePath, gameEventType);
                    
                    mGameEvent.objectReferenceValue = asset;
                    
                    Debug.Log($"{gameEventType.Name} Asset Created!: {relativePath}");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(mOnStart);
            EditorGUILayout.PropertyField(mOnEnd);
            EditorGUILayout.PropertyField(mOnCancel);
            EditorGUILayout.PropertyField(mRestartDelay);
            EditorGUILayout.PropertyField(mStartTiming);
            EditorGUILayout.PropertyField(mEndTiming);
            
            serializedObject.ApplyModifiedProperties();

            // base.OnInspectorGUI();
        }
        
        private void OnSceneGUI()
        {
            var listener = target as MonoBehaviour;
        
            var listenerGameEventValue = target.GetType().GetField("gameEvent").GetValue(listener) as ScriptableObject;
            
            if(listenerGameEventValue == null) return;
            
            var listenerGameEventType = listenerGameEventValue.GetType();
            var listenerGameEventName = listenerGameEventValue.ToString();
        
            var targetType = target.GetType();
        
            var targetArgs = targetType.BaseType.GetGenericArguments();
            
            var behaviours = FindObjectsOfType<MonoBehaviour>();
        
            Handles.Label(listener.transform.position,
                $"{listener.name} ({targetType.Name})\n{listenerGameEventValue.name} ({listenerGameEventType.Name})",
                GameEventEditorUtility.ListenerStyle);
            
            foreach (var behaviour in behaviours)
            {
                var behaviourType = behaviour.GetType();
        
                if (listener == behaviour || behaviourType == targetType) continue;
        
                var fields = behaviourType.GetRuntimeFields();
        
                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;

                    if (targetArgs.Length > 2 && fieldType == targetArgs[1])
                    {
                        var gameEventName = field.GetValue(behaviour).ToString();
                        // var gameEventName = GetValueRecursively(field, behaviour).ToString();

                        if(gameEventName != listenerGameEventName) continue;
                        
                        Handles.color = GameEventEditorUtility.lineColor;
                        Handles.DrawDottedLine(listener.transform.position, behaviour.transform.position, 3);

                        DrawEventDot(behaviour.transform.position, listener.transform.position);

                        Handles.Label(behaviour.transform.position,
                            $"{behaviour.name} ({behaviour.GetType().Name})\n{listenerGameEventValue.name} ({fieldType.Name})",
                            GameEventEditorUtility.GameEventStyle);
                        break;
                    }
                }
            }
        }
    }

    public class BaseGameEventListenerEditor : UnityEditor.Editor
    {
        protected Type gameEventType;
        
        protected SerializedProperty mGameEvent;
        protected SerializedProperty mOnStart;
        protected SerializedProperty mOnEnd;
        protected SerializedProperty mOnCancel;
        protected SerializedProperty mRestartDelay;
        protected SerializedProperty mStartTiming;
        protected SerializedProperty mEndTiming;

        protected readonly string defaultCreateAssetName = "New Game Event";
        protected readonly string savePanelTitle = "Save Game Event.";
        protected readonly string createButtonText = "Create";
        
        private SerializedProperty mStartDelayed;

        private float RestartTimeCheck = 0;
        private float startDelayTimeCheck = 0;

        protected virtual void OnEnable()
        {
            mGameEvent = serializedObject.FindProperty("gameEvent");
            mOnStart = serializedObject.FindProperty("onStart");
            mOnEnd = serializedObject.FindProperty("onEnd");
            mOnCancel = serializedObject.FindProperty("onCancel");
            mRestartDelay = serializedObject.FindProperty("restartDelay");
            mStartTiming = serializedObject.FindProperty("startTiming");
            mEndTiming = serializedObject.FindProperty("endTiming");
            
            mRestartDelay = serializedObject.FindProperty("restartDelay");
            mStartDelayed = serializedObject.FindProperty("startDelayed");
            mStartTiming = serializedObject.FindProperty("startTiming");
        }

        protected void DrawEventDot(Vector3 from, Vector3 to)
        {
            if (mStartDelayed.boolValue)
            {
                var direction = (to - from).normalized;
                var distance = Vector3.Distance(to, from);
                
                RestartTimeCheck += Time.deltaTime /mStartTiming.floatValue;
                var point = Mathf.Lerp(0, distance, RestartTimeCheck);

                Handles.color = GameEventEditorUtility.dotColor;
                Handles.DrawLine(from + direction * point, from + direction * (point + 0.25f));
            }
            else
            {
                RestartTimeCheck = 0;
                startDelayTimeCheck = 0;
            }
        }

        private HashSet<Type> nonRecursiveTypes = new HashSet<Type>
        {
            typeof(System.Int16), typeof(System.Int32), typeof(System.Int64), typeof(System.UInt16),
            typeof(System.UInt32), typeof(System.UInt64), typeof(System.String), typeof(System.Boolean),
            typeof(System.Byte),
            typeof(System.Char), typeof(System.Double), typeof(UnityEngine.Vector2),typeof(UnityEngine.Vector3), typeof(UnityEngine.Quaternion),
            typeof(UnityEngine.ScriptableObject)
        };
        
        protected object GetValueRecursively(FieldInfo field, object target)
        {
            var value = field.GetValue(target);

            if (nonRecursiveTypes.Contains(value.GetType()))
            {
                return value;
            }

            var inFields = field.GetType().GetFields();
                
            foreach (var fieldInfo in inFields)
            {
                return GetValueRecursively(fieldInfo, value);
            }

            return value;
        }
    }

    public static class GameEventEditorUtility
    {
        public static readonly Color lineColor = Color.green;
        public static readonly Color gameObjectNameColor = new Color(0.15f,0.15f,0.15f);
        public static readonly Color listenerColor = new Color(0.15f,0.15f,0.15f);
        public static readonly Color dotColor = Color.red;

        public static readonly GUIStyle GameEventStyle = new GUIStyle
        {
            normal =
            {
                textColor = gameObjectNameColor,
            },
            fontSize = 13,
            fontStyle = FontStyle.Normal,
        };
        
        public static readonly GUIStyle ListenerStyle = new GUIStyle
        {
            normal =
            {
                textColor = listenerColor,
            },
            fontSize = 13,
            fontStyle = FontStyle.Normal,
        };
    }
}