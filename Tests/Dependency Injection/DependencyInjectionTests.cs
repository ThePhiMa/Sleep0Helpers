using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class DependencyInjectionTests
{
    private DependencyContainer container;

    [SetUp]
    public void Setup()
    {
        container = DependencyContainer.Instance;
        // Clear any existing registrations
        typeof(DependencyContainer).GetField("_singletons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(container, new System.Collections.Generic.Dictionary<System.Type, object>());
        typeof(DependencyContainer).GetField("_factories", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(container, new System.Collections.Generic.Dictionary<System.Type, System.Func<object>>());
        typeof(DependencyContainer).GetField("_sceneScoped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(container, new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<System.Type, object>>());
    }

    [Test]
    public void SingletonScope_ReturnsSameInstance()
    {
        Assert.IsNotNull(container);

        container.Register<ISingletonService>(() => new SingletonService());

        var testObject = new GameObject().AddComponent<SingletonServiceBehaviour>();
        container.InjectDependencies(testObject);

        var instance1 = testObject.SingletonService;
        var instance2 = testObject.SingletonService;

        Assert.IsNotNull(instance1);
        Assert.IsNotNull(instance2);
        Assert.AreEqual(instance1, instance2);

        instance1.DoSomething();
        instance2.DoSomething();
    }

    [Inject]
    public TransientService TransientService;

    [Test]
    public void TransientScope_ReturnsExistingInstance()
    {
        Assert.IsNotNull(container);

        var transientService2 = new TransientService();

        container.Register<TransientService>(() => transientService2);

        container.InjectDependencies(this);

        var instance = this.TransientService;

        Assert.IsNotNull(instance);

        instance.DoSomething();
    }

    [Test]
    public void TransientScope_ReturnsNewInstance()
    {
        Assert.IsNotNull(container);

        container.Register<ITransientService>(() => new TransientService());

        var testObject = new GameObject().AddComponent<TransientServiceBehaviour>();
        container.InjectDependencies(testObject);

        var instance1 = testObject.TransientService;
        var instance2 = testObject.TransientService;

        Assert.IsNotNull(instance1);
        Assert.IsNotNull(instance2);
        Assert.AreNotEqual(instance1, instance2);

        instance1.DoSomething();
        instance2.DoSomething();
    }

    [UnityTest]
    public IEnumerator PerSceneScope_ReturnsSameInstanceWithinScene()
    {
        Assert.IsNotNull(container);

        container.Register<ISceneService>(() => new SceneService());

        // Load a test scene
        yield return SceneManager.LoadSceneAsync("TestScene1", LoadSceneMode.Additive);
        Scene testScene = SceneManager.GetSceneByName("TestScene1");
        SceneManager.SetActiveScene(testScene);

        var testObject1 = new GameObject().AddComponent<SceneServiceBehaviour>();
        var testObject2 = new GameObject().AddComponent<SceneServiceBehaviour>();

        container.InjectDependencies(testObject1);
        container.InjectDependencies(testObject2);

        var instance1 = testObject1.SceneService;
        var instance2 = testObject2.SceneService;

        Assert.IsNotNull(instance1);
        Assert.IsNotNull(instance2);
        Assert.AreEqual(instance1, instance2);

        instance1.DoSomething();
        instance2.DoSomething();

        // Clean up
        yield return SceneManager.UnloadSceneAsync(testScene);
    }

    [UnityTest]
    public IEnumerator PerSceneScope_ReturnsDifferentInstancesBetweenScenes()
    {
        Assert.IsNotNull(container);

        container.Register<ISceneService>(() => new SceneService());

        // Load first test scene
        yield return SceneManager.LoadSceneAsync("TestScene1", LoadSceneMode.Additive);
        Scene testScene1 = SceneManager.GetSceneByName("TestScene1");
        SceneManager.SetActiveScene(testScene1);

        var testObject1 = new GameObject().AddComponent<SceneServiceBehaviour>();
        container.InjectDependencies(testObject1);
        var instance1 = testObject1.SceneService;

        // Load second test scene
        yield return SceneManager.LoadSceneAsync("TestScene2", LoadSceneMode.Additive);
        Scene testScene2 = SceneManager.GetSceneByName("TestScene2");
        SceneManager.SetActiveScene(testScene2);

        var testObject2 = new GameObject().AddComponent<SceneServiceBehaviour>();
        container.InjectDependencies(testObject2);
        var instance2 = testObject2.SceneService;

        Assert.IsNotNull(instance1);
        Assert.IsNotNull(instance2);
        Assert.AreNotEqual(instance1, instance2);

        instance1.DoSomething();
        instance2.DoSomething();

        // Clean up
        yield return SceneManager.UnloadSceneAsync(testScene1);
        yield return SceneManager.UnloadSceneAsync(testScene2);
    }
}