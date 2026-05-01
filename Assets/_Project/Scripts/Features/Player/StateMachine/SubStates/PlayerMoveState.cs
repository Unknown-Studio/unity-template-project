using UnityEngine;

namespace Suhdo.Features.Player.StateMachine.SubStates
{
    /// <summary>
    /// Sub State: Di chuyển — dùng chung cho cả NormalState và CombatState.
    /// 
    /// Sử dụng Rigidbody2D.MovePosition cho physics-based movement.
    /// Đọc input từ IInputService thông qua Ctx (PlayerStateMachine).
    /// Khi người chơi thả joystick → chuyển về IdleState.
    /// </summary>
    public class PlayerMoveState : PlayerBaseState
    {
        public PlayerMoveState(PlayerStateMachine ctx) : base(ctx) { }

        public override void EnterState()
        {
            // TODO: Bật Run animation
            // Ctx.Animator.Play("Run");

            Debug.Log("<color=#90EE90>[HSM]</color> Enter MoveState");
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            HandleRotation();
        }

        public override void PhysicsUpdateState()
        {
            HandleMovement();
        }

        public override void ExitState()
        {
            // Dừng di chuyển khi rời state
            Ctx.Rigidbody2D.linearVelocity = Vector2.zero;

            Debug.Log("<color=#90EE90>[HSM]</color> Exit MoveState");
        }

        public override void CheckSwitchStates()
        {
            // Khi người chơi ngừng di chuyển → về Idle
            if (!Ctx.InputService.IsMoving)
            {
                SwitchState(Ctx.IdleState);
            }
        }

        /// <summary>
        /// Di chuyển Player bằng Rigidbody2D.MovePosition trong FixedUpdate.
        /// moveSpeed lấy từ PlayerStats (UnityAtoms FloatReference).
        /// </summary>
        private void HandleMovement()
        {
            Vector2 moveInput = Ctx.InputService.MoveDirection;
            float speed = Ctx.PlayerStats.moveSpeed.Value;

            Vector2 targetPosition = Ctx.Rigidbody2D.position + moveInput * speed * Time.fixedDeltaTime;
            Ctx.Rigidbody2D.MovePosition(targetPosition);
        }

        /// <summary>
        /// Flip sprite theo hướng di chuyển (trái/phải) cho game 2D.
        /// </summary>
        private void HandleRotation()
        {
            Vector2 moveInput = Ctx.InputService.MoveDirection;

            if (moveInput.x != 0)
            {
                Vector3 localScale = Ctx.transform.localScale;
                localScale.x = moveInput.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                Ctx.transform.localScale = localScale;
            }
        }
    }
}
