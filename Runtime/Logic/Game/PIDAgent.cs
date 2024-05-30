using Sleep0.Math;
using System;
using UnityEngine;

namespace Sleep0.Logic
{
    [RequireComponent(typeof(Rigidbody))]
    public class PIDAgent : ManagedBehaviour, IManagedUpdatable
    {
        public enum PIDType
        {
            MainThrust,
            SideThrust,
            Torque
        }

        public bool UpdatePIDValuesEachFrame = true;

        public float MaxDecelerationDistancePercent => _maxDecelerationDistance;
        public float MaxAngleToTarget => _maxAngleToTarget;
        public float StoppingDistance => _stoppingDistance;
        public Vector3 Thrust => _thrust;
        public Vector3 SideThrust => _sideThrust;
        public Vector3 UpThrust => _upThrust;
        public Vector3 Torque => _torque;
        public Vector3 CurrentVelocity => _rigidbody != null ? _rigidbody.velocity : Vector3.zero;
        public Transform CurrentTarget => _currentTarget;

        public PIDControllerVector3 MainThrustController => _mainThrustController;
        public PIDController SideThrustController => _sideThrustController;
        public PIDController UpThrustController => _upThrustController;
        public PIDControllerQuaternion TorqueController => _torqueController;

        public int UpdateOrder => 0;

        public bool IsAutotuningActive;

        [Header("PID Agent Settings")]
        [SerializeField] private float _maxDecelerationDistance = 10f;
        [SerializeField] private float _maxAngleToTarget = 10f;
        [SerializeField] private float _stoppingDistance = 10f;
        [SerializeField] private DerivativeMeasurement _derivativeMeasurement = DerivativeMeasurement.Velocity;
        [Header("PID Values")]
        [SerializeField] private PIDValuesSO _pidMainThrustValues;
        [SerializeField] private PIDValuesSO _pidSideThrustValues;
        [SerializeField] private PIDValuesSO _pidTorqueValues;

        private Rigidbody _rigidbody;
        private Vector3 _thrust = Vector3.zero;
        private Vector3 _sideThrust = Vector3.zero;
        private Vector3 _upThrust = Vector3.zero;
        private Vector3 _torque = Vector3.zero;
        private Transform _currentTarget;

        private PIDControllerVector3 _mainThrustController;
        private PIDController _sideThrustController;
        private PIDController _upThrustController;
        private PIDControllerQuaternion _torqueController;
        private PIDAutotuner _autotuner;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _mainThrustController = new PIDControllerVector3(_pidMainThrustValues);
            _sideThrustController = new PIDController(_pidSideThrustValues);
            _upThrustController = new PIDController(_pidSideThrustValues);
            _torqueController = new PIDControllerQuaternion(_pidTorqueValues);

            _autotuner = new PIDAutotuner(_pidMainThrustValues, _mainThrustController, PIDAutotuner.MethodType.ZieglerNichols);
        }

        public void ManagedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                IsAutotuningActive = true;
            }

            if (IsAutotuningActive)
            {
                Debug.Log("Autotuning...");
                IsAutotuningActive = !_autotuner.TuningUpdate();
            }

            var mult = 10f;
            //using (Draw.InLocalSpace(transform))
            //{
            //    Draw.Arrow(Vector3.zero, Vector3.forward * _thrust.magnitude, Color.blue);
            //    Draw.Arrow(Vector3.zero, Vector3.right * _sideThrust.magnitude * mult, Color.red);
            //    Draw.Arrow(Vector3.zero, Vector3.up * _upThrust.magnitude * mult, Color.green);

            //    //Draw.Arrow(Vector3.zero, _torque * mult, Color.yellow);
            //}

            if (UpdatePIDValuesEachFrame)
            {
                _mainThrustController.SetPIDValues(_pidMainThrustValues);
                _torqueController.SetPIDValues(_pidTorqueValues);
            }
        }

        private void FixedUpdate()
        {
            _thrust = Vector3.zero;
            _sideThrust = Vector3.zero;
            _upThrust = Vector3.zero;
            _torque = Vector3.zero;

            return;

            Vector3 targetPosition = _currentTarget.position;
            Vector3 currentPosition = transform.position;

            Vector3 direction = targetPosition - currentPosition;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            //_torque = _torqueController.Update(_rigidbody.angularVelocity, transform.rotation, targetRotation, Time.fixedDeltaTime);
            //_rigidbody.AddTorque(_torque, ForceMode.Force);

            // ---------------------------- Position Controller Test 1 ----------------------------
            //Vector3 projectedToTarget = Vector3.Project(direction, transform.forward);
            //Vector3 error = targetPosition - currentPosition;
            //_thrust = _positionController.Update(error, Time.fixedDeltaTime);
            //_rigidbody.AddRelativeForce(_thrust, ForceMode.Acceleration); 

            // ---------------------------- Position Controller Test 2 ----------------------------
            // Calculate the desired velocity based on the distance to the target
            //float distance = direction.magnitude;
            //Vector3 desiredVelocity = direction.normalized * Mathf.Sqrt(2 * distance * _positionController.KP.y);

            //// Calculate the error based on the difference between the desired velocity and the current velocity
            //Vector3 velocityError = desiredVelocity - _rigidbody.velocity;

            //_thrust = _positionController.Update(velocityError, Time.fixedDeltaTime);
            //_rigidbody.AddRelativeForce(_thrust, ForceMode.Acceleration);

            // ---------------------------- Position Controller Test 3 ----------------------------
            // Calculate the desired velocity based on the distance to the target
            //float distance = direction.magnitude;
            //Vector3 desiredVelocity = direction.normalized * Mathf.Sqrt(2 * distance * _mainThrustController.KP.z);

            // Calculate the error based on the difference between the desired velocity and the current velocity
            //Vector3 velocityError = desiredVelocity - _rigidbody.velocity;

            //// Calculate the thrust needed to correct the velocity error
            //_thrust = _mainThrustController.Update(targetPosition, currentPosition, Time.fixedDeltaTime);

            //// Apply the thrust in the direction of the spacecraft's forward vector
            //// This simulates the main thruster
            //_rigidbody.AddForce(transform.forward * _thrust.magnitude, ForceMode.Force);

            //// Calculate the steering force needed to correct the velocity error
            //// This is the component of the thrust that is perpendicular to the spacecraft's forward vector
            //Vector3 steeringForce = _thrust - Vector3.Project(_thrust, transform.forward);

            //// Apply the steering force
            //// This simulates the side thrusters
            ////_rigidbody.AddForce(steeringForce, ForceMode.Force);

            //Vector3 sidewaysVelocity = Vector3.ProjectOnPlane(_rigidbody.velocity, transform.forward);
            //Vector3 desiredSidewaysVelocity = Vector3.zero; // Assuming you want to stop any sideways motion
            //_sideThrust = _sideThrustController.Update(sidewaysVelocity, desiredSidewaysVelocity, Time.fixedDeltaTime);
            //_rigidbody.AddForce(transform.right * _sideThrust.magnitude, ForceMode.Force);


            // The idea I have right now is:
            // 1.) Check if the distance can be covered with the 6d side thrusters if the distance is not to far away
            // 2.) If the distance is too far away, use the main thruster to get closer to the target
            //     if the forward thrust is oscillating back to 0 (meaning the D value has kicked in), turn the main thruster off
            // 3.) Turn the ship around 180 degrees as fast as possible and use the main thruster to slow down the ship
            // 4.) When the ship is close to the target, turn off main thrusters and use the 6d side thrusters to align the ship with the target
            // Important is: The main thruster should only be used to get closer to the target, not to align the ship with the target
            // Also: Torque is the most important force, te main thrust comes after and is proportionally inverse to the torque force
            // (I.e. if the torque is high, the main thrust should be low and vice versa)
            // And then the side thust is proportionally inverse to the main thrust AND the torque force


            // 1. First calculate and apply the torque
            _torque = _torqueController.Update(_rigidbody.angularVelocity, transform.rotation, targetRotation, Time.fixedDeltaTime);
            //_rigidbody.AddTorque(_torque, ForceMode.Force);

            // 2. Then calculate the forward thrust, which is inversely proportional to the torque
            float reductionFactorTorque = 1 - Mathf.Sqrt(Mathf.Clamp(_torque.magnitude, 0, 1));
            _thrust = _mainThrustController.UpdatePosition(currentPosition, targetPosition, Time.fixedDeltaTime); // * reductionFactorTorque;
            _rigidbody.AddForce(transform.forward * _thrust.magnitude, ForceMode.Force);

            // 3. Lastly, calculate the side thrust, which is only for slight adjustments
            Vector3 sidewaysVelocity = Vector3.ProjectOnPlane(_rigidbody.velocity, transform.forward);
            Vector3 desiredSidewaysVelocity = Vector3.zero; // Assuming you want to stop any sideways motion
            _sideThrust = _sideThrustController.Update(sidewaysVelocity, desiredSidewaysVelocity, Time.fixedDeltaTime);
            //_rigidbody.AddForce(transform.right * _sideThrust.magnitude, ForceMode.Force);

            Vector3 upVelocity = Vector3.Project(_rigidbody.velocity, transform.up);
            Vector3 desiredUpVelocity = Vector3.zero; // Assuming you want to stop any up motion
            _upThrust = _upThrustController.Update(upVelocity, desiredUpVelocity, Time.fixedDeltaTime);
            //_rigidbody.AddForce(transform.up * _upThrust.magnitude, ForceMode.Force);
        }

        public void UpdateTorque(Quaternion targetRotation, float torqueMofifier = 1.0f, ForceMode forceMode = ForceMode.Force)
        {
            _torque = _torqueController.Update(_rigidbody.angularVelocity, _rigidbody.transform.rotation, targetRotation, Time.fixedDeltaTime);
            _rigidbody.AddTorque(_torque * torqueMofifier, forceMode);
        }

        public void UpdateTorque(Vector3 currentAngularVelocity, Vector3 targetAngularVelocity)
        {
            _torque = _torqueController.Update(currentAngularVelocity, targetAngularVelocity, Time.fixedDeltaTime);
            _rigidbody.AddTorque(_torque, ForceMode.Force);
        }

        //public void UpdateMainThrust(Vector3 targetPosition, float thrustMofifier = 1.0f, ForceMode forceMode = ForceMode.Force)
        //{
        //    _thrust = _mainThrustController.Update(transform.position, targetPosition, Time.fixedDeltaTime);
        ////    _rigidbody.AddForce(transform.forward * _thrust.magnitude * thrustMofifier, forceMode);
        //    _rigidbody.AddForce(_thrust * thrustMofifier, forceMode);
        //}

        public void UpdateMainThrust(float forwardVelocity, float targetVelocity, float thrustMofifier = 1.0f, ForceMode forceMode = ForceMode.Force)
        {
            _thrust = _mainThrustController.UpdateVelocity(new Vector3(0f, 0f, forwardVelocity), new Vector3(0f, 0f, targetVelocity), Time.fixedDeltaTime);

            //_thrust = new Vector3(0f, 0f, _thrust.z);

            // This uses the main thruster to move the spacecraft forward, and only forward.
            if (_thrust.z < 0)
                return;

            _rigidbody.AddForce(transform.forward * _thrust.magnitude * thrustMofifier, forceMode);
            //_rigidbody.AddForce(_thrust * thrustMofifier, forceMode);
        }

        public void UpdateSideThrust(float sidewaysVelocity, float desiredSidewaysVelocity, float thrustMofifier = 1.0f, ForceMode forceMode = ForceMode.Force)
        {
            float sideThrust = _sideThrustController.Update(sidewaysVelocity, desiredSidewaysVelocity, Time.fixedDeltaTime, DerivativeMeasurement.Velocity);
            //_rigidbody.AddForce(transform.right * _sideThrust.magnitude * thrustMofifier, forceMode);
            _sideThrust = new Vector3(sideThrust, 0f, 0f);
            _rigidbody.AddForce(transform.right * sideThrust * thrustMofifier, forceMode);
        }

        public void UpdateUpThrust(Vector3 upVelocity, Vector3 desiredUpVelocity, float thrustModifier = 1.0f, ForceMode forceMode = ForceMode.Force)
        {
            _upThrust = _upThrustController.Update(upVelocity, desiredUpVelocity, Time.fixedDeltaTime);
            _rigidbody.AddForce(_rigidbody.transform.up * _upThrust.magnitude, forceMode);
        }

        public void ChangePValue(float valueChange, PIDType pidType)
        {
            switch (pidType)
            {
                case PIDType.MainThrust:
                    _pidMainThrustValues.PGain += valueChange;
                    break;
                case PIDType.SideThrust:
                    _pidSideThrustValues.PGain += valueChange;
                    break;
                case PIDType.Torque:
                    _pidTorqueValues.PGain += valueChange;
                    break;
            }
        }

        public void StartAutoTuning(Action onFinished)
        {
            _autotuner.SetMethodType(PIDAutotuner.MethodType.ZieglerNichols);
            IsAutotuningActive = true;
        }

        public void SetCurrentAutoTarget(Transform target)
        {
            _currentTarget = target;
        }

        public void Reset()
        {
            _mainThrustController.Reset();
            _sideThrustController.Reset();
            _upThrustController.Reset();
            _torqueController.Reset();
        }
    }
}