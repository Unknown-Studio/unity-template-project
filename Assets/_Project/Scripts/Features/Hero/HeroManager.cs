using System;
using UnityEngine;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using Suhdo.Core;
using UnityEngine.AddressableAssets;

namespace Suhdo.Features.Hero
{
    /// <summary>
    /// Giả lập một class cần load nhân vật thông qua Addressables.
    /// Nó sử dụng cơ chế Inject từ Reflex để nhận IAssetProvider.
    /// </summary>
    public class HeroManager : MonoBehaviour
    {
        // Reflex sẽ tự động inject dependency này vào khi HeroManager được khởi tạo hoặc OnEnable
        [Inject] private readonly IAssetProvider _assetProvider;
        
        [SerializeField] private AssetReference _playerReference;

        private GameObject _spawnedHero;
        private Sprite _heroIcon;

        private async void Start()
        {
            // Tránh NullRef nếu load trong Editor mà chưa install Bindings
            if (_assetProvider == null)
            {
                Debug.LogError("[HeroManager] Chờ Reflex Inject IAssetProvider thất bại. Hãy chắc rằng nó được đăng ký trong ProjectInstaller!");
                return;
            }

            // 1. Load và Khởi tạo Prefab trực tiếp
            // Giả định bạn có AddressableKey.Prefabs.Player, nếu chưa có thì thay string "Player"
            Debug.Log("[HeroManager] Bắt đầu load Hero Prefab...");
            
            // Note: Chúng ta đang sử dụng "Player" như một raw string address. Trong thực tế bạn gọi AddressableKeys.Prefabs.Player.
            _spawnedHero = await _assetProvider.InstantiateAsync(_playerReference, transform, false);
            
            if (_spawnedHero != null)
            {
                Debug.Log("[HeroManager] Tải Hero thành công!");
            }

            // 2. Load riêng rẽ một Asset thô (Ví dụ: Icon)
            Debug.Log("[HeroManager] Bắt đầu load Hero Icon...");
            _heroIcon = await _assetProvider.LoadAssetAsync<Sprite>("HeroIcon");
        }

        private void OnDestroy()
        {
            // Tự động giải phóng khi Scene bị unload hoặc Object bị hủy
            if (_assetProvider == null) return;
            
            Debug.Log("[HeroManager] Dọn dẹp tài nguyên...");

            if (_spawnedHero != null)
            {
                // Gọi hàm release Instance
                _assetProvider.Unload(_spawnedHero);
            }

            if (_heroIcon != null)
            {
                // Phát hành Handle của asset raw dựa theo key
                _assetProvider.Unload("HeroIcon");
            }
        }
    }
}
