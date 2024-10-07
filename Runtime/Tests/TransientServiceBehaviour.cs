using Sleep0.Logic;
using UnityEngine;

public class TransientServiceBehaviour : MonoBehaviour
{
    [Inject(LifetimeScope.Transient)] private ITransientService _transientService;

    public ITransientService TransientService
    {
        get
        {
            DependencyContainer.Instance.InjectDependencies(this);
            return _transientService;
        }
    }
}