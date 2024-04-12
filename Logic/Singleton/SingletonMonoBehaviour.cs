using UnityEngine;

namespace Sleep0.Logic
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        public static T Instance { get; private set; }

        protected void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("There can only be one instance active.");
                Destroy(this);
            }

            Instance = this as T;
        }
    }
}