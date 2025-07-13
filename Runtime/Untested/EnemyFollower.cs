using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Basic enemy that follows a target (typically the player).
/// Can be configured to chase when player is in range, follow patrol points when not.
/// </summary>
public class EnemyFollower : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Tag to automatically find and follow (usually 'Player')")]
    public string TargetTag = "Player";
    
    [Tooltip("Manually assign a target to follow (overrides tag-based targeting)")]
    public Transform TargetTransform;
    
    [Tooltip("How fast the enemy moves when chasing the target")]
    public float ChaseSpeed = 3f;
    
    [Tooltip("How close the enemy tries to get to the target")]
    public float StoppingDistance = 1f;
    
    [Header("Detection")]
    [Tooltip("Maximum distance to detect and follow the target")]
    public float DetectionRange = 10f;
    
    [Tooltip("Should draw detection range in editor")]
    public bool ShowDetectionRange = true;
    
    [Tooltip("Check if there are obstacles between enemy and target")]
    public bool CheckLineOfSight = true;
    
    [Tooltip("Layers to consider as obstacles for line of sight")]
    public LayerMask ObstacleLayers;
    
    [Header("Patrol")]
    [Tooltip("Should the enemy patrol when target is not in range")]
    public bool PatrolWhenIdle = false;
    
    [Tooltip("Patrol points to follow when idle (will be auto-generated if empty but patrol is enabled)")]
    public Transform[] PatrolPoints;
    
    [Tooltip("Speed when patrolling")]
    public float PatrolSpeed = 1.5f;
    
    [Tooltip("Time to wait at each patrol point")]
    public float WaitTime = 1f;
    
    [Header("Animation")]
    [Tooltip("Does this enemy use animation?")]
    public bool UseAnimation = false;
    
    [Tooltip("Parameter name for the animation controller's speed parameter")]
    public string SpeedParameterName = "Speed";
    
    [Tooltip("Parameter name for the animation controller's attacking parameter")]
    public string AttackParameterName = "Attack";
    
    [Header("Events")]
    [Tooltip("Event triggered when the target is detected")]
    public UnityEvent<Transform> OnTargetDetected;
    
    [Tooltip("Event triggered when the target is lost")]
    public UnityEvent OnTargetLost;
    
    [Tooltip("Event triggered when the enemy reaches the target")]
    public UnityEvent OnReachedTarget;
    
    // Current state
    private bool _targetInRange = false;
    private Vector3 _lastKnownTargetPosition;
    
    // Patrol variables
    private int _currentPatrolIndex = 0;
    private float _waitTimer = 0f;
    private bool _waiting = false;
    
    // Component references
    private Animator _animator;
    private Rigidbody _rb;
    private Rigidbody2D _rb2D;
    
    // Start is called before the first frame update
    private void Start()
    {
        // Get components
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _rb2D = GetComponent<Rigidbody2D>();
        
        // If no target assigned, try to find by tag
        if (TargetTransform == null && !string.IsNullOrEmpty(TargetTag))
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(TargetTag);
            if (targetObj != null)
            {
                TargetTransform = targetObj.transform;
            }
        }
        
        // Generate patrol points if needed
        if (PatrolWhenIdle && (PatrolPoints == null || PatrolPoints.Length == 0))
        {
            GeneratePatrolPoints();
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Check if target is in range
        if (TargetTransform != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, TargetTransform.position);
            bool hasLineOfSight = true;
            
            // Check for obstacles if enabled
            if (CheckLineOfSight)
            {
                Vector3 directionToTarget = (TargetTransform.position - transform.position).normalized;
                
                // Use 3D or 2D raycasting based on which rigidbody is present
                if (_rb != null)
                {
                    hasLineOfSight = !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, ObstacleLayers);
                }
                else if (_rb2D != null)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, ObstacleLayers);
                    hasLineOfSight = hit.collider == null;
                }
            }
            
            // Determine if target is in range and visible
            bool newTargetInRange = distanceToTarget <= DetectionRange && hasLineOfSight;
            
            // Handle target detection/loss events
            if (newTargetInRange && !_targetInRange)
            {
                OnTargetDetected?.Invoke(TargetTransform);
            }
            else if (!newTargetInRange && _targetInRange)
            {
                OnTargetLost?.Invoke();
            }
            
            _targetInRange = newTargetInRange;
            
            // Update last known position if in range
            if (_targetInRange)
            {
                _lastKnownTargetPosition = TargetTransform.position;
            }
        }
        
        // Move based on current state
        if (_targetInRange && TargetTransform != null)
        {
            ChaseTarget();
        }
        else if (PatrolWhenIdle && PatrolPoints != null && PatrolPoints.Length > 0)
        {
            Patrol();
        }
        else
        {
            // Just stop
            SetMovementSpeed(0f);
        }
    }
    
    // Chase the target
    private void ChaseTarget()
    {
        // Calculate direction to target
        Vector3 targetPosition = TargetTransform.position;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        // Move if not within stopping distance
        if (distanceToTarget > StoppingDistance)
        {
            MoveToPosition(targetPosition, ChaseSpeed);
        }
        else
        {
            // Stop moving and trigger reached event
            SetMovementSpeed(0f);
            OnReachedTarget?.Invoke();
            
            // Trigger attack animation if using animations
            if (UseAnimation && _animator != null && !string.IsNullOrEmpty(AttackParameterName))
            {
                _animator.SetTrigger(AttackParameterName);
            }
        }
    }
    
    // Follow patrol points
    private void Patrol()
    {
        // Don't patrol if no points
        if (PatrolPoints.Length == 0)
            return;
        
        // Get current patrol target
        Transform target = PatrolPoints[_currentPatrolIndex];
        float distanceToPoint = Vector3.Distance(transform.position, target.position);
        
        // Check if waiting at a point
        if (_waiting)
        {
            SetMovementSpeed(0f);
            _waitTimer -= Time.deltaTime;
            
            if (_waitTimer <= 0f)
            {
                _waiting = false;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Length;
            }
        }
        // Move to next point
        else
        {
            if (distanceToPoint > 0.1f)
            {
                MoveToPosition(target.position, PatrolSpeed);
            }
            else
            {
                // Reached the point, start waiting
                _waiting = true;
                _waitTimer = WaitTime;
            }
        }
    }
    
    // Move to a specific position
    private void MoveToPosition(Vector3 targetPosition, float speed)
    {
        // Get direction to move
        Vector3 direction;
        
        // For 2D games, ignore Z axis
        if (_rb2D != null)
        {
            direction = new Vector3(
                targetPosition.x - transform.position.x,
                targetPosition.y - transform.position.y,
                0
            ).normalized;
            
            // Move the 2D rigidbody
            _rb2D.linearVelocity = direction * speed;
        }
        else
        {
            direction = (targetPosition - transform.position).normalized;
            
            // For 3D movement, ignore Y axis if rigidbody isn't kinematic
            if (_rb != null && !_rb.isKinematic)
            {
                direction.y = 0;
                direction = direction.normalized;
                
                // Move the 3D rigidbody
                _rb.linearVelocity = direction * speed;
            }
            else
            {
                // Use simple transform movement for kinematic objects
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            }
        }
        
        // Rotate to face direction (2D should use different rotation)
        if (_rb2D != null)
        {
            // For 2D, flip the sprite based on direction
            if (direction.x != 0)
            {
                transform.localScale = new Vector3(
                    direction.x < 0 ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }
        else
        {
            // For 3D, rotate to face direction
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
        
        // Update animation
        SetMovementSpeed(speed);
    }
    
    // Update animator with movement speed
    private void SetMovementSpeed(float speed)
    {
        if (UseAnimation && _animator != null && !string.IsNullOrEmpty(SpeedParameterName))
        {
            _animator.SetFloat(SpeedParameterName, speed);
        }
    }
    
    // Generate simple patrol points around the starting position
    private void GeneratePatrolPoints()
    {
        PatrolPoints = new Transform[4];
        
        for (int i = 0; i < 4; i++)
        {
            GameObject point = new GameObject("PatrolPoint_" + i);
            point.transform.parent = transform.parent; // Same parent as enemy
            
            // Create a square patrol route
            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 3f;
            point.transform.position = transform.position + offset;
            
            PatrolPoints[i] = point.transform;
        }
        
        Debug.Log("Generated 4 patrol points around " + gameObject.name);
    }
    
    // Draw detection range in editor
    private void OnDrawGizmosSelected()
    {
        if (ShowDetectionRange)
        {
            Gizmos.color = _targetInRange ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
            
            // Draw line to target if detected
            if (_targetInRange && TargetTransform != null)
            {
                Gizmos.DrawLine(transform.position, TargetTransform.position);
            }
        }
        
        // Draw patrol route
        if (PatrolWhenIdle && PatrolPoints != null && PatrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            
            for (int i = 0; i < PatrolPoints.Length; i++)
            {
                if (PatrolPoints[i] != null)
                {
                    // Draw point
                    Gizmos.DrawSphere(PatrolPoints[i].position, 0.2f);
                    
                    // Draw line to next point
                    if (i < PatrolPoints.Length - 1 && PatrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(PatrolPoints[i].position, PatrolPoints[i + 1].position);
                    }
                    else if (i == PatrolPoints.Length - 1 && PatrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(PatrolPoints[i].position, PatrolPoints[0].position);
                    }
                }
            }
        }
    }
}
