using System;
using System.Reflection;
using UnityEditor;

public static class EditorExtension
{
    /// <summary>
    /// SerializedProperty의 실제 값을 object 형식으로 리턴한다.
    /// </summary>
    /// <param name="target">대상</param>
    /// <param name="propertyName">에디터 클래스의 대상이 되는 원본 클래스에서의 프로퍼티 이름</param>
    /// <returns></returns>
    public static object GetFieldValue(SerializedProperty target, string propertyName)
    {
        Type targetType = target.serializedObject.targetObject.GetType();

        FieldInfo targetField =
            targetType.GetField(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

        return targetField.GetValue(target.serializedObject.targetObject);
    }
}
