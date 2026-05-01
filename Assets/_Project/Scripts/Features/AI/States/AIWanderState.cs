using Suhdo.Core.FSM;
using UnityEngine;

namespace Suhdo.Features.AI.States
{
    /// <summary>
    /// Trạng thái đi lang thang ngẫu nhiên của AI.
    /// AI chọn 1 điểm ngẫu nhiên trong bán kính wanderRadius, di chuyển đến đó,
    /// chờ 1 khoảng thời gian, rồi chọn điểm tiếp theo.
    /// Luôn kiểm tra Player có trong tầm phát hiện để chuyển sang PursueState.
    /// </summary>
    public class AIWanderState : BaseState
    {
        private readonly AIController _ctx;
        private Vector2 _wanderTarget;
        private float _waitTimer;
        private bool _isWaiting;
        private Vector2 _startPosition;

        public AIWanderState(AIController ctx, StateMachine stateMachine)
            : base(stateMachine)
        {
            _ctx = ctx;
        }

        public override void Enter()
        {
            _startPosition = _ctx.transform.position;
            _isWaiting = false;
            _waitTimer = 0f;
            PickRandomTarget();

            Debug.Log("[AIWanderState] Enter — Bắt đầu đi lang thang.");
        }

        public override void Update()
        {
            // Ưu tiên: Kiểm tra Player → Pursue
            if (_ctx.IsPlayerInRange())
            {
                stateMachine.ChangeState(_ctx.PursueState);
                return;
            }

            if (_isWaiting)
            {
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= _ctx.WanderInterval)
                {
                    _isWaiting = false;
                    _waitTimer = 0f;
                    PickRandomTarget();
                }
            }
            else
            {
                // Kiểm tra đã đến nơi chưa
                float distance = Vector2.Distance(_ctx.transform.position, _wanderTarget);
                if (distance < 0.3f)
                {
                    _isWaiting = true;
                    _ctx.Rigidbody2D.linearVelocity = Vector2.zero;
                }
            }
        }

        public override void PhysicsUpdate()
        {
            if (_isWaiting) return;

            // Di chuyển về phía target
            Vector2 direction = (_wanderTarget - (Vector2)_ctx.transform.position).normalized;
            _ctx.Rigidbody2D.MovePosition(
                _ctx.Rigidbody2D.position + direction * _ctx.MoveSpeed * Time.fixedDeltaTime
            );
        }

        public override void Exit()
        {
            _ctx.Rigidbody2D.linearVelocity = Vector2.zero;
            Debug.Log("[AIWanderState] Exit");
        }

        /// <summary>
        /// Chọn 1 điểm ngẫu nhiên trong bán kính wanderRadius quanh vị trí ban đầu.
        /// </summary>
        private void PickRandomTarget()
        {
            Vector2 randomOffset = Random.insideUnitCircle * _ctx.WanderRadius;
            _wanderTarget = _startPosition + randomOffset;
        }
    }
}
