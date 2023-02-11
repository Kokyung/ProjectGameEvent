using System;
using System.Linq;
using UnityEngine;

namespace BKK.Extension
{
    public static class StringUtility
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static string EraseWhiteSpace(this string source)
        {
            return string.Concat(source.Where(c => !char.IsWhiteSpace(c)));
        }

        public static string GetPath(this Component component) {
            return component.transform.GetPath() + "/" + component.GetType().ToString();
        }
    
        public static string GetPath(this Transform current) {
            if (current.parent == null) return current.name;
            return current.parent.GetPath() + "/" + current.name;
        }
        
        public static string[] GetStringArrayFromEnum(this Type t)
        {
            return Enum.GetNames(t);
        }
        
        public static int GetEnumCount(this Type t)
        {
            return Enum.GetNames(t).Length;
        }
        
        public static T ConvertToEnum<T>(this string _str)
        {
            try { return (T)Enum.Parse(typeof(T), _str); }
            catch { return (T)Enum.Parse(typeof(T), "none"); }
        }
        
        public static string ConvertToString<T>(this T _enum) where T : Enum
        {
            try { return Enum.GetName(typeof(T), _enum); }
            catch { return String.Empty; }
        }
    }
}
