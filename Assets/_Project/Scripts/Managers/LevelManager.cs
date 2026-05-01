using UnityEngine;

namespace Suhdo.Managers
{
    /// <summary>
    /// LevelManager được cài đặt ở scope Scene (ví dụ: Main.unity).
    /// Quản lý spawn quái, thông tin vùng đất hiện tại.
    /// Kế thừa MonoBehaviour vì có thể cần Instantiate các prefab.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private GameManager _gameManager;

        // Phương thức Inject được Reflex gọi khi construct
        [Reflex.Attributes.Inject]
        public void Construct(GameManager gameManager)
        {
            _gameManager = gameManager;
            Debug.Log("[LevelManager] Constructed with GameManager dependency.");
        }

        private void Start()
        {
            // Bắt đầu màn chơi
            if (_gameManager.CurrentState != GameState.Playing)
            {
                _gameManager.ChangeState(GameState.Playing);
            }
        }
    }
}
