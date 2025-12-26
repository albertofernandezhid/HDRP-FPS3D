namespace HDRP_FPS3D.Enemy
{
    public abstract class EnemyBaseState
    {
        public abstract void EnterState(EnemyStateMachine enemy);
        public abstract void UpdateState(EnemyStateMachine enemy);
        public abstract void ExitState(EnemyStateMachine enemy);
    }
}