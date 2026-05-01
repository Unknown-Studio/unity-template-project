using System;
using Suhdo.Data;
using Suhdo.Features.Weapons;
using Suhdo.Managers.Input;
using UnityEngine;

namespace Suhdo.Features.Player
{
    /// <summary>
    /// MonoBehaviour gắn trên Player GameObject, chịu trách nhiệm hiển thị
    /// visual prefab của vũ khí đang trang bị.
    ///
    /// Subscribe OnWeaponChanged từ IWeaponService (đã inject qua Reflex).
    /// Khi vũ khí thay đổi → despawn prefab cũ, spawn prefab mới tại WeaponAnchor.
    ///
    /// KHÔNG chứa combat logic — chỉ quản lý visual.
    /// </summary>
    public class WeaponHandler : MonoBehaviour
    {
        // ═══════════════════════════════════════════════
        //  SERIALIZED FIELDS
        // ═══════════════════════════════════════════════

        [Header("Anchor")]
        [Tooltip("Transform nơi gắn weapon prefab (thường là tay phải hoặc trước ngực Player)")]
        [SerializeField] private Transform weaponAnchor;

        // ═══════════════════════════════════════════════
        //  RUNTIME STATE
        // ═══════════════════════════════════════════════

        private IWeaponService _weaponService;
        public IInputService _inputService;
        private GameObject _currentWeaponVisual;

        // ═══════════════════════════════════════════════
        //  SETUP — Nhận IWeaponService từ PlayerStateMachine
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Gọi bởi PlayerStateMachine.Start() sau khi DI inject xong.
        /// Không dùng [Inject] trực tiếp vì WeaponHandler là child component.
        /// </summary>
        public void Initialize(IWeaponService weaponService, IInputService inputService)
        {
            _weaponService = weaponService;
            _inputService = inputService;
            _weaponService.OnWeaponChanged += HandleWeaponChanged;

            // Hiển thị vũ khí ban đầu nếu đã có
            if (_weaponService.CurrentWeaponData != null)
                HandleWeaponChanged(_weaponService.CurrentWeaponData);
        }

        private void OnDestroy()
        {
            if (_weaponService != null)
                _weaponService.OnWeaponChanged -= HandleWeaponChanged;
        }

        private void Update()
        {
            if (_inputService == null) return;
            
            Vector2 direction = _inputService.MoveDirection;
            
            // Không xoay nếu joystick được thả ra (giữ hướng cũ)
            if (direction.sqrMagnitude < 0.01f) return;

            // Lấy dấu của scale X (nhân vật đang quay phải = 1, quay trái = -1)
            float flipX = Mathf.Sign(transform.localScale.x);

            // Bù trừ góc xoay: Nếu nhân vật lật trái, ta đảo ngược trục X của input
            float angle = Mathf.Atan2(direction.y, direction.x * flipX) * Mathf.Rad2Deg;
            
            // Gán vào localRotation (xoay tương đối với nhân vật) thay vì rotation (world)
            weaponAnchor.localRotation = Quaternion.Euler(0, 0, angle);
        }

        // ═══════════════════════════════════════════════
        //  VISUAL MANAGEMENT
        // ═══════════════════════════════════════════════

        private void HandleWeaponChanged(WeaponData newWeaponData)
        {
            // Despawn visual cũ
            if (_currentWeaponVisual != null)
            {
                Destroy(_currentWeaponVisual);
                _currentWeaponVisual = null;
            }

            // Nếu unequip → không spawn gì
            if (newWeaponData == null || newWeaponData.weaponPrefab == null)
            {
                Debug.Log("<color=#FFA500>[WeaponHandler]</color> No weapon prefab to spawn.");
                return;
            }

            // Spawn tại WeaponAnchor
            Transform anchor = weaponAnchor != null ? weaponAnchor : transform;
            _currentWeaponVisual = Instantiate(newWeaponData.weaponPrefab, anchor);
            _currentWeaponVisual.transform.localPosition = Vector3.zero;
            _currentWeaponVisual.transform.localRotation = Quaternion.identity;

            Debug.Log($"<color=#FFA500>[WeaponHandler]</color> Spawned visual: {newWeaponData.weaponName}");
        }
    }
}
