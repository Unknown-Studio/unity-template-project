using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Suhdo.Managers.Input
{
    /// <summary>
    /// Service quản lý Input sử dụng code thuần của New Input System.
    /// Dành riêng cho game Idle Arcade: Người chơi chỉ có duy nhất thao tác Di chuyển (Joystick).
    /// Các hành động như Chặt cây, Đánh quái là tự động (Auto-attack).
    /// </summary>
    public class MobileInputService : IInputService, IDisposable
    {
        private readonly InputAction _moveAction;

        public Vector2 MoveDirection => _moveAction.ReadValue<Vector2>();
        
        // Trả về true nếu người chơi đang kéo joystick (ngưỡng 0.01 để tránh trôi ngón tay)
        public bool IsMoving => MoveDirection.sqrMagnitude > 0.01f;

        public MobileInputService()
        {
            Debug.Log("AAAAA");
            // Thiết lập Action loại "Value" (liên tục quét giá trị, như Joystick)
            _moveAction = new InputAction(name: "IdleMove", type: InputActionType.Value);
            
            // 1. Phím tắt để test trên máy tính (WASD)
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            // 2. Dành cho Mobile Joystick (Component OnScreenStick của Unity sẽ giả lập phím Gamepad)
            _moveAction.AddBinding("<Gamepad>/leftStick");

            // Kích hoạt action
            _moveAction.Enable();

            Debug.Log("[InputService] MobileInputService initialized (WASD + Joystick).");
        }

        public void Dispose()
        {
            _moveAction?.Disable();
            _moveAction?.Dispose();
        }
    }
}
