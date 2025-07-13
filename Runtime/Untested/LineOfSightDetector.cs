using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects targets that enter a vision cone and provides events when targets are spotted or lost.
/// </summary>
public class LineOfSightDetector : MonoBehaviour
{
    [Header("Vision Settings")]
    [Tooltip("Field of view angle in degrees")]
    [Range(0f, 360f)]
    public float FieldOfViewAngle = 90f;

    [Tooltip("How far the character can see")]
    public float ViewDistance = 10f;

    [Tooltip("Should targets be detected behind obstacles")]
    public bool CheckForObstacles = true;

    [Tooltip("Layers that block line of sight")]
    public LayerMask ObstacleLayers;

    [Tooltip("Layers that contain potential targets")]
    public LayerMask TargetLayers;

    [Header("Target Filtering")]
    [Tooltip("Tags of objects to detect (leave empty to detect all)")]
    public List<string> TargetTags = new List<string>();

    [Tooltip("Check frequency in seconds (lower values = more responsive, higher values = better performance)")]
    public float CheckFrequency = 0.2f;

    [Header("Debug Visualization")]
    [Tooltip("Should the vision cone be drawn in the editor")]
    public bool ShowVisionCone = true;

    [Tooltip("Should rays to detected targets be drawn")]
    public bool ShowRaysToTargets = true;

    [Tooltip("Color of the vision cone when no targets are detected")]
    public Color NormalVisionColor = new Color(0.5f, 0.5f, 0.1f, 0.3f);

    [Tooltip("Color of the vision cone when targets are detected")]
    public Color AlertVisionColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("Events")]
    [Tooltip("Event triggered when any target is first spotted")]
    public UnityEvent OnAnyTargetSpotted;

    [Tooltip("Event triggered when all targets are lost")]
    public UnityEvent OnAllTargetsLost;

    [Tooltip("Event triggered when a specific target is spotted")]
    public UnityEvent<GameObject> OnTargetSpotted;

    [Tooltip("Event triggered when a specific target is lost")]
    public UnityEvent<GameObject> OnTargetLost;

    // Currently visible targets
    private HashSet<GameObject> _visibleTargets = new HashSet<GameObject>();

    // Timer for checking
    private float _checkTimer = 0f;

    // Cache for performance
    private Transform _transform;

    // For 2D detection
    private bool _is2D;

    // Start is called before the first frame update
    private void Start()
    {
        _transform = transform;

        // Check if we're in a 2D project
        _is2D = GetComponent<Rigidbody2D>() != null || GetComponent<Collider2D>() != null;

        // If no target tags specified, detect all layers in the target mask
        if (TargetTags.Count == 0)
        {
            Debug.Log("LineOfSightDetector: No target tags specified. Will detect all objects on the target layers.");
        }

        // Initial check
        CheckLineOfSight();
    }

    // Update is called once per frame
    private void Update()
    {
        // Update timer
        _checkTimer += Time.deltaTime;

        // Check line of sight periodically
        if (_checkTimer >= CheckFrequency)
        {
            CheckLineOfSight();
            _checkTimer = 0f;
        }
    }

    // Check for targets in line of sight
    private void CheckLineOfSight()
    {
        // Keep track of targets still visible
        HashSet<GameObject> stillVisibleTargets = new HashSet<GameObject>();

        // Find all potential targets
        Collider[] targetsInRange = new Collider[0];
        Collider2D[] targetsInRange2D = new Collider2D[0];

        if (_is2D)
        {
            // For 2D projects, use circle overlap
            targetsInRange2D = Physics2D.OverlapCircleAll(_transform.position, ViewDistance, TargetLayers);
        }
        else
        {
            // For 3D projects, use sphere overlap
            targetsInRange = Physics.OverlapSphere(_transform.position, ViewDistance, TargetLayers);
        }

        // Process 3D targets
        foreach (Collider col in targetsInRange)
        {
            GameObject target = col.gameObject;

            // Skip if null or self
            if (target == null || target == gameObject)
                continue;

            // Check tags if specified
            if (TargetTags.Count > 0)
            {
                bool hasMatchingTag = false;
                foreach (string tag in TargetTags)
                {
                    if (target.CompareTag(tag))
                    {
                        hasMatchingTag = true;
                        break;
                    }
                }

                if (!hasMatchingTag)
                    continue;
            }

            // Check if target is in view
            if (IsInLineOfSight(target))
            {
                stillVisibleTargets.Add(target);

                // If this is a newly spotted target
                if (!_visibleTargets.Contains(target))
                {
                    // Add to visible targets
                    _visibleTargets.Add(target);

                    // Trigger target spotted events
                    OnTargetSpotted?.Invoke(target);

                    // If this is the first target, trigger any target spotted
                    if (_visibleTargets.Count == 1)
                    {
                        OnAnyTargetSpotted?.Invoke();
                    }
                }
            }
        }

        // Process 2D targets
        foreach (Collider2D col in targetsInRange2D)
        {
            GameObject target = col.gameObject;

            // Skip if null or self
            if (target == null || target == gameObject)
                continue;

            // Check tags if specified
            if (TargetTags.Count > 0)
            {
                bool hasMatchingTag = false;
                foreach (string tag in TargetTags)
                {
                    if (target.CompareTag(tag))
                    {
                        hasMatchingTag = true;
                        break;
                    }
                }

                if (!hasMatchingTag)
                    continue;
            }

            // Check if target is in view
            if (IsInLineOfSight(target))
            {
                stillVisibleTargets.Add(target);

                // If this is a newly spotted target
                if (!_visibleTargets.Contains(target))
                {
                    // Add to visible targets
                    _visibleTargets.Add(target);

                    // Trigger target spotted events
                    OnTargetSpotted?.Invoke(target);

                    // If this is the first target, trigger any target spotted
                    if (_visibleTargets.Count == 1)
                    {
                        OnAnyTargetSpotted?.Invoke();
                    }
                }
            }
        }

        // Find targets that are no longer visible
        List<GameObject> lostTargets = new List<GameObject>();

        foreach (GameObject oldTarget in _visibleTargets)
        {
            if (!stillVisibleTargets.Contains(oldTarget))
            {
                lostTargets.Add(oldTarget);
            }
        }

        // Remove lost targets and trigger events
        foreach (GameObject lostTarget in lostTargets)
        {
            _visibleTargets.Remove(lostTarget);

            // Trigger target lost event
            OnTargetLost?.Invoke(lostTarget);
        }

        // If all targets are lost, trigger event
        if (_visibleTargets.Count == 0 && lostTargets.Count > 0)
        {
            OnAllTargetsLost?.Invoke();
        }
    }

    // Check if a target is in line of sight
    private bool IsInLineOfSight(GameObject target)
    {
        // Calculate direction to target
        Vector3 targetPosition = target.transform.position;
        Vector3 directionToTarget = targetPosition - _transform.position;

        // Check if within view distance
        float distanceToTarget = directionToTarget.magnitude;
        if (distanceToTarget > ViewDistance)
            return false;

        // For 2D, ignore Z axis for angle calculation
        if (_is2D)
        {
            directionToTarget = new Vector3(directionToTarget.x, directionToTarget.y, 0).normalized;

            // In 2D, forward is usually the right vector (X-axis)
            Vector3 forward = _transform.right;

            // Calculate angle
            float angle = Vector3.Angle(forward, directionToTarget);

            // Check if within field of view angle
            if (angle > FieldOfViewAngle * 0.5f)
                return false;

            // Check for obstacles if enabled
            if (CheckForObstacles)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    _transform.position,
                    directionToTarget,
                    distanceToTarget,
                    ObstacleLayers
                );

                if (hit.collider != null)
                    return false;
            }
        }
        else
        {
            directionToTarget.Normalize();

            // Calculate angle
            float angle = Vector3.Angle(_transform.forward, directionToTarget);

            // Check if within field of view angle
            if (angle > FieldOfViewAngle * 0.5f)
                return false;

            // Check for obstacles if enabled
            if (CheckForObstacles)
            {
                RaycastHit hit;
                if (Physics.Raycast(
                    _transform.position,
                    directionToTarget,
                    out hit,
                    distanceToTarget,
                    ObstacleLayers))
                {
                    // If hit something that isn't the target, view is blocked
                    if (hit.collider.gameObject != target)
                        return false;
                }
            }
        }

        // If we got here, target is visible
        return true;
    }

    // Get all currently visible targets
    public GameObject[] GetVisibleTargets()
    {
        GameObject[] targets = new GameObject[_visibleTargets.Count];
        _visibleTargets.CopyTo(targets);
        return targets;
    }

    // Check if a specific target is visible
    public bool IsTargetVisible(GameObject target)
    {
        return _visibleTargets.Contains(target);
    }

    // Get the number of visible targets
    public int GetVisibleTargetCount()
    {
        return _visibleTargets.Count;
    }

    // Force a check for all targets
    public void ForceCheck()
    {
        CheckLineOfSight();
    }

    // Draw debug visualization
    private void OnDrawGizmosSelected()
    {
        if (!ShowVisionCone)
            return;

        Transform transform = this.transform;
        Vector3 position = transform.position;

        // Choose color based on whether targets are visible
        Gizmos.color = Application.isPlaying && _visibleTargets.Count > 0
            ? AlertVisionColor
            : NormalVisionColor;

        // For 2D visualization
        if (_is2D || GetComponent<Rigidbody2D>() != null || GetComponent<Collider2D>() != null)
        {
            // Draw a 2D vision cone
            Vector3 forward = base.transform.right;

            // Calculate cone directions
            float halfFOV = FieldOfViewAngle * 0.5f * Mathf.Deg2Rad;
            Vector3 rightDir = new Vector3(
                Mathf.Cos(halfFOV) * forward.x - Mathf.Sin(halfFOV) * forward.y,
                Mathf.Sin(halfFOV) * forward.x + Mathf.Cos(halfFOV) * forward.y,
                0
            );

            Vector3 leftDir = new Vector3(
                Mathf.Cos(-halfFOV) * forward.x - Mathf.Sin(-halfFOV) * forward.y,
                Mathf.Sin(-halfFOV) * forward.x + Mathf.Cos(-halfFOV) * forward.y,
                0
            );

            // Draw main view line
            Gizmos.DrawRay(position, forward * ViewDistance);

            // Draw the edges of the vision cone
            Gizmos.DrawRay(position, rightDir * ViewDistance);
            Gizmos.DrawRay(position, leftDir * ViewDistance);

            // Draw arc connecting the edges (approximated with lines)
            int arcSegments = 20;
            Vector3 prevDir = rightDir;

            for (int i = 1; i <= arcSegments; i++)
            {
                float t = (float)i / arcSegments;
                float angle = Mathf.Lerp(halfFOV, -halfFOV, t);

                Vector3 nextDir = new Vector3(
                    Mathf.Cos(angle) * forward.x - Mathf.Sin(angle) * forward.y,
                    Mathf.Sin(angle) * forward.x + Mathf.Cos(angle) * forward.y,
                    0
                );

                Gizmos.DrawLine(
                    position + prevDir * ViewDistance,
                    position + nextDir * ViewDistance
                );

                prevDir = nextDir;
            }
        }
        else
        {
            // Draw a 3D vision cone
            Vector3 forward = base.transform.forward;

            // Draw main view line
            Gizmos.DrawRay(position, forward * ViewDistance);

            // Calculate cone directions
            Quaternion leftRayRotation = Quaternion.AngleAxis(-FieldOfViewAngle * 0.5f, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(FieldOfViewAngle * 0.5f, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * forward;
            Vector3 rightRayDirection = rightRayRotation * forward;

            // Draw the edges of the vision cone
            Gizmos.DrawRay(position, leftRayDirection * ViewDistance);
            Gizmos.DrawRay(position, rightRayDirection * ViewDistance);

            // Draw arc connecting the edges (approximated with lines)
            int arcSegments = 20;
            Vector3 prevDir = rightRayDirection;

            for (int i = 1; i <= arcSegments; i++)
            {
                float t = (float)i / arcSegments;
                Quaternion rotation = Quaternion.Slerp(rightRayRotation, leftRayRotation, t);
                Vector3 nextDir = rotation * forward;

                Gizmos.DrawLine(
                    position + prevDir * ViewDistance,
                    position + nextDir * ViewDistance
                );

                prevDir = nextDir;
            }
        }

        // Draw connections to visible targets
        if (ShowRaysToTargets && Application.isPlaying)
        {
            Gizmos.color = Color.red;

            foreach (GameObject target in _visibleTargets)
            {
                if (target != null)
                {
                    Gizmos.DrawLine(position, target.transform.position);
                }
            }
        }
    }
}
