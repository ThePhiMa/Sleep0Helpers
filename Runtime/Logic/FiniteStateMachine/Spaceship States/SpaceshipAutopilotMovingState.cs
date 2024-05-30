using Sleep0.Logic.Game;
using UnityEngine;

namespace Sleep0.Logic.FiniteStateMachine
{
    public class SpaceshipAutopilotMovingState : SpaceshipBaseState
    {
        private enum State
        {
            TurnTowardsTarget,
            ForwardThrustMovement,
            ForwardThrustDeceleration,
            SideThrustersMovement,
            TurningAround,
            NoMovement
        }

        private State _currentState = State.ForwardThrustMovement;
        private float _distanceToTarget = 0.0f;
        private float _distanceToDecelerate = 0.0f;
        private Vector3 _localVelocity = Vector3.zero;

        public SpaceshipAutopilotMovingState(Rigidbody rigidBody, Spaceship spaceship, PIDAgent pidAgent) : base(rigidBody, spaceship, pidAgent)
        {
        }

        public override void Enter()
        {
            Debug.Log("SpaceshipAutopilotMovingState Enter");

            _currentState = State.TurnTowardsTarget;

            _distanceToTarget = Vector3.Distance(_PIDAgent.CurrentTarget.position, _Transform.position);
            _distanceToDecelerate = Mathf.Min(_distanceToTarget / 1.5f, _distanceToTarget * (_PIDAgent.MaxDecelerationDistancePercent / 100f));

            _PIDAgent.Reset();
            _IsActive = true;
        }

        public override void Execute()
        {
            if (_IsActive == false)
                return;

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

            _localVelocity = _Transform.InverseTransformDirection(_Rigidbody.velocity);

            switch (_currentState)
            {
                case State.TurnTowardsTarget:
                    TurnTowardsTarget();
                    break;
                case State.ForwardThrustMovement:
                    ForwardThrustMovement();
                    break;
                case State.ForwardThrustDeceleration:
                    ForwardThrustDeceleration();
                    break;
                case State.SideThrustersMovement:
                    SideThrustersMovement();
                    break;
                case State.TurningAround:
                    TurningAround();
                    break;
                case State.NoMovement:
                default:
                    NoMovement();
                    break;
            }

            /*
            Vector3 targetPosition = _PIDAgent.CurrentTarget.position;
            Vector3 currentPosition = _Transform.position;

            Vector3 direction = targetPosition - currentPosition;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 1. First calculate and apply the torque
            _PIDAgent.UpdateTorque(targetRotation);

            // 2. Then calculate the forward thrust, which is inversely proportional to the torque
            float reductionFactorTorque = 1 - Mathf.Sqrt(Mathf.Clamp(_Torque.magnitude, 0, 1));
            _PIDAgent.UpdateMainThrust(targetPosition, reductionFactorTorque);

            // 3. Lastly, calculate the side thrust, which is only for slight adjustments
            Vector3 sidewaysVelocity = Vector3.ProjectOnPlane(_Rigidbody.velocity, _Transform.forward);
            Vector3 desiredSidewaysVelocity = Vector3.zero; // Assuming you want to stop any sideways motion
            _PIDAgent.UpdateSideThrust(sidewaysVelocity, desiredSidewaysVelocity);

            Vector3 upVelocity = Vector3.Project(_Rigidbody.velocity, _Transform.up);
            Vector3 desiredUpVelocity = Vector3.zero; // Assuming you want to stop any up motion
            _PIDAgent.UpdateUpThrust(upVelocity, desiredUpVelocity)
            */
        }

        private void TurnTowardsTarget()
        {
            //Debug.Log("Turning towards target");

            var directionToTarget = (_PIDAgent.CurrentTarget.position - _Transform.position).normalized;
            var targetRotation = Quaternion.LookRotation(directionToTarget);

            _PIDAgent.UpdateTorque(targetRotation, _Spaceship.TorqueMultiplier);

            if (Vector3.Dot(_Transform.forward, directionToTarget) > 0.99f)
            {
                _PIDAgent.Reset();
                _currentState = State.ForwardThrustMovement;
            }
        }

        private void ForwardThrustMovement()
        {
            //Debug.Log($"Moving towards target {_distanceToTarget}:{_distanceToDecelerate}");

            _distanceToTarget = Vector3.Distance(_PIDAgent.CurrentTarget.position, _Transform.position);
            var directionToTarget = (_PIDAgent.CurrentTarget.position - _Transform.position).normalized;
            var targetRotation = Quaternion.LookRotation(directionToTarget);

            //_PIDAgent.UpdateMainThrust(_PIDAgent.CurrentTarget.position, _Spaceship.MainThrustMultiplier);

            //_PIDAgent.UpdateSideThrust(_localVelocity.x, 0f);
            _PIDAgent.UpdateMainThrust(_localVelocity.z, 10f, _Spaceship.MainThrustMultiplier);
            _PIDAgent.UpdateTorque(targetRotation, _Spaceship.TorqueMultiplier);

            if (_distanceToTarget < _distanceToDecelerate)
            {
                _PIDAgent.Reset();
                _currentState = State.TurningAround;
            }
        }

        private void ForwardThrustDeceleration()
        {
            //Debug.Log("Decelerating");

            var currentHeading = _PIDAgent.CurrentTarget.position - _Transform.position;
            var distance = currentHeading.magnitude;
            var directionToTarget = currentHeading / distance;

            // When the ship is close to the target, turn off main thrusters and use the 6d side thrusters to align the ship with the target
            //_PIDAgent.UpdateMainThrust(_PIDAgent.CurrentTarget.position, _Spaceship.MainThrustMultiplier);
            //_PIDAgent.UpdateSideThrust(_localVelocity.x, 0f);
            _PIDAgent.UpdateMainThrust(_localVelocity.z, 0f, _Spaceship.MainThrustMultiplier);
            _PIDAgent.UpdateSideThrust(_localVelocity.x, 0f);
            _PIDAgent.UpdateTorque(Quaternion.LookRotation(-directionToTarget), _Spaceship.TorqueMultiplier);

            if (distance < _PIDAgent.StoppingDistance)
            {
                _PIDAgent.Reset();
                //_currentState = State.TurningAround;
                _currentState = State.NoMovement;
            }
        }

        private void SideThrustersMovement()
        {

        }

        private void TurningAround()
        {
            //Debug.Log($"Turning around 180 degrees");

            var currentHeading = _PIDAgent.CurrentTarget.position - _Transform.position;
            var distance = currentHeading.magnitude;
            var directionToTarget = currentHeading / distance;
            var heading = Vector3.Dot(_Transform.forward, directionToTarget);

            //using (Draw.WithColor(Color.red))
            //{
            //    Draw.Arrow(_Transform.position, _PIDAgent.CurrentTarget.position);
            //}

            Quaternion targetRotation = Quaternion.LookRotation(-directionToTarget);
            _PIDAgent.UpdateTorque(targetRotation, _Spaceship.TorqueMultiplier);

            //Debug.Log($"Turning around 180 degrees (Angle to target:{heading}| direction {directionToTarget})");

            if (Mathf.Abs(heading) < 0.1f) // _PIDAgent.MaxAngleToTarget)
            {
                _PIDAgent.Reset();
                _currentState = State.ForwardThrustDeceleration;
            }
        }

        private void NoMovement()
        {
            //Debug.Log("No movement");

            _PIDAgent.UpdateMainThrust(_localVelocity.z, 0f, _Spaceship.MainThrustMultiplier);
            _PIDAgent.UpdateSideThrust(_localVelocity.x, 0f);
            //_PIDAgent.UpdateUpThrust(_Rigidbody.velocity, Vector3.zero);
            _PIDAgent.UpdateTorque(_Rigidbody.angularVelocity, Vector3.zero);
        }

        public override void Exit()
        {
            Debug.Log("SpaceshipAutopilotMovingState Exit");

            _IsActive = false;
        }
    }
}