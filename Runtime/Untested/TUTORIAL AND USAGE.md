# Unity Game Components - Tutorial & Usage Guide

Welcome to this collection of ready-to-use Unity components! This package contains various scripts that will help you quickly implement common game mechanics without having to write everything from scratch. This guide will explain how to use each component, with examples and best practices.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Movement Components](#movement-components)
   - [BasicMovement2D](#basicmovement2d)
   - [PlatformerController](#platformercontroller)
   - [PhysicsBasedMovement](#physicsbasedmovement)
   - [MouseLookCamera](#mouselookcamera)
3. [Enemy & AI Components](#enemy--ai-components)
   - [PatrollingEnemy](#patrollingenemy)
   - [EnemyFollower](#enemyfollower)
   - [LineOfSightDetector](#lineofsightdetector)
   - [SimpleStateMachine](#simplestatemachine)
4. [Combat & Interaction Components](#combat--interaction-components)
   - [HealthSystem](#healthsystem)
   - [ObjectShooter](#objectshooter)
   - [HomingMissile](#homingmissile)
   - [CollectibleItem](#collectibleitem)
5. [Game Management Components](#game-management-components)
   - [ScoreManager](#scoremanager)
   - [TimerManager](#timermanager)
   - [TriggerZone](#triggerzone)
6. [Component Combinations](#component-combinations)
7. [Troubleshooting](#troubleshooting)

## Getting Started

### Installation

1. Import this package into your Unity project
2. All scripts are located in the `Runtime/Untested` folder
3. Drag and drop the scripts onto your GameObjects or add them via the Inspector

### General Usage Tips

- Each script has tooltips in the Inspector that explain what each setting does
- Most components will automatically add required dependencies if they're missing
- Use the Inspector to configure each component's properties
- Components marked "untested" work as designed but may need adjustments for your specific game

## Movement Components

### BasicMovement2D

This component provides simple 2D movement using WASD or arrow keys. It can be used for top-down or side-scrolling games.

**Usage Example:**
```csharp
// Add to your player GameObject
var movement = gameObject.AddComponent<BasicMovement2D>();
movement.MoveSpeed = 7f; // Make the player move faster
movement.UseTopDownMovement = true; // For top-down games
// UseTopDownMovement = false; // For side-scrollers
```

**Required Components:**
- Rigidbody2D (will be added automatically if missing)

**Key Properties:**
- `MoveSpeed`: How fast the character moves
- `UseTopDownMovement`: Set to true for top-down games, false for side-scrollers

**Best Practices:**
- For top-down games, set Rigidbody2D.gravityScale to 0
- For side-scrollers, leave gravity on and set UseTopDownMovement to false
- Add a Collider2D to the GameObject for proper physics interactions

### PlatformerController

A simple 2D platformer controller with jumping mechanics.

**Usage Example:**
```csharp
// Add to your player GameObject
var platformer = gameObject.AddComponent<PlatformerController>();
platformer.JumpForce = 12f; // Higher jumps
platformer.GroundLayer = LayerMask.GetMask("Ground"); // Set ground detection layer
```

**Required Components:**
- Rigidbody2D (will be added automatically if missing)
- Collider2D (add a BoxCollider2D or similar)

**Key Properties:**
- `MoveSpeed`: Horizontal movement speed
- `JumpForce`: Strength of jump
- `GroundLayer`: Layer mask for detecting the ground
- `GroundCheck`: Transform at the character's feet for ground detection

**Best Practices:**
- Create an empty child GameObject at the character's feet and assign it as the GroundCheck
- Set up a separate layer for ground objects and configure the GroundLayer property
- Make sure your ground objects have colliders

### PhysicsBasedMovement

Physics-based movement controller using forces, ideal for vehicles or objects that should follow realistic physics.

**Usage Example:**
```csharp
// Add to a car or boat GameObject
var physics = gameObject.AddComponent<PhysicsBasedMovement>();
physics.AccelerationForce = 15f; // Stronger acceleration
physics.BrakingDrag = 2f; // Quicker stopping
```

**Required Components:**
- Rigidbody (will be added automatically if missing)

**Key Properties:**
- `AccelerationForce`: Force applied when moving forward
- `TurnForce`: Force applied when turning
- `MaxSpeed`: Maximum speed the object can reach
- `BrakingDrag`: Drag applied when no input is detected

**Best Practices:**
- Adjust the Rigidbody mass to match the feel of your vehicle
- For heavy vehicles, increase mass and AccelerationForce
- For slippery surfaces, reduce the BrakingDrag value

### MouseLookCamera

First-person mouse look camera controller that handles mouse movement for looking around.

**Usage Example:**
```csharp
// Add to your camera or player GameObject
var mouseLook = gameObject.AddComponent<MouseLookCamera>();
mouseLook.MouseSensitivity = 1.5f; // Adjust sensitivity
mouseLook.InvertY = true; // Invert vertical looking
```

**Required Components:**
- Camera (can be on the same object or a child)

**Key Properties:**
- `MouseSensitivity`: How sensitive the camera is to mouse movement
- `InvertY`: Whether to invert the Y axis of the mouse
- `UpperLookLimit`: How far up the player can look (in degrees)
- `LowerLookLimit`: How far down the player can look (in degrees)

**Best Practices:**
- Attach to the player's parent object to handle horizontal rotation
- If attached to a parent, the camera should be a child handling only vertical rotation
- Use with a CharacterController or Rigidbody for complete first-person controls

## Enemy & AI Components

### PatrollingEnemy

Makes an object patrol between waypoints and detect players that enter its vision cone.

**Usage Example:**
```csharp
// Add to enemy GameObject
var patrol = gameObject.AddComponent<PatrollingEnemy>();
patrol.MoveSpeed = 2.5f; // Set patrol speed
patrol.ChaseSpeed = 4f; // Set chase speed when player detected
patrol.FieldOfViewAngle = 90f; // 90-degree vision cone
```

**Required Components:**
- Rigidbody or Rigidbody2D (for movement)
- Collider or Collider2D (for proper physics)

**Key Properties:**
- `Waypoints`: List of points to patrol between
- `FieldOfViewAngle`: How wide the enemy can see
- `ViewDistance`: How far the enemy can see
- `ChaseWhenDetected`: Should the enemy chase the player when detected

**Best Practices:**
- Create empty GameObjects as waypoints and assign them in the Inspector
- Set the TargetTag to "Player" to detect the player character
- Use the ShowVisionCone option to visualize the detection range in the editor

### EnemyFollower

A basic enemy that follows a target (typically the player), with options for chase and patrol behaviors.

**Usage Example:**
```csharp
// Add to enemy GameObject
var follower = gameObject.AddComponent<EnemyFollower>();
follower.TargetTag = "Player"; // Set target tag
follower.DetectionRange = 15f; // Detect from further away
follower.PatrolWhenIdle = true; // Patrol when player not in range
```

**Required Components:**
- Rigidbody or Rigidbody2D (for movement)

**Key Properties:**
- `TargetTag`: Tag to automatically find and follow (usually "Player")
- `ChaseSpeed`: How fast the enemy moves when chasing
- `DetectionRange`: Maximum distance to detect and follow the target
- `PatrolWhenIdle`: Should the enemy patrol when target is not in range

**Best Practices:**
- Use the OnTargetDetected event for triggering alerts or animations
- Set up the ObstacleLayers to prevent enemies from seeing through walls
- If using patrol, create patrol points or let the system generate them automatically

### LineOfSightDetector

Detects targets that enter a vision cone and provides events when targets are spotted or lost.

**Usage Example:**
```csharp
// Add to enemy or security camera GameObject
var sight = gameObject.AddComponent<LineOfSightDetector>();
sight.FieldOfViewAngle = 60f; // Narrow field of view
sight.ViewDistance = 20f; // Long-range detection
sight.TargetLayers = LayerMask.GetMask("Player"); // Only detect player layer
```

**Required Components:**
- None, but works well with PatrollingEnemy or EnemyFollower

**Key Properties:**
- `FieldOfViewAngle`: Field of view angle in degrees
- `ViewDistance`: How far the character can see
- `CheckForObstacles`: Should targets be detected behind obstacles
- `TargetTags`: Tags of objects to detect

**Best Practices:**
- Connect the OnTargetSpotted event to trigger alarms or enemy behavior changes
- Use different layers for players and NPCs to control what can be detected
- For stealth games, adjust the FieldOfViewAngle and ViewDistance for different enemy types

### SimpleStateMachine

Simple state machine for enemy AI with idle, patrol, chase, and attack states.

**Usage Example:**
```csharp
// Add to enemy GameObject
var stateMachine = gameObject.AddComponent<SimpleStateMachine>();
stateMachine.StartState = SimpleStateMachine.EnemyState.Patrol;
stateMachine.PatrolSpeed = 2f;
stateMachine.ChaseSpeed = 5f;
stateMachine.AttackRange = 2f;
```

**Required Components:**
- Rigidbody or Rigidbody2D (for movement)

**Key Properties:**
- `CurrentState`: Current state of the enemy
- `StartState`: Initial state to start in
- `DetectionRange`: How far the enemy can detect the target
- `PatrolPoints`: Points to patrol between
- `AttackRange`: How close to get before attacking

**Best Practices:**
- Connect to an Animator to trigger different animations for each state
- Use the OnStateChanged event to trigger effects or sounds
- Set up the PatrolPoints before play or use the auto-generation feature

## Combat & Interaction Components

### HealthSystem

Simple health system that handles health, damage, healing, and death events.

**Usage Example:**
```csharp
// Add to player or enemy GameObject
var health = gameObject.AddComponent<HealthSystem>();
health.MaxHealth = 100f;
health.InvulnerabilityTime = 1f; // 1 second of invulnerability after taking damage
```

**Usage in other scripts:**
```csharp
// To damage an entity
HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
if (enemyHealth != null) {
    enemyHealth.TakeDamage(25f); // Deal 25 damage
}

// To heal an entity
playerHealth.Heal(10f); // Restore 10 health
```

**Required Components:**
- None

**Key Properties:**
- `MaxHealth`: Maximum health amount
- `CurrentHealth`: Current health amount
- `DestroyOnDeath`: If true, object will be destroyed when health reaches 0
- `InvulnerabilityTime`: Time of invulnerability after taking damage

**Best Practices:**
- Connect the OnDamage event to trigger visual feedback
- Connect the OnDeath event to trigger death animations or game over states
- Use GetHealthPercentage() to display health bars

### ObjectShooter

Shoots prefabs from a specified position with configurable properties. Useful for guns, turrets, or projectile launchers.

**Usage Example:**
```csharp
// Add to weapon GameObject
var shooter = gameObject.AddComponent<ObjectShooter>();
shooter.ProjectilePrefab = bulletPrefab; // Assign bullet prefab
shooter.FirePoint = muzzleTransform; // Set where bullets come from
shooter.FireRate = 0.2f; // 5 shots per second
```

**Required Components:**
- None

**Key Properties:**
- `ProjectilePrefab`: Prefab to shoot
- `FirePoint`: Transform that defines where projectiles are spawned
- `ProjectileSpeed`: Speed of the projectile
- `FireRate`: Cooldown between shots in seconds
- `SpreadAngle`: Random spread angle (0 = perfectly straight)

**Best Practices:**
- Create an empty child GameObject as the FirePoint
- For automatic weapons, set AutoFire to true
- For shotguns, increase ProjectilesPerShot and set an appropriate SpreadPattern
- Use the MuzzleFlashEffect for visual feedback

### HomingMissile

Makes an object follow a target with configurable tracking behavior, useful for homing missiles or seeking projectiles.

**Usage Example:**
```csharp
// Add to missile prefab
var missile = gameObject.AddComponent<HomingMissile>();
missile.TargetTag = "Enemy"; // Target enemies
missile.MoveSpeed = 12f; // Fast missile
missile.TurnSpeed = 3f; // How quickly it adjusts course
missile.Lifetime = 5f; // Self-destruct after 5 seconds
```

**Required Components:**
- Rigidbody or Rigidbody2D (for movement)

**Key Properties:**
- `TargetTag`: Tag to use for auto-finding targets
- `SelectionMode`: How to select a target when multiple are available
- `MoveSpeed`: How fast the missile moves
- `TurnSpeed`: How quickly the missile can change direction
- `DestroyOnHit`: Should the missile destroy itself after hitting something

**Best Practices:**
- Add a TrailRenderer component for visual effect
- Set the ActivationDelay to create a "lock-on" time before tracking
- Use PingPongWaypoints to create a realistic missile that "searches" for targets
- Connect the OnHitTarget event to trigger explosions or damage

### CollectibleItem

Makes an object collectible by players or other specified objects. Perfect for coins, power-ups, or health pickups.

**Usage Example:**
```csharp
// Add to coin or power-up GameObject
var collectible = gameObject.AddComponent<CollectibleItem>();
collectible.Value = 10; // Worth 10 points
collectible.BobUpAndDown = true; // Animated movement
collectible.Rotate = true; // Constant rotation
```

**Required Components:**
- Collider or Collider2D set as a trigger

**Key Properties:**
- `CollectorTags`: Tags that can collect this item (leave empty for all tags)
- `Value`: Value of this collectible (for use with score systems)
- `DestroyOnCollect`: Should this object be destroyed when collected
- `CollectionEffect`: Effect to spawn when collected

**Best Practices:**
- Set the CollectorTags to "Player" to prevent enemies from collecting items
- Use the OnCollected event to trigger game-specific behaviors
- Add an AudioClip for CollectionSound to provide feedback
- Combine with ScoreManager to automatically add points when collected

## Game Management Components

### ScoreManager

Simple score manager to track and update a score value. Can be attached to players for individual scores or to a manager object for global score.

**Usage Example:**
```csharp
// Add to Game Manager or Player GameObject
var scoreManager = gameObject.AddComponent<ScoreManager>();
scoreManager.StartingScore = 0;
scoreManager.SaveHighScore = true; // Enable high score saving
```

**Usage in other scripts:**
```csharp
// To add points
scoreManager.AddScore(50); // Add 50 points

// To reset the score
scoreManager.ResetScore(); // Back to starting value
```

**Required Components:**
- None

**Key Properties:**
- `StartingScore`: Starting score value
- `CurrentScore`: Current score
- `HighScore`: Highest score achieved
- `ScoreMultiplier`: Current score multiplier
- `SaveHighScore`: Save high score to PlayerPrefs

**Best Practices:**
- Use a single ScoreManager on a persistent GameObject for global scores
- Connect OnScoreChanged to update UI elements
- Connect OnNewHighScore to trigger celebrations or notifications
- Set the HighScoreKey to a unique value if tracking multiple high scores

### TimerManager

Simple timer system that can count up or down, useful for creating time limits or tracking elapsed time.

**Usage Example:**
```csharp
// Add to Game Manager GameObject
var timer = gameObject.AddComponent<TimerManager>();
timer.CountDown = true; // Count down from Duration
timer.Duration = 180f; // 3 minute time limit
timer.DisplayFormat = TimerManager.TimeFormat.Standard; // MM:SS format
```

**Usage in other scripts:**
```csharp
// To pause the timer
timer.PauseTimer();

// To add time (e.g., time bonus)
timer.AddTime(30f); // Add 30 seconds
```

**Required Components:**
- None

**Key Properties:**
- `CountDown`: Should the timer count down (true) or up (false)
- `Duration`: Timer duration in seconds
- `StartAutomatically`: Should the timer start automatically on game start
- `Loop`: Should the timer loop when it finishes
- `DisplayFormat`: Format to display the time

**Best Practices:**
- Connect OnTimerFinish to trigger level completion or game over
- Connect OnFormattedTimeChanged to update UI elements
- Use AddTime() to implement time bonuses or penalties
- For speedrun mechanics, set CountDown to false and use OnTimeChanged

### TriggerZone

Simple trigger zone that detects when objects enter, stay in, or exit the zone. Great for checkpoints, area triggers, or interaction zones.

**Usage Example:**
```csharp
// Add to an empty GameObject with a Collider
var trigger = gameObject.AddComponent<TriggerZone>();
trigger.TargetTags.Add("Player"); // Only detect players
trigger.ActivationKey = KeyCode.E; // Optional interaction key
```

**Required Components:**
- Collider or Collider2D with "Is Trigger" set to true

**Key Properties:**
- `TargetLayers`: Layers that will trigger this zone
- `TargetTags`: Tags that will trigger this zone (leave empty for all tags)
- `ActivationKey`: Optional key that must be pressed while in trigger to activate
- `ActivateOncePerPress`: Should the activation key trigger only once or repeatedly while held

**Best Practices:**
- Use BoxCollider for rectangular areas or SphereCollider for circular areas
- Connect OnTriggerEnterEvent for immediate effects
- Connect OnActivationEvent for player-activated triggers
- For interaction prompts, use OnTriggerEnterEvent to show UI and OnTriggerExitEvent to hide it

## Component Combinations

Here are some useful combinations of components to create common gameplay mechanics:

### Player Character (Top-Down)
- BasicMovement2D (UseTopDownMovement = true)
- HealthSystem
- ScoreManager

### Player Character (Platformer)
- PlatformerController
- HealthSystem
- CollectibleItem detector script

### Enemy Patrol with Detection
- PatrollingEnemy
- HealthSystem
- ObjectShooter (to attack the player)

### Turret
- LineOfSightDetector
- ObjectShooter
- SimpleStateMachine (for different behaviors)

### Collectible Coins
- CollectibleItem
- (Add ScoreManager to player or game manager)

### Time Attack Level
- TimerManager (CountDown = true)
- ScoreManager (for score tracking)
- TriggerZone (for finish line)

## Troubleshooting

### Common Issues

1. **Object doesn't move:**
   - Check if Rigidbody/Rigidbody2D is present
   - Ensure no other scripts are controlling the same Rigidbody
   - Verify the object isn't constrained in the Rigidbody settings

2. **Collisions not working:**
   - Make sure both objects have Colliders
   - Check that one isn't set as a Trigger if you need physical collisions
   - Verify Physics/Physics2D layers and collision matrix

3. **Enemy doesn't detect player:**
   - Confirm the player has the correct tag (usually "Player")
   - Check the detection range and angle values
   - Ensure no obstacles are blocking the line of sight
   - Verify the ObstacleLayers are set correctly

4. **Components interfering with each other:**
   - Avoid multiple movement controllers on the same GameObject
   - Use GetComponent to reference other components instead of duplicating functionality

### Debug Tips

- Use the debug visualizations (like ShowVisionCone) to see what's happening
- Connect events to Debug.Log calls to track when they're triggered
- Use the Unity Profiler to identify performance issues
- Check the Console for any warning or error messages

### Need More Help?

If you're stuck with a specific component or have questions about implementation, consider:

1. Reviewing the tooltips and documentation for each component
2. Looking at the comments in the source code
3. Creating a minimal test scene to isolate the issue
4. Checking the Unity documentation for the underlying systems

---

Happy game development! These components should give you a solid foundation for creating many different types of games in Unity.
