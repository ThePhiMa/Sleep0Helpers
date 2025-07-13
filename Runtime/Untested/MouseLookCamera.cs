using UnityEngine;

/// <summary>
/// First-person mouse look camera controller.
/// Attach to the main camera or a parent object.
/// </summary>
public class MouseLookCamera : MonoBehaviour
{
    [Header("Look Settings")]
    [Tooltip("Mouse sensitivity for looking around")]
    [Range(0.1f, 10f)]
    public float MouseSensitivity = 2f;
    
    [Tooltip("Invert the Y axis of the mouse (looking up/down)")]
    public bool InvertY = false;
    
    [Header("Rotation Limits")]
    [Tooltip("Limit how far up the player can look (in degrees)")]
    [Range(0f, 90f)]
    public float UpperLookLimit = 80f;
    
    [Tooltip("Limit how far down the player can look (in degrees)")]
    [Range(0f, 90f)]
    public float LowerLookLimit = 80f;
    
    // Camera rotation values
    private float _rotationX = 0f;
    private float _rotationY = 0f;
    
    // Reference to the camera transform (if not on the same object)
    private Transform _cameraTransform;
    
    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // If this script is on the camera itself, use this transform
        // Otherwise, try to find the camera in children
        _cameraTransform = GetComponent<Camera>() != null ? transform : GetComponentInChildren<Camera>()?.transform;
        
        if (_cameraTransform == null)
        {
            _cameraTransform = transform; // Default to this transform if no camera is found
            Debug.LogWarning("No camera found. Mouse look will rotate this object directly.");
        }
        
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        // Initialize rotation values based on current rotation
        _rotationY = transform.eulerAngles.y;
        _rotationX = _cameraTransform.localEulerAngles.x;
        
        // Fix initial rotation if it's outside our desired range
        if (_rotationX > 180f)
        {
            _rotationX -= 360f;
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Get mouse input
        float _mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
        float _mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * (InvertY ? 1f : -1f);
        
        // Update rotation values based on mouse movement
        _rotationY += _mouseX;
        _rotationX += _mouseY;
        
        // Clamp the vertical rotation (looking up/down)
        _rotationX = Mathf.Clamp(_rotationX, -UpperLookLimit, LowerLookLimit);
        
        // Apply rotation to the player object (horizontal rotation)
        transform.rotation = Quaternion.Euler(0f, _rotationY, 0f);
        
        // Apply rotation to the camera (vertical rotation)
        _cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        
        // Allow cursor unlock with Escape key (for testing)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }
    
    // Toggle the cursor lock state (useful for menus)
    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
