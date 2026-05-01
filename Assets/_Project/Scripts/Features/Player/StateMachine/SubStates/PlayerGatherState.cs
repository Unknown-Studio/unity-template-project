using UnityEngine;

namespace Suhdo.Features.Player.StateMachine.SubStates
{
    /// <summary>
    /// Sub State: Thu thập tài nguyên (cưa máy, chặt cây...).
    /// CHỈ khả dụng trong NormalState (Root).
    /// 
    /// Player tự động thực hiện hành động khi đứng trong vùng tương tác (Idle Arcade pattern).
    /// Thoát khi: người chơi di chuyển, hoặc rời khỏi action zone.
    /// </summary>
    public class PlayerGatherState : PlayerBaseState
    {
        private float _actionTimer;

        public PlayerGatherState(PlayerStateMachine ctx) : base(ctx) { }

        public override void EnterState()
        {
            // Dừng di chuyển khi bắt đầu thu thập
            Ctx.Rigidbody2D.linearVelocity = Vector2.zero;

            // Reset timer
            _actionTimer = 0f;

            // TODO: Bật animation cưa máy / thu thập
            // Ctx.Animator.Play("Gather");

            // TODO: Bật VFX (tia lửa, bụi...)
            // Ctx.GatherVFX?.SetActive(true);

            Debug.Log("<color=#FFA500>[HSM]</color> Enter GatherState — Bắt đầu thu thập!");
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            HandleGatherLoop();
        }

        public override void ExitState()
        {
            // TODO: Tắt VFX
            // Ctx.GatherVFX?.SetActive(false);

            _actionTimer = 0f;

            Debug.Log("<color=#FFA500>[HSM]</color> Exit GatherState");
        }

        public override void CheckSwitchStates()
        {
            // Nếu người chơi di chuyển → interrupt, về Idle
            if (Ctx.InputService.IsMoving)
            {
                SwitchState(Ctx.IdleState);
                return;
            }

            // Nếu rời khỏi action zone → về Idle
            if (!Ctx.IsInActionZone)
            {
                SwitchState(Ctx.IdleState);
            }
        }

        /// <summary>
        /// Vòng lặp thu thập: mỗi interval thực hiện 1 lần action.
        /// attackSpeed lấy từ PlayerStats (UnityAtoms FloatReference).
        /// </summary>
        private void HandleGatherLoop()
        {
            _actionTimer += Time.deltaTime;

            float interval = 1f / Ctx.PlayerStats.attackSpeed.Value;

            if (_actionTimer >= interval)
            {
                _actionTimer -= interval;
                PerformGather();
            }
        }

        /// <summary>
        /// Thực hiện 1 lần thu thập. Gọi CurrencyManager nếu cần thưởng ngay.
        /// </summary>
        private void PerformGather()
        {
            float gatherPower = Ctx.PlayerStats.attackPower.Value;

            // TODO: Áp damage lên resource object
            // Ctx.CurrentInteractable?.TakeDamage(gatherPower);

            // TODO: Thưởng tiền khi thu thập xong
            // Ctx.CurrencyManager.AddGold(1);

            Debug.Log($"<color=#FFA500>[HSM]</color> Gather! Power: {gatherPower}");
        }
    }
}
