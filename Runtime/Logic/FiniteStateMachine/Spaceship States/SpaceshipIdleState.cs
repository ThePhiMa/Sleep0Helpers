using Sleep0.Logic.Game;
using UnityEngine;

namespace Sleep0.Logic.FiniteStateMachine
{
    public class SpaceshipIdleState : SpaceshipBaseState
    {
        public SpaceshipIdleState(Rigidbody rigidBody, Spaceship spaceship, PIDAgent pidAgent) : base(rigidBody, spaceship, pidAgent)
        {
        }

        public override void Enter()
        {
            Debug.Log("SpaceshipIdleState Enter");
        }

        public override void Execute()
        {
            Debug.Log("SpaceshipIdleState Execute");
        }

        public override void Exit()
        {
            Debug.Log("SpaceshipIdleState Exit");
        }
    }
}