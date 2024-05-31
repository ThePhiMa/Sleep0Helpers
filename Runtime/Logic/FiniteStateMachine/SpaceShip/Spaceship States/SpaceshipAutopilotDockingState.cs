using Sleep0.Logic.Game;
using UnityEngine;

namespace Sleep0.Logic.FiniteStateMachine
{
    public class SpaceshipAutopilotDockingState : SpaceshipBaseState
    {
        public SpaceshipAutopilotDockingState(Rigidbody rigidBody, Spaceship spaceship, PIDAgent pidAgent) : base(rigidBody, spaceship, pidAgent)
        {
        }

        public override void Enter()
        {
            Debug.Log("SpaceshipAutopilotMovingState Enter");
        }

        public override void Execute()
        {
            Debug.Log("SpaceshipAutopilotMovingState Execute");
        }

        public override void Exit()
        {
            Debug.Log("SpaceshipAutopilotMovingState Exit");
        }
    }
}