using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace BKK.GameEventArchitecture.Editor
{
    [CustomEditor(typeof(GameEventListener))]
    public class GameEventListenerEditor : BaseGameEventListenerEditor
    {
        private void OnSceneGUI()
        {
            var listener = target as GameEventListener; 
            
            var listenerGameEventValue = target.GetType().GetField("gameEvent").GetValue(listener) as ScriptableObject;
            var listenerGameEventType = listenerGameEventValue.GetType();
            
            if(listenerGameEventValue == null) return;
            
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
        private SerializedProperty mRestartDelay;
        private SerializedProperty mStartDelayed;
        private SerializedProperty mStartTiming;

        private float RestartTimeCheck = 0;
        private float startDelayTimeCheck = 0;

        private void OnEnable()
        {
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
        public static readonly Color gameObjectNameColor = Color.gray;
        public static readonly Color listenerColor = Color.gray;
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