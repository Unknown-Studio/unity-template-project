using Suhdo.Features.Weapons;
using UnityEngine;

namespace Suhdo.Features.Player.StateMachine.SubStates
{
    /// <summary>
    /// Sub State: Tấn công kẻ thù.
    /// CHỈ khả dụng trong CombatState (Root).
    /// 
    /// Player tự động tấn công khi có target trong tầm (Idle Arcade auto-attack pattern).
    /// Thoát khi: người chơi di chuyển, hết target, hoặc rời action zone.
    /// </summary>
    public class PlayerAttackState : PlayerBaseState
    {
        private float _attackTimer;
        private int _comboCount;
        private const int MAX_COMBO = 3;

        // Cache IWeaponStat khi Enter để tránh property lookup mỗi frame
        private IWeaponStat _weaponStat;

        public PlayerAttackState(PlayerStateMachine ctx) : base(ctx) { }

        public override void EnterState()
        {
            // Cache IWeaponStat một lần — trả về buffed hoặc base tùy WeaponService
            _weaponStat = Ctx.WeaponService?.CurrentWeaponStat;

            if (_weaponStat == null)
            {
                // Không có vũ khí → về Idle ngay
                Debug.LogWarning("<color=#FF4444>[AttackState]</color> No weapon equipped! Returning to Idle.");
                SwitchState(Ctx.IdleState);
                return;
            }

            // Dừng di chuyển khi bắt đầu tấn công
            Ctx.Rigidbody2D.linearVelocity = Vector2.zero;

            // Reset combo và timer
            _attackTimer = 0f;
            _comboCount  = 0;

            // TODO: Switch animation set theo WeaponType
            // switch (_weaponStat.GetWeaponType()) { ... }
            // Ctx.Animator.Play("Attack_01");

            Debug.Log($"<color=#FF4444>[HSM]</color> Enter AttackState — [{_weaponStat.GetWeaponType()}] " +
                      $"{_weaponStat.GetWeaponId()} | DMG: {_weaponStat.GetDamage():F1} " +
                      $"| Rate: {_weaponStat.GetAttackRate():F1}/s");
        }

        public override void UpdateState()
        {
            // Guard: nếu weapon bị unequip mid-combat
            if (_weaponStat == null) return;

            CheckSwitchStates();
            HandleAttackLoop();
        }

        public override void ExitState()
        {
            _attackTimer = 0f;
            _comboCount  = 0;
            _weaponStat  = null;

            Debug.Log("<color=#FF4444>[HSM]</color> Exit AttackState");
        }

        public override void CheckSwitchStates()
        {
            // Nếu người chơi di chuyển → interrupt, về Idle
            if (Ctx.InputService.IsMoving)
            {
                SwitchState(Ctx.IdleState);
                return;
            }

            // Nếu rời khỏi action/combat zone → về Idle
            if (!Ctx.IsInActionZone)
            {
                SwitchState(Ctx.IdleState);
            }
        }

        /// <summary>
        /// Vòng lặp auto-attack dựa trên attackRate của vũ khí.
        /// Interval = 1f / GetAttackRate() — tự động cập nhật nếu có Buff tốc độ.
        /// </summary>
        private void HandleAttackLoop()
        {
            _attackTimer += Time.deltaTime;

            // Đọc trực tiếp từ IWeaponStat — đã bao gồm buff nếu có
            float interval = 1f / _weaponStat.GetAttackRate();

            if (_attackTimer >= interval)
            {
                _attackTimer -= interval;
                PerformAttack();
            }
        }

        /// <summary>
        /// Thực hiện 1 đòn tấn công. Combo tăng dần, reset sau MAX_COMBO.
        /// Damage đọc từ IWeaponStat.GetDamage() — tự động áp buff nếu có.
        /// </summary>
        private void PerformAttack()
        {
            _comboCount = (_comboCount % MAX_COMBO) + 1;

            // Đọc damage từ IWeaponStat — nguồn duy nhất, bao gồm cả Buff
            float damage = _weaponStat.GetDamage();

            // TODO: Áp damage lên enemy trong tầm GetAttackRange()
            // float range = _weaponStat.GetAttackRange();
            // Ctx.CurrentTarget?.TakeDamage(damage);

            // TODO: Switch animation theo WeaponType + combo
            // Ctx.Animator.Play($"Attack_{_comboCount:D2}");

            Debug.Log($"<color=#FF4444>[HSM]</color> [{_weaponStat.GetWeaponType()}] Combo {_comboCount} " +
                      $"→ Damage: <b>{damage:F1}</b>");
        }
    }
}
