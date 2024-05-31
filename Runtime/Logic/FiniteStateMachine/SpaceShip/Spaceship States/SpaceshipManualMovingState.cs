using Sleep0.Logic.Game;
using UnityEngine;

namespace Sleep0.Logic.FiniteStateMachine
{
    public class SpaceshipManualMovingState : SpaceshipBaseState
    {
        public SpaceshipManualMovingState(Rigidbody rigidBody, Spaceship spaceship, PIDAgent pidAgent) : base(rigidBody, spaceship, pidAgent)
        {
        }

        public override void Enter()
        {
            Debug.Log("SpaceshipManualMovingState Enter");
        }

        public override void Execute()
        {
            Debug.Log("SpaceshipManualMovingState Execute");
        }

        public override void Exit()
        {
            Debug.Log("SpaceshipManualMovingState Exit");
        }
    }
}