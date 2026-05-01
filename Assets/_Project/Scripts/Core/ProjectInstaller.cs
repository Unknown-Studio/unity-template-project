using Reflex.Core;
using Reflex.Enums;
using UnityEngine;
using Suhdo.Features.Weapons;
using Suhdo.Managers;
using Suhdo.Managers.Save;
using Suhdo.Managers.Input;
using Resolution = Reflex.Enums.Resolution;

namespace Suhdo.Core
{
    /// <summary>
    /// Installer chạy ở cấp độ toàn dự án (Project Context).
    /// Các service ở đây sẽ tồn tại xuyên suốt mọi scene.
    /// Cập nhật: Tương thích Reflex 14.3.0
    /// </summary>
    public class ProjectInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            // --- Đăng ký Save Manager ---
            // Singleton: Chỉ tạo 1 instance duy nhất
            // Eager: Khởi tạo ngay lập tức khi Container được build
            builder.RegisterType(typeof(PlayerPrefsSaveManager), new[] { typeof(ISaveManager) }, Lifetime.Singleton, Resolution.Eager);
            
            // --- Đăng ký Input Service ---
            builder.RegisterType(typeof(MobileInputService), new[] { typeof(IInputService) }, Lifetime.Singleton, Resolution.Eager);

            // --- Đăng ký các Manager toàn cục ---
            builder.RegisterType(typeof(GameManager), Lifetime.Singleton, Resolution.Eager);
            builder.RegisterType(typeof(CurrencyManager), Lifetime.Singleton, Resolution.Eager);
            
            // --- Đăng ký Asset Provider ---
            // AddressableAssetProvider quản lý tài nguyên nên khởi tạo Lazy (khi cần mới tạo) để tối ưu Startup
            builder.RegisterType(typeof(AddressableAssetProvider), new[] { typeof(IAssetProvider) }, Lifetime.Singleton, Resolution.Lazy);

            // --- Đăng ký Weapon Service ---
            // Lazy: chỉ tạo khi Player scene load xong và Construct() được gọi
            builder.RegisterType(typeof(WeaponService), new[] { typeof(IWeaponService) }, Lifetime.Singleton, Resolution.Lazy);
            
            Debug.Log("<color=#5EE05E>[ProjectInstaller]</color> Reflex 14.3.0 services registered successfully.");
        }
    }
}
