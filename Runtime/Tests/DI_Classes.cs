using UnityEngine;

public class SingletonService : ISingletonService
{
    public void DoSomething() => Debug.Log("Singleton service doing something");
}

public class TransientService : ITransientService
{
    public void DoSomething() => Debug.Log("Transient service doing something");
}

public class SceneService : ISceneService
{
    public void DoSomething() => Debug.Log("Scene service doing something");
}