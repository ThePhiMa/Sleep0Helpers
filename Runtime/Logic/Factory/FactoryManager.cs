using System.Collections.Generic;
using UnityEngine;

namespace Sleep0.Logic
{
    public class FactoryManager
    {
        public static FactoryManager Instance => _instance ??= new FactoryManager();
        private static FactoryManager _instance;

        private Dictionary<string, GameObjectFactory> factories = new Dictionary<string, GameObjectFactory>();

        public void RegisterFactory<T>(string key, GameObject prefab) where T : GameObjectFactory, new()
        {
            if (!factories.ContainsKey(key))
            {
                factories[key] = new T();
                factories[key].Initialize(prefab);
            }
            else
            {
                Debug.LogWarning($"Factory for type {key} already registered.");
            }
        }

        public GameObjectFactory GetFactory(string key)
        {
            if (factories.TryGetValue(key, out GameObjectFactory factory))
            {
                return factory;
            }
            throw new KeyNotFoundException($"No factory found for key '{key}'");
        }

        public GameObject CreateObject(string key, Transform parent)
        {
            if (factories.TryGetValue(key, out GameObjectFactory factory))
            {
                return factory.Create(parent);
            }
            throw new KeyNotFoundException($"No factory found for key '{key}'");
        }

        public void UnregisterFactory(string key)
        {
            if (factories.TryGetValue(key, out GameObjectFactory factory))
            {
                factories.Remove(key);
            }
            throw new KeyNotFoundException($"No factory found for key '{key}'");
        }
    }
}