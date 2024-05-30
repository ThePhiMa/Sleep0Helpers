using System;
using UnityEngine;

namespace Sleep0
{
    public static class GameObjectExtensions
    {
        public static void DestroySafe(this GameObject gameObject)
        {
            if (Application.IsPlaying(gameObject))
                GameObject.Destroy(gameObject);
            else
                GameObject.DestroyImmediate(gameObject);
        }

        public static T CreateInstance<T>(params object[] args) where T : class, new()
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T), args);
            }
            catch (MissingMethodException)
            {
                return new T();
            }
        }
    }
}