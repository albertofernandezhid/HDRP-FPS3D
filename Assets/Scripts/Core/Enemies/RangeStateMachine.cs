using UnityEngine;
using UnityEngine.AI;

namespace HDRP_FPS3D.Enemy
{
    public class RangeStateMachine : MonoBehaviour, IEnemy
    {
        public Transform Player { get; private set; }
        public float PatrolSpeed = 3f;
        public float ChaseSpeed = 6f;
        public float DetectionRange = 20f;
        public float ChaseRange = 12f;
        public float AttackRange = 7f;
        public float PatrolRadius = 5f;
        public float AttackDamage = 10f;
        public float AttackCooldown = 1.5f;
        public float RotationSpeed = 5f;
        public Transform[] PatrolPoints;

        public GameObject AttackPrefab;
        public Transform AttackPoint;
        public float ProjectileSpeed = 20f;

        private EnemyBaseState _currentState;
        private NavMeshAgent _agent;
        private EnemyHealth _health;
        private Animator _animator;
        private float _lastAttackTime;
        private bool _isPlayerDetected;
        private Vector3 _initialPosition;

        public NavMeshAgent Agent => _agent;
        public Animator Animator => _animator;
        public EnemyHealth Health => _health;
        public bool IsPlayerDetected => _isPlayerDetected;
        public Vector3 InitialPosition => _initialPosition;
        float IEnemy.PatrolSpeed => PatrolSpeed;
        float IEnemy.ChaseSpeed => ChaseSpeed;
        float IEnemy.AttackRange => AttackRange;
        float IEnemy.ChaseRange => ChaseRange;
        float IEnemy.DetectionRange => DetectionRange;
        float IEnemy.PatrolRadius => PatrolRadius;
        Transform[] IEnemy.PatrolPoints => PatrolPoints;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<EnemyHealth>();
            _animator = GetComponent<Animator>();
            _initialPosition = transform.position;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        private void Start()
        {
            FindPlayer();
            _currentState = new EnemyPatrolState();
            _currentState.EnterState(this);
        }

        private void Update()
        {
            if (Player == null) FindPlayer();
            if (Player == null || _health.IsDead) return;
            CheckPlayerDetection();
            if (_isPlayerDetected) LookAtPlayer();
            _currentState?.UpdateState(this);
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) Player = playerObj.transform;
        }

        private void CheckPlayerDetection()
        {
            if (Player == null) return;
            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
            _isPlayerDetected = distanceToPlayer <= DetectionRange;
        }

        public void LookAtPlayer()
        {
            if (Player == null) return;
            Vector3 direction = (Player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }
        }

        public void SwitchState(EnemyBaseState newState)
        {
            _currentState?.ExitState(this);
            _currentState = newState;
            _currentState.EnterState(this);
        }

        public void SetAttackTime(float time) => _lastAttackTime = time;
        public bool CanAttack() => Time.time >= _lastAttackTime + AttackCooldown;

        public void AnimationEvent_ShootProjectile()
        {
            if (Player == null || AttackPrefab == null || AttackPoint == null) return;

            Vector3 targetDir = (Player.position - AttackPoint.position).normalized;
            Quaternion launchRotation = Quaternion.LookRotation(targetDir);
            GameObject projectile = Instantiate(AttackPrefab, AttackPoint.position, launchRotation);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.linearVelocity = targetDir * ProjectileSpeed;
            }

            EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
            if (projScript != null) projScript.Damage = AttackDamage;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, ChaseRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
        }
    }
}