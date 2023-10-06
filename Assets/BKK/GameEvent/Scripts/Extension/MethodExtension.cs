using System;
using System.Reflection;
using UnityEditor;

namespace BKK
{
    public static class MethodExtension
    {
        /// <summary>
        /// 특정 오브젝트의 메서드를 실행시킨다.
        /// </summary>
        /// <param name="methodInfo">메서드 정보</param>
        /// <param name="target">타겟 오브젝트</param>
        /// <param name="value">매개변수</param>
        public static void ExecuteMethod(object target, MethodInfo methodInfo, object value)
        {
            methodInfo.Invoke(target, new[] { value });
        }
    }
}