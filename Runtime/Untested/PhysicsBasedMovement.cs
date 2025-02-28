using UnityEngine;

/// <summary>
/// Physics-based movement controller using forces.
/// Good for objects that should follow physics rules (cars, boats, etc.)
/// Attach to any GameObject with a Rigidbody component.
/// </summary>
public class PhysicsBasedMovement : MonoBehaviour
{
    [Header("Movement Forces")]
    [Tooltip("Force applied when moving forward")]
    public float AccelerationForce = 10f;
    
    [Tooltip("Force applied when turning")]
    public float TurnForce = 5f;
    
    [Tooltip("Maximum speed the object can reach")]
    public float MaxSpeed = 15f;
    
    [Header("Drag Settings")]
    [Tooltip("Drag applied when no input is detected")]
    public float BrakingDrag = 3f;
    
    [Tooltip("Normal drag when moving")]
    public float MovingDrag = 0.5f;
    
    // Reference to the Rigidbody component
    private Rigidbody _rb;
    
    // Input values
    private float _forwardInput;
    private float _turnInput;
    
    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Get reference to the Rigidbody component
        _rb = GetComponent<Rigidbody>();
        
        // If no Rigidbody is attached, add one
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
            _rb.mass = 1f;
            _rb.linearDamping = MovingDrag;
            _rb.angularDamping = 0.5f;
            
            Debug.Log("Rigidbody added automatically. Adjust physics properties in the inspector for best results!");
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Get input for movement
        _forwardInput = Input.GetAxis("Vertical");
        _turnInput = Input.GetAxis("Horizontal");
        
        // Adjust drag based on input
        _rb.linearDamping = Mathf.Approximately(_forwardInput, 0f) ? BrakingDrag : MovingDrag;
    }
    
    // FixedUpdate is used for physics updates
    private void FixedUpdate()
    {
        // Calculate current speed (local forward direction only)
        float _currentSpeed = Vector3.Dot(_rb.linearVelocity, transform.forward);
        
        // Apply forward/backward force if not exceeding max speed
        if (Mathf.Abs(_currentSpeed) < MaxSpeed || Mathf.Sign(_forwardInput) != Mathf.Sign(_currentSpeed))
        {
            // Add force in the forward direction
            _rb.AddForce(transform.forward * _forwardInput * AccelerationForce);
        }
        
        // Apply turning force (only when moving)
        if (_rb.linearVelocity.magnitude > 0.1f)
        {
            // Use torque to rotate around the up axis
            _rb.AddTorque(transform.up * _turnInput * TurnForce);
        }
    }
}
