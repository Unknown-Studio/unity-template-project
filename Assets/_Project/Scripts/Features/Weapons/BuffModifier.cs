namespace Suhdo.Features.Weapons
{
    /// <summary>
    /// Struct mô tả một hiệu ứng Buff hoặc Debuff áp lên vũ khí.
    ///
    /// Dùng struct (value type) để tránh GC allocation khi add/remove nhiều buff.
    ///
    /// Cách dùng:
    ///   var buff = new BuffModifier { DamageMultiplier = 1.5f, Duration = 5f };
    ///   weaponService.AddBuff(buff);
    ///
    /// Mở rộng: Thêm field mới (CritChance, RangeMultiplier...) mà không
    /// cần thay đổi IWeaponStat hay WeaponService.
    /// </summary>
    public struct BuffModifier
    {
        /// <summary>Nhân với GetDamage(). 1.0 = không đổi, 1.5 = +50% damage.</summary>
        public float DamageMultiplier;

        /// <summary>Nhân với GetAttackRate(). 1.0 = không đổi, 2.0 = +100% tốc độ.</summary>
        public float AttackRateMultiplier;

        /// <summary>Nhân với GetAttackRange(). 1.0 = không đổi.</summary>
        public float RangeMultiplier;

        /// <summary>
        /// Thời gian hiệu lực (giây). 0 = vĩnh viễn đến khi ClearBuffs() được gọi.
        /// WeaponService sẽ implement timer tick-down logic.
        /// </summary>
        public float Duration;

        /// <summary>Tag nhận dạng buff, dùng để remove buff cụ thể (e.g. "rage_buff").</summary>
        public string Tag;

        /// <summary>Shorthand tạo buff không thời hạn với chỉ DamageMultiplier.</summary>
        public static BuffModifier Damage(float multiplier, string tag = "") =>
            new BuffModifier
            {
                DamageMultiplier    = multiplier,
                AttackRateMultiplier = 1f,
                RangeMultiplier     = 1f,
                Duration            = 0f,
                Tag                 = tag,
            };

        /// <summary>Shorthand tạo buff tốc độ tấn công.</summary>
        public static BuffModifier Speed(float multiplier, float duration = 0f, string tag = "") =>
            new BuffModifier
            {
                DamageMultiplier    = 1f,
                AttackRateMultiplier = multiplier,
                RangeMultiplier     = 1f,
                Duration            = duration,
                Tag                 = tag,
            };
    }
}
