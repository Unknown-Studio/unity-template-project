using UnityEngine;

namespace Suhdo.Core.FSM
{
    /// <summary>
    /// Bộ điều phối chuyển đổi giữa các State.
    /// Đảm bảo thứ tự gọi đúng: Exit() state cũ → Enter() state mới.
    /// MonoBehaviour sở hữu StateMachine này chỉ cần gọi Update()/FixedUpdate() mỗi frame.
    /// </summary>
    public class StateMachine
    {
        public BaseState CurrentState { get; private set; }

        /// <summary>
        /// Khởi tạo StateMachine với State đầu tiên. Gọi 1 lần duy nhất trong Start().
        /// </summary>
        public void Initialize(BaseState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
            Debug.Log($"[StateMachine] Initialized with: {startingState.GetType().Name}");
        }

        /// <summary>
        /// Chuyển sang State mới. Tự động gọi Exit() cho state cũ và Enter() cho state mới.
        /// </summary>
        public void ChangeState(BaseState newState)
        {
            if (newState == CurrentState) return;

            Debug.Log($"[StateMachine] {CurrentState.GetType().Name} → {newState.GetType().Name}");
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        /// <summary>
        /// Gọi từ MonoBehaviour.Update() — delegate xuống State hiện tại.
        /// </summary>
        public void Update()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// Gọi từ MonoBehaviour.FixedUpdate() — delegate xuống State hiện tại.
        /// </summary>
        public void FixedUpdate()
        {
            CurrentState?.PhysicsUpdate();
        }
    }
}
