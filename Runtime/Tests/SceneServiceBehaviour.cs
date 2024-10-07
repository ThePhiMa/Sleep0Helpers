using Sleep0.Logic;
using UnityEngine;

public class SceneServiceBehaviour : MonoBehaviour
{
    [Inject(LifetimeScope.PerScene)] public ISceneService SceneService;
}