using Sleep0.Logic.Game;
using UnityEngine;

namespace Sleep0.Logic.FiniteStateMachine
{
    public class SpaceshipAutopilotLandingState : SpaceshipBaseState
    {
        public SpaceshipAutopilotLandingState(Rigidbody rigidBody, Spaceship spaceship, PIDAgent pidAgent) : base(rigidBody, spaceship, pidAgent)
        {
        }

        public override void Enter()
        {
            Debug.Log("SpaceshipAutopilotLandingState Enter");
        }

        public override void Execute()
        {
            Debug.Log("SpaceshipAutopilotLandingState Execute");
        }

        public override void Exit()
        {
            Debug.Log("SpaceshipAutopilotLandingState Exit");
        }
    }
}