using UnityEngine;

namespace Suhdo.Features.Player.StateMachine.RootStates
{
    /// <summary>
    /// Root State: Trạng thái chiến đấu.
    /// 
    /// Sub States khả dụng:
    /// - IdleState: đứng yên (chờ tấn công)
    /// - MoveState: di chuyển trong chiến đấu
    /// - AttackState: đang tấn công kẻ thù
    /// 
    /// Chuyển về NormalState khi Player rời vùng chiến đấu.
    /// </summary>
    public class PlayerCombatState : PlayerBaseState
    {
        public PlayerCombatState(PlayerStateMachine ctx) : base(ctx)
        {
            IsRootState = true;
        }

        public override void EnterState()
        {
            // Khởi tạo sub state mặc định = Idle
            InitializeSubState(Ctx.IdleState);

            // TODO: Bật Combat UI (health bar, combat indicator...)
            // TODO: Thay đổi animation set sang combat stance

            Debug.Log("<color=#FF6B6B>[HSM]</color> Enter CombatState");
        }

        public override void UpdateState()
        {
            // 1. Kiểm tra chuyển Root State
            CheckSwitchStates();

            // 2. Propagate xuống Sub State
            UpdateSubState();
        }

        public override void PhysicsUpdateState()
        {
            // Propagate physics xuống Sub State
            PhysicsUpdateSubState();
        }

        public override void ExitState()
        {
            // TODO: Tắt Combat UI

            Debug.Log("<color=#FF6B6B>[HSM]</color> Exit CombatState");
        }

        public override void CheckSwitchStates()
        {
            // Khi rời vùng chiến đấu → về NormalState
            if (!Ctx.IsInCombatZone)
            {
                SwitchState(Ctx.NormalState);
            }
        }
    }
}
