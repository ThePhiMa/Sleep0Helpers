using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Shoots prefabs from a specified position with configurable properties.
/// Can be used for enemies shooting projectiles, player weapons, or environmental hazards.
/// </summary>
public class ObjectShooter : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("Prefab to shoot")]
    public GameObject ProjectilePrefab;
    
    [Tooltip("Transform that defines where projectiles are spawned")]
    public Transform FirePoint;
    
    [Tooltip("Speed of the projectile")]
    public float ProjectileSpeed = 10f;
    
    [Tooltip("Time in seconds before projectile is destroyed (0 = never)")]
    public float ProjectileLifetime = 5f;
    
    [Header("Firing Settings")]
    [Tooltip("Can fire automatically without calling Fire()")]
    public bool AutoFire = false;
    
    [Tooltip("Cooldown between shots in seconds")]
    public float FireRate = 0.5f;
    
    [Tooltip("Random variation in fire rate (0 = consistent timing)")]
    public float FireRateVariation = 0.1f;
    
    [Tooltip("Maximum number of projectiles (0 = unlimited)")]
    public int MaxProjectiles = 0;
    
    [Tooltip("Should the projectile inherit velocity from parent?")]
    public bool InheritVelocity = false;
    
    [Header("Spread Settings")]
    [Tooltip("Random spread angle (0 = perfectly straight)")]
    public float SpreadAngle = 5f;
    
    [Tooltip("Number of projectiles to fire at once")]
    public int ProjectilesPerShot = 1;
    
    [Tooltip("Spread pattern for multiple projectiles")]
    public SpreadPattern MultipleProjectilePattern = SpreadPattern.Random;
    
    [Header("Aiming")]
    [Tooltip("Should the shooter automatically aim at a target?")]
    public bool AutoAim = false;
    
    [Tooltip("Target to aim at (if null, will find by tag)")]
    public Transform TargetTransform;
    
    [Tooltip("Tag to automatically find targets")]
    public string TargetTag = "Player";
    
    [Tooltip("Maximum angle to auto-aim (0 = no limit)")]
    public float MaxAimAngle = 45f;
    
    [Tooltip("Lead the target's movement (0 = no leading, 1 = perfect leading)")]
    [Range(0f, 1f)]
    public float LeadTargetAmount = 0f;
    
    [Header("Audio & VFX")]
    [Tooltip("Sound to play when firing")]
    public AudioClip FireSound;
    
    [Tooltip("Volume of the fire sound")]
    [Range(0f, 1f)]
    public float FireSoundVolume = 1f;
    
    [Tooltip("Visual effect to spawn when firing")]
    public GameObject MuzzleFlashEffect;
    
    [Tooltip("Duration of muzzle flash effect")]
    public float MuzzleFlashDuration = 0.1f;
    
    [Header("Events")]
    [Tooltip("Event triggered when firing")]
    public UnityEvent OnFire;
    
    [Tooltip("Event triggered when out of projectiles")]
    public UnityEvent OnOutOfProjectiles;
    
    // Enum for spread patterns
    public enum SpreadPattern
    {
        Random,
        Even,
        Circular
    }
    
    // Private variables
    private float _nextFireTime = 0f;
    private int _projectilesRemaining;
    private Rigidbody _rb;
    private Rigidbody2D _rb2D;
    private AudioSource _audioSource;
    private GameObject _currentMuzzleFlash;
    private bool _is2D;
    
    // Start is called before the first frame update
    private void Start()
    {
        // If no fire point specified, use self
        if (FirePoint == null)
        {
            FirePoint = transform;
        }
        
        // Set initial projectile count
        _projectilesRemaining = MaxProjectiles;
        
        // Get components
        _rb = GetComponent<Rigidbody>();
        _rb2D = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        
        // Check if this is a 2D project
        _is2D = _rb2D != null;
        
        // Find target by tag if auto-aim is enabled
        if (AutoAim && TargetTransform == null && !string.IsNullOrEmpty(TargetTag))
        {
            FindTarget();
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Auto fire if enabled
        if (AutoFire && Time.time >= _nextFireTime)
        {
            Fire();
        }
        
        // Find target if needed for auto-aim
        if (AutoAim && TargetTransform == null && Time.frameCount % 30 == 0)
        {
            FindTarget();
        }
    }
    
    /// <summary>
    /// Fire a projectile
    /// </summary>
    /// <returns>True if fired successfully, false otherwise</returns>
    public bool Fire()
    {
        // Check fire rate cooldown
        if (Time.time < _nextFireTime)
            return false;
            
        // Check if we have projectiles left
        if (MaxProjectiles > 0)
        {
            if (_projectilesRemaining <= 0)
            {
                OnOutOfProjectiles?.Invoke();
                return false;
            }
            
            _projectilesRemaining--;
        }
        
        // Set next fire time with optional variation
        float variation = Random.Range(-FireRateVariation, FireRateVariation);
        _nextFireTime = Time.time + Mathf.Max(0.01f, FireRate + variation);
        
        // Fire the projectiles
        for (int i = 0; i < ProjectilesPerShot; i++)
        {
            FireProjectile(i);
        }
        
        // Play fire sound if set
        if (FireSound != null)
        {
            if (_audioSource != null)
            {
                _audioSource.PlayOneShot(FireSound, FireSoundVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(FireSound, FirePoint.position, FireSoundVolume);
            }
        }
        
        // Show muzzle flash effect if set
        if (MuzzleFlashEffect != null)
        {
            ShowMuzzleFlash();
        }
        
        // Trigger the fire event
        OnFire?.Invoke();
        
        return true;
    }
    
    // Fire an individual projectile
    private void FireProjectile(int projectileIndex)
    {
        // Make sure we have a prefab
        if (ProjectilePrefab == null)
        {
            Debug.LogError("ObjectShooter: No ProjectilePrefab assigned to " + gameObject.name);
            return;
        }
        
        // Calculate firing direction
        Vector3 direction = CalculateFireDirection(projectileIndex);
        
        // Instantiate the projectile
        GameObject projectile = Instantiate(ProjectilePrefab, FirePoint.position, Quaternion.identity);
        
        // Face the correct direction
        if (_is2D)
        {
            // For 2D, use the Z angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            // For 3D, rotate to face direction
            projectile.transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Apply velocity to the projectile
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        Rigidbody2D projectileRb2D = projectile.GetComponent<Rigidbody2D>();
        
        // Handle 2D projectiles
        if (projectileRb2D != null)
        {
            // Calculate velocity
            Vector2 velocity = new Vector2(direction.x, direction.y) * ProjectileSpeed;
            
            // Add parent velocity if needed
            if (InheritVelocity && _rb2D != null)
            {
                velocity += _rb2D.linearVelocity;
            }
            
            projectileRb2D.linearVelocity = velocity;
        }
        // Handle 3D projectiles
        else if (projectileRb != null)
        {
            // Calculate velocity
            Vector3 velocity = direction * ProjectileSpeed;
            
            // Add parent velocity if needed
            if (InheritVelocity && _rb != null)
            {
                velocity += _rb.linearVelocity;
            }
            
            projectileRb.linearVelocity = velocity;
        }
        // Fallback for objects without rigidbody
        else
        {
            // Add a simple mover script if needed
            ProjectileMover mover = projectile.GetComponent<ProjectileMover>();
            if (mover == null)
            {
                mover = projectile.AddComponent<ProjectileMover>();
            }
            
            mover.Direction = direction;
            mover.Speed = ProjectileSpeed;
        }
        
        // Set lifetime
        if (ProjectileLifetime > 0)
        {
            Destroy(projectile, ProjectileLifetime);
        }
    }
    
    // Calculate the direction to fire based on settings
    private Vector3 CalculateFireDirection(int projectileIndex)
    {
        Vector3 baseDirection;
        
        // Determine base firing direction
        if (AutoAim && TargetTransform != null)
        {
            // Calculate direction to target
            baseDirection = CalculateAimDirection();
            
            // Check if target is within aiming angle
            if (MaxAimAngle > 0)
            {
                float angle = Vector3.Angle(FirePoint.forward, baseDirection);
                if (angle > MaxAimAngle)
                {
                    // Target is outside aiming angle, use forward direction
                    baseDirection = FirePoint.forward;
                }
            }
        }
        else
        {
            // Use forward direction of fire point
            baseDirection = FirePoint.forward;
        }
        
        // Apply spread based on pattern
        Vector3 spreadDirection = baseDirection;
        
        if (ProjectilesPerShot > 1)
        {
            switch (MultipleProjectilePattern)
            {
                case SpreadPattern.Random:
                    // Random deviation within spread angle
                    spreadDirection = ApplyRandomSpread(baseDirection);
                    break;
                    
                case SpreadPattern.Even:
                    // Evenly distribute projectiles in a line
                    float angleStep = SpreadAngle / Mathf.Max(1, ProjectilesPerShot - 1);
                    float currentAngle = -SpreadAngle / 2 + angleStep * projectileIndex;
                    spreadDirection = ApplyEvenSpread(baseDirection, currentAngle);
                    break;
                    
                case SpreadPattern.Circular:
                    // Distribute in a circle
                    float circleAngle = 360f / ProjectilesPerShot * projectileIndex;
                    spreadDirection = ApplyCircularSpread(baseDirection, circleAngle);
                    break;
            }
        }
        else if (SpreadAngle > 0)
        {
            // Single projectile with random spread
            spreadDirection = ApplyRandomSpread(baseDirection);
        }
        
        return spreadDirection.normalized;
    }
    
    // Apply random deviation within the spread angle
    private Vector3 ApplyRandomSpread(Vector3 direction)
    {
        if (SpreadAngle <= 0)
            return direction;
            
        // Apply random deviation
        float randomAngle = Random.Range(-SpreadAngle, SpreadAngle);
        float randomAngle2 = Random.Range(-SpreadAngle, SpreadAngle);
        
        // Apply the spread
        Quaternion rotation = Quaternion.Euler(randomAngle2, randomAngle, 0);
        return rotation * direction;
    }
    
    // Apply even spread for multiple projectiles
    private Vector3 ApplyEvenSpread(Vector3 direction, float angle)
    {
        if (_is2D)
        {
            // For 2D, rotate around Z axis
            return Quaternion.Euler(0, 0, angle) * direction;
        }
        else
        {
            // For 3D, rotate around Y axis (horizontal spread)
            return Quaternion.Euler(0, angle, 0) * direction;
        }
    }
    
    // Apply circular spread for multiple projectiles
    private Vector3 ApplyCircularSpread(Vector3 direction, float angle)
    {
        // Create a rotation perpendicular to the firing direction
        Quaternion baseRotation = Quaternion.LookRotation(direction);
        Quaternion spreadRotation = Quaternion.Euler(
            SpreadAngle * Mathf.Sin(angle * Mathf.Deg2Rad),
            SpreadAngle * Mathf.Cos(angle * Mathf.Deg2Rad),
            0
        );
        
        return baseRotation * spreadRotation * Vector3.forward;
    }
    
    // Calculate direction for auto-aiming
    private Vector3 CalculateAimDirection()
    {
        if (TargetTransform == null)
            return FirePoint.forward;
            
        Vector3 targetPosition = TargetTransform.position;
        
        // Lead the target if specified
        if (LeadTargetAmount > 0)
        {
            // Try to predict where the target will be
            Rigidbody targetRb = TargetTransform.GetComponent<Rigidbody>();
            Rigidbody2D targetRb2D = TargetTransform.GetComponent<Rigidbody2D>();
            
            Vector3 targetVelocity = Vector3.zero;
            
            if (targetRb != null)
            {
                targetVelocity = targetRb.linearVelocity;
            }
            else if (targetRb2D != null)
            {
                targetVelocity = new Vector3(targetRb2D.linearVelocity.x, targetRb2D.linearVelocity.y, 0);
            }
            
            // Calculate time for projectile to reach target
            float distance = Vector3.Distance(FirePoint.position, targetPosition);
            float timeToTarget = distance / ProjectileSpeed;
            
            // Apply leading based on target velocity
            targetPosition += targetVelocity * timeToTarget * LeadTargetAmount;
        }
        
        // Calculate direction to target
        Vector3 direction = targetPosition - FirePoint.position;
        
        return direction.normalized;
    }
    
    // Find a target with the specified tag
    private void FindTarget()
    {
        if (string.IsNullOrEmpty(TargetTag))
            return;
            
        GameObject targetObj = GameObject.FindGameObjectWithTag(TargetTag);
        if (targetObj != null)
        {
            TargetTransform = targetObj.transform;
        }
    }
    
    // Show muzzle flash effect
    private void ShowMuzzleFlash()
    {
        // Remove existing muzzle flash
        if (_currentMuzzleFlash != null)
        {
            Destroy(_currentMuzzleFlash);
        }
        
        // Create new muzzle flash
        _currentMuzzleFlash = Instantiate(MuzzleFlashEffect, FirePoint.position, FirePoint.rotation, FirePoint);
        
        // Destroy after duration
        Destroy(_currentMuzzleFlash, MuzzleFlashDuration);
    }
    
    /// <summary>
    /// Add more projectiles to the available count
    /// </summary>
    /// <param name="amount">Number of projectiles to add</param>
    public void AddProjectiles(int amount)
    {
        if (MaxProjectiles > 0)
        {
            _projectilesRemaining += amount;
            
            // Cap at max projectiles
            if (_projectilesRemaining > MaxProjectiles)
            {
                _projectilesRemaining = MaxProjectiles;
            }
        }
    }
    
    /// <summary>
    /// Set the firing direction to point at a specific position
    /// </summary>
    /// <param name="targetPosition">World position to aim at</param>
    public void AimAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - FirePoint.position).normalized;
        
        if (_is2D)
        {
            // For 2D, rotate around Z axis
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            FirePoint.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            // For 3D, use LookRotation
            FirePoint.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    /// <summary>
    /// Get the number of projectiles remaining (if limited)
    /// </summary>
    /// <returns>Number of projectiles remaining, -1 if unlimited</returns>
    public int GetProjectilesRemaining()
    {
        return MaxProjectiles > 0 ? _projectilesRemaining : -1;
    }
}

/// <summary>
/// Simple component to move a projectile in a straight line
/// Added automatically to projectiles without Rigidbody components
/// </summary>
public class ProjectileMover : MonoBehaviour
{
    public Vector3 Direction = Vector3.forward;
    public float Speed = 10f;
    
    private void Update()
    {
        transform.position += Direction * Speed * Time.deltaTime;
    }
}
