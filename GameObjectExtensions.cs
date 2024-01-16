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
    } 
}