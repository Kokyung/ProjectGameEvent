using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BKK.GameEventArchitecture.Editor
{
    [CustomEditor(typeof(GameEventListener))]
    public class GameEventListenerEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var listener = target as GameEventListener; 
            
            var behaviours = FindObjectsOfType<MonoBehaviour>();

            foreach (var behaviour in behaviours)
            {
                var type = behaviour.GetType();
                var fields = type.GetRuntimeFields();

                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;
                    if (fieldType.IsAssignableFrom(typeof(GameEvent)))
                    {
                        Handles.DrawDottedLine(listener.transform.position, behaviour.transform.position, 3);
                        
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

            var targetType = target.GetType();

            var targetArgs = targetType.BaseType.GetGenericArguments();
            
            var behaviours = FindObjectsOfType<MonoBehaviour>();

            foreach (var behaviour in behaviours)
            {
                var behaviourType = behaviour.GetType();

                if (behaviourType == targetType) continue;
                
                var fields = behaviourType.GetRuntimeFields();

                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;

                    if (fieldType == targetArgs[1])
                    {
                        Handles.DrawDottedLine(listener.transform.position, behaviour.transform.position, 3);
                        break;
                    }
                }
            }
        }
        
        
    }
}