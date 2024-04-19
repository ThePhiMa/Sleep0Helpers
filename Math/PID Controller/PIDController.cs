using System;
using UnityEngine;

namespace Sleep0.Math
{
    [Serializable]
    public class PIDController
    {
        [SerializeField] private PIDValuesSO _pidValues;

        private Vector3 _positionIntegral;
        private Vector3 _positionLastError;

        private Vector3 _orientationIntegral;
        private Vector3 _orientationLastError;

        private Vector3 _kp;
        private Vector3 _ki;
        private Vector3 _kd;


        [ContextMenu("Calculate PID Values")]
        public void CalculatePIDValues()
        {
            // Ziegler-Nichols method
            _pidValues.PGain = 0.6f * _pidValues.PGain;
            _pidValues.IGain = 2f * _pidValues.PGain / _pidValues.OscillationTime;
            _pidValues.DGain = _pidValues.PGain * _pidValues.OscillationTime / 8f;

            SetLocalPIDValues();
        }

        /// <summary>
        /// Updates the PID controller for position control from the PIDValues scriptable object.
        /// </summary>
        public void SetLocalPIDValues()
        {
            _kp = new Vector3(_pidValues.PGain, _pidValues.PGain, _pidValues.PGain);
            _ki = new Vector3(_pidValues.IGain, _pidValues.IGain, _pidValues.IGain);
            _kd = new Vector3(_pidValues.DGain, _pidValues.DGain, _pidValues.DGain);
        }

        //PID coefficients
        public Vector3 proportionalGain;
        public Vector3 integralGain;
        public Vector3 derivativeGain;

        public float outputMin = -1;
        public float outputMax = 1;
        public float integralSaturation = 1;
        public DerivativeMeasurement derivativeMeasurement;

        public Vector3 valueLast;
        public Vector3 errorLast;
        public Vector3 integrationStored;
        public Vector3 velocity;  //only used for the info display
        public bool derivativeInitialized;

        public enum DerivativeMeasurement
        {
            Velocity,
            ErrorRateOfChange
        }

        /// <summary>
        /// Updates the PID controller for position control.
        /// </summary>
        /// <param name="currentValue">Current position values</param>
        /// <param name="targetValue">Target position values</param>
        /// <param name="dt">Time delta</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Vector3 UpdatePosition(Vector3 currentValue, Vector3 targetValue, float dt)
        {
            if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

            // Calculate error
            Vector3 error = targetValue - currentValue;

            // Proportional term
            Vector3 P = Vector3.Scale(_kp, error);

            _positionIntegral = Vector3.ClampMagnitude(_positionIntegral + error * dt, integralSaturation);
            // Integral term
            Vector3 I = Vector3.Scale(_ki, _positionIntegral);

            Vector3 errorRateOfChange = (error - _positionLastError) / dt;
            _positionLastError = error;

            Vector3 valueRateOfChange = (currentValue - valueLast) / dt;
            valueLast = currentValue;
            velocity = valueRateOfChange;

            Vector3 deriveMeasure = derivativeMeasurement == DerivativeMeasurement.Velocity
                ? -(currentValue - currentValue) / dt
                : errorRateOfChange;

            // Derivative term
            Vector3 D = Vector3.Scale(_kd, deriveMeasure);

            Vector3 result = P + I + D;

            return Vector3.ClampMagnitude(result, outputMax);
        }

        /// <summary>
        /// Updates the PID controller for orientation control.
        /// </summary>
        /// <param name="currentRotation">Current rotation values</param>
        /// <param name="targetRotation">Target rotation values</param>
        /// <param name="dt">Time delta</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Vector3 UpdateRotation(Quaternion currentRotation, Quaternion targetRotation, float dt)
        {
            if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

            // Calculate error
            Quaternion errorRotation = Quaternion.Inverse(currentRotation) * targetRotation;
            Vector3 error;
            float angle;
            errorRotation.ToAngleAxis(out angle, out error);
            if (angle > 180) angle -= 360;  // Convert angle to [-180, 180]
            error *= angle;                 // Scale the error vector by the angle

            // Proportional term
            Vector3 P = Vector3.Scale(_kp, error);

            // Integral term
            _orientationIntegral = Vector3.ClampMagnitude(_orientationIntegral + error * dt, integralSaturation);
            Vector3 I = Vector3.Scale(_ki, _orientationIntegral);

            // Derivative term
            Vector3 errorRateOfChange = (error - _orientationLastError) / dt;
            _orientationLastError = error;

            Vector3 D = Vector3.Scale(_kd, errorRateOfChange);

            Vector3 result = P + I + D;

            // Convert the result to a rotation vector (axis-angle representation)
            Quaternion resultRotation = Quaternion.Euler(Vector3.ClampMagnitude(result, outputMax));
            Vector3 resultAxis;
            float resultAngle;
            resultRotation.ToAngleAxis(out resultAngle, out resultAxis);
            if (resultAngle > 180) resultAngle -= 360; // Convert angle to [-180, 180]

            return resultAxis * resultAngle;
        }
    }
}
