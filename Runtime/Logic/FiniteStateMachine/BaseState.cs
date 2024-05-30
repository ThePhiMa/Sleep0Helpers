namespace Sleep0.Logic.FiniteStateMachine
{
    public abstract class BaseState
    {
        public abstract void Enter();

        public abstract void Execute();

        public abstract void Exit();
    }
}