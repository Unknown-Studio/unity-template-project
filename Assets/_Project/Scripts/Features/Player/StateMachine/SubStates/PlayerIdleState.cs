using Suhdo.Features.Player.StateMachine.RootStates;
using UnityEngine;

namespace Suhdo.Features.Player.StateMachine.SubStates
{
    /// <summary>
    /// Sub State: Đứng yên — dùng chung cho cả NormalState và CombatState.
    /// 
    /// Logic chuyển state phụ thuộc vào Super State hiện tại:
    /// - Trong NormalState: có thể chuyển sang GatherState (nếu trong action zone)
    /// - Trong CombatState: có thể chuyển sang AttackState (nếu có target)
    /// - Cả hai: chuyển sang MoveState khi có input di chuyển
    /// </summary>
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerStateMachine ctx) : base(ctx) { }

        public override void EnterState()
        {
            // Dừng di chuyển
            Ctx.Rigidbody2D.linearVelocity = Vector2.zero;

            // TODO: Bật Idle animation
            // Ctx.Animator.Play("Idle");

            Debug.Log("<color=#87CEEB>[HSM]</color> Enter IdleState");
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
        }

        public override void ExitState()
        {
            Debug.Log("<color=#87CEEB>[HSM]</color> Exit IdleState");
        }

        public override void CheckSwitchStates()
        {
            // Ưu tiên 1: Kiểm tra input di chuyển → MoveState
            if (Ctx.InputService.IsMoving)
            {
                SwitchState(Ctx.MoveState);
                return;
            }

            // Ưu tiên 2: Logic phụ thuộc Super State
            // Kiểm tra Super State để quyết định chuyển sang state nào
            // (SOLID: mỗi state tự biết điều kiện chuyển, không dùng switch-case dài)

            // Trong NormalState + đứng trong action zone → GatherState
            if (Ctx.CurrentState is PlayerNormalState && Ctx.IsInActionZone)
            {
                SwitchState(Ctx.GatherState);
                return;
            }

            // Trong CombatState + đứng trong action zone → AttackState
            if (Ctx.CurrentState is PlayerCombatState && Ctx.IsInActionZone)
            {
                SwitchState(Ctx.AttackState);
                return;
            }
        }
    }
}
