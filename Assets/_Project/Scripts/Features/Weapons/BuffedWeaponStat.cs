using System.Collections.Generic;
using Suhdo.Data;

namespace Suhdo.Features.Weapons
{
    /// <summary>
    /// Wrapper implement IWeaponStat, áp danh sách BuffModifier lên WeaponData gốc.
    ///
    /// Pattern: Decorator — bao quanh WeaponData mà không thay đổi nó.
    ///
    /// Consumer (PlayerAttackState) KHÔNG biết đây là WeaponData hay BuffedWeaponStat
    /// vì cả hai đều implement IWeaponStat.
    ///
    ///   IWeaponStat stat = weaponService.CurrentWeaponStat;
    ///   float dmg = stat.GetDamage(); // Tự động trả về base hoặc buffed
    /// </summary>
    public class BuffedWeaponStat : IWeaponStat
    {
        private readonly WeaponData _base;
        private readonly IReadOnlyList<BuffModifier> _buffs;

        public BuffedWeaponStat(WeaponData baseData, IReadOnlyList<BuffModifier> buffs)
        {
            _base  = baseData;
            _buffs = buffs;
        }

        // ═══════════════════════════════════════════════
        //  IWeaponStat — tính chỉ số có buff
        // ═══════════════════════════════════════════════

        /// <inheritdoc/>
        public float GetDamage()
        {
            float result = _base.baseDamage;
            foreach (var b in _buffs)
                result *= b.DamageMultiplier;
            return result;
        }

        /// <inheritdoc/>
        public float GetAttackRate()
        {
            float result = _base.attackRate;
            foreach (var b in _buffs)
                result *= b.AttackRateMultiplier;
            return result;
        }

        /// <inheritdoc/>
        public float GetAttackRange()
        {
            float result = _base.attackRange;
            foreach (var b in _buffs)
                result *= b.RangeMultiplier;
            return result;
        }

        /// <inheritdoc/>
        public WeaponType GetWeaponType() => _base.weaponType;

        /// <inheritdoc/>
        public string GetWeaponId() => _base.weaponId;
    }
}
