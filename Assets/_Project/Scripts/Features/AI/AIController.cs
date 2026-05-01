using Suhdo.Core.FSM;
using Suhdo.Features.AI.States;
using UnityEngine;

namespace Suhdo.Features.AI
{
    /// <summary>
    /// AIController là Context Object cho AI — tương tự PlayerController nhưng cho NPC/kẻ thù.
    /// 
    /// Khác biệt với PlayerController:
    /// - Không cần IInputService (AI tự quyết định hướng đi).
    /// - Có thể dùng NavMeshAgent hoặc Rigidbody2D tùy game.
    /// - Có reference đến Player Transform để đuổi theo.
    /// 
    /// Sử dụng CÙNG bộ BaseState + StateMachine như Player.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AIController : MonoBehaviour
    {
        // ═══════════════════════════════════════════════
        //  SERIALIZED FIELDS
        // ═══════════════════════════════════════════════

        [Header("AI Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private float wanderRadius = 5f;
        [SerializeField] private float wanderInterval = 3f;

        [Header("References")]
        [Tooltip("Kéo thả Player Transform vào đây hoặc tìm tự động trong Start")]
        [SerializeField] private Transform playerTransform;

        // ═══════════════════════════════════════════════
        //  PUBLIC PROPERTIES (cho States truy cập)
        // ═══════════════════════════════════════════════

        public Rigidbody2D Rigidbody2D { get; private set; }
        public float MoveSpeed => moveSpeed;
        public float DetectionRange => detectionRange;
        public float WanderRadius => wanderRadius;
        public float WanderInterval => wanderInterval;
        public Transform PlayerTransform => playerTransform;

        // ═══════════════════════════════════════════════
        //  STATE MACHINE & STATES
        // ═══════════════════════════════════════════════

        private StateMachine _stateMachine;

        /// <summary>Trạng thái đi lang thang ngẫu nhiên.</summary>
        public AIWanderState WanderState { get; private set; }

        /// <summary>Trạng thái đuổi theo Player.</summary>
        public AIPursueState PursueState { get; private set; }

        // ═══════════════════════════════════════════════
        //  UNITY LIFECYCLE
        // ═══════════════════════════════════════════════

        private void Awake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            // Tạo StateMachine + States
            _stateMachine = new StateMachine();
            WanderState = new AIWanderState(this, _stateMachine);
            PursueState = new AIPursueState(this, _stateMachine);

            // Bắt đầu từ Wander
            _stateMachine.Initialize(WanderState);
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        // ═══════════════════════════════════════════════
        //  HELPER METHODS (cho States gọi)
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Kiểm tra Player có nằm trong tầm phát hiện không.
        /// </summary>
        public bool IsPlayerInRange()
        {
            if (playerTransform == null) return false;
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            return distance <= detectionRange;
        }

        /// <summary>
        /// Vẽ Gizmos để debug detection range trong Scene view.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
        }
    }
}
