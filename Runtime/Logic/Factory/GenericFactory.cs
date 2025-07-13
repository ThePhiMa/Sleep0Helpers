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

    //public class GenericFactory<T, U, V> : GenericFactory<T>
    //    where T : MonoBehaviour
    //    where U : ParameteredMonobehaviour<V> 
    //{
    //    public GenericFactory(T prefab) : base(prefab)
    //    {
    //    }

    //    public T GetInstance(V parameter)
    //    {
    //        T result = Instantiate(_prefab);
    //        result.GetComponent<U>().OnCreate(parameter);
    //        return result;
    //    }
    //}

    //public class GenericFactory<T, U, V, W> : GenericFactory<T>
    //   where T : MonoBehaviour
    //   where U : ParameteredMonobehaviour<V, W>
    //{
    //    public GenericFactory(T prefab) : base(prefab)
    //    {
    //    }

    //    public T GetInstance(V parameter1, W parameter2)
    //    {
    //        T result = Instantiate(_prefab);
    //        result.GetComponent<U>().OnCreate(parameter1, parameter2);
    //        return result;
    //    }
    //}

    //public class GenericFactory<T, U, V, W, X> : GenericFactory<T>
    //   where T : MonoBehaviour
    //   where U : ParameteredMonobehaviour<V, W, X>
    //{
    //    public GenericFactory(T prefab) : base(prefab)
    //    {
    //    }

    //    public T GetInstance(V parameter1, W parameter2, X parameter3)
    //    {
    //        T result = Instantiate(_prefab);
    //        result.GetComponent<U>().OnCreate(parameter1, parameter2, parameter3);
    //        return result;
    //    }
    //}

    //public class GenericFactory<T, U, V, W, X, Y> : GenericFactory<T>
    //   where T : MonoBehaviour
    //   where U : ParameteredMonobehaviour<V, W, X, Y>
    //{
    //    public GenericFactory(T prefab) : base(prefab)
    //    {
    //    }

    //    public T GetInstance(V parameter1, W parameter2, X parameter3, Y parameter4)
    //    {
    //        T result = Instantiate(_prefab);
    //        result.GetComponent<U>().OnCreate(parameter1, parameter2, parameter3, parameter4);
    //        return result;
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

        public GameObjectFactory() { }

        public void Initialize(GameObject prefab)
        {
            this.prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
        }

        public GameObject Create(Transform parent = null)
        {
            if (prefab == null)
                throw new ArgumentNullException("GameObjectFactory does not have a prefab set.");

            return GameObject.Instantiate(prefab, parent);
        }

        public T Create<T>(Transform parent = null, params object[] parameters) where T : Component
        {
            if (prefab == null)
                throw new ArgumentNullException("GameObjectFactory does not have a prefab set.");

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

    public abstract class GameObjectFactory<T>
         where T : Component
    {
        [SerializeField]
        protected GameObject _prefab;

        public GameObjectFactory(GameObject prefab)
        {
            _prefab = prefab;
        }

        public virtual GameObject GetObject()
        {
            return MonoBehaviour.Instantiate(_prefab);
        }

        protected virtual T GetComponenOfType(GameObject parent)
        {
            // ToDo: Should I just add the component if there is non? Why not? Why should I? Questions over questions.
            T component = parent.GetComponent<T>();
            if (component == null)
                DebugHelper.LogError(parent, $"Gameobject {parent.name} has no component of type {typeof(T).Name}");
            return component;
        }
    }

    public abstract class GameObjectFactory<T, U> : GameObjectFactory<T>
        where T : ParameteredManagedBehaviour<U>
    {
        public GameObjectFactory(GameObject prefab) : base(prefab)
        {
        }

        public (GameObject, T) GetObjects(U parameter)
        {
            GameObject result = GameObject.Instantiate(_prefab);
            T component = GetComponenOfType(result);
            component?.OnCreate(parameter);
            return (result, component);
        }
    }

    public abstract class GameObjectFactory<T, U, V> : GameObjectFactory<T>
        where T : ParameteredManagedBehaviour<U, V>
    {
        public GameObjectFactory(GameObject prefab) : base(prefab)
        {
        }

        public (GameObject, T) GetObjects(U parameter1, V parameter2)
        {
            GameObject result = GameObject.Instantiate(_prefab);
            T component = GetComponenOfType(result);
            component?.OnCreate(parameter1, parameter2);
            return (result, component);
        }
    }

    public abstract class GameObjectFactory<T, U, V, W> : GameObjectFactory<T>
        where T : ParameteredManagedBehaviour<U, V, W>
    {
        public GameObjectFactory(GameObject prefab) : base(prefab)
        {
        }

        public (GameObject, T) GetObjects(U parameter1, V parameter2, W parameter3)
        {
            GameObject result = GameObject.Instantiate(_prefab);
            T component = GetComponenOfType(result);
            component?.OnCreate(parameter1, parameter2, parameter3);
            return (result, component);
        }
    }

    public abstract class GameObjectFactory<T, U, V, W, X> : GameObjectFactory<T>
        where T : ParameteredManagedBehaviour<U, V, W, X>
    {
        public GameObjectFactory(GameObject prefab) : base(prefab)
        {
        }

        public (GameObject, T) GetObjects(U parameter1, V parameter2, W parameter3, X parameter4)
        {
            GameObject result = GameObject.Instantiate(_prefab);
            T component = GetComponenOfType(result);
            component?.OnCreate(parameter1, parameter2, parameter3, parameter4);
            return (result, component);
        }
    }
}