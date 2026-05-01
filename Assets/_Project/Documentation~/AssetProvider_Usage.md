# Hướng dẫn sử dụng hệ thống IAssetProvider

Hệ thống `IAssetProvider` là cầu nối chuẩn hóa (Interface) kết hợp sức mạnh của **Addressables**, cấu trúc tiêm phụ thuộc (Dependency Injection) của **Reflex**, và xử lý bất đồng bộ bằng **UniTask**. Cơ chế này cho phép các lập trình viên dễ dàng gọi asset (hình ảnh, âm thanh, dữ liệu) hoặc instantiate Prefab mà không cần tự xử lý quy trình Unload (Giải phóng bộ nhớ) rườm rà của Addressables.

## Cách hoạt động của bộ quản lý bộ nhớ

Mỗi một lần gọi `LoadAssetAsync` hoặc `InstantiateAsync`, lớp thực thi `AddressableAssetProvider` sẽ:
1. Yêu cầu tải nội dung qua Server / Local Disk.
2. Lưu trữ Handle (Bản tham chiếu Reference Count) vào Dictionary đệm (`_handles`, `_instanceHandles`).
3. Nếu bạn gọi hàm Load 2 lần cho cùng một resource, Provider sẽ tự động khóa và trả về UniTask đang được tải hiện tại (tránh gọi Request cấp phát 2 cái).
4. Bạn có trách nhiệm gọi `.Unload()` ở những Component cần hủy để giảm Reference Count. Khi Reference Count = 0, bộ nhớ (RAM/VRAM) sẽ được thu hồi.

> [!WARNING]
> Mặc dù Provider đã track handle, bạn vẫn **BẮT BUỘC** gọi phương thức `Unload()` tại hàm `OnDestroy` hoặc khi ngắt State để hoàn thành vòng đời giải phóng Memory.

## Cách Inject ở bất cứ đâu

Mọi đối tượng MonoBehavior hay C# class tiêu chuẩn đều có thể gọi ra hệ thống này, miễn là container của Reflex quản lý đối tượng đó.

### Sử dụng Inject Attribute (Bắt buộc với MonoBehaviour)
```csharp
using Reflex.Attributes;
using Suhdo.Core;
using UnityEngine;

public class MyComponent : MonoBehaviour
{
    [Inject] private readonly IAssetProvider _assetProvider;

    private async void Start()
    {
        // 1. Tải một Resource đơn lẻ kiểu Texture
        Texture2D myTex = await _assetProvider.LoadAssetAsync<Texture2D>("BossTexture");

        // 2. Tải và tạo Instance trên Hierarchy
        GameObject playerEntity = await _assetProvider.InstantiateAsync("PlayerCharacter");
    }
}
```

### Sử dụng Constructor Injection (Bắt buộc với Class thuần C#)
```csharp
public class FeatureController
{
    private readonly IAssetProvider _assetProvider;

    public FeatureController(IAssetProvider assetProvider)
    {
        _assetProvider = assetProvider;
    }
}
```

## Các phương pháp Giải phóng Tài nguyên

Bạn có 2 cách dựa vào định dạng asset: Tải theo file cấu hình (`Unload(string)`) hoặc Giải phóng dựa vào thực thể GameObject/Component đã được instantiate (`Unload<T>(T asset)`).

```csharp
// Đối với LoadAssetAsync(string)
_assetProvider.Unload("BossTexture");

// Đối với InstantiateAsync(...)
_assetProvider.Unload(playerEntity);
```

> [!TIP]  
> Nếu bạn thay class game sang cảnh (Scene) khác, class `AddressableAssetProvider` sẽ được thu hồi. Kỹ thuật `Dispose()` của class này sẽ tự động gọi `UnloadAll()` và quét dọn 100% Asset Handle đang trỏ lên nó. Bạn có thể không cần thiết phải check null các handle bị trùng nữa!
