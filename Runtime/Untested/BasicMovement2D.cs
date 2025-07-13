using UnityEngine;

/// <summary>
/// Simple 2D movement controller using WASD or arrow keys.
/// Attach to any GameObject with a Rigidbody2D component.
/// </summary>
public class BasicMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second")]
    public float MoveSpeed = 5f;

    [Tooltip("Set to true for top-down games, false for side-scrollers")]
    public bool UseTopDownMovement = true;

    // Reference to the Rigidbody2D component
    private Rigidbody2D _rb;

    // Store movement input
    private Vector2 _movementInput;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Get reference to the Rigidbody2D component
        _rb = GetComponent<Rigidbody2D>();

        // If no Rigidbody2D is attached, add one
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = UseTopDownMovement ? 0f : 1f; // Turn off gravity for top-down games
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Get horizontal and vertical input (WASD or arrow keys)
        float _horizontalInput = Input.GetAxisRaw("Horizontal");
        float _verticalInput = Input.GetAxisRaw("Vertical");

        // Create a Vector2 for the movement direction
        _movementInput = new Vector2(_horizontalInput, UseTopDownMovement ? _verticalInput : 0f);

        // Normalize to prevent faster diagonal movement
        if (_movementInput.magnitude > 1f)
        {
            _movementInput.Normalize();
        }
    }

    // FixedUpdate is used for physics updates
    private void FixedUpdate()
    {
        // Move the character using physics
        _rb.linearVelocity = new Vector2(
            _movementInput.x * MoveSpeed,
            UseTopDownMovement ? _movementInput.y * MoveSpeed : _rb.linearVelocity.y
        );
    }
}
