using System;
using System.Collections.Generic;
using System.Reflection;

public class InjectAttribute : Attribute
{
    public LifetimeScope Scope { get; }

    public InjectAttribute(LifetimeScope scope = LifetimeScope.Singleton)
    {
        Scope = scope;
    }
}

public enum LifetimeScope
{
    Singleton,
    Transient,
    PerScene
}

public class DependencyContainer
{
    private static DependencyContainer _instance;
    public static DependencyContainer Instance => _instance ??= new DependencyContainer();

    private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
    private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
    private readonly Dictionary<string, Dictionary<Type, object>> _sceneScoped = new Dictionary<string, Dictionary<Type, object>>();

    private DependencyContainer() { }

    public void Register<T>(Func<T> factory)
    {
        _factories[typeof(T)] = () => factory();
    }

    public void InjectDependencies(object target)
    {
        var type = target.GetType();
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        foreach (var field in fields)
        {
            var injectAttribute = field.GetCustomAttribute<InjectAttribute>();
            if (injectAttribute != null)
            {
                var fieldType = field.FieldType;
                object dependency = ResolveDependency(fieldType, injectAttribute.Scope);
                field.SetValue(target, dependency);
            }
        }
    }

    private object ResolveDependency(Type type, LifetimeScope scope)
    {
        switch (scope)
        {
            case LifetimeScope.Singleton:
                if (!_singletons.TryGetValue(type, out var singleton))
                {
                    singleton = CreateInstance(type);
                    _singletons[type] = singleton;
                }
                return singleton;

            case LifetimeScope.Transient:
                return CreateInstance(type);

            case LifetimeScope.PerScene:
                var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (!_sceneScoped.TryGetValue(sceneName, out var sceneInstances))
                {
                    sceneInstances = new Dictionary<Type, object>();
                    _sceneScoped[sceneName] = sceneInstances;
                }
                if (!sceneInstances.TryGetValue(type, out var sceneInstance))
                {
                    sceneInstance = CreateInstance(type);
                    sceneInstances[type] = sceneInstance;
                }
                return sceneInstance;

            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
        }
    }

    private object CreateInstance(Type type)
    {
        if (_factories.TryGetValue(type, out var factory))
        {
            return factory();
        }
        throw new Exception($"No factory registered for type {type}");
    }

    // Method to clear per-scene dependencies when a scene is unloaded
    public void ClearSceneDependencies(string sceneName)
    {
        _sceneScoped.Remove(sceneName);
    }
}