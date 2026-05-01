using Suhdo.Managers.Input;
using UnityEngine;

namespace Suhdo.Features.Player.StateMachine
{
    /// <summary>
    /// Abstract base class cho mọi Player State trong hệ thống Hierarchical State Machine (HSM).
    /// 
    /// Mỗi State có thể có:
    /// - _currentSuperState: State cha (Root chứa nó). Null nếu chính nó là Root.
    /// - _currentSubState: State con đang active. Null nếu không có sub state.
    /// 
    /// Luồng gọi:
    /// PlayerStateMachine.Update() → Root.UpdateState() → Root.CheckSwitchStates() → Sub.UpdateState() → Sub.CheckSwitchStates()
    /// 
    /// Truy cập Services:
    /// Mọi state đều truy cập IInputService, CurrencyManager... thông qua Ctx (PlayerStateMachine).
    /// IInputService được inject 1 lần vào PlayerStateMachine bởi Reflex DI,
    /// tất cả state con đều dùng Ctx.InputService mà không cần khai báo lại.
    /// </summary>
    public abstract class PlayerBaseState
    {
        // ═══════════════════════════════════════════════
        //  CONTEXT & HIERARCHY
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Context object — tham chiếu đến PlayerStateMachine (MonoBehaviour).
        /// Qua đây truy cập: InputService, CurrencyManager, Rigidbody2D, PlayerStats...
        /// </summary>
        protected PlayerStateMachine Ctx { get; private set; }

        /// <summary>State cha đang chứa state này. Null nếu đây là Root State.</summary>
        private PlayerBaseState _currentSuperState;

        /// <summary>State con đang active bên trong state này. Null nếu không có sub state.</summary>
        private PlayerBaseState _currentSubState;

        /// <summary>Xác định đây có phải Root State hay không (không có Super).</summary>
        protected bool IsRootState { get; set; }

        // ═══════════════════════════════════════════════
        //  CONSTRUCTOR
        // ═══════════════════════════════════════════════

        public PlayerBaseState(PlayerStateMachine ctx)
        {
            Ctx = ctx;
        }

        // ═══════════════════════════════════════════════
        //  ABSTRACT LIFECYCLE (bắt buộc override)
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Gọi khi state được kích hoạt.
        /// Root State: khởi tạo sub state mặc định bằng InitializeSubState().
        /// Sub State: reset animation, biến cục bộ...
        /// </summary>
        public abstract void EnterState();

        /// <summary>
        /// Gọi mỗi frame. XỬ LÝ LOGIC + gọi CheckSwitchStates().
        /// Root PHẢI gọi UpdateSubState() ở cuối hàm để propagate xuống sub.
        /// </summary>
        public abstract void UpdateState();

        /// <summary>
        /// Gọi khi rời state. Dọn dẹp: tắt VFX, reset timer...
        /// Root: tự động exit sub state trước.
        /// </summary>
        public abstract void ExitState();

        /// <summary>
        /// Kiểm tra điều kiện chuyển state. Gọi SwitchState() khi cần.
        /// Root kiểm tra chuyển Root (Normal ↔ Combat).
        /// Sub kiểm tra chuyển Sub (Idle ↔ Move ↔ Gather/Attack).
        /// </summary>
        public abstract void CheckSwitchStates();

        /// <summary>
        /// Gọi mỗi FixedUpdate frame. Dùng cho logic vật lý (Rigidbody2D).
        /// Virtual thay vì abstract — không phải state nào cũng cần physics.
        /// </summary>
        public virtual void PhysicsUpdateState() { }

        // ═══════════════════════════════════════════════
        //  HIERARCHY PROPAGATION
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Gọi UpdateState() của sub state hiện tại.
        /// Root State PHẢI gọi hàm này ở cuối UpdateState() của mình.
        /// </summary>
        protected void UpdateSubState()
        {
            _currentSubState?.UpdateState();
        }

        /// <summary>
        /// Gọi PhysicsUpdateState() của sub state hiện tại.
        /// Root State PHẢI gọi hàm này ở cuối PhysicsUpdateState() của mình.
        /// </summary>
        protected void PhysicsUpdateSubState()
        {
            _currentSubState?.PhysicsUpdateState();
        }

        // ═══════════════════════════════════════════════
        //  STATE SWITCHING
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Chuyển sang state mới. Logic thông minh:
        /// - Nếu state hiện tại là ROOT (IsRootState = true) → đổi root state trên Ctx.
        /// - Nếu state hiện tại là SUB (có super) → đổi sub state trong super.
        /// </summary>
        protected void SwitchState(PlayerBaseState newState)
        {
            // Exit state hiện tại (+ exit sub nếu có)
            ExitState();

            // Enter state mới
            newState.EnterState();

            if (IsRootState)
            {
                // Đổi root state — cập nhật trên PlayerStateMachine
                Ctx.SetCurrentState(newState);
            }
            else if (_currentSuperState != null)
            {
                // Đổi sub state — cập nhật trong super state
                _currentSuperState.SetSubState(newState);
            }
        }

        // ═══════════════════════════════════════════════
        //  HIERARCHY SETUP
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Thiết lập sub state ban đầu cho Root State. Gọi trong EnterState() của Root.
        /// Tự động set quan hệ cha-con hai chiều.
        /// </summary>
        protected void InitializeSubState(PlayerBaseState subState)
        {
            SetSubState(subState);
            subState.EnterState();
        }

        /// <summary>Gán sub state và thiết lập quan hệ hai chiều.</summary>
        protected void SetSubState(PlayerBaseState subState)
        {
            _currentSubState = subState;
            subState.SetSuperState(this);
        }

        /// <summary>Gán super state (state cha).</summary>
        protected void SetSuperState(PlayerBaseState superState)
        {
            _currentSuperState = superState;
        }

        // ═══════════════════════════════════════════════
        //  HELPER
        // ═══════════════════════════════════════════════

        /// <summary>Trả về tên state để debug log.</summary>
        public override string ToString() => GetType().Name;
    }
}
