using Suhdo.Managers.Save;
using UnityEngine;

namespace Suhdo.Managers
{
    public enum GameState
    {
        Boot,
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// GameManager quản lý luồng Game State chính. 
    /// Không kế thừa MonoBehaviour nếu không cần thiết, giúp unit test dễ dàng.
    /// </summary>
    public class GameManager
    {
        private readonly ISaveManager _saveManager;
        
        public GameState CurrentState { get; private set; }

        // Constructor Injection bằng Reflex
        public GameManager(ISaveManager saveManager)
        {
            _saveManager = saveManager;
            CurrentState = GameState.Boot;
            Debug.Log("[GameManager] Initialized.");
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            CurrentState = newState;
            Debug.Log($"[GameManager] State changed to: {newState}");
            // Có thể publish sự kiện bằng UnityAtoms GameEvent tại đây
        }
    }
}
