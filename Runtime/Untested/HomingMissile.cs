using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Makes an object follow a target with configurable tracking behavior.
/// Useful for homing missiles, seeking enemies, or guided projectiles.
/// </summary>
public class HomingMissile : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Transform to follow (if null, can find by tag)")]
    public Transform TargetTransform;
    
    [Tooltip("Tag to use for auto-finding targets")]
    public string TargetTag = "Player";
    
    [Tooltip("How to select a target when multiple are available")]
    public TargetSelectionMode SelectionMode = TargetSelectionMode.Nearest;
    
    [Tooltip("Time before starting to follow target (seconds)")]
    public float ActivationDelay = 0.5f;
    
    [Tooltip("Maximum time to track target (0 = unlimited)")]
    public float MaxTrackingTime = 5f;
    
    [Header("Movement")]
    [Tooltip("How fast the missile moves")]
    public float MoveSpeed = 10f;
    
    [Tooltip("How quickly the missile accelerates to max speed")]
    public float Acceleration = 15f;
    
    [Tooltip("How quickly the missile can change direction")]
    public float TurnSpeed = 5f;
    
    [Tooltip("Random offset to apply to the target position")]
    public float Wobble = 0f;
    
    [Tooltip("Should the missile maintain a constant height")]
    public bool LockYAxis = false;
    
    [Header("Approach Settings")]
    [Tooltip("Should the missile slow down when approaching the target")]
    public bool SlowDownOnApproach = false;
    
    [Tooltip("Distance at which to start slowing down")]
    public float SlowdownDistance = 3f;
    
    [Tooltip("Minimum speed when slowing down")]
    public float MinSpeed = 2f;
    
    [Header("Lifespan")]
    [Tooltip("Should the missile destroy itself after hitting something")]
    public bool DestroyOnHit = true;
    
    [Tooltip("Time before the missile self-destructs (0 = never)")]
    public float Lifetime = 10f;
    
    [Tooltip("Effect to spawn when destroyed")]
    public GameObject DestructionEffect;
    
    [Header("Events")]
    [Tooltip("Event triggered when hitting a target")]
    public UnityEvent<GameObject> OnHitTarget;
    
    [Tooltip("Event triggered when the missile self-destructs")]
    public UnityEvent OnSelfDestruct;
    
    // Target selection modes
    public enum TargetSelectionMode
    {
        Nearest,
        Random,
        StrongestSignal,  // Most directly in front of the missile
        First             // First found (usually closest to origin)
    }
    
    // Private variables
    private Rigidbody _rb;
    private Rigidbody2D _rb2D;
    private bool _is2D;
    private float _currentSpeed = 0f;
    private bool _isActive = false;
    private float _activationTimer = 0f;
    private float _lifeTimer = 0f;
    private Vector3 _targetPosition;
    private float _wobbleTimer = 0f;
    private Vector3 _lastPosition;
    private TrailRenderer _trail;
    
    // Start is called before the first frame update
    private void Start()
    {
        // Get components
        _rb = GetComponent<Rigidbody>();
        _rb2D = GetComponent<Rigidbody2D>();
        _trail = GetComponent<TrailRenderer>();
        
        // Determine if 2D or 3D
        _is2D = _rb2D != null;
        
        // If no target and we have a tag, try to find a target
        if (TargetTransform == null && !string.IsNullOrEmpty(TargetTag))
        {
            FindTarget();
        }
        
        // Initialize position tracking
        _lastPosition = transform.position;
        
        // Set initial speed
        _currentSpeed = _is2D ? 
            (_rb2D != null ? _rb2D.linearVelocity.magnitude : 0) : 
            (_rb != null ? _rb.linearVelocity.magnitude : 0);
            
        // Start activation delay
        if (ActivationDelay > 0)
        {
            StartCoroutine(ActivateAfterDelay());
        }
        else
        {
            _isActive = true;
        }
        
        // Set up self-destruction timer
        if (Lifetime > 0)
        {
            Invoke("SelfDestruct", Lifetime);
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Update timers
        _lifeTimer += Time.deltaTime;
        _wobbleTimer += Time.deltaTime;
        
        // Check for max tracking time
        if (MaxTrackingTime > 0 && _lifeTimer >= MaxTrackingTime)
        {
            // Stop tracking but keep moving
            TargetTransform = null;
        }
        
        // If not active yet, just continue straight
        if (!_isActive)
        {
            ContinueStraight();
            return;
        }
        
        // Try to find a target if we don't have one
        if (TargetTransform == null && !string.IsNullOrEmpty(TargetTag) && Time.frameCount % 10 == 0)
        {
            FindTarget();
            
            // If still no target, continue straight
            if (TargetTransform == null)
            {
                ContinueStraight();
                return;
            }
        }
        
        // If we have a target, home in on it
        if (TargetTransform != null)
        {
            // Get the basic target position
            _targetPosition = TargetTransform.position;
            
            // Apply wobble if set
            if (Wobble > 0)
            {
                float wobbleX = Mathf.Sin(_wobbleTimer * 7.3f) * Wobble;
                float wobbleY = Mathf.Cos(_wobbleTimer * 6.1f) * Wobble;
                float wobbleZ = Mathf.Sin(_wobbleTimer * 8.7f) * Wobble;
                
                _targetPosition += new Vector3(wobbleX, wobbleY, wobbleZ);
            }
            
            // Force Y position if locking Y axis
            if (LockYAxis)
            {
                _targetPosition.y = transform.position.y;
            }
            
            // Calculate direction to target
            Vector3 direction = (_targetPosition - transform.position).normalized;
            
            // Calculate distance to target
            float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);
            
            // Calculate target speed based on approach settings
            float targetSpeed = MoveSpeed;
            if (SlowDownOnApproach && distanceToTarget < SlowdownDistance)
            {
                // Lerp speed between min and max based on distance
                float t = distanceToTarget / SlowdownDistance;
                targetSpeed = Mathf.Lerp(MinSpeed, MoveSpeed, t);
            }
            
            // Update speed with acceleration
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, Acceleration * Time.deltaTime);
            
            // Rotate towards target
            RotateTowards(direction);
            
            // Move forward
            MoveForward();
        }
        else
        {
            // No target, continue straight
            ContinueStraight();
        }
    }
    
    // Continue moving in current direction
    private void ContinueStraight()
    {
        // Accelerate to max speed
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, MoveSpeed, Acceleration * Time.deltaTime);
        
        // Move forward
        MoveForward();
    }
    
    // Move in the forward direction
    private void MoveForward()
    {
        // For 2D rigidbody
        if (_is2D && _rb2D != null)
        {
            // Use rigidbody velocity
            Vector2 forwardVelocity = new Vector2(
                Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad),
                Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad)
            ) * _currentSpeed;
            
            _rb2D.linearVelocity = forwardVelocity;
        }
        // For 3D rigidbody
        else if (_rb != null)
        {
            _rb.linearVelocity = transform.forward * _currentSpeed;
        }
        // Fallback to transform
        else
        {
            transform.position += (_is2D ? transform.right : transform.forward) * _currentSpeed * Time.deltaTime;
        }
    }
    
    // Rotate towards a direction
    private void RotateTowards(Vector3 direction)
    {
        // For 2D
        if (_is2D)
        {
            // Calculate angle in 2D plane
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Get current angle
            float currentAngle = transform.eulerAngles.z;
            
            // Normalize angle to -180 to 180 range
            if (currentAngle > 180f)
                currentAngle -= 360f;
                
            // Calculate target angle in same range
            float targetAngle = angle;
            if (targetAngle > 180f)
                targetAngle -= 360f;
                
            // Interpolate between current and target angle
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, TurnSpeed * Time.deltaTime);
            
            // Apply rotation
            transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }
        // For 3D
        else
        {
            // Use Quaternion to smoothly rotate
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                TurnSpeed * Time.deltaTime
            );
        }
    }
    
    // Find a target based on tag and selection mode
    private void FindTarget()
    {
        if (string.IsNullOrEmpty(TargetTag))
            return;
            
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(TargetTag);
        
        if (potentialTargets.Length == 0)
            return;
            
        // Just one target, no need for selection logic
        if (potentialTargets.Length == 1)
        {
            TargetTransform = potentialTargets[0].transform;
            return;
        }
        
        // Multiple targets, use selection mode
        switch (SelectionMode)
        {
            case TargetSelectionMode.Nearest:
                FindNearestTarget(potentialTargets);
                break;
                
            case TargetSelectionMode.Random:
                TargetTransform = potentialTargets[Random.Range(0, potentialTargets.Length)].transform;
                break;
                
            case TargetSelectionMode.StrongestSignal:
                FindStrongestSignalTarget(potentialTargets);
                break;
                
            case TargetSelectionMode.First:
                TargetTransform = potentialTargets[0].transform;
                break;
        }
    }
    
    // Find the nearest target
    private void FindNearestTarget(GameObject[] potentialTargets)
    {
        GameObject nearestTarget = null;
        float closestDistance = float.MaxValue;
        
        foreach (GameObject target in potentialTargets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTarget = target;
            }
        }
        
        if (nearestTarget != null)
        {
            TargetTransform = nearestTarget.transform;
        }
    }
    
    // Find the target most directly in front
    private void FindStrongestSignalTarget(GameObject[] potentialTargets)
    {
        GameObject bestTarget = null;
        float bestScore = -1f;
        
        Vector3 forward = _is2D ? transform.right : transform.forward;
        
        foreach (GameObject target in potentialTargets)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float directionScore = Vector3.Dot(forward, directionToTarget);
            
            // Higher dot product means more directly in front
            if (directionScore > bestScore)
            {
                bestScore = directionScore;
                bestTarget = target;
            }
        }
        
        if (bestTarget != null)
        {
            TargetTransform = bestTarget.transform;
        }
    }
    
    // Activate the missile after a delay
    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(ActivationDelay);
        _isActive = true;
    }
    
    // Handle collision with other objects
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    // Handle collision with other objects (2D)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    // Handle trigger collisions
    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    // Handle trigger collisions (2D)
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }
    
    // Process collision with an object
    private void HandleCollision(GameObject other)
    {
        // Skip collisions with self or parent
        if (other == gameObject || (transform.parent != null && other == transform.parent.gameObject))
            return;
            
        // Trigger hit event
        OnHitTarget?.Invoke(other);
        
        // Destroy if set
        if (DestroyOnHit)
        {
            DestroyMissile();
        }
    }
    
    // Self destruct when lifetime ends
    private void SelfDestruct()
    {
        OnSelfDestruct?.Invoke();
        DestroyMissile();
    }
    
    // Common destruction logic
    private void DestroyMissile()
    {
        // Spawn effect if specified
        if (DestructionEffect != null)
        {
            Instantiate(DestructionEffect, transform.position, Quaternion.identity);
        }
        
        // Destroy the missile
        Destroy(gameObject);
    }
    
    // Draw debug information
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_isActive || TargetTransform == null)
            return;
            
        // Draw line to target
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, _targetPosition);
        
        // Draw slowdown radius if applicable
        if (SlowDownOnApproach)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(_targetPosition, SlowdownDistance);
        }
    }
}
