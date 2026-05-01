using Suhdo.Data;

namespace Suhdo.Features.Weapons
{
    /// <summary>
    /// Interface chuẩn để đọc chỉ số chiến đấu của vũ khí.
    ///
    /// Tại sao cần interface này?
    /// ─────────────────────────────────────────────────
    /// Mọi consumer (PlayerAttackState, UI, AI...) đều chỉ cần biết
    /// "vũ khí này có bao nhiêu damage?" mà không quan tâm nguồn gốc:
    ///   • WeaponData      → chỉ số gốc từ ScriptableObject
    ///   • BuffedWeaponStat → chỉ số sau khi áp Buff/Debuff
    ///   • MockWeapon      → dùng trong Unit Test
    ///
    /// Buff System Integration:
    ///   Khi cần Buff, chỉ tạo BuffedWeaponStat wrapping WeaponData.
    ///   PlayerAttackState KHÔNG cần thay đổi gì — vẫn gọi GetDamage().
    /// </summary>
    public interface IWeaponStat
    {
        /// <summary>
        /// Sát thương thực tế mỗi hit (đã áp Buff/Debuff nếu có).
        /// </summary>
        float GetDamage();

        /// <summary>
        /// Số lần tấn công mỗi giây. Attack interval = 1f / GetAttackRate().
        /// </summary>
        float GetAttackRate();

        /// <summary>
        /// Tầm đánh — radius cho Melee/Area, max range cho Ranged.
        /// </summary>
        float GetAttackRange();

        /// <summary>
        /// Loại vũ khí — dùng để switch animation set trong PlayerAttackState.
        /// </summary>
        WeaponType GetWeaponType();

        /// <summary>
        /// ID định danh duy nhất, dùng để save/load trạng thái trang bị.
        /// </summary>
        string GetWeaponId();
    }
}
