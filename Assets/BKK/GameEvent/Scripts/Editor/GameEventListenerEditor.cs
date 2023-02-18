using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace BKK.GameEventArchitecture.Editor
{
    [CustomEditor(typeof(GameEventListener))]
    public class GameEventListenerEditor : UnityEditor.Editor
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

                        if(gameEventName != listenerGameEventName) break;
                        
                        Handles.color = GameEventEditorUtility.lineColor;
                        Handles.DrawDottedLine(listener.transform.position, behaviour.transform.position, 3);
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
    public class CustomTypeGameEventListenerEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var listener = target as MonoBehaviour;
        
            var listenerGameEventValue = target.GetType().GetField("gameEvent").GetValue(listener) as ScriptableObject;
            var listenerGameEventType = listenerGameEventValue.GetType();
            
            if(listenerGameEventValue == null) return;
            
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
        
                    if (fieldType == targetArgs[1])
                    {
                        var gameEventName = field.GetValue(behaviour).ToString();
        
                        if(gameEventName != listenerGameEventName) continue;
                        
                        Handles.color = GameEventEditorUtility.lineColor;
                        Handles.DrawDottedLine(listener.transform.position, behaviour.transform.position, 3);
        
                        // var direction = (behaviour.transform.position - listener.transform.position).normalized;
                        // var distance = Vector3.Distance(behaviour.transform.position, listener.transform.position);
                        //
                        // var point = Mathf.Lerp(0, distance, 3 * Time.deltaTime);
                        //
                        // Handles.color = Color.red;
                        // Handles.DrawLine(listener.transform.position + direction * point,
                        //     listener.transform.position + direction * (point + 0.25f));
        
                        Handles.Label(behaviour.transform.position,
                            $"{behaviour.name} ({behaviour.GetType().Name})\n{listenerGameEventValue.name} ({fieldType.Name})",
                            GameEventEditorUtility.GameEventStyle);
                        break;
                    }
                }
            }
        }
    }

    public static class GameEventEditorUtility
    {
        // public static readonly Color lineColor = new Color(0.2f, 0.1f, 0.7f);
        public static readonly Color lineColor = Color.green;
        public static readonly Color gameObjectNameColor = Color.gray;
        public static readonly Color listenerColor = Color.gray;

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