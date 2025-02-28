# Sleep0Helpers

![License: Unlicense](https://img.shields.io/badge/license-Unlicense-blue.svg)

## Overview

Sleep0.Helpers is a modular framework designed to accelerate Unity development by providing common patterns, utilities, and extensions that can be reused across projects. This package includes tools for dependency injection, event management, PID controllers, managed behaviors, and more.

## Features

### Editor Extensions

- **Auto Compiler Toggler** - Enable/disable automatic compilation with a keyboard shortcut (Ctrl+Alt+A)
- **Inspector Extensions** - Transform manipulation tools (reset position, rotation, scale)
- **Toolbar Extensions** - Adds quick access buttons for common actions (Compile, Build, Project Settings)
- **Work Timer** - Tracks your work time with auto-start/stop capabilities based on editor focus

### Dependency Injection

A lightweight DI system with support for different lifetime scopes:
- Singleton
- Transient
- Per Scene

```csharp
// Register services
DependencyContainer.Instance.Register<ISingletonService>(() => new SingletonService());

// Inject dependencies with attributes
[Inject(LifetimeScope.Singleton)] 
public ISingletonService service;
```

### Event System

Decoupled messaging system to facilitate communication between components:

```csharp
// Define an event
public class GameStartedEvent : IGameEvent { }

// Subscribe to events
EventBus.Subscribe<GameStartedEvent>(OnGameStarted);

// Publish events
EventBus.Publish(new GameStartedEvent());
```

### Managed Behaviours

A framework for update management with execution order:

```csharp
public class MyUpdatableBehaviour : ManagedBehaviour, IManagedUpdatable
{
    public void ManagedUpdate()
    {
        // Guaranteed execution order
    }
}
```

### Math Extensions

- **PID Controllers** - Proportional-Integral-Derivative controllers for smooth motion control
- **Quaternion Extensions** - Advanced quaternion operations
- **Vector3 Extensions** - Additional vector operations
- **Bit Extensions** - Bit manipulation helpers

### Factory System

Factory pattern implementation for game object creation and pooling:

```csharp
// Register factory
FactoryManager.Instance.RegisterFactory<EnemyFactory>("Enemy", enemyPrefab);

// Create objects
GameObject enemy = FactoryManager.Instance.CreateObject("Enemy", transform);
```

### Camera Controller

First-person camera controller with input support:

```csharp
// Add to scene
var controller = Instantiate(fpsControllerPrefab);
```

## Installation

### Using Unity Package Manager (UPM)

1. Open the Package Manager window in Unity
2. Click the "+" button and select "Add package from git URL..."
3. Enter the repository URL: `https://github.com/yourusername/Sleep0.Helpers.git`

### Manual Installation

1. Download or clone this repository
2. Copy the contents to your Unity project's Assets folder

## Requirements

- Unity 2020.3 or newer
- .NET Standard 2.0

## Usage Examples

### Dependency Injection

```csharp
// In a startup script
void Start()
{
    DependencyContainer.Instance.Register<IGameService>(() => new GameService());
    DependencyContainer.Instance.Register<IPlayerService>(() => new PlayerService());
}

// In other scripts
public class Player : MonoBehaviour
{
    [Inject(LifetimeScope.Singleton)]
    private IGameService gameService;
    
    void Start()
    {
        DependencyContainer.Instance.InjectDependencies(this);
    }
}
```

### PID Controller

```csharp
// Create a PID Controller
PIDController controller = new PIDController(pidValues);

// Update in fixed update
void FixedUpdate()
{
    float currentVelocity = rigidbody.velocity.magnitude;
    float targetVelocity = 10f;
    
    float force = controller.Update(currentVelocity, targetVelocity, Time.fixedDeltaTime);
    rigidbody.AddForce(transform.forward * force);
}
```

## License

This project is licensed under the Unlicense. You are free to use, modify, and distribute the code as you see fit. Happy Coding! :)

## Contributions

Contributions are welcome! Feel free to submit pull requests or open issues to improve the package.

---

For more detailed information on each script and its usage, refer to the [repository files](https://github.com/ThePhiMa/Sleep0Helpers).
