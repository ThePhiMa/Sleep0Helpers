using UnityEngine;

public class SingletonServiceBehaviour : MonoBehaviour
{
    [Inject(LifetimeScope.Singleton)] public ISingletonService SingletonService;
}