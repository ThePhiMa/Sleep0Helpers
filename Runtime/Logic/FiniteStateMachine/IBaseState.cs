namespace Sleep0.Logic.FiniteStateMachine
{
    public interface IBaseState
    {
        void Enter();

        void Execute();

        void Exit();
    }
}