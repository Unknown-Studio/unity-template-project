using Cysharp.Threading.Tasks;
using Suhdo.Data;

namespace Suhdo.Features.Weapons
{
    /// <summary>
    /// Interface cho service quản lý vũ khí đang trang bị của Player.
    ///
    /// Inject vào PlayerStateMachine qua Reflex DI.
    /// Các State truy cập qua Ctx.WeaponService.
    /// </summary>
    public interface IWeaponService
    {
        /// <summary>
        /// Chỉ số vũ khí đang trang bị (có thể đã wrap qua BuffedWeaponStat).
        /// Null nếu Player chưa trang bị vũ khí nào.
        /// </summary>
        IWeaponStat CurrentWeaponStat { get; }

        /// <summary>
        /// Dữ liệu gốc của vũ khí đang trang bị.
        /// Null nếu Player chưa trang bị vũ khí nào.
        /// </summary>
        WeaponData CurrentWeaponData { get; }

        /// <summary>
        /// Trang bị vũ khí mới. Ghi đè vũ khí đang mang.
        /// Raise event OnWeaponChanged để WeaponHandler cập nhật visual.
        /// </summary>
        void EquipWeapon(WeaponData weaponData);

        /// <summary>
        /// Load và trang bị vũ khí dựa trên ID hoặc Addressable Key.
        /// Phù hợp để load từ Save Game hoặc Config.
        /// </summary>
        UniTask EquipWeaponByIdAsync(string weaponId);

        /// <summary>
        /// Tháo vũ khí hiện tại. CurrentWeaponStat sẽ trở về null.
        /// </summary>
        void UnequipWeapon();

        /// <summary>
        /// Thêm buff tạm thời vào vũ khí đang trang bị.
        /// BuffedWeaponStat sẽ tự động tính toán chỉ số mới.
        /// </summary>
        void AddBuff(BuffModifier buff);

        /// <summary>
        /// Xóa toàn bộ buff đang active.
        /// </summary>
        void ClearAllBuffs();

        /// <summary>
        /// Event raised khi vũ khí thay đổi (equip/unequip).
        /// WeaponHandler subscribe để spawn/despawn prefab.
        /// </summary>
        event System.Action<WeaponData> OnWeaponChanged;
    }
}
