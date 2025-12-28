namespace HDRP_FPS3D.Enemy
{
    public abstract class EnemyBaseState
    {
        public abstract void EnterState(IEnemy enemy);
        public abstract void UpdateState(IEnemy enemy);
        public abstract void ExitState(IEnemy enemy);
    }
}