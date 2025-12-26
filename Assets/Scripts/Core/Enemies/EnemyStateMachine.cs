using UnityEngine;
using UnityEngine.AI;

namespace HDRP_FPS3D.Enemy
{
    public class EnemyStateMachine : MonoBehaviour
    {
        public Transform Player { get; private set; }
        public float PatrolSpeed = 3f;
        public float ChaseSpeed = 6f;
        public float AttackRange = 5f;
        public float DetectionRange = 15f;
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
        private float _lastAttackTime;
        private bool _isPlayerDetected;

        public EnemyBaseState CurrentState => _currentState;
        public NavMeshAgent Agent => _agent;
        public EnemyHealth Health => _health;
        public float LastAttackTime => _lastAttackTime;
        public bool IsPlayerDetected => _isPlayerDetected;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent == null) _agent = gameObject.AddComponent<NavMeshAgent>();

            _health = GetComponent<EnemyHealth>();
            if (_health == null) _health = gameObject.AddComponent<EnemyHealth>();

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

            if (_isPlayerDetected)
            {
                LookAtPlayer();
            }

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

        public void SetAttackTime(float time)
        {
            _lastAttackTime = time;
        }

        public bool CanAttack()
        {
            return Time.time >= _lastAttackTime + AttackCooldown;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
        }
    }
}