// Test behaviour for injection
using UnityEngine;

public class TestBehaviour : MonoBehaviour
{
    [Inject(LifetimeScope.Singleton)] public ISingletonService SingletonService;
    [Inject(LifetimeScope.Transient)] public ITransientService TransientService;
    [Inject(LifetimeScope.PerScene)] public ISceneService SceneService;
}