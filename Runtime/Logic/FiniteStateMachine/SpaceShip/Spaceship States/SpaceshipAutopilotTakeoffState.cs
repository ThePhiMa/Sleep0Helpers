using Sleep0.Logic.Game;
using UnityEngine;

namespace Sleep0.Logic.FiniteStateMachine
{
    public class SpaceshipAutopilotTakeoffState : SpaceshipBaseState
    {
        public SpaceshipAutopilotTakeoffState(Rigidbody rigidBody, Spaceship spaceship, PIDAgent pidAgent) : base(rigidBody, spaceship, pidAgent)
        {
        }

        public override void Enter()
        {
            Debug.Log("SpaceshipAutopilotTakeoffState Enter");
        }

        public override void Execute()
        {
            Debug.Log("SpaceshipAutopilotTakeoffState Execute");
        }

        public override void Exit()
        {
            Debug.Log("SpaceshipAutopilotTakeoffState Exit");
        }
    }
}