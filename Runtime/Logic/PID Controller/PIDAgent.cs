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
                //IsAutotuningActive = true;
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

        public void UpdateMainThrust(float forwardVelocity, float targetVelocity, float thrustMofifier = 1.0f, ForceMode forceMode = ForceMode.Force)
        {
            _thrust = _mainThrustController.UpdateVelocity(new Vector3(0f, 0f, forwardVelocity), new Vector3(0f, 0f, targetVelocity), Time.fixedDeltaTime);

            // This uses the main thruster to move the spacecraft forward, and only forward.
            if (_thrust.z < 0)
            {
                //    return;
                _rigidbody.AddForce(-transform.forward * _thrust.magnitude * thrustMofifier, forceMode);
            }
            else
            {
                _rigidbody.AddForce(transform.forward * _thrust.magnitude * thrustMofifier, forceMode);
            }
        }

        public void UpdateSideThrust(float sidewaysVelocity, float desiredSidewaysVelocity, float thrustMofifier = 1.0f, ForceMode forceMode = ForceMode.Force)
        {
            float sideThrust = _sideThrustController.Update(sidewaysVelocity, desiredSidewaysVelocity, Time.fixedDeltaTime, DerivativeMeasurement.Velocity);
            //_sideThrust = new Vector3(sideThrust, 0f, 0f);
            _rigidbody.AddForce(transform.right * sideThrust * thrustMofifier, forceMode);
        }

        public void UpdateUpThrust(Vector3 upVelocity, Vector3 desiredUpVelocity, float thrustModifier = 1.0f, ForceMode forceMode = ForceMode.Force)
        {
            _upThrust = _upThrustController.Update(upVelocity, desiredUpVelocity, Time.fixedDeltaTime);
            _rigidbody.AddForce(transform.up * _upThrust.magnitude, forceMode);
        }

        public void UpdateUpThrust(float upVelocity, float desiredUpVelocity, float thrustMofifier = 1.0f, ForceMode forceMode = ForceMode.Force)
        {
            float upThrust = _upThrustController.Update(upVelocity, desiredUpVelocity, Time.fixedDeltaTime, DerivativeMeasurement.Velocity);
            //_upThrust = new Vector3(0f, upThrust, 0f);
            _rigidbody.AddForce(transform.up * upThrust * thrustMofifier, forceMode);
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
            _mainThrustController?.Reset();
            _sideThrustController?.Reset();
            _upThrustController?.Reset();
            _torqueController?.Reset();
        }
    }
}