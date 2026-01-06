using UnityEngine;
using UnityEngine.AI;

namespace HDRP_FPS3D.Enemy
{
    public class MeleeStateMachine : MonoBehaviour, IEnemy
    {
        public Transform Player { get; private set; }
        public float PatrolSpeed = 3f;
        public float ChaseSpeed = 6f;
        public float RotationSpeed = 7f;
        public float PatrolRadius = 5f;
        public Transform[] PatrolPoints;

        public float DetectionRange = 20f;
        public float ChaseRange = 12f;
        public float AttackRange = 2.5f;
        public float AttackDamage = 20f;
        public float AttackCooldown = 1.2f;

        public Transform AttackHitboxCenter;
        public float HitboxRadius = 1.0f;
        public LayerMask PlayerLayer;

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

            var playerHealth = Player.GetComponent<PlayerHealth>();
            bool isPlayerAlive = playerHealth != null && playerHealth.IsAlive();

            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

            _isPlayerDetected = (distanceToPlayer <= DetectionRange) && isPlayerAlive;
        }

        public void LookAtPlayer()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) return;

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

        public bool CanAttack() => Time.time >= _lastAttackTime + AttackCooldown;

        public void PerformMeleeAttack()
        {
            _lastAttackTime = Time.time;
        }

        public void AnimationEvent_HitPlayer()
        {
            Collider[] hitPlayers = Physics.OverlapSphere(AttackHitboxCenter.position, HitboxRadius, PlayerLayer);
            foreach (Collider player in hitPlayers)
            {
                var damageable = player.GetComponent<IDamageable>();
                if (damageable != null) damageable.TakeDamage(AttackDamage);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, ChaseRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
            if (AttackHitboxCenter != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(AttackHitboxCenter.position, HitboxRadius);
            }
        }
    }
}