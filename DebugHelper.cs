using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sleep0
{
    public static class DebugHelper
    {
        private static void DoLog(Action<string, Object> logFunction, string prefix, Object @object, object message, [CallerLineNumber] int line = 0, [CallerMemberName] string caller = "")
        {
#if UNITY_EDITOR
            logFunction($"{prefix}[<color=lightblue>{@object.name}<color=grey>:{caller}:{line}</color></color>]: {message}", @object);
#endif
        }

        public static void Log(this Object @object, object message, [CallerLineNumber] int line = 0, [CallerMemberName] string caller = "")
        {
            DoLog(Debug.Log, "", @object, message, line, caller);
        }

        public static void LogError(this Object @object, object messags)
        {
            DoLog(Debug.LogError, "<color=red>!</color>", @object, messags);
        }

        public static void LogWarning(this Object @object, object message)
        {
            DoLog(Debug.LogWarning, "<color=yellow>!</color>", @object, message);
        }

        public static void LogSuccess(this Object @object, object message)
        {
            DoLog(Debug.Log, "<color=green>></color>", @object, message);
        }
    }
}
