namespace Suhdo.Data
{
    /// <summary>
    /// Phân loại vũ khí, dùng để lựa chọn animation set, hiệu ứng âm thanh,
    /// và logic tấn công phù hợp trong PlayerAttackState.
    /// </summary>
    public enum WeaponType
    {
        /// <summary>Cận chiến: Cưa máy, Dao, Búa — hitbox gần người chơi.</summary>
        Melee,

        /// <summary>Tầm xa: Súng nước, Súng cà rốt — bắn projectile.</summary>
        Ranged,

        /// <summary>Diện rộng: Bom khói, Điện xung — damage theo vùng (AoE).</summary>
        Area,
    }
}
