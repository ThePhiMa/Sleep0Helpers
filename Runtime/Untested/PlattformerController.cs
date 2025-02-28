using UnityEngine;

/// <summary>
/// Simple 2D platformer controller with jumping.
/// Attach to any GameObject with a Rigidbody2D and a Collider2D.
/// </summary>
public class PlatformerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second")]
    public float MoveSpeed = 5f;
    
    [Tooltip("Jump force applied")]
    public float JumpForce = 10f;
    
    [Header("Ground Check")]
    [Tooltip("Transform to check if player is grounded (usually an empty child at the feet)")]
    public Transform GroundCheck;
    
    [Tooltip("Layer mask for ground detection")]
    public LayerMask GroundLayer;
    
    [Tooltip("Radius of the ground check circle")]
    public float GroundCheckRadius = 0.2f;
    
    // Reference to the Rigidbody2D component
    private Rigidbody2D _rb;
    
    // Store movement input
    private float _horizontalInput;
    
    // Track if the player is grounded
    private bool _isGrounded;
    
    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Get reference to the Rigidbody2D component
        _rb = GetComponent<Rigidbody2D>();
        
        // If no Rigidbody2D is attached, add one
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Create a ground check object if not assigned
        if (GroundCheck == null)
        {
            GameObject checkObject = new GameObject("GroundCheck");
            checkObject.transform.parent = transform;
            checkObject.transform.localPosition = new Vector3(0, -1f, 0); // Position at feet
            GroundCheck = checkObject.transform;
            
            Debug.Log("Ground check created. Set the ground layer in the inspector!");
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Get horizontal input (A/D or Left/Right arrows)
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Check if player is on the ground
        _isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, GroundLayer);
        
        // Jump when the jump button is pressed and player is grounded
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f); // Reset any downward velocity
            _rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        }
    }
    
    // FixedUpdate is used for physics updates
    private void FixedUpdate()
    {
        // Move the character horizontally
        _rb.linearVelocity = new Vector2(_horizontalInput * MoveSpeed, _rb.linearVelocity.y);
    }
    
    // Draw gizmos for the ground check (visible in Scene view)
    private void OnDrawGizmosSelected()
    {
        if (GroundCheck != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(GroundCheck.position, GroundCheckRadius);
        }
    }
}
