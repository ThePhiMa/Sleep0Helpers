using UnityEngine;

namespace Sleep0.Math
{
    public class PIDController
    {
        [SerializeField] private PIDValuesSO _pidValues;

        private Vector3 _integral;
        private Vector3 _lastError;

        private Vector3 _kp;
        private Vector3 _ki;
        private Vector3 _kd;

        public PIDController()
        {
            SetPIDValues();
        }

        [ContextMenu("Calculate PID Values")]
        public void CalculatePIDValues()
        {
            // Ziegler-Nichols method
            _pidValues.PGain = 0.6f * _pidValues.PGain;
            _pidValues.IGain = 2f * _pidValues.PGain / _pidValues.OscillationTime;
            _pidValues.DGain = _pidValues.PGain * _pidValues.OscillationTime / 8f;
        }

        private void SetPIDValues()
        {
            _kp = new Vector3(_pidValues.PGain, _pidValues.PGain, _pidValues.PGain);
            _ki = new Vector3(_pidValues.IGain, _pidValues.IGain, _pidValues.IGain);
            _kd = new Vector3(_pidValues.DGain, _pidValues.DGain, _pidValues.DGain);
        }

        // Used for position control.
        public Vector3 Update(Vector3 setpoint, Vector3 actual, float deltaTime)
        {
            // Calculate error
            Vector3 error = setpoint - actual;

            // Proportional term
            Vector3 pOut = Vector3.Scale(_kp, error);

            // Integral term
            _integral += error * deltaTime;
            Vector3 iOut = Vector3.Scale(_ki, _integral);

            // Derivative term
            Vector3 derivative = (error - _lastError) / deltaTime;
            Vector3 dOut = Vector3.Scale(_kd, derivative);

            // Remember last error
            _lastError = error;

            // Add the three terms to get the total output
            Vector3 output = pOut + iOut + dOut;

            return output;
        }

        // Used for orientation control.
        public Vector3 Update(Quaternion setpoint, Quaternion actual, float deltaTime)
        {
            // Calculate error
            Quaternion errorQuat = setpoint * Quaternion.Inverse(actual);
            Vector3 error = ToEuler(errorQuat);

            // Proportional term
            Vector3 pOut = Vector3.Scale(_kp, error);

            // Integral term
            _integral += error * deltaTime;
            Vector3 iOut = Vector3.Scale(_ki, _integral);

            // Derivative term
            Vector3 derivative = (error - _lastError) / deltaTime;
            Vector3 dOut = Vector3.Scale(_kd, derivative);

            // Remember last error
            _lastError = error;

            // Add the three terms to get the total output
            Vector3 output = pOut + iOut + dOut;

            return output;
        }

        private Vector3 ToEuler(Quaternion q)
        {
            // Convert the quaternion to a rotation vector (axis-angle representation)
            Vector3 axis;
            float angle;
            q.ToAngleAxis(out angle, out axis);
            return axis * angle;
        }
    }
}