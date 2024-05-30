using Sleep0.Math;
using UnityEngine;

namespace Sleep0.Logic
{
    public class PIDAutotuner
    {
        public enum MethodType
        {
            ZieglerNichols
        }

        private PIDValuesSO _pidValues;
        private IPIDController _pidController;

        // Ziegler-Nichols method
        private float _oscillationStartTime;
        private float _oscillationEndTime;
        private bool _isOscillating = false;

        private MethodType _methodType;

        public PIDAutotuner(PIDValuesSO pidValues, IPIDController pidController, MethodType methodType)
        {
            _pidValues = pidValues;
            _pidController = pidController;
            SetMethodType(methodType);
        }

        public void SetMethodType(MethodType methodType)
        {
            _methodType = methodType;
        }

        public bool TuningUpdate()
        {
            switch (_methodType)
            {
                case MethodType.ZieglerNichols:
                    return ZieglerNicholsMethod();
            }

            return false;
        }

        private bool ZieglerNicholsMethod()
        {
            // Step 1: Set I and D gains to zero
            _pidValues.IGain = 0;
            _pidValues.DGain = 0;

            // Step 2: Increase P gain until the output oscillates
            //_pidValues.PGain += 0.01f;

            // Step 3: Measure the oscillation period
            float error = _pidController.GetError();
            if (error > 0 && !_isOscillating)
            {
                _oscillationStartTime = Time.time;
                _isOscillating = true;
            }
            else if (error < 0 && _isOscillating)
            {
                _oscillationEndTime = Time.time;
                _isOscillating = false;

                // Step 4: Calculate the ultimate gain (Ku) and the period of oscillation (Tu)
                float Ku = _pidValues.PGain;
                float Tu = _oscillationEndTime - _oscillationStartTime;

                // Step 5: Apply the Ziegler-Nichols tuning rules
                _pidValues.PGain = 0.6f * Ku;
                _pidValues.IGain = 2f * _pidValues.PGain / Tu;
                _pidValues.DGain = _pidValues.PGain * Tu / 8f;

                return true;
            }

            return false;
        }
    }
}