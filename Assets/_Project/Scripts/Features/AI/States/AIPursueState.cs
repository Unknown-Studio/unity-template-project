using Suhdo.Core.FSM;
using UnityEngine;

namespace Suhdo.Features.AI.States
{
    /// <summary>
    /// Trạng thái đuổi theo Player.
    /// AI di chuyển về phía Player Transform liên tục.
    /// Khi Player ra khỏi tầm phát hiện → quay lại WanderState.
    /// </summary>
    public class AIPursueState : BaseState
    {
        private readonly AIController _ctx;

        /// <summary>
        /// Hệ số tốc độ khi đuổi (nhanh hơn wander 1 chút để tạo cảm giác nguy hiểm).
        /// </summary>
        private const float PURSUE_SPEED_MULTIPLIER = 1.3f;

        public AIPursueState(AIController ctx, StateMachine stateMachine)
            : base(stateMachine)
        {
            _ctx = ctx;
        }

        public override void Enter()
        {
            // TODO: Bật animation chạy nhanh / tấn công
            // _ctx.Animator.Play("Run");

            Debug.Log("[AIPursueState] Enter — Đuổi theo Player!");
        }

        public override void Update()
        {
            // Nếu Player ra khỏi tầm → quay lại Wander
            if (!_ctx.IsPlayerInRange())
            {
                stateMachine.ChangeState(_ctx.WanderState);
                return;
            }

            // Flip sprite theo hướng di chuyển
            HandleFacing();
        }

        public override void PhysicsUpdate()
        {
            if (_ctx.PlayerTransform == null) return;

            Vector2 direction = ((Vector2)_ctx.PlayerTransform.position - _ctx.Rigidbody2D.position).normalized;
            float speed = _ctx.MoveSpeed * PURSUE_SPEED_MULTIPLIER;

            _ctx.Rigidbody2D.MovePosition(
                _ctx.Rigidbody2D.position + direction * speed * Time.fixedDeltaTime
            );
        }

        public override void Exit()
        {
            _ctx.Rigidbody2D.linearVelocity = Vector2.zero;
            Debug.Log("[AIPursueState] Exit — Mất dấu Player.");
        }

        /// <summary>
        /// Flip sprite AI theo hướng đuổi Player.
        /// </summary>
        private void HandleFacing()
        {
            if (_ctx.PlayerTransform == null) return;

            float directionX = _ctx.PlayerTransform.position.x - _ctx.transform.position.x;
            if (directionX != 0)
            {
                Vector3 localScale = _ctx.transform.localScale;
                localScale.x = directionX > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                _ctx.transform.localScale = localScale;
            }
        }
    }
}
