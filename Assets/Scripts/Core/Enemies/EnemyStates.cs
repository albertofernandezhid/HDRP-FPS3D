using UnityEngine;
using UnityEngine.AI;

namespace HDRP_FPS3D.Enemy
{
    public class EnemyPatrolState : EnemyBaseState
    {
        private int _currentPatrolIndex;
        private float _waitTime = 4f;
        private float _waitTimer;
        private bool _isPerformingMicroSearch;
        private Quaternion _targetSearchRotation;
        private float _rotationTimer;

        public override void EnterState(IEnemy enemy)
        {
            enemy.Agent.speed = enemy.PatrolSpeed;
            enemy.Agent.stoppingDistance = 0.5f;
            enemy.Agent.updateRotation = true;
            _isPerformingMicroSearch = false;
            SetNextMainWaypoint(enemy);
        }

        public override void UpdateState(IEnemy enemy)
        {
            if (enemy.Health.IsDead) return;

            PlayerHealth playerHealth = enemy.Player.GetComponent<PlayerHealth>();
            bool isPlayerAlive = playerHealth != null && playerHealth.IsAlive();

            float angleToTarget = Vector3.Angle(enemy.transform.forward, enemy.Agent.desiredVelocity);
            if (enemy.Agent.remainingDistance > enemy.Agent.stoppingDistance)
            {
                enemy.Agent.speed = (angleToTarget > 45f) ? 0.5f : enemy.PatrolSpeed;
            }

            enemy.Animator.SetFloat("Speed", enemy.Agent.velocity.magnitude, 0.1f, Time.deltaTime);

            if (isPlayerAlive)
            {
                float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.Player.position);
                if (distanceToPlayer <= enemy.ChaseRange)
                {
                    enemy.SwitchState(new EnemyChaseState());
                    return;
                }

                if (enemy.IsPlayerDetected)
                {
                    enemy.Agent.isStopped = true;
                    enemy.Agent.updateRotation = false;
                    enemy.Animator.SetFloat("Speed", 0, 0.1f, Time.deltaTime);
                    return;
                }
            }

            if (_isPerformingMicroSearch)
            {
                _waitTimer -= Time.deltaTime;
                if (enemy.Agent.remainingDistance <= enemy.Agent.stoppingDistance)
                {
                    enemy.Agent.updateRotation = false;
                    HandleIntelligentLook(enemy);
                    if (Random.value < 0.01f && _waitTimer > 1f)
                    {
                        MoveToRandomLocalPoint(enemy);
                    }
                }
                else
                {
                    enemy.Agent.updateRotation = true;
                }

                if (_waitTimer <= 0)
                {
                    _isPerformingMicroSearch = false;
                    SetNextMainWaypoint(enemy);
                }
                return;
            }

            enemy.Agent.isStopped = false;
            enemy.Agent.updateRotation = true;

            if (enemy.Agent.remainingDistance <= enemy.Agent.stoppingDistance && !enemy.Agent.pathPending)
            {
                _isPerformingMicroSearch = true;
                _waitTimer = _waitTime;
                _rotationTimer = 0;
            }
        }

        private void SetNextMainWaypoint(IEnemy enemy)
        {
            bool hasPoints = enemy.PatrolPoints != null && enemy.PatrolPoints.Length > 0;
            Vector3 centerPoint = hasPoints ? enemy.PatrolPoints[_currentPatrolIndex].position : enemy.InitialPosition;
            if (hasPoints) _currentPatrolIndex = (_currentPatrolIndex + 1) % enemy.PatrolPoints.Length;
            else _currentPatrolIndex = 0;
            enemy.Agent.SetDestination(centerPoint);
        }

        private void MoveToRandomLocalPoint(IEnemy enemy)
        {
            int pointsCount = (enemy.PatrolPoints != null) ? enemy.PatrolPoints.Length : 0;
            Vector3 center = (pointsCount > 0) ? enemy.PatrolPoints[(_currentPatrolIndex == 0 ? pointsCount - 1 : _currentPatrolIndex - 1)].position : enemy.InitialPosition;
            Vector3 randomPoint = center + Random.insideUnitSphere * enemy.PatrolRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, enemy.PatrolRadius, NavMesh.AllAreas))
            {
                enemy.Agent.SetDestination(hit.position);
            }
        }

        private void HandleIntelligentLook(IEnemy enemy)
        {
            _rotationTimer -= Time.deltaTime;
            if (_rotationTimer <= 0)
            {
                float randomAngle = Random.Range(-60f, 60f);
                _targetSearchRotation = Quaternion.Euler(0, enemy.transform.eulerAngles.y + randomAngle, 0);
                _rotationTimer = Random.Range(1.2f, 2.5f);
            }
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, _targetSearchRotation, Time.deltaTime * 2.5f);
        }

        public override void ExitState(IEnemy enemy)
        {
            enemy.Agent.isStopped = false;
            enemy.Agent.updateRotation = true;
        }
    }

    public class EnemyChaseState : EnemyBaseState
    {
        public override void EnterState(IEnemy enemy)
        {
            enemy.Agent.speed = enemy.ChaseSpeed;
            enemy.Agent.stoppingDistance = enemy.AttackRange * 0.8f;
            enemy.Agent.updateRotation = true;
            enemy.Agent.isStopped = false;
        }

        public override void UpdateState(IEnemy enemy)
        {
            if (enemy.Health.IsDead) return;

            PlayerHealth playerHealth = enemy.Player.GetComponent<PlayerHealth>();
            if (playerHealth == null || !playerHealth.IsAlive())
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

            if (distanceToPlayer > enemy.ChaseRange)
            {
                enemy.SwitchState(new EnemyPatrolState());
                return;
            }

            float angleToTarget = Vector3.Angle(enemy.transform.forward, (enemy.Player.position - enemy.transform.position).normalized);
            enemy.Agent.speed = (angleToTarget > 60f) ? 0.5f : enemy.ChaseSpeed;
            enemy.Animator.SetFloat("Speed", enemy.Agent.velocity.magnitude, 0.1f, Time.deltaTime);
            enemy.Agent.SetDestination(enemy.Player.position);
        }

        public override void ExitState(IEnemy enemy) => enemy.Agent.ResetPath();
    }

    public class EnemyAttackState : EnemyBaseState
    {
        public override void EnterState(IEnemy enemy)
        {
            enemy.Agent.isStopped = true;
            enemy.Agent.updateRotation = false;
            enemy.Animator.SetFloat("Speed", 0f);
            enemy.Animator.ResetTrigger("Attack");
        }

        public override void UpdateState(IEnemy enemy)
        {
            if (enemy.Health.IsDead) return;

            PlayerHealth playerHealth = enemy.Player.GetComponent<PlayerHealth>();
            if (playerHealth == null || !playerHealth.IsAlive())
            {
                enemy.SwitchState(new EnemyPatrolState());
                return;
            }

            bool isAttacking = enemy.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") || enemy.Animator.IsInTransition(0);
            if (!isAttacking)
            {
                Vector3 directionToPlayer = (enemy.Player.position - enemy.transform.position).normalized;
                directionToPlayer.y = 0;
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * 8f);
                }
            }

            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.Player.position);
            if (distanceToPlayer > enemy.AttackRange * 1.2f && !isAttacking)
            {
                enemy.SwitchState(new EnemyChaseState());
                return;
            }

            if (enemy.CanAttack() && !isAttacking)
            {
                enemy.Animator.SetTrigger("Attack");
                ExecuteAttack(enemy);
            }
        }

        private void ExecuteAttack(IEnemy enemy)
        {
            if (enemy is RangeStateMachine rangeEnemy)
            {
                rangeEnemy.SetAttackTime(Time.time);
            }
            else if (enemy is MeleeStateMachine meleeEnemy)
            {
                meleeEnemy.PerformMeleeAttack();
            }
        }

        public override void ExitState(IEnemy enemy)
        {
            enemy.Agent.isStopped = false;
            enemy.Agent.updateRotation = true;
        }
    }
}