using System.Collections.Generic;
using UnityEngine;

namespace Sleep0.Logic
{
    public class FactoryManager<T> where T : FactoryManager<T>, new()
    {
        public static T Instance => _instance ??= new T();
        private static T _instance;

        private Dictionary<string, GameObjectFactory> factories = new Dictionary<string, GameObjectFactory>();

        public virtual void RegisterFactory<TFactory>(string key, GameObject prefab) where TFactory : GameObjectFactory, new()
        {
            if (!factories.ContainsKey(key))
            {
                factories[key] = new TFactory();
                factories[key].Initialize(prefab);
            }
            else
            {
                Debug.LogWarning($"Factory for type {key} already registered.");
            }
        }

        public virtual GameObjectFactory GetFactory(string key)
        {
            if (factories.TryGetValue(key, out GameObjectFactory factory))
            {
                return factory;
            }
            throw new KeyNotFoundException($"No factory found for key '{key}'");
        }

        public virtual GameObject CreateObject(string key, Transform parent)
        {
            if (factories.TryGetValue(key, out GameObjectFactory factory))
            {
                return factory.Create(parent);
            }
            throw new KeyNotFoundException($"No factory found for key '{key}'");
        }

        public virtual void UnregisterFactory(string key)
        {
            if (factories.TryGetValue(key, out GameObjectFactory factory))
            {
                factories.Remove(key);
                return;
            }
            throw new KeyNotFoundException($"No factory found for key '{key}'");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _instance = null;
        }
    }
}