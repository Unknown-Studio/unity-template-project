using UnityEngine;

namespace Suhdo.Features.Player.StateMachine.RootStates
{
    /// <summary>
    /// Root State: Trạng thái bình thường (thăm dò, thu thập tài nguyên).
    /// 
    /// Sub States khả dụng:
    /// - IdleState: đứng yên
    /// - MoveState: di chuyển
    /// - GatherState: thu thập tài nguyên (cưa máy)
    /// 
    /// Chuyển sang CombatState khi Player vào vùng chiến đấu.
    /// </summary>
    public class PlayerNormalState : PlayerBaseState
    {
        public PlayerNormalState(PlayerStateMachine ctx) : base(ctx)
        {
            IsRootState = true;
        }

        public override void EnterState()
        {
            // Khởi tạo sub state mặc định = Idle
            InitializeSubState(Ctx.IdleState);

            Debug.Log("<color=#5EE05E>[HSM]</color> Enter NormalState");
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
            Debug.Log("<color=#5EE05E>[HSM]</color> Exit NormalState");
        }

        public override void CheckSwitchStates()
        {
            // Khi vào vùng chiến đấu → chuyển sang CombatState
            if (Ctx.IsInCombatZone)
            {
                SwitchState(Ctx.CombatState);
            }
        }
    }
}
