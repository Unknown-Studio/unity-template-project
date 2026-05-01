using Suhdo.Features.Weapons;
using UnityEngine;

namespace Suhdo.Data
{
    /// <summary>
    /// Nguồn dữ liệu gốc (Single Source of Truth) cho một loại vũ khí.
    ///
    /// Cách tạo vũ khí mới:
    ///   Assets > Create > Suhdo/Data/Weapon Data → đặt tên → điền chỉ số.
    ///
    /// KHÔNG chứa logic — chỉ chứa dữ liệu thuần.
    /// Logic tấn công nằm trong PlayerAttackState.
    /// Buff/Debuff apply qua IWeaponStat layer (BuffedWeaponStat).
    /// </summary>
    [CreateAssetMenu(fileName = "SO_NewWeapon", menuName = "Suhdo/Data/Weapon Data", order = 2)]
    public class WeaponData : ScriptableObject, IWeaponStat
    {
        // ═══════════════════════════════════════════════
        //  IDENTITY
        // ═══════════════════════════════════════════════

        [Header("Identity")]
        [Tooltip("Unique key định danh vũ khí, dùng để save/load (e.g. 'weapon_chainsaw')")]
        public string weaponId;

        [Tooltip("Tên hiển thị trong UI")]
        public string weaponName;

        [Tooltip("Phân loại vũ khí — ảnh hưởng đến animation và hitbox logic")]
        public WeaponType weaponType;

        // ═══════════════════════════════════════════════
        //  COMBAT STATS
        // ═══════════════════════════════════════════════

        [Header("Combat Stats")]
        [Tooltip("Sát thương cơ bản mỗi lần đánh (trước khi áp Buff)")]
        [Min(0f)]
        public float baseDamage = 10f;

        [Tooltip("Số lần tấn công mỗi giây (hits/sec). Interval = 1 / attackRate")]
        [Min(0.1f)]
        public float attackRate = 1.5f;

        [Tooltip("Tầm đánh — radius (Melee/Area) hoặc max projectile range (Ranged), đơn vị Unity units")]
        [Min(0f)]
        public float attackRange = 1.5f;

        // ═══════════════════════════════════════════════
        //  VISUALS
        // ═══════════════════════════════════════════════

        [Header("Visuals")]
        [Tooltip("Icon hiển thị trong UI inventory / HUD")]
        public Sprite icon;

        [Tooltip("Prefab spawn tại WeaponAnchor trên Player khi trang bị")]
        public GameObject weaponPrefab;

        // ═══════════════════════════════════════════════
        //  IWeaponStat — Trả về chỉ số gốc (không buff)
        // ═══════════════════════════════════════════════

        /// <inheritdoc/>
        public float GetDamage() => baseDamage;

        /// <inheritdoc/>
        public float GetAttackRate() => attackRate;

        /// <inheritdoc/>
        public float GetAttackRange() => attackRange;

        /// <inheritdoc/>
        public WeaponType GetWeaponType() => weaponType;

        /// <inheritdoc/>
        public string GetWeaponId() => weaponId;
    }
}
