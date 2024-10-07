using Sleep0.Logic;
using UnityEngine;

public class SingletonServiceBehaviour : MonoBehaviour
{
    [Inject(LifetimeScope.Singleton)] public ISingletonService SingletonService;
}