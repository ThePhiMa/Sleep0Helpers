using System;
using UnityEngine;

namespace Sleep0
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Destroys the GameObject safely, taking into account whether the application is playing or not.
        /// </summary>
        /// <param name="gameObject"></param>
        public static void DestroySafe(this GameObject gameObject)
        {
            if (Application.IsPlaying(gameObject))
                GameObject.Destroy(gameObject);
            else
                GameObject.DestroyImmediate(gameObject);
        }

        /// <summary>
        /// Creates an instance of the given type, taking into account whether the type has a parametered constructor or not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
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