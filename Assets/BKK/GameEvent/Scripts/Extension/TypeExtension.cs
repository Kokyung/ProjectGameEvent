using System;

public static class TypeExtension
{
    public static bool IsSubclassOfRawGeneric(this Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic.IsSubclassOf(cur))
            {
                return true;
            }

            toCheck = toCheck.BaseType;
        }

        return false;
    }
}
