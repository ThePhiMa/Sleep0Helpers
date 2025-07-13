using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple state machine for enemy AI with idle, patrol, chase, and attack states.
/// Easily extendable for more complex behavior.
/// </summary>
public class SimpleStateMachine : MonoBehaviour
{
    [Header("State Settings")]
    [Tooltip("Current state of the enemy")]
    public EnemyState CurrentState = EnemyState.Idle;
    
    [Tooltip("Initial state to start in")]
    public EnemyState StartState = EnemyState.Idle;
    
    [Header("Target Detection")]
    [Tooltip("Tag of the target to search for")]
    public string TargetTag = "Player";
    
    [Tooltip("Transform of the target (found automatically if null)")]
    public Transform TargetTransform;
    
    [Tooltip("How far the enemy can detect the target")]
    public float DetectionRange = 10f;
    
    [Tooltip("Layers that block line of sight")]
    public LayerMask ObstacleLayers;
    
    [Tooltip("Should check if obstacles block line of sight")]
    public bool CheckLineOfSight = true;
    
    [Header("Movement Settings")]
    [Tooltip("How fast the enemy moves in each state")]
    public float IdleSpeed = 0f;
    public float PatrolSpeed = 2f;
    public float ChaseSpeed = 4f;
    public float AttackSpeed = 1f;
    
    [Tooltip("How close to get before attacking")]
    public float AttackRange = 1.5f;
    
    [Tooltip("How long to wait after attacking before attacking again")]
    public float AttackCooldown = 1f;
    
    [Header("Patrol Settings")]
    [Tooltip("Points to patrol between")]
    public List<Transform> PatrolPoints = new List<Transform>();
    
    [Tooltip("How long to wait at each patrol point")]
    public float PatrolWaitTime = 2f;
    
    [Tooltip("Should the patrol loop back to the start")]
    public bool LoopPatrol = true;
    
    [Header("Idle Settings")]
    [Tooltip("Should the enemy look around randomly while idle")]
    public bool RandomLookAroundWhenIdle = true;
    
    [Tooltip("How long to stay idle before patrolling")]
    public float IdleTime = 3f;
    
    [Header("Chase Settings")]
    [Tooltip("Give up chasing after this time (0 = never)")]
    public float ChaseTimeout = 8f;
    
    [Tooltip("How close to get when chasing")]
    public float StoppingDistance = 1f;
    
    [Header("Animation")]
    [Tooltip("Should use animator for state transitions")]
    public bool UseAnimator = false;
    
    [Tooltip("Parameter names in the animator")]
    public string StateParameterName = "State";
    public string SpeedParameterName = "Speed";
    public string AttackTriggerName = "Attack";
    
    [Header("Events")]
    public UnityEvent<EnemyState> OnStateChanged;
    public UnityEvent OnTargetSpotted;
    public UnityEvent OnTargetLost;
    public UnityEvent OnAttackStarted;
    public UnityEvent OnAttackEnded;
    
    // Enum for possible enemy states
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Return,
        Dead
    }
    
    // Private variables for state management
    private EnemyState _previousState;
    private float _stateTimer = 0f;
    private bool _stateEntered = false;
    private int _currentPatrolIndex = 0;
    private bool _isWaiting = false;
    private float _waitTimer = 0f;
    private bool _attackOnCooldown = false;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private bool _targetVisible = false;
    private float _lastTargetSeenTime = 0f;
    
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
        
        // Save initial position and rotation
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        
        // Find target if not assigned
        if (TargetTransform == null && !string.IsNullOrEmpty(TargetTag))
        {
            GameObject target = GameObject.FindGameObjectWithTag(TargetTag);
            if (target != null)
            {
                TargetTransform = target.transform;
            }
        }
        
        // Generate patrol points if needed
        if (PatrolPoints.Count == 0)
        {
            GeneratePatrolPoints();
        }
        
        // Set initial state
        ChangeState(StartState);
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Check for target visibility
        CheckTargetVisibility();
        
        // Update current state
        UpdateCurrentState();
        
        // Handle state transitions
        CheckStateTransitions();
    }
    
    // Check if target is visible
    private void CheckTargetVisibility()
    {
        bool wasVisible = _targetVisible;
        _targetVisible = false;
        
        if (TargetTransform != null)
        {
            // Calculate distance and direction to target
            float distanceToTarget = Vector3.Distance(transform.position, TargetTransform.position);
            
            // Check if within detection range
            if (distanceToTarget <= DetectionRange)
            {
                // Check line of sight if needed
                bool hasLineOfSight = true;
                
                if (CheckLineOfSight)
                {
                    Vector3 directionToTarget = (TargetTransform.position - transform.position).normalized;
                    
                    // Use appropriate raycast based on physics type
                    if (_rb2D != null)
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, ObstacleLayers);
                        hasLineOfSight = hit.collider == null;
                    }
                    else
                    {
                        hasLineOfSight = !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, ObstacleLayers);
                    }
                }
                
                // If we can see the target
                if (hasLineOfSight)
                {
                    _targetVisible = true;
                    _lastTargetSeenTime = Time.time;
                    
                    // Trigger event if this is first detection
                    if (!wasVisible)
                    {
                        OnTargetSpotted?.Invoke();
                    }
                }
            }
        }
        
        // Target lost event
        if (wasVisible && !_targetVisible)
        {
            OnTargetLost?.Invoke();
        }
    }
    
    // Update behavior based on current state
    private void UpdateCurrentState()
    {
        // Initialize state if just entered
        if (!_stateEntered)
        {
            EnterState(CurrentState);
            _stateEntered = true;
        }
        
        // Update state timer
        _stateTimer += Time.deltaTime;
        
        // Perform state behavior
        switch (CurrentState)
        {
            case EnemyState.Idle:
                UpdateIdleState();
                break;
                
            case EnemyState.Patrol:
                UpdatePatrolState();
                break;
                
            case EnemyState.Chase:
                UpdateChaseState();
                break;
                
            case EnemyState.Attack:
                UpdateAttackState();
                break;
                
            case EnemyState.Return:
                UpdateReturnState();
                break;
                
            case EnemyState.Dead:
                // Do nothing when dead
                break;
        }
        
        // Update animator if used
        if (UseAnimator && _animator != null)
        {
            _animator.SetInteger(StateParameterName, (int)CurrentState);
        }
    }
    
    // Check if we should transition to a different state
    private void CheckStateTransitions()
    {
        // Common transitions based on target visibility
        if (_targetVisible && (CurrentState == EnemyState.Idle || CurrentState == EnemyState.Patrol || CurrentState == EnemyState.Return))
        {
            ChangeState(EnemyState.Chase);
            return;
        }
        
        // State-specific transitions
        switch (CurrentState)
        {
            case EnemyState.Idle:
                // Transition to patrol after idle time
                if (_stateTimer >= IdleTime)
                {
                    ChangeState(EnemyState.Patrol);
                }
                break;
                
            case EnemyState.Patrol:
                // Nothing specific here, handled in update patrol
                break;
                
            case EnemyState.Chase:
                // Check if should attack
                if (_targetVisible && TargetTransform != null)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, TargetTransform.position);
                    
                    if (distanceToTarget <= AttackRange && !_attackOnCooldown)
                    {
                        ChangeState(EnemyState.Attack);
                        return;
                    }
                }
                
                // Give up chase if timed out
                if (ChaseTimeout > 0 && !_targetVisible && Time.time - _lastTargetSeenTime >= ChaseTimeout)
                {
                    ChangeState(EnemyState.Return);
                }
                break;
                
            case EnemyState.Attack:
                // Return to chase after attack completes (handled in update attack state)
                break;
                
            case EnemyState.Return:
                // Go back to idle when returned to start
                float distanceToStart = Vector3.Distance(transform.position, _startPosition);
                if (distanceToStart < 0.1f)
                {
                    ChangeState(EnemyState.Idle);
                }
                break;
        }
    }
    
    // Enter a new state
    private void EnterState(EnemyState newState)
    {
        _stateTimer = 0f;
        
        switch (newState)
        {
            case EnemyState.Idle:
                // Stop movement
                SetVelocity(Vector3.zero);
                break;
                
            case EnemyState.Patrol:
                // Initialize patrol variables
                if (_currentPatrolIndex >= PatrolPoints.Count)
                {
                    _currentPatrolIndex = 0;
                }
                _isWaiting = false;
                break;
                
            case EnemyState.Attack:
                // Trigger attack animation and event
                if (UseAnimator && _animator != null)
                {
                    _animator.SetTrigger(AttackTriggerName);
                }
                
                // Trigger attack event
                OnAttackStarted?.Invoke();
                
                // Start attack cooldown
                StartCoroutine(AttackCooldownRoutine());
                break;
                
            case EnemyState.Return:
                // Nothing special on enter
                break;
        }
    }
    
    // Update logic for idle state
    private void UpdateIdleState()
    {
        // Set idle speed
        SetMovementSpeed(IdleSpeed);
        
        // Look around randomly if enabled
        if (RandomLookAroundWhenIdle && _stateTimer > 1f && Random.value < 0.01f)
        {
            // Random rotation for 3D
            if (_rb2D == null)
            {
                float randomYaw = Random.Range(-120f, 120f);
                transform.rotation = Quaternion.Euler(0, randomYaw, 0) * _startRotation;
            }
            // Random flip for 2D
            else if (Random.value < 0.5f)
            {
                Vector3 scale = transform.localScale;
                scale.x = -scale.x;
                transform.localScale = scale;
            }
        }
    }
    
    // Update logic for patrol state
    private void UpdatePatrolState()
    {
        // Skip if no patrol points
        if (PatrolPoints.Count == 0)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // Get current patrol target
        Transform target = PatrolPoints[_currentPatrolIndex];
        
        // If waiting at a point
        if (_isWaiting)
        {
            SetVelocity(Vector3.zero);
            _waitTimer -= Time.deltaTime;
            
            // Resume patrol when wait is over
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                
                // Move to next patrol point
                _currentPatrolIndex++;
                if (_currentPatrolIndex >= PatrolPoints.Count)
                {
                    if (LoopPatrol)
                    {
                        _currentPatrolIndex = 0;
                    }
                    else
                    {
                        ChangeState(EnemyState.Idle);
                        return;
                    }
                }
            }
        }
        // Move to current patrol point
        else
        {
            float distanceToPoint = Vector3.Distance(transform.position, target.position);
            
            // Check if reached the waypoint
            if (distanceToPoint <= 0.1f)
            {
                // Start waiting
                _isWaiting = true;
                _waitTimer = PatrolWaitTime;
                SetVelocity(Vector3.zero);
            }
            else
            {
                // Move towards waypoint
                Vector3 direction = (target.position - transform.position).normalized;
                MoveInDirection(direction, PatrolSpeed);
            }
        }
    }
    
    // Update logic for chase state
    private void UpdateChaseState()
    {
        if (TargetTransform == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }
        
        // Calculate distance to target
        float distanceToTarget = Vector3.Distance(transform.position, TargetTransform.position);
        
        // If within stopping distance, slow down
        if (distanceToTarget <= StoppingDistance)
        {
            SetVelocity(Vector3.zero);
            
            // Face the target
            FaceTarget();
        }
        else
        {
            // Move towards target
            Vector3 direction = (TargetTransform.position - transform.position).normalized;
            MoveInDirection(direction, ChaseSpeed);
        }
    }
    
    // Update logic for attack state
    private void UpdateAttackState()
    {
        // Face the target
        if (TargetTransform != null)
        {
            FaceTarget();
        }
        
        // Attack animations and effects are handled by the animation events
        // or we can use a timer-based approach if no animation
        if (!UseAnimator && _stateTimer >= 0.5f)
        {
            // Attack completed, return to chase
            OnAttackEnded?.Invoke();
            ChangeState(EnemyState.Chase);
        }
    }
    
    // Update logic for return state
    private void UpdateReturnState()
    {
        // Move back to starting position
        Vector3 direction = (_startPosition - transform.position).normalized;
        MoveInDirection(direction, PatrolSpeed);
        
        // Rotate back to starting rotation if close enough
        float distanceToStart = Vector3.Distance(transform.position, _startPosition);
        if (distanceToStart < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _startRotation, Time.deltaTime * 5f);
        }
    }
    
    // Change to a new state
    private void ChangeState(EnemyState newState)
    {
        // Skip if same state
        if (newState == CurrentState)
            return;
            
        // Store previous state
        _previousState = CurrentState;
        
        // Change state
        CurrentState = newState;
        _stateEntered = false;
        
        // Trigger state changed event
        OnStateChanged?.Invoke(newState);
    }
    
    // Moves in a direction at specified speed
    private void MoveInDirection(Vector3 direction, float speed)
    {
        // Skip if no direction
        if (direction == Vector3.zero)
            return;
            
        // Set animator speed
        SetMovementSpeed(speed);
        
        // For 2D movement
        if (_rb2D != null)
        {
            // Adjust for 2D movement
            Vector2 moveDir = new Vector2(direction.x, direction.y).normalized;
            _rb2D.linearVelocity = moveDir * speed;
            
            // Flip sprite based on movement direction
            if (moveDir.x != 0)
            {
                transform.localScale = new Vector3(
                    moveDir.x < 0 ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }
        // For 3D movement
        else
        {
            // For rigidbody movement
            if (_rb != null)
            {
                // For ground movement, zero out y component
                if (!_rb.isKinematic)
                {
                    direction.y = 0;
                    direction = direction.normalized;
                }
                
                _rb.linearVelocity = direction * speed;
            }
            // For transform movement
            else
            {
                transform.position += direction * speed * Time.deltaTime;
            }
            
            // Rotate to face direction
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }
    
    // Set velocity directly
    private void SetVelocity(Vector3 velocity)
    {
        if (_rb2D != null)
        {
            _rb2D.linearVelocity = new Vector2(velocity.x, velocity.y);
        }
        else if (_rb != null)
        {
            _rb.linearVelocity = velocity;
        }
        
        // Update animator speed parameter
        SetMovementSpeed(velocity.magnitude);
    }
    
    // Set the movement speed parameter for the animator
    private void SetMovementSpeed(float speed)
    {
        if (UseAnimator && _animator != null)
        {
            _animator.SetFloat(SpeedParameterName, speed);
        }
    }
    
    // Face towards the target
    private void FaceTarget()
    {
        if (TargetTransform == null)
            return;
            
        Vector3 direction = (TargetTransform.position - transform.position).normalized;
        
        // For 2D
        if (_rb2D != null)
        {
            // Flip sprite based on target direction
            if (direction.x != 0)
            {
                transform.localScale = new Vector3(
                    direction.x < 0 ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }
        // For 3D
        else
        {
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }
    
    // Handle attack cooldown
    private IEnumerator AttackCooldownRoutine()
    {
        _attackOnCooldown = true;
        yield return new WaitForSeconds(AttackCooldown);
        _attackOnCooldown = false;
    }
    
    // Generate default patrol points if none are specified
    private void GeneratePatrolPoints()
    {
        PatrolPoints.Clear();
        
        // Create a parent for the waypoints
        GameObject waypointsParent = new GameObject(gameObject.name + "_PatrolPoints");
        waypointsParent.transform.parent = transform.parent;
        
        // Create four patrol points in a square pattern
        for (int i = 0; i < 4; i++)
        {
            GameObject point = new GameObject("PatrolPoint_" + i);
            point.transform.parent = waypointsParent.transform;
            
            // Position in a square pattern
            float angle = i * 90f * Mathf.Deg2Rad;
            float radius = 3f;
            Vector3 position = transform.position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            
            point.transform.position = position;
            PatrolPoints.Add(point.transform);
        }
        
        Debug.Log("Created 4 default patrol points for " + gameObject.name);
    }
    
    // Called by animation events when an attack animation completes
    public void OnAttackAnimationEnded()
    {
        if (CurrentState == EnemyState.Attack)
        {
            // Attack completed, trigger event
            OnAttackEnded?.Invoke();
            
            // Return to chase
            ChangeState(EnemyState.Chase);
        }
    }
    
    // Set the enemy state from other scripts
    public void SetState(EnemyState newState)
    {
        ChangeState(newState);
    }
    
    // Force the enemy to die
    public void Die()
    {
        ChangeState(EnemyState.Dead);
        
        // Stop all movement
        SetVelocity(Vector3.zero);
        
        // Disable components if needed
        if (_rb != null) _rb.isKinematic = true;
        if (_rb2D != null) _rb2D.simulated = false;
        
        // Could also add death animation or particle effects here
    }
    
    // Draw gizmos for debugging
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = _targetVisible ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
        
        // Draw patrol points
        if (PatrolPoints != null && PatrolPoints.Count > 0)
        {
            Gizmos.color = Color.blue;
            
            for (int i = 0; i < PatrolPoints.Count; i++)
            {
                if (PatrolPoints[i] != null)
                {
                    // Draw point
                    Gizmos.DrawSphere(PatrolPoints[i].position, 0.2f);
                    
                    // Draw line to next point
                    if (i < PatrolPoints.Count - 1 && PatrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(PatrolPoints[i].position, PatrolPoints[i + 1].position);
                    }
                    else if (i == PatrolPoints.Count - 1 && PatrolPoints[0] != null && LoopPatrol)
                    {
                        Gizmos.DrawLine(PatrolPoints[i].position, PatrolPoints[0].position);
                    }
                }
            }
        }
        
        // Draw line to target if chasing
        if (_targetVisible && TargetTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, TargetTransform.position);
        }
    }
}
