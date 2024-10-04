using Sleep0.Logic;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FactoryManager
{
    public static FactoryManager Instance => _instance ??= new FactoryManager();
    private static FactoryManager _instance;

    private Dictionary<Type, object> factories = new Dictionary<Type, object>();

    public void RegisterFactory<T>(GameObjectFactory<T> factory) where T : Component
    {
        Type componentType = typeof(T);
        if (!factories.ContainsKey(componentType))
        {
            factories[componentType] = factory;
        }
        else
        {
            Debug.LogWarning($"Factory for type {componentType.Name} already registered.");
        }
    }

    public GameObjectFactory<T> GetFactory<T>() where T : Component
    {
        Type componentType = typeof(T);
        if (factories.TryGetValue(componentType, out object factory))
        {
            return (GameObjectFactory<T>)factory;
        }

        Debug.LogError($"No factory registered for type {componentType.Name}.");
        return null;
    }

    public GameObject CreateObject<T>() where T : Component
    {
        GameObjectFactory<T> factory = GetFactory<T>();
        if (factory != null)
        {
            return factory.GetObject();
        }
        return null;
    }

    public void UnregisterFactory<T>() where T : Component
    {
        Type componentType = typeof(T);
        if (factories.ContainsKey(componentType))
        {
            factories.Remove(componentType);
        }
        else
        {
            Debug.LogWarning($"No factory registered for type {componentType.Name} to unregister.");
        }
    }
}