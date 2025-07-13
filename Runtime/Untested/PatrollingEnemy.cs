using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Makes an object patrol between a series of waypoints.
/// Can detect players that enter its vision cone.
/// </summary>
public class PatrollingEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second")]
    public float MoveSpeed = 2f;
    
    [Tooltip("How close to get to a waypoint before moving to the next one")]
    public float WaypointReachedDistance = 0.1f;
    
    [Tooltip("Time to wait at each waypoint before moving to the next")]
    public float WaitTimeAtWaypoints = 1f;
    
    [Tooltip("Should the patrol route loop back to the start?")]
    public bool LoopWaypoints = true;
    
    [Tooltip("Should the patrol reverse direction when reaching the end instead of teleporting to start?")]
    public bool PingPongWaypoints = false;
    
    [Header("Waypoints")]
    [Tooltip("List of points to patrol between (if empty, will generate from child objects)")]
    public List<Transform> Waypoints = new List<Transform>();
    
    [Tooltip("Should waypoints be created from child objects if none are specified?")]
    public bool UseChildrenAsWaypoints = true;
    
    [Header("Detection")]
    [Tooltip("Can this enemy detect and chase the player?")]
    public bool CanDetectPlayer = true;
    
    [Tooltip("Tag to look for when detecting (usually 'Player')")]
    public string TargetTag = "Player";
    
    [Tooltip("Field of view angle (how wide the enemy can see)")]
    [Range(0f, 360f)]
    public float FieldOfViewAngle = 90f;
    
    [Tooltip("How far the enemy can see")]
    public float ViewDistance = 5f;
    
    [Tooltip("Layers that will block the enemy's line of sight")]
    public LayerMask ObstacleLayers;
    
    [Tooltip("Should the enemy's vision be visualized in the editor?")]
    public bool ShowVisionCone = true;
    
    [Header("Chase Settings")]
    [Tooltip("Should the enemy chase the player when detected?")]
    public bool ChaseWhenDetected = true;
    
    [Tooltip("Movement speed when chasing the player")]
    public float ChaseSpeed = 3.5f;
    
    [Tooltip("Maximum time to chase before giving up")]
    public float MaxChaseTime = 5f;
    
    [Tooltip("How close to get to the target when chasing")]
    public float StoppingDistance = 0.5f;
    
    [Header("Events")]
    [Tooltip("Event triggered when a target is detected")]
    public UnityEvent<GameObject> OnDetectedTarget;
    
    [Tooltip("Event triggered when the enemy loses sight of the target")]
    public UnityEvent OnLostTarget;
    
    [Tooltip("Event triggered when the enemy reaches the target")]
    public UnityEvent OnReachedTarget;
    
    // Private state variables
    private int _currentWaypointIndex = 0;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;
    private bool _isMovingForward = true;
    private bool _hasDetectedTarget = false;
    private GameObject _detectedTarget = null;
    private float _chaseTimer = 0f;
    
    // Cache components
    private Rigidbody _rb;
    private Rigidbody2D _rb2D;
    
    // Start is called before the first frame update
    private void Start()
    {
        // Get components
        _rb = GetComponent<Rigidbody>();
        _rb2D = GetComponent<Rigidbody2D>();
        
        // Generate waypoints from children if needed
        if (UseChildrenAsWaypoints && Waypoints.Count == 0)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Waypoint") || child.name.StartsWith("Waypoint") || child.name.StartsWith("wp_"))
                {
                    Waypoints.Add(child);
                    // Optionally hide the waypoint objects
                    child.gameObject.SetActive(false);
                }
            }
            
            if (Waypoints.Count == 0)
            {
                Debug.LogWarning("No waypoints found in children. Creating default waypoints.");
                CreateDefaultWaypoints();
            }
        }
        
        // Validate waypoints
        if (Waypoints.Count == 0)
        {
            Debug.LogError("No waypoints assigned. Creating default waypoints.");
            CreateDefaultWaypoints();
        }
        
        // Set initial position to first waypoint if very close
        if (Vector3.Distance(transform.position, Waypoints[0].position) < 0.1f)
        {
            // Already at first waypoint, start waiting
            _isWaiting = true;
            _waitTimer = WaitTimeAtWaypoints;
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Detect targets if enabled
        if (CanDetectPlayer)
        {
            DetectTargets();
        }
        
        // Update behavior based on state
        if (_hasDetectedTarget && ChaseWhenDetected)
        {
            ChaseTarget();
        }
        else
        {
            Patrol();
        }
    }
    
    // Handle patrolling between waypoints
    private void Patrol()
    {
        // Don't patrol if no waypoints
        if (Waypoints.Count == 0)
            return;
            
        // If waiting at a waypoint
        if (_isWaiting)
        {
            // Stop movement
            SetVelocity(Vector3.zero);
            
            // Count down wait timer
            _waitTimer -= Time.deltaTime;
            
            // Resume movement if timer is up
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                
                // Update waypoint index
                UpdateWaypointIndex();
            }
        }
        // Move to current waypoint
        else
        {
            Transform currentWaypoint = Waypoints[_currentWaypointIndex];
            
            // Calculate distance to waypoint
            float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
            
            // Check if we've reached the waypoint
            if (distanceToWaypoint <= WaypointReachedDistance)
            {
                // Start waiting
                _isWaiting = true;
                _waitTimer = WaitTimeAtWaypoints;
                SetVelocity(Vector3.zero);
            }
            else
            {
                // Move towards waypoint
                Vector3 targetPosition = currentWaypoint.position;
                
                // For 2D games, make sure to use the correct plane
                if (_rb2D != null)
                {
                    targetPosition.z = transform.position.z;
                }
                
                // Calculate direction
                Vector3 moveDirection = (targetPosition - transform.position).normalized;
                
                // Move towards waypoint
                SetVelocity(moveDirection * MoveSpeed);
                
                // Face the movement direction
                FaceDirection(moveDirection);
            }
        }
    }
    
    // Update the waypoint index based on patrol settings
    private void UpdateWaypointIndex()
    {
        // If using ping-pong
        if (PingPongWaypoints)
        {
            if (_isMovingForward)
            {
                _currentWaypointIndex++;
                
                // Check if reached the end
                if (_currentWaypointIndex >= Waypoints.Count)
                {
                    if (Waypoints.Count > 1)
                    {
                        _currentWaypointIndex = Waypoints.Count - 2;
                        _isMovingForward = false;
                    }
                    else
                    {
                        _currentWaypointIndex = 0;
                    }
                }
            }
            else
            {
                _currentWaypointIndex--;
                
                // Check if reached the start
                if (_currentWaypointIndex < 0)
                {
                    _currentWaypointIndex = 1;
                    _isMovingForward = true;
                    
                    // If only one waypoint, fix index
                    if (Waypoints.Count <= 1)
                    {
                        _currentWaypointIndex = 0;
                    }
                }
            }
        }
        // If looping or standard movement
        else
        {
            _currentWaypointIndex++;
            
            // Loop back to start if needed
            if (_currentWaypointIndex >= Waypoints.Count)
            {
                if (LoopWaypoints)
                {
                    _currentWaypointIndex = 0;
                }
                else
                {
                    // Stop at last waypoint
                    _currentWaypointIndex = Waypoints.Count - 1;
                }
            }
        }
    }
    
    // Detect targets in the vision cone
    private void DetectTargets()
    {
        // Find all potential targets with the specified tag
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(TargetTag);
        
        // Track if we've seen the target this frame
        bool foundTarget = false;
        
        foreach (GameObject target in potentialTargets)
        {
            // Calculate direction and distance to target
            Vector3 directionToTarget = target.transform.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;
            
            // Check if in range
            if (distanceToTarget <= ViewDistance)
            {
                // Normalize for angle check
                directionToTarget.Normalize();
                
                // Get the angle between forward direction and direction to target
                float angle = Vector3.Angle(transform.forward, directionToTarget);
                
                // Check if within field of view angle
                if (angle <= FieldOfViewAngle * 0.5f)
                {
                    // Check for obstacles between enemy and target
                    bool hasLineOfSight = true;
                    
                    // Use the appropriate raycast based on 2D or 3D
                    if (_rb2D != null)
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, ObstacleLayers);
                        hasLineOfSight = hit.collider == null;
                    }
                    else
                    {
                        hasLineOfSight = !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, ObstacleLayers);
                    }
                    
                    // Target is visible
                    if (hasLineOfSight)
                    {
                        // Set target as detected
                        foundTarget = true;
                        
                        // If this is a new detection
                        if (!_hasDetectedTarget || _detectedTarget != target)
                        {
                            _detectedTarget = target;
                            _hasDetectedTarget = true;
                            _chaseTimer = 0f;
                            
                            // Trigger detection event
                            OnDetectedTarget?.Invoke(target);
                        }
                        
                        // No need to check other targets
                        break;
                    }
                }
            }
        }
        
        // Handle losing sight of target
        if (_hasDetectedTarget && !foundTarget)
        {
            // Increment the chase timer if we're chasing
            if (ChaseWhenDetected)
            {
                _chaseTimer += Time.deltaTime;
                
                // If we've been chasing too long, give up
                if (_chaseTimer >= MaxChaseTime)
                {
                    LoseTarget();
                }
            }
            else
            {
                LoseTarget();
            }
        }
    }
    
    // Handle losing the target
    private void LoseTarget()
    {
        _hasDetectedTarget = false;
        _detectedTarget = null;
        _chaseTimer = 0f;
        
        // Trigger lost target event
        OnLostTarget?.Invoke();
    }
    
    // Chase the detected target
    private void ChaseTarget()
    {
        if (_detectedTarget == null)
        {
            LoseTarget();
            return;
        }
        
        // Calculate direction and distance to target
        Vector3 targetPosition = _detectedTarget.transform.position;
        
        // For 2D games, make sure to use the correct plane
        if (_rb2D != null)
        {
            targetPosition.z = transform.position.z;
        }
        
        Vector3 directionToTarget = targetPosition - transform.position;
        float distanceToTarget = directionToTarget.magnitude;
        
        // Check if we've reached the target
        if (distanceToTarget <= StoppingDistance)
        {
            // Stop and trigger reached event
            SetVelocity(Vector3.zero);
            OnReachedTarget?.Invoke();
        }
        else
        {
            // Move towards target
            directionToTarget.Normalize();
            SetVelocity(directionToTarget * ChaseSpeed);
            
            // Face the target
            FaceDirection(directionToTarget);
        }
    }
    
    // Set velocity based on rigidbody type
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
        else
        {
            // Fallback to transform movement
            transform.position += velocity * Time.deltaTime;
        }
    }
    
    // Face a direction
    private void FaceDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            if (_rb2D != null)
            {
                // 2D: Flip sprite based on direction
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
                // 3D: Rotate to face direction
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
    }
    
    // Create default waypoints if none are specified
    private void CreateDefaultWaypoints()
    {
        Waypoints.Clear();
        
        // Create a parent for the waypoints
        GameObject waypointsParent = new GameObject(gameObject.name + "_Waypoints");
        waypointsParent.transform.parent = transform.parent;
        
        // Create four waypoints in a square pattern
        for (int i = 0; i < 4; i++)
        {
            GameObject waypoint = new GameObject("Waypoint_" + i);
            waypoint.transform.parent = waypointsParent.transform;
            waypoint.tag = "Waypoint";
            
            // Position in a square pattern
            float angle = i * 90f * Mathf.Deg2Rad;
            float radius = 3f;
            Vector3 position = transform.position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            
            waypoint.transform.position = position;
            Waypoints.Add(waypoint.transform);
        }
        
        Debug.Log("Created 4 default waypoints for " + gameObject.name);
    }
    
    // Draw gizmos for waypoints and vision cone
    private void OnDrawGizmosSelected()
    {
        // Draw waypoints and connections
        if (Waypoints != null && Waypoints.Count > 0)
        {
            // Draw waypoint connections
            Gizmos.color = Color.blue;
            for (int i = 0; i < Waypoints.Count; i++)
            {
                if (Waypoints[i] != null)
                {
                    // Draw waypoint
                    Gizmos.DrawSphere(Waypoints[i].position, 0.2f);
                    
                    // Draw line to next waypoint
                    if (i < Waypoints.Count - 1 && Waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(Waypoints[i].position, Waypoints[i + 1].position);
                    }
                    // If looping, connect last to first
                    else if (i == Waypoints.Count - 1 && LoopWaypoints && Waypoints[0] != null)
                    {
                        Gizmos.DrawLine(Waypoints[i].position, Waypoints[0].position);
                    }
                }
            }
            
            // Highlight current waypoint
            if (Application.isPlaying && _currentWaypointIndex < Waypoints.Count && Waypoints[_currentWaypointIndex] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(Waypoints[_currentWaypointIndex].position, 0.3f);
            }
        }
        
        // Draw vision cone
        if (ShowVisionCone && CanDetectPlayer)
        {
            // Draw view distance
            Gizmos.color = _hasDetectedTarget ? Color.red : Color.yellow;
            
            // For 2D, use a different approach to draw vision cone
            if (_rb2D != null)
            {
                // Draw an arc for 2D vision
                Vector3 forward = transform.right; // In 2D, right is typically forward
                
                // Create an approximation of an arc with lines
                int segments = 20;
                float angleStep = FieldOfViewAngle / segments;
                float startAngle = -FieldOfViewAngle / 2;
                
                Vector3 previousPoint = transform.position + Quaternion.Euler(0, 0, startAngle) * forward * ViewDistance;
                
                for (int i = 1; i <= segments; i++)
                {
                    float angle = startAngle + angleStep * i;
                    Vector3 newPoint = transform.position + Quaternion.Euler(0, 0, angle) * forward * ViewDistance;
                    Gizmos.DrawLine(previousPoint, newPoint);
                    previousPoint = newPoint;
                }
                
                // Draw lines from origin to arc edges
                Vector3 leftEdge = transform.position + Quaternion.Euler(0, 0, startAngle) * forward * ViewDistance;
                Vector3 rightEdge = transform.position + Quaternion.Euler(0, 0, startAngle + FieldOfViewAngle) * forward * ViewDistance;
                Gizmos.DrawLine(transform.position, leftEdge);
                Gizmos.DrawLine(transform.position, rightEdge);
            }
            else
            {
                // Draw a cone for 3D vision
                Vector3 forward = transform.forward;
                
                // Draw main view line
                Gizmos.DrawRay(transform.position, forward * ViewDistance);
                
                // Draw cone edges
                float halfFOV = FieldOfViewAngle * 0.5f * Mathf.Deg2Rad;
                
                // Calculate cone directions using quaternions
                Vector3 rightDir = Quaternion.Euler(0, FieldOfViewAngle * 0.5f, 0) * forward;
                Vector3 leftDir = Quaternion.Euler(0, -FieldOfViewAngle * 0.5f, 0) * forward;
                
                // Draw the edges of the vision cone
                Gizmos.DrawRay(transform.position, rightDir * ViewDistance);
                Gizmos.DrawRay(transform.position, leftDir * ViewDistance);
                
                // Draw arc connecting the edges (approximated with lines)
                int arcSegments = 20;
                Vector3 prevDir = rightDir;
                
                for (int i = 1; i <= arcSegments; i++)
                {
                    float t = i / (float)arcSegments;
                    Quaternion rotation = Quaternion.Euler(0, -FieldOfViewAngle * t, 0);
                    Vector3 nextDir = rotation * rightDir;
                    
                    Gizmos.DrawLine(
                        transform.position + prevDir * ViewDistance,
                        transform.position + nextDir * ViewDistance
                    );
                    
                    prevDir = nextDir;
                }
            }
        }
    }
}
