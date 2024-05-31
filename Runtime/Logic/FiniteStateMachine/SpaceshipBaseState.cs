using Sleep0.Logic.Game;
using Sleep0.Math;
using System;
using UnityEngine;

namespace Sleep0.Logic.FiniteStateMachine
{
    public abstract class SpaceshipBaseState : BaseState, IBaseState
    {
        protected bool _IsActive = false;

        protected Spaceship _Spaceship;
        protected Rigidbody _Rigidbody;
        protected PIDAgent _PIDAgent;

        protected Vector3 _MainThrust;
        protected Vector3 _SideThrust;
        protected Vector3 _UpThrust;
        protected Vector3 _Torque;

        protected Transform _Transform => _Rigidbody.transform;

        protected PIDControllerVector3 _MainThrustController => _PIDAgent.MainThrustController;
        protected PIDController _SideThrustController => _PIDAgent.SideThrustController;
        protected PIDController _UpThrustController => _PIDAgent.UpThrustController;
        protected PIDControllerQuaternion _TorqueController => _PIDAgent.TorqueController;

        public SpaceshipBaseState(Rigidbody rigidBody, Spaceship spaceship, PIDAgent pidAgent) : base()
        {
            if (rigidBody == null)
                throw new ArgumentNullException(nameof(rigidBody));

            if (spaceship == null)
                throw new ArgumentNullException(nameof(spaceship));

            if (pidAgent == null)
                throw new ArgumentNullException(nameof(pidAgent));

            _Rigidbody = rigidBody;
            _Spaceship = spaceship;
            _PIDAgent = pidAgent;
        }

        public override void Enter()
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }

        public override void Exit()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}