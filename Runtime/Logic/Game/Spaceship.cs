using Sleep0.Logic.FiniteStateMachine;
using UnityEngine;

namespace Sleep0.Logic.Game
{
    [RequireComponent(typeof(PIDAgent))]
    public class Spaceship : ManagedBehaviour, IManagedUpdatable
    {
        [Header("Targets")]
        [SerializeField] private Transform[] _targets;
        [Header("Speed Multipliers")]
        [SerializeField] private float _mainThrustMultiplier = 1.0f;
        [SerializeField] private float _sideThrustMultiplier = 1.0f;
        [SerializeField] private float _upThrustMultiplier = 1.0f;
        [SerializeField] private float _torqueMultiplier = 1.0f;
        [Header("Thuster Flames")]
        [SerializeField] private GameObject _flameLeft;
        [SerializeField] private GameObject _flameRight;
        [SerializeField] private GameObject _flameUp;
        [SerializeField] private GameObject _flameDown;
        [SerializeField] private GameObject _flameFront;
        [SerializeField] private GameObject _flameBack;
        [SerializeField] private float flameSize = 1.0f;
        [SerializeField] private float _flameLerpSpeed = 1.0f;
        [SerializeField] private float _flameMinSize = 0.01f;

        private Rigidbody _rigidbody;
        private PIDAgent _pidAgent;
        private int _currentTargetIndex = 2;

        private SpaceshipBaseState _currentState;
        private SpaceshipIdleState _idleState;
        private SpaceshipAutopilotMovingState _autopilotMovingState;

        public float MainThrustMultiplier => _mainThrustMultiplier;
        public float SideThrustMultiplier => _sideThrustMultiplier;
        public float UpThrustMultiplier => _upThrustMultiplier;
        public float TorqueMultiplier => _torqueMultiplier;

        public int UpdateOrder => 0;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _pidAgent = GetComponent<PIDAgent>();
        }

        private void Start()
        {
            if (_pidAgent == null)
                Debug.LogError("PIDAgent component not found");
            else
            {
                _idleState = new SpaceshipIdleState(_rigidbody, this, _pidAgent);
                _autopilotMovingState = new SpaceshipAutopilotMovingState(_rigidbody, this, _pidAgent);
            }

            ChangeTarget();

            ResetFlames();

            SetState(_autopilotMovingState);
        }

        public void ManagedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeTarget();
            }

            if (_currentState == null)
                SetState(_idleState);

            _currentState.Execute();

            SetVisualFlameOutput(_rigidbody.velocity);
        }

        private void ResetFlames()
        {
            SetScale(_flameLeft, 0.1f);
            SetScale(_flameRight, 0.1f);
            SetScale(_flameUp, 0.1f);
            SetScale(_flameDown, 0.1f);
            SetScale(_flameFront, 0.1f);
            SetScale(_flameBack, 0.1f);
        }

        private void ChangeTarget()
        {
            _currentTargetIndex = (_currentTargetIndex + 1) % _targets.Length;
            Debug.Log($"New target: {_targets[_currentTargetIndex].name}");

            _pidAgent.SetCurrentAutoTarget(_targets[_currentTargetIndex]);

            SetState(_autopilotMovingState);
        }

        public void SetState(SpaceshipBaseState state)
        {
            if (_currentState != null)
                _currentState.Exit();

            _currentState = state;
            _currentState.Enter();
        }

        private Vector3 _lastVelocity;
        public void SetVisualFlameOutput(Vector3 _forceOutput)
        {
            _forceOutput = transform.InverseTransformDirection(_rigidbody.velocity);

            // Calculate the thrust in the direction of each thruster
            //float leftThrust = Vector3.Dot(_forceOutput, -_flameLeft.transform.forward);
            //float rightThrust = Vector3.Dot(_forceOutput, _flameRight.transform.forward);
            //float upThrust = Vector3.Dot(_forceOutput, _flameUp.transform.forward);
            //float downThrust = Vector3.Dot(_forceOutput, -_flameDown.transform.forward);
            //float frontThrust = Vector3.Dot(_forceOutput, -_flameFront.transform.forward);
            //float backThrust = Vector3.Dot(_forceOutput, _flameBack.transform.forward);

            //// Scale the flames based on the thrust
            //SetScale(_flameLeft, Mathf.Max(0, leftThrust));
            //SetScale(_flameRight, Mathf.Max(0, rightThrust));
            //SetScale(_flameUp, Mathf.Max(0, upThrust));
            //SetScale(_flameDown, Mathf.Max(0, downThrust));
            //SetScale(_flameFront, Mathf.Max(0, frontThrust));
            //SetScale(_flameBack, Mathf.Max(0, _forceOutput.z));

            //SetFlameScale(_flameLeft, _flameRight, _forceOutput.x);
            //SetFlameScale(_flameUp, _flameDown, _forceOutput.y);
            //SetFlameScale(_flameFront, _flameBack, _forceOutput.z);

            var localThrust = _pidAgent.Thrust; //transform.InverseTransformDirection(_pidAgent.Thrust);
                                                //Debug.Log("Local thrust: " + localThrust.ToString());

            // Calculate the rate of change of velocity (acceleration)
            Vector3 acceleration = (_forceOutput - _lastVelocity) / Time.deltaTime;
            _lastVelocity = _forceOutput;

            // Convert acceleration to local space
            Vector3 localAcceleration = acceleration; // transform.InverseTransformDirection(acceleration);

            // Set the scale of the flames based on the acceleration
            SetFlameScale(_flameLeft, _flameRight, -localAcceleration.x);
            SetFlameScale(_flameUp, _flameDown, localAcceleration.y);
            SetFlameScale(_flameFront, _flameBack, localAcceleration.z);
        }

        //private void SetFlameScale(GameObject positiveAxisFlame, GameObject negativeAxisFlame, float forceOutput)
        //{
        //    if (forceOutput < -0.01f)
        //    {
        //        SetScale(positiveAxisFlame, Mathf.Abs(forceOutput));
        //        SetScale(negativeAxisFlame, 0.0f);
        //    }
        //    else if (forceOutput > 0.01f)
        //    {
        //        SetScale(negativeAxisFlame, forceOutput);
        //        SetScale(positiveAxisFlame, 0.0f);
        //    }
        //    else
        //    {
        //        SetScale(positiveAxisFlame, 0.0f);
        //        SetScale(negativeAxisFlame, 0.0f);
        //    }
        //}        

        private void SetFlameScale(GameObject positiveAxisFlame, GameObject negativeAxisFlame, float forceOutput)
        {
            float targetScale = Mathf.Abs(forceOutput);
            float currentScalePositive = positiveAxisFlame.transform.localScale.x;
            float currentScaleNegative = negativeAxisFlame.transform.localScale.x;

            if (forceOutput < -_flameMinSize)
            {
                float newScalePositive = Mathf.Lerp(currentScalePositive, targetScale, _flameLerpSpeed * Time.deltaTime);
                SetScale(positiveAxisFlame, newScalePositive);
                SetScale(negativeAxisFlame, 0.0f);
            }
            else if (forceOutput > _flameMinSize)
            {
                float newScaleNegative = Mathf.Lerp(currentScaleNegative, targetScale, _flameLerpSpeed * Time.deltaTime);
                SetScale(negativeAxisFlame, newScaleNegative);
                SetScale(positiveAxisFlame, 0.0f);
            }
            else
            {
                float newScalePositive = Mathf.Lerp(currentScalePositive, 0.0f, _flameLerpSpeed * Time.deltaTime);
                float newScaleNegative = Mathf.Lerp(currentScaleNegative, 0.0f, _flameLerpSpeed * Time.deltaTime);
                SetScale(positiveAxisFlame, newScalePositive);
                SetScale(negativeAxisFlame, newScaleNegative);
            }
        }

        private void SetScale(GameObject gameObject, float scale, float minScale = 0f, float maxScale = 1f)
        {
            scale = Mathf.Clamp(scale, minScale, maxScale);

            gameObject.transform.localScale = new Vector3(scale, scale, scale) * flameSize;
        }

        public string GetCurrentState()
        {
            return _currentState != null ? _currentState.ToString() : "None";
        }
    }
}