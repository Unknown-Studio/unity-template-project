using UnityEngine;
using Cysharp.Threading.Tasks;
using Suhdo.Core;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors; // Thêm namespace này để dùng GameObjectInjector

namespace Suhdo.Samples
{
    /// <summary>
    /// Sample component. Đính kèm script này vào một GameObject trống trong Scene.
    /// Script sẽ hiện giao diện nút bấm (chạy ở Play Mode) để bạn test thử mọi API của Asset Provider.
    /// </summary>
    public class AddressablesSampleTest : MonoBehaviour
    {
        // Nhờ ProjectInstaller, thuộc tính này sẽ tự động được gán
        [Inject] private readonly IAssetProvider _assetProvider;
        [Inject] private readonly Container _container;

        [Header("Test Configuration")]
        [Tooltip("Nhập Addressable Key hoặc Label muốn test")]
        public string testAddressableKey = "Player"; 

        private GameObject _spawnedInstance;
        private string _logOutput = "Sẵn sàng (Play Mode)\n";

        private void OnGUI()
        {
            // Bật cửa sổ test rộng rãi
            GUILayout.BeginArea(new Rect(20, 300, 1080, 1920));
            
            GUILayout.Box("🕹 Màn hình Test Addressables OTA (On-Demand)", GUILayout.Height(30));

            GUILayout.Label("Khóa Asset / Label (Key):", GUILayout.Width(200));
            testAddressableKey = GUILayout.TextField(testAddressableKey, GUILayout.Height(60));

            GUILayout.Space(10);

            if (GUILayout.Button("🔄 1. Kiểm tra Catalog Updates", GUILayout.Height(80)))
            {
                TestCheckUpdates().Forget();
            }

            if (GUILayout.Button($"📏 2. Kiểm tra Dung lượng cần tải: [{testAddressableKey}]", GUILayout.Height(80)))
            {
                TestGetSize().Forget();
            }

            if (GUILayout.Button($"📥 3. Tải Asset: [{testAddressableKey}]", GUILayout.Height(80)))
            {
                TestDownload().Forget();
            }

            if (GUILayout.Button($"🚀 4. Instantiate (Sinh ra Object)", GUILayout.Height(80)))
            {
                TestInstantiate().Forget();
            }

            if (GUILayout.Button($"🗑 5. Unload (Hủy Bộ Nhớ)", GUILayout.Height(80)))
            {
                TestUnload();
            }

            GUILayout.Space(20);
            GUILayout.Label("📝 Bảng Log Runtime:", GUILayout.Height(40));
            
            // Text area log
            var style = new GUIStyle(GUI.skin.textArea);
            style.wordWrap = true;
            GUILayout.TextArea(_logOutput, style, GUILayout.Height(600));

            GUILayout.EndArea();
        }

        private async UniTaskVoid TestCheckUpdates()
        {
            Log("Đang kết nối để kiểm tra bản đồ Catalog mới...");
            bool hasUpdate = await _assetProvider.CheckForCatalogUpdatesAsync();
            Log(hasUpdate ? "✅ Catalog Đã Cập Nhật! (Phát hiện map tài nguyên mới)" : "⭕ Catalog của bạn đang là mới nhất.");
        }

        private async UniTaskVoid TestGetSize()
        {
            Log($"Đang quét tìm file của [{testAddressableKey}]...");
            long size = await _assetProvider.GetDownloadSizeAsync(testAddressableKey);
            
            if (size > 0)
                Log($"📦 Bạn cần tải: {size / 1048576f:F2} MB ({size} bytes).");
            else
                Log($"✅ Máy bạn đã có sẵn {testAddressableKey} (Dung lượng tải: 0 Byte).");
        }

        private async UniTaskVoid TestDownload()
        {
            Log($"[Start] Tiến hành tải [{testAddressableKey}]...");
            
            await _assetProvider.DownloadDependenciesAsync(testAddressableKey, progress => 
            {
                // Bạn có thể liên kết progress này vào Thanh Slider Loading của Game
                Log($"Đang tải... {progress * 100:F1}%", replaceLastLine: true);
            });
            
            Log($"🎯 [Done] Tải {testAddressableKey} hoàn tất!");
        }

        private async UniTaskVoid TestInstantiate()
        {
            if (_spawnedInstance != null)
            {
                Log("⚠ Đã tạo nhân vật rồi. Xin hãy Unload trước!");
                return;
            }

            if (_assetProvider == null)
            {
                Log("⚠ Lỗi: Reflex Inject IAssetProvider thất bại (Có chắc bạn đã setup ProjectInstaller?)");
                return;
            }

            Log($"Đang gọi Asset [{testAddressableKey}] trồi lên...");
            _spawnedInstance = await _assetProvider.InstantiateAsync(testAddressableKey, transform);
            
            if (_spawnedInstance != null)
            {
                Log($"✅ Đã tạo thành công: {_spawnedInstance.name}");
                
                // Mẹo nhỏ: Dịch object di chuyển một chút để bạn dễ nhìn thấy
                _spawnedInstance.transform.position = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                
                // --- ĐÂY LÀ CÁCH INJECT VÀO PREFAB RUNTIME (Reflex 14.3.0+) ---
                // Vì Prefab được sinh ra bởi Addressables, ta cần gọi Inject thủ công.
                // GameObjectInjector sẽ quét đệ quy các component bên trong để tiêm phụ thuộc.
                GameObjectInjector.InjectRecursive(_spawnedInstance, _container);
                
                Log("💉 Đã thực hiện Inject các Service vào Prefab thành công (Reflex 14.3.0).");
            }
            else
            {
                Log($"❌ Thất bại: Không tìm thấy Key '{testAddressableKey}' trong Server/Addressables.");
            }
        }

        private void TestUnload()
        {
            if (_spawnedInstance != null)
            {
                Log($"Đang thu hồi {_spawnedInstance.name}...");
                
                // Truyền trực tiếp GameObject vào hàm Unload (Siêu tiện lợi!)
                _assetProvider.Unload(_spawnedInstance);
                _spawnedInstance = null;
                
                Log("🧹 Đã gọi Garbage Collection cho Asset thành công.");
            }
            else
            {
                Log("🤷‍♂️ Không có gì trên Scene để xóa cả.");
            }
        }

        // --- Utils for Text Logging ---
        private void Log(string message, bool replaceLastLine = false)
        {
            Debug.Log($"[AddressablesSample] {message}");
            
            if (replaceLastLine && _logOutput.Contains("\n"))
            {
                int lastNewline = _logOutput.IndexOf('\n');
                _logOutput = $"{message}\n" + _logOutput.Substring(lastNewline + 1);
            }
            else
            {
                _logOutput = $"{message}\n" + _logOutput;   
            }
            
            // Giữ lại log ngắn cho dễ nhìn màn hình
            if (_logOutput.Length > 2000)
                _logOutput = _logOutput.Substring(0, 2000);
        }
    }
}
