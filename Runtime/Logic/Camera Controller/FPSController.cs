using UnityEngine;

namespace Sleep0.Logic
{
    public class FPSController : MonoBehaviour
    {
        [Header("Mouse")]
        [SerializeField] private bool _isMouseMovementEnabled = true;
        [SerializeField] private float _mouseSensitivity = 2.0f;
        [SerializeField] private bool _clampVerticalRotation = true;
        [SerializeField] private float _verticalRange = 60.0f;
        [SerializeField] private bool _clampHorizontalRotation = false;
        [SerializeField] private float _horizontalRange = 180.0f;
        [Header("Movement")]
        [SerializeField] private bool _isMovementEnabled = true;
        [SerializeField] private float _walkSpeed = 5.0f;
        [SerializeField] private float _sprintSpeed = 10.0f;
        [Header("UI")]
        [SerializeField] private LayerMask _interactibeUILayer;

        private PlayerInputActions _inputActions;

        private float _movementSpeed => _isSprinting ? _sprintSpeed : _walkSpeed;
        private float _verticalRotation = 0;
        private float _horizontalRotation = 0;
        private bool _isSprinting = false;

        protected virtual void Awake()
        {
            _inputActions = new PlayerInputActions();
        }

        protected virtual void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _verticalRotation = transform.localEulerAngles.x;
            _horizontalRotation = transform.localEulerAngles.y;
        }

        protected virtual void OnEnable()
        {
            _inputActions?.Player.Enable();
        }

        protected virtual void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        protected virtual void Update()
        {
            if (_isMouseMovementEnabled)
            {
                LockCursor();
                UpdateMouseMovement();
            }
            else
            {
                UnlockCursor();
            }

            if (_isMovementEnabled)
                UpdateKeysMovement();
        }

        private void UpdateMouseMovement()
        {
            // Rotation
            Vector2 look = _inputActions.Player.Look.ReadValue<Vector2>() * (_mouseSensitivity * Time.deltaTime);

            _verticalRotation -= look.y * _mouseSensitivity;
            if (_clampVerticalRotation)
                _verticalRotation = Mathf.Clamp(_verticalRotation, -_verticalRange, _verticalRange);
            _horizontalRotation += look.x * _mouseSensitivity;
            if (_clampHorizontalRotation)
                _horizontalRotation = Mathf.Clamp(_horizontalRotation, -_horizontalRange, _horizontalRange);

            transform.localRotation = Quaternion.Euler(_verticalRotation, _horizontalRotation, 0);
        }

        private void UpdateKeysMovement()
        {
            // Movement
            _isSprinting = _inputActions.Player.SprintToggle.ReadValue<float>() > 0.5f;

            Vector2 movement = _inputActions.Player.Move.ReadValue<Vector2>() * (_movementSpeed * Time.deltaTime);

            Vector3 speed = new Vector3(movement.x, 0, movement.y);
            speed = transform.rotation * speed;

            transform.position += speed * Time.deltaTime;
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}