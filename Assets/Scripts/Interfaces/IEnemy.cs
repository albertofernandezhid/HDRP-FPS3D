using UnityEngine;
using UnityEngine.AI;

namespace HDRP_FPS3D.Enemy
{
    public interface IEnemy
    {
        Transform transform { get; }
        Transform Player { get; }
        NavMeshAgent Agent { get; }
        EnemyHealth Health { get; }
        float PatrolSpeed { get; }
        float ChaseSpeed { get; }
        float AttackRange { get; }
        float ChaseRange { get; }
        float DetectionRange { get; }
        float PatrolRadius { get; }
        Vector3 InitialPosition { get; }
        Transform[] PatrolPoints { get; }
        bool IsPlayerDetected { get; }
        void LookAtPlayer();
        void SwitchState(EnemyBaseState newState);
        bool CanAttack();
    }
}