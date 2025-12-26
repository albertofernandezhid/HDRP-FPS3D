using UnityEngine;

namespace HDRP_FPS3D.Enemy
{
    public class EnemyPatrolState : EnemyBaseState
    {
        private int _currentPatrolIndex;

        public override void EnterState(EnemyStateMachine enemy)
        {
            enemy.Agent.speed = enemy.PatrolSpeed;
            enemy.Agent.stoppingDistance = 0.5f;
            enemy.Agent.isStopped = false;
            SetNextPatrolPoint(enemy);
        }

        public override void UpdateState(EnemyStateMachine enemy)
        {
            if (enemy.Health.IsDead) return;

            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.Player.position);

            if (enemy.IsPlayerDetected && distanceToPlayer <= enemy.AttackRange)
            {
                enemy.SwitchState(new EnemyAttackState());
                return;
            }

            if (enemy.Agent.remainingDistance <= enemy.Agent.stoppingDistance && !enemy.Agent.pathPending)
            {
                SetNextPatrolPoint(enemy);
            }
        }

        private void SetNextPatrolPoint(EnemyStateMachine enemy)
        {
            if (enemy.PatrolPoints == null || enemy.PatrolPoints.Length == 0) return;
            _currentPatrolIndex = (_currentPatrolIndex + 1) % enemy.PatrolPoints.Length;
            enemy.Agent.SetDestination(enemy.PatrolPoints[_currentPatrolIndex].position);
        }

        public override void ExitState(EnemyStateMachine enemy)
        {
            enemy.Agent.ResetPath();
        }
    }

    public class EnemyChaseState : EnemyBaseState
    {
        public override void EnterState(EnemyStateMachine enemy)
        {
            enemy.Agent.speed = enemy.ChaseSpeed;
            enemy.Agent.stoppingDistance = enemy.AttackRange * 0.9f;
            enemy.Agent.isStopped = false;
        }

        public override void UpdateState(EnemyStateMachine enemy)
        {
            if (enemy.Health.IsDead) return;

            if (!enemy.IsPlayerDetected)
            {
                enemy.SwitchState(new EnemyPatrolState());
                return;
            }

            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.Player.position);

            if (distanceToPlayer <= enemy.AttackRange)
            {
                enemy.SwitchState(new EnemyAttackState());
                return;
            }

            enemy.Agent.SetDestination(enemy.Player.position);
        }

        public override void ExitState(EnemyStateMachine enemy)
        {
            enemy.Agent.ResetPath();
        }
    }

    public class EnemyAttackState : EnemyBaseState
    {
        public override void EnterState(EnemyStateMachine enemy)
        {
            enemy.Agent.isStopped = true;
        }

        public override void UpdateState(EnemyStateMachine enemy)
        {
            if (enemy.Health.IsDead) return;

            if (!enemy.IsPlayerDetected)
            {
                enemy.SwitchState(new EnemyPatrolState());
                return;
            }

            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.Player.position);

            if (distanceToPlayer > enemy.AttackRange)
            {
                enemy.SwitchState(new EnemyChaseState());
                return;
            }

            if (enemy.CanAttack())
            {
                AttackPlayer(enemy);
            }
        }

        private void AttackPlayer(EnemyStateMachine enemy)
        {
            if (enemy.AttackPrefab != null && enemy.AttackPoint != null)
            {
                Vector3 targetDir = (enemy.Player.position - enemy.AttackPoint.position).normalized;
                Quaternion launchRotation = Quaternion.LookRotation(targetDir);

                GameObject projectile = Object.Instantiate(enemy.AttackPrefab, enemy.AttackPoint.position, launchRotation);

                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = false;
                    rb.linearVelocity = targetDir * enemy.ProjectileSpeed;
                }

                EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
                if (projScript != null)
                {
                    projScript.Damage = enemy.AttackDamage;
                }
            }

            enemy.SetAttackTime(Time.time);
        }

        public override void ExitState(EnemyStateMachine enemy)
        {
            enemy.Agent.isStopped = false;
        }
    }
}