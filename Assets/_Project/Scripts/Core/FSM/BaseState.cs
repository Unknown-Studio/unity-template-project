namespace Suhdo.Core.FSM
{
    /// <summary>
    /// Lớp trừu tượng nền tảng cho mọi State trong hệ thống FSM.
    /// Mỗi State cụ thể (Player, AI) sẽ kế thừa class này và override các hàm cần thiết.
    /// Dùng virtual thay vì abstract để State con chỉ cần override những gì thực sự cần.
    /// </summary>
    public abstract class BaseState
    {
        protected readonly StateMachine stateMachine;

        public BaseState(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        /// <summary>
        /// Được gọi khi State bắt đầu hoạt động (vừa chuyển sang State này).
        /// Dùng để khởi tạo animation, reset biến, bật hiệu ứng...
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Được gọi mỗi frame từ MonoBehaviour.Update().
        /// Dùng để kiểm tra điều kiện chuyển State, xử lý logic không liên quan vật lý.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Được gọi mỗi fixed frame từ MonoBehaviour.FixedUpdate().
        /// Dùng cho logic vật lý: di chuyển Rigidbody, raycast, lực...
        /// </summary>
        public virtual void PhysicsUpdate() { }

        /// <summary>
        /// Được gọi khi rời khỏi State này (chuyển sang State khác).
        /// Dùng để dọn dẹp: tắt VFX, reset timer, disable collider...
        /// </summary>
        public virtual void Exit() { }
    }
}
