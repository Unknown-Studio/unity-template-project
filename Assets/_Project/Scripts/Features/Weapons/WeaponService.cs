using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Suhdo.Core;
using Suhdo.Data;
using UnityEngine;

namespace Suhdo.Features.Weapons
{
    /// <summary>
    /// Service quản lý vũ khí đang trang bị của Player.
    ///
    /// Đăng ký trong ProjectInstaller:
    ///   builder.RegisterType(typeof(WeaponService), new[] { typeof(IWeaponService) },
    ///       Lifetime.Singleton, Resolution.Lazy);
    ///
    /// Không phải MonoBehaviour — inject pure C# qua Reflex DI vào PlayerStateMachine.
    ///
    /// Luồng hoạt động:
    ///   1. EquipWeapon(weaponData)     → lưu _currentWeapon, raise OnWeaponChanged
    ///   2. AddBuff(buff)               → push vào _activeBuffs, invalidate cache
    ///   3. PlayerAttackState gọi       → CurrentWeaponStat.GetDamage() → tự động áp buff
    /// </summary>
    public class WeaponService : IWeaponService
    {
        // ═══════════════════════════════════════════════
        //  DEPENDENCIES
        // ═══════════════════════════════════════════════

        private readonly IAssetProvider _assetProvider;

        public WeaponService(IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
        }

        // ═══════════════════════════════════════════════
        //  STATE
        // ═══════════════════════════════════════════════

        private WeaponData _currentWeapon;
        private readonly List<BuffModifier> _activeBuffs = new List<BuffModifier>();

        /// <summary>
        /// Cache IWeaponStat — rebuild khi equip hoặc buff thay đổi.
        /// Tránh tạo object mới mỗi frame trong GetDamage().
        /// </summary>
        private IWeaponStat _cachedStat;
        private bool _statDirty = true;

        // ═══════════════════════════════════════════════
        //  IWeaponService
        // ═══════════════════════════════════════════════

        /// <inheritdoc/>
        public event Action<WeaponData> OnWeaponChanged;

        /// <inheritdoc/>
        public WeaponData CurrentWeaponData => _currentWeapon;

        /// <inheritdoc/>
        public IWeaponStat CurrentWeaponStat
        {
            get
            {
                if (_currentWeapon == null) return null;

                if (_statDirty)
                {
                    // Nếu không có buff → dùng WeaponData trực tiếp (zero alloc)
                    // Nếu có buff → wrap qua BuffedWeaponStat
                    _cachedStat = _activeBuffs.Count == 0
                        ? (IWeaponStat)_currentWeapon
                        : new BuffedWeaponStat(_currentWeapon, _activeBuffs);

                    _statDirty = false;
                }

                return _cachedStat;
            }
        }

        /// <inheritdoc/>
        public void EquipWeapon(WeaponData weaponData)
        {
            if (weaponData == null)
            {
                Debug.LogWarning("[WeaponService] EquipWeapon called with null WeaponData.");
                return;
            }

            _currentWeapon = weaponData;
            _activeBuffs.Clear();
            InvalidateCache();

            Debug.Log($"<color=#FFA500>[WeaponService]</color> Equipped: {weaponData.weaponName} " +
                      $"| DMG: {weaponData.baseDamage} | Rate: {weaponData.attackRate}/s");

            OnWeaponChanged?.Invoke(weaponData);
        }

        /// <inheritdoc/>
        public async UniTask EquipWeaponByIdAsync(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return;

            Debug.Log($"<color=#FFA500>[WeaponService]</color> Loading weapon by ID: {weaponId}...");

            try
            {
                // Load asset từ Addressables thông qua AssetProvider
                WeaponData weaponData = await _assetProvider.LoadAssetAsync<WeaponData>(weaponId);
                
                if (weaponData != null)
                {
                    EquipWeapon(weaponData);
                }
                else
                {
                    Debug.LogError($"[WeaponService] Failed to load WeaponData with ID: {weaponId}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaponService] Error loading weapon {weaponId}: {e.Message}");
            }
        }

        /// <inheritdoc/>
        public void UnequipWeapon()
        {
            var previous = _currentWeapon;
            _currentWeapon = null;
            _activeBuffs.Clear();
            InvalidateCache();

            Debug.Log($"<color=#FFA500>[WeaponService]</color> Unequipped: {previous?.weaponName}");

            OnWeaponChanged?.Invoke(null);
        }

        /// <inheritdoc/>
        public void AddBuff(BuffModifier buff)
        {
            _activeBuffs.Add(buff);
            InvalidateCache();

            Debug.Log($"<color=#FFD700>[WeaponService]</color> Buff added: " +
                      $"DMG x{buff.DamageMultiplier} | Rate x{buff.AttackRateMultiplier} " +
                      $"| Tag: {buff.Tag}");
        }

        /// <inheritdoc/>
        public void ClearAllBuffs()
        {
            _activeBuffs.Clear();
            InvalidateCache();

            Debug.Log("<color=#FFD700>[WeaponService]</color> All buffs cleared.");
        }

        // ═══════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════════════

        private void InvalidateCache()
        {
            _statDirty  = true;
            _cachedStat = null;
        }
    }
}
