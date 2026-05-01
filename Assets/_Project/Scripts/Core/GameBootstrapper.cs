using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using Suhdo.Managers;

namespace Suhdo.Core
{
    /// <summary>
    /// Single Entry Point của game.
    /// Script này nằm ở scene Boot.unity, có nhiệm vụ load các hệ thống con và chuyển sang scene Main sau khi xong.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        private GameManager _gameManager;

        [Inject]
        public void Construct(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        private async void Start()
        {
            Debug.Log("[GameBootstrapper] Starting initialization sequence...");
            
            // 1. Initialize 3rd party SDKs (Firebase, Ads, Analytics) here
            await InitializeSDKsAsync();

            // 2. Tải Data từ Server hoặc Local nếu cần
            await LoadGameDataAsync();

            // 3. Đổi trạng thái sang Main Menu hoặc Playing
            _gameManager.ChangeState(GameState.MainMenu);

            // 4. Load Scene chính (ví dụ Scene có build index 1)
            Debug.Log("[GameBootstrapper] Loading Main Scene...");
            await SceneManager.LoadSceneAsync(1).ToUniTask();
        }

        private async UniTask InitializeSDKsAsync()
        {
            // Giả lập delay
            await UniTask.Delay(500);
            Debug.Log("[GameBootstrapper] SDKs initialized.");
        }

        private async UniTask LoadGameDataAsync()
        {
            // Giả lập loading delay
            await UniTask.Delay(500);
            Debug.Log("[GameBootstrapper] Game data loaded.");
        }
    }
}
