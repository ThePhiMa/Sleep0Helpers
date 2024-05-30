# com.sleep0.helpers

This package contains a collection of helper scripts I (try) to use for all my projects.

Note: This readme is partially generated via GitHUb Copilot, I still need to double- and tripple-check it. ¯\\_(ツ)_/¯

## Scripts

# GenericFactory.cs

A collection of generic factory classes used for creating instances of GameObjects with various parameters.

## GameObjectFactory<T>

**File:** `GenericFactory.cs`

**Description:** This is an abstract class that serves as a base for creating instances of GameObjects. It requires a type parameter `T` which should be a Component. The class has a protected GameObject `_prefab` which is the prefab to be instantiated. The `GetObject` method is used to create an instance of the prefab. The `GetComponenOfType` method is used to get a component of type `T` from a GameObject.

**Usage:** This class is used by creating a subclass and implementing the abstract methods. The subclass can then be used to create instances of GameObjects with a specific component.

## MonobehaviourManager.cs, ManagedBehaviour.cs, ParameteredManagedBehaviour.cs

# MonobehaviourManager.cs

This file contains the `MonobehaviourManager` class, which is a singleton that manages the update cycle of objects implementing the `IManagedUpdatable`, `IManagedLateUpdatable`, and `IManagedFixedUpdatable` interfaces.
The purpose is to pool the use of Update(), FixedUpdate() and LateUpdate() so to avoid too many unmanaged->managed calls.
What Unity basically does, it checking what class implements on of those methods and than does a unmanaged->managed call each time. Normally, this won't impact performance much, but with a lot of objects (and without using DOTS/ECS), why not optimize the performance a tiny bit? :)
(Check this out: https://blog.unity.com/engine-platform/10000-update-calls)

## MonobehaviourManager

**File:** `MonobehaviourManager.cs`

**Description:** The `MonobehaviourManager` class is responsible for managing the update cycle of objects. It maintains lists of objects that implement the `IManagedUpdatable`, `IManagedLateUpdatable`, and `IManagedFixedUpdatable` interfaces and calls their respective update methods in the `Update`, `LateUpdate`, and `FixedUpdate` methods. It also provides methods to register and unregister objects, as well as to clear all registered objects.

**Usage:** An object that needs to be managed by the `MonobehaviourManager` should implement one of the managed updatable interfaces and register itself with the manager.

# SingletonMonoBehaviour.cs

This file contains the `SingletonMonoBehaviour<T>` class, which is an abstract class that other MonoBehaviour classes can inherit from to become singletons.

## SingletonMonoBehaviour<T>

**File:** `SingletonMonoBehaviour.cs`

**Description:** The `SingletonMonoBehaviour<T>` class is a generic abstract class that ensures only one instance of a MonoBehaviour of type `T` exists. It provides a static `Instance` property to access the singleton instance. In its `Awake` method, it checks if an instance already exists and destroys the new one if it does, ensuring that only one instance can exist.

**Usage:** To use this class, create a MonoBehaviour that inherits from `SingletonMonoBehaviour<T>`, replacing `T` with the class name.

# GameObjectExtensions.cs

This file contains the `GameObjectExtensions` static class, which provides extension methods for the `GameObject` class in Unity.

## GameObjectExtensions

**File:** `GameObjectExtensions.cs`

**Description:** The `GameObjectExtensions` class provides a `DestroySafe` extension method for the `GameObject` class. This method checks if the application is playing before deciding whether to use `GameObject.Destroy` or `GameObject.DestroyImmediate` to destroy the GameObject.

# PIDController.cs in com.sleep0.helpers

This file contains the `PIDController` class, which is a Proportional-Integral-Derivative (PID) controller used for controlling game objects in Unity.

## PIDController

**File:** `PIDController.cs`

**Description:** The `PIDController` class is a PID controller that can be used for controlling the position and orientation of game objects. It calculates the error between a setpoint and the actual value and uses this to calculate the output that should be applied to the game object to minimize the error. The controller uses the Ziegler-Nichols method to calculate the PID gains.

**Usage:** To use this class, create an instance of `PIDController` and call the `Update` method in your game object's update loop, passing in the setpoint, the actual value, and the time delta (fixed time delta if used for physics!).

## Extension Methods

This package includes several extension method classes that add useful functionality to existing C# and Unity types.

### ArrayExtensions.cs

This file contains extension methods for arrays. These methods provide additional functionality for manipulating and querying arrays, such as methods for finding the index of an element, checking if an array is empty, and more.

### BitExtensions.cs

This file contains extension methods for working with bits and bitwise operations. These methods provide additional functionality for manipulating and querying bits, such as methods for setting and clearing specific bits, checking if a specific bit is set, and more.

### MathExtensions.cs

This file contains extension methods for mathematical operations. These methods provide additional functionality for mathematical operations, such as methods for clamping values within a range, rounding to the nearest multiple of a number, and more.

### Vector3Extensions.cs

This file contains extension methods for the `Vector3` class in Unity. These methods provide additional functionality for manipulating and querying `Vector3` instances, such as methods for calculating the distance between two points, checking if a point is within a certain distance of another, and more.

### DebugHelper.cs

This file contains the `DebugHelper` class. This class provides additional functionality for debugging your application. It includes methods for logging messages, warnings, and errors with additional information such as the method name, file name, and line number where the log was called. This can be extremely useful for tracking down issues in your code. It may also contain methods for drawing debug shapes and lines in the Unity scene view, which can help visualize what's happening in your game.


...

## Installation

- Open the Unity Package Manager (Window->Package Manager)
- Click on the + symbol on the top left
- Click "Install package from git URL..."
- Add this repos git url: https://github.com/ThePhiMa/Sleep0Helpers.git

## License

Yeah, I don't really care. You are free to do whatever you want with this code. Have fun! :)
