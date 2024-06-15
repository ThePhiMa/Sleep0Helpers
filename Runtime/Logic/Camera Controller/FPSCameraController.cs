using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Sleep0.Logic
{
    public class FPSCameraController : MonoBehaviour
    {
        [Header("Mouse")]
        [SerializeField] private float _mouseSensitivity = 2.0f;
        [SerializeField] private bool _clampVerticalRotation = true;
        [SerializeField] private float _verticalRange = 60.0f;
        [SerializeField] private bool _clampHorizontalRotation = false;
        [SerializeField] private float _horizontalRange = 180.0f;
        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 5.0f;
        [SerializeField] private float _sprintSpeed = 10.0f;
        [Header("UI")]
        [SerializeField] private LayerMask _interactibeUILayer;

        private PlayerInputActions _inputActions;

        private float _movementSpeed => _isSprinting ? _sprintSpeed : _walkSpeed;
        private float _verticalRotation = 0;
        private float _horizontalRotation = 0;
        private bool _isSprinting = false;

        private void Awake()
        {
            // Activate inputs
            _inputActions = new PlayerInputActions();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }

        protected void OnEnable()
        {
            _inputActions?.Player.Enable();
        }

        protected void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        void Update()
        {
            UpdateMouseMovement();
            UpdateKeysMovement();
        }

        private void FixedUpdate()
        {
            HandleRaycast();
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

        private void HandleRaycast()
        {
            if (_inputActions.UI.Click.WasPerformedThisFrame())
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = new Vector2(Screen.width / 2, Screen.height / 2)
                };

                // Perform raycast
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                // Process results
                foreach (RaycastResult result in results)
                {
                    // Check if the result is a UI element on the right layer and try to click it
                    if (_interactibeUILayer.Contains(result.gameObject.layer))
                    {
                        // Simulate a click on the detected UI element
                        var clickHandler = result.gameObject.GetComponent<IPointerClickHandler>();
                        clickHandler?.OnPointerClick(pointerData);
                        break;
                    }
                }
            }
        }
    }
}