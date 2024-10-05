using System;
using UnityEngine;

namespace Sleep0.Logic
{
    //public class GenericFactory<T> : MonoBehaviour<T>
    //{
    //    [SerializeField]
    //    protected T _prefab;

    //    public GenericFactory(T prefab)
    //    {
    //        _prefab = prefab;
    //    }

    //    public virtual T GetInstance()
    //    {
    //        return Instantiate(_prefab);
    //    }
    //}

    // GameObject factories
    public interface IInitializable
    {
        void Initialize(object initData);
    }

    // The main factory class
    public class GameObjectFactory
    {
        private GameObject prefab;

        public GameObjectFactory(GameObject prefab)
        {
            this.prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
        }

        public GameObject Create(Transform parent = null)
        {
            return GameObject.Instantiate(prefab, parent);
        }

        public T Create<T>(Transform parent = null, params object[] parameters) where T : Component
        {
            GameObject instance = Create(parent);
            T component = instance.GetComponent<T>();

            if (component == null)
            {
                UnityEngine.Object.Destroy(instance);
                throw new InvalidOperationException($"Prefab does not have component of type {typeof(T)}");
            }

            if (component is IInitializable initializable)
            {
                initializable.Initialize(parameters);
            }
            else if (parameters.Length > 0)
            {
                Debug.LogWarning($"Parameters provided, but {typeof(T).Name} does not implement IInitializable");
            }

            return component;
        }
    }
}