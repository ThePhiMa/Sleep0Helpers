using System;
using UnityEngine;

namespace Sleep0.Math
{
    public interface IPIDController
    {
        float GetError();
    }

    public class PIDController : IPIDController
    {
        private float _integral;
        private float _lastError;

        private float _kp;
        private float _ki;
        private float _kd;

        public float Kp => _kp;
        public float Ki => _ki;
        public float Kd => _kd;

        private float _integralMax = 100f;
        private float MaxOutput = 100f;

        public PIDController(PIDValuesSO pidValues)
        {
            SetPIDValues(pidValues);
        }

        [ContextMenu("Calculate PID Values")]
        public void CalculatePIDValues(ref PIDValuesSO pidValues)
        {
            // Ziegler-Nichols method
            pidValues.PGain = 0.6f * pidValues.PGain;
            pidValues.IGain = 2f * pidValues.PGain / pidValues.OscillationTime;
            pidValues.DGain = pidValues.PGain * pidValues.OscillationTime / 8f;

            SetPIDValues(pidValues);
        }

        public void SetPIDValues(PIDValuesSO pidValues)
        {
            _kp = pidValues.PGain;
            _ki = pidValues.IGain;
            _kd = pidValues.DGain;
        }

        public float GetError()
        {
            return _lastError;
        }

        public float outputMin = -1;
        public float outputMax = 1;
        public float integralSaturation;
        public DerivativeMeasurement derivativeMeasurement;

        public float valueLast;
        public float errorLast;
        public float integrationStored;
        public float velocity;  //only used for the info display
        public bool derivativeInitialized;

        public void Reset()
        {
            valueLast = 0;
            errorLast = 0;
            integrationStored = 0;
            velocity = 0;
            derivativeInitialized = false;
        }

        public float Update(float currentValue, float targetValue, float dt, DerivativeMeasurement derivativeMeasurement)
        {
            if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

            float error = targetValue - currentValue;

            //calculate P term
            float P = _kp * error;

            //calculate I term
            integrationStored = Mathf.Clamp(integrationStored + (error * dt), -integralSaturation, integralSaturation);
            float I = _ki * integrationStored;

            //calculate both D terms
            float errorRateOfChange = (error - errorLast) / dt;
            errorLast = error;

            float valueRateOfChange = (currentValue - valueLast) / dt;
            valueLast = currentValue;
            velocity = valueRateOfChange;

            //choose D term to use
            float deriveMeasure = 0;

            if (derivativeInitialized)
            {
                if (derivativeMeasurement == DerivativeMeasurement.Velocity)
                {
                    deriveMeasure = -valueRateOfChange;
                }
                else
                {
                    deriveMeasure = errorRateOfChange;
                }
            }
            else
            {
                derivativeInitialized = true;
            }

            float D = _kd * deriveMeasure;

            float result = P + I + D;

            return Mathf.Clamp(result, outputMin, outputMax);
        }

        public float Update3(float error, float delta, float deltaTime)
        {
            _integralMax = MaxOutput / _ki;

            // Calculate error
            //float error = setpoint - actual;

            // Proportional term
            //float pOut = _kp * error;

            //// Integral term
            //_integral += error * deltaTime;
            //float iOut = _ki * _integral;

            //// Derivative term
            //float derivative = (error - _lastError) / deltaTime;
            //float dOut = _kd * derivative;

            //// Remember last error
            //_lastError = error;

            //// Add the three terms to get the total output
            //float output = pOut + iOut + dOut;

            //return output;

            _integral += error * deltaTime;
            _integral = Mathf.Clamp(_integral, -_integralMax, _integralMax);

            float derivative = delta / deltaTime;
            float pOut = _kp * error;
            float iOut = _ki * _integral;
            float dOut = _kd * derivative;
            float output = pOut + iOut + dOut;

            output = Mathf.Clamp(output, -MaxOutput, MaxOutput);

            return output;
        }

        public float Update2(float currentVelocity, float targetVelocity, float deltaTime)
        {
            // Calculate error
            float error = targetVelocity - currentVelocity;

            _integral += error * deltaTime;
            _integral = Mathf.Clamp(_integral, -_integralMax, _integralMax);

            float derivative = (error - _lastError) / deltaTime;
            _lastError = error;

            float pOut = _kp * error;
            float iOut = _ki * _integral;
            float dOut = _kd * derivative;
            float output = pOut + iOut + dOut;

            output = Mathf.Clamp(output, -MaxOutput, MaxOutput);

            return output;
        }

        public Vector3 Update(Vector3 sidewaysVelocity, Vector3 targetSidewaysVelocity, float deltaTime)
        {
            // Calculate the error in the sideways direction
            float error = (targetSidewaysVelocity - sidewaysVelocity).magnitude;

            // Update the PID controller
            _integral += error * deltaTime;
            _integral = Mathf.Clamp(_integral, -_integralMax, _integralMax);

            float derivative = (error - _lastError) / deltaTime;
            float pOut = _kp * error;
            float iOut = _ki * _integral;
            float dOut = _kd * derivative;
            float output = pOut + iOut + dOut;

            output = Mathf.Clamp(output, -MaxOutput, MaxOutput);

            // Apply the steering force in the opposite direction of the sideways velocity
            //Vector3 steeringForce = -output * sidewaysVelocity.normalized;
            Vector3 steeringForce = output * sidewaysVelocity.normalized;

            return steeringForce;
        }
    }

    public enum DerivativeMeasurement
    {
        Velocity,
        ErrorRateOfChange
    }

    public class PIDControllerVector3 : IPIDController
    {
        public enum Axis
        {
            X,
            Y,
            Z,
        }

        private PIDController[] _pIDControllers;

        public PIDControllerVector3(PIDValuesSO pidValues)
        {
            _pIDControllers = new PIDController[4];

            for (int i = 0; i < 4; i++)
            {
                _pIDControllers[i] = new PIDController(pidValues);
            }
        }

        public void SetPIDValues(PIDValuesSO pidValues)
        {
            for (int i = 0; i < 4; i++)
            {
                _pIDControllers[i].SetPIDValues(pidValues);
            }
        }

        public void SetPIDValues(Axis axis, PIDValuesSO pidValues)
        {
            _pIDControllers[(int)axis].SetPIDValues(pidValues);
        }

        public float GetError()
        {
            throw new NotImplementedException();
        }

        public Vector3 UpdatePosition(Vector3 currentPosition, Vector3 targetPosition, float deltaTime)
        {
            float errorX = targetPosition.x - currentPosition.x;
            float errorY = targetPosition.y - currentPosition.y;
            float errorZ = targetPosition.z - currentPosition.z;

            float thrustX = _pIDControllers[0].Update(errorX, 0, deltaTime, DerivativeMeasurement.ErrorRateOfChange);
            float thrustY = _pIDControllers[1].Update(errorY, 0, deltaTime, DerivativeMeasurement.ErrorRateOfChange);
            float thrustZ = _pIDControllers[2].Update(errorZ, 0, deltaTime, DerivativeMeasurement.ErrorRateOfChange);

            return new Vector3(thrustX, thrustY, thrustZ);
        }

        public Vector3 UpdateVelocity(Vector3 currentVelocity, Vector3 targetVelocity, float deltaTime)
        {
            float thrustX = _pIDControllers[0].Update(currentVelocity.x, targetVelocity.x, deltaTime, DerivativeMeasurement.Velocity);
            float thrustY = _pIDControllers[1].Update(currentVelocity.y, targetVelocity.y, deltaTime, DerivativeMeasurement.Velocity);
            float thrustZ = _pIDControllers[2].Update(currentVelocity.z, targetVelocity.z, deltaTime, DerivativeMeasurement.Velocity);

            return new Vector3(thrustX, thrustY, thrustZ);
        }

        public void Reset()
        {
            for (int i = 0; i < 4; i++)
            {
                _pIDControllers[i].Reset();
            }
        }

        //public Vector3 Update2(Vector3 currentVelocity, Vector3 targetVelocity, float dt)
        //{
        //    if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

        //    // Calculate error
        //    Vector3 error = targetValue - currentValue;

        //    // Proportional term
        //    Vector3 P = Vector3.Scale(_kp, error);

        //    _positionIntegral = Vector3.ClampMagnitude(_positionIntegral + error * dt, integralSaturation);
        //    // Integral term
        //    Vector3 I = Vector3.Scale(_ki, _positionIntegral);

        //    Vector3 errorRateOfChange = (error - _positionLastError) / dt;
        //    _positionLastError = error;

        //    Vector3 valueRateOfChange = (currentValue - valueLast) / dt;
        //    valueLast = currentValue;
        //    velocity = valueRateOfChange;

        //    Vector3 deriveMeasure = Vector3.zero;
        //    if (derivativeInitialized)
        //    {
        //        if (derivativeMeasurement == DerivativeMeasurement.Velocity)
        //            deriveMeasure = -valueRateOfChange;
        //        else
        //            deriveMeasure = errorRateOfChange;
        //    }
        //    else
        //    {
        //        derivativeInitialized = true;
        //    }

        //    // Derivative term
        //    Vector3 D = Vector3.Scale(_kd, deriveMeasure);

        //    Vector3 result = P + I + D;

        //    return Vector3.ClampMagnitude(result, outputMax);
        //}
    }

    public class PIDControllerQuaternion : IPIDController
    {
        public enum Axis
        {
            X,
            Y,
            Z,
            W
        }

        private PIDController[] _pIDControllers;

        private Quaternion _lastError;

        public PIDControllerQuaternion(PIDValuesSO pidValues)
        {
            _pIDControllers = new PIDController[4];

            for (int i = 0; i < 4; i++)
            {
                _pIDControllers[i] = new PIDController(pidValues);
            }
        }

        public void SetPIDValues(PIDValuesSO pidValues)
        {
            for (int i = 0; i < 4; i++)
            {
                _pIDControllers[i].SetPIDValues(pidValues);
            }
        }

        public void SetPIDValues(Axis axis, PIDValuesSO pidValues)
        {
            _pIDControllers[(int)axis].SetPIDValues(pidValues);
        }

        public float GetError()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            for (int i = 0; i < 4; i++)
            {
                _pIDControllers[i].Reset();
            }
        }

        public Vector3 Update(Vector3 currentAngularVelocity, Vector3 targetAngularVelocity, float deltaTime)
        {
            // Calculate error
            Vector3 error = targetAngularVelocity - currentAngularVelocity;

            // Update the PID controllers
            float torqueX = _pIDControllers[0].Update2(currentAngularVelocity.x, targetAngularVelocity.x, deltaTime);
            float torqueY = _pIDControllers[1].Update2(currentAngularVelocity.y, targetAngularVelocity.y, deltaTime);
            float torqueZ = _pIDControllers[2].Update2(currentAngularVelocity.z, targetAngularVelocity.z, deltaTime);

            return new Vector3(torqueX, torqueY, torqueZ);
        }

        public Vector3 Update(Vector3 angularVeclocity, Quaternion current, Quaternion target, float deltaTime)
        {
            Quaternion rotationDelta = current.RotationDelta(target);
            Matrix4x4 rotationMatrix = OrthogonalizeRotationMatrix(rotationDelta);

            _lastError = Quaternion.identity.Substract(rotationDelta);
            Quaternion angularVelocityQuat = Quaternion.Euler(angularVeclocity);
            Quaternion deltaVelocity = angularVelocityQuat.Multiply(rotationDelta);

            Quaternion torque = CalculateTorque(_lastError, deltaVelocity, deltaTime);
            torque = torque.Multiply(rotationMatrix);
            Quaternion inverseTorque = torque.Multiply(-1.0f);

            return new Vector3(inverseTorque.x, inverseTorque.y, inverseTorque.z);
        }

        private Quaternion CalculateTorque(Quaternion error, Quaternion angularVelocity, float deltaTime)
        {
            return new Quaternion
            {
                x = _pIDControllers[0].Update3(error.x, angularVelocity.x, deltaTime),
                y = _pIDControllers[1].Update3(error.y, angularVelocity.y, deltaTime),
                z = _pIDControllers[2].Update3(error.z, angularVelocity.z, deltaTime),
                w = _pIDControllers[3].Update3(error.w, angularVelocity.w, deltaTime)
            };
        }

        private Matrix4x4 OrthogonalizeRotationMatrix(Quaternion rotationDelta)
        {
            return new Matrix4x4()
            {
                m00 =
                    -rotationDelta.x * -rotationDelta.x + -rotationDelta.y * -rotationDelta.y +
                    -rotationDelta.z * -rotationDelta.z,
                m01 =
                    -rotationDelta.x * rotationDelta.w + -rotationDelta.y * -rotationDelta.z +
                    -rotationDelta.z * rotationDelta.y,
                m02 =
                    -rotationDelta.x * rotationDelta.z + -rotationDelta.y * rotationDelta.w +
                    -rotationDelta.z * -rotationDelta.x,
                m03 =
                    -rotationDelta.x * -rotationDelta.y + -rotationDelta.y * rotationDelta.x +
                    -rotationDelta.z * rotationDelta.w,
                m10 =
                    rotationDelta.w * -rotationDelta.x + -rotationDelta.z * -rotationDelta.y +
                    rotationDelta.y * -rotationDelta.z,
                m11 =
                    rotationDelta.w * rotationDelta.w + -rotationDelta.z * -rotationDelta.z +
                    rotationDelta.y * rotationDelta.y,
                m12 =
                    rotationDelta.w * rotationDelta.z + -rotationDelta.z * rotationDelta.w +
                    rotationDelta.y * -rotationDelta.x,
                m13 =
                    rotationDelta.w * -rotationDelta.y + -rotationDelta.z * rotationDelta.x +
                    rotationDelta.y * rotationDelta.w,
                m20 =
                    rotationDelta.z * -rotationDelta.x + rotationDelta.w * -rotationDelta.y +
                    -rotationDelta.x * -rotationDelta.z,
                m21 =
                    rotationDelta.z * rotationDelta.w + rotationDelta.w * -rotationDelta.z +
                    -rotationDelta.x * rotationDelta.y,
                m22 =
                    rotationDelta.z * rotationDelta.z + rotationDelta.w * rotationDelta.w +
                    -rotationDelta.x * -rotationDelta.x,
                m23 =
                    rotationDelta.z * -rotationDelta.y + rotationDelta.w * rotationDelta.x +
                    -rotationDelta.x * rotationDelta.w,
                m30 =
                    -rotationDelta.y * -rotationDelta.x + rotationDelta.x * -rotationDelta.y +
                    rotationDelta.w * -rotationDelta.z,
                m31 =
                    -rotationDelta.y * rotationDelta.w + rotationDelta.x * -rotationDelta.z +
                    rotationDelta.w * rotationDelta.y,
                m32 =
                    -rotationDelta.y * rotationDelta.z + rotationDelta.x * rotationDelta.w +
                    rotationDelta.w * -rotationDelta.x,
                m33 =
                    -rotationDelta.y * -rotationDelta.y + rotationDelta.x * rotationDelta.x +
                    rotationDelta.w * rotationDelta.w,
            };
        }
    }
}