# Sleep0Helpers

![License: Unlicense](https://img.shields.io/badge/license-Unlicense-blue.svg)

## Overview

Sleep0Helpers is a Unity package containing a collection of helper scripts designed to streamline game development. These utilities cover common operations, extension methods, and design patterns.

## Features

- **GenericFactory.cs**: A base class for creating GameObject instances with various parameters.
  - *GameObjectFactory*: An abstract class that serves as a base for creating GameObject instances with specific components.

- **MonobehaviourManager.cs**: Manages update cycles of objects implementing custom interfaces.
  - *MonobehaviourManager*: A singleton that manages the update cycle of objects implementing `IManagedUpdatable`, `IManagedLateUpdatable`, and `IManagedFixedUpdatable` interfaces.

- **SingletonMonoBehaviour.cs**: Ensures a single instance of MonoBehaviour types within the scene.
  - *SingletonMonoBehaviour<T>*: A generic abstract class ensuring only one instance of a MonoBehaviour of type `T` exists.

- **GameObjectExtensions.cs**: Provides extension methods for GameObjects.
  - *GameObjectExtensions*: Includes methods like `DestroySafe` for safely destroying GameObjects.

- **PIDController.cs**: Implements a Proportional-Integral-Derivative controller for GameObjects.
  - *PIDController*: A controller used for smooth and responsive control systems.

- **Array Extensions**: Adds utility methods for array manipulation, such as shuffling or finding the index of an element.

- **Bit Extensions**: Provides methods for bitwise operations, enhancing the readability and performance of bit manipulation tasks.

- **Math Extensions**: Includes additional mathematical functions and constants not available in Unity’s Mathf class, such as linear interpolation for various types.

- **Vector3 Extensions**: Adds convenience methods for Vector3 operations, like projecting vectors or calculating distances.

- **DebugHelper.cs**: Provides additional functionality for debugging, including logging with method, file, and line information.

## Installation

1. Open Unity Package Manager (Window -> Package Manager).
2. Click the "+" icon and select "Add package from git URL...".
3. Enter the repository URL: `https://github.com/ThePhiMa/Sleep0Helpers.git`.

## Usage

Refer to the individual script files for detailed usage instructions and examples. Each script includes comments and example code snippets to help you integrate it into your project.

## License

This project is licensed under the Unlicense. You are free to use, modify, and distribute the code as you see fit. Happy coding!

## Contributions

Contributions are welcome! Feel free to submit pull requests or open issues to improve the package.

---

For more detailed information on each script and its usage, refer to the [repository files](https://github.com/ThePhiMa/Sleep0Helpers).
