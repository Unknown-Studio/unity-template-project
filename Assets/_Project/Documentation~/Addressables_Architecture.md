# Unity Addressables — Kiến trúc Remote Hosting cho TemplateProject

> **Bối cảnh**: Unity 6 (`6000.3.12f1`) · Addressables `2.9.1` · Dự án Hybrid-Casual
> **Mục tiêu**: Giảm kích thước build ban đầu, tải nội dung theo nhu cầu, dễ cập nhật content mà không cần re-submit store.

---

## 1. Cấu trúc Thư mục (Folder Structure)

### 1.1 Nguyên tắc cốt lõi

| Nguyên tắc | Mô tả |
|---|---|
| **Tách biệt rõ ràng** | Folder `_Local` chứa asset đóng gói sẵn trong build, folder `_Remote` chứa asset tải về sau |
| **Không trộn lẫn** | Một asset chỉ nằm ở **một nơi duy nhất**. Nếu di chuyển từ Local → Remote, phải move file |
| **Mirror structure** | `_Remote` phản chiếu cấu trúc con của `_Local` để dễ tìm kiếm |

### 1.2 Cấu trúc thư mục

```
Assets/
├── _Project/
│   ├── _Local/                          ← 📦 Đóng gói SẴN trong build
│   │   ├── Art/
│   │   │   ├── Materials/               ← Material cơ bản (UI chính, player)
│   │   │   ├── Sprites/                 ← Icon hệ thống, UI core
│   │   │   └── Textures/                ← Texture thiết yếu
│   │   ├── Audio/
│   │   │   ├── Music/                   ← Nhạc nền menu chính
│   │   │   └── SFX/                     ← Âm thanh UI cơ bản (click, popup)
│   │   ├── Prefabs/
│   │   │   ├── Characters/              ← Player prefab chính
│   │   │   ├── UI/                      ← UI Canvas, Popup cốt lõi
│   │   │   └── Core/                    ← Game systems prefab
│   │   └── Scenes/
│   │       ├── Boot.unity               ← Scene khởi tạo
│   │       └── MainMenu.unity           ← Menu chính
│   │
│   ├── _Remote/                         ← 🌐 Tải về SAU từ CDN
│   │   ├── Art/
│   │   │   ├── Materials/               ← Material theo theme/season
│   │   │   ├── Models/                  ← 3D models (environment, items)
│   │   │   ├── Sprites/                 ← Sprite sheet cho content mới
│   │   │   └── Textures/               ← Texture HD, environment
│   │   ├── Audio/
│   │   │   ├── Music/                   ← Nhạc in-game, event
│   │   │   └── SFX/                     ← SFX gameplay
│   │   ├── Prefabs/
│   │   │   ├── Characters/              ← NPC, Enemy, Skin mới
│   │   │   ├── Environment/             ← Props, obstacles
│   │   │   ├── Items/                   ← Collectibles, power-ups
│   │   │   └── UI/                      ← UI cho feature mới
│   │   └── Scenes/
│   │       ├── Levels/                  ← Level_001.unity ... Level_NNN.unity
│   │       └── Events/                  ← Event_Halloween.unity ...
│   │
│   ├── Scripts/                         ← ⚙️ KHÔNG addressable (luôn trong build)
│   │   ├── Core/
│   │   ├── Data/
│   │   ├── Events/
│   │   ├── Features/
│   │   ├── Managers/
│   │   ├── UI/
│   │   └── Utils/
│   │
│   ├── SO_Data/                         ← ScriptableObject data
│   │   ├── _Local/                      ← Config cốt lõi (GameSettings, etc.)
│   │   └── _Remote/                     ← Data content (LevelData, ShopItems)
│   │
│   └── Settings/                        ← Project settings (không addressable)
│
├── AddressableAssetsData/               ← ⚠️ Auto-generated bởi Addressables
│   ├── AssetGroups/
│   └── AssetGroupTemplates/
│
└── [Third-party folders giữ nguyên]
```

### 1.3 Quy tắc phân loại Local vs Remote

| Đưa vào **Local** | Đưa vào **Remote** |
|---|---|
| Asset cần ngay khi mở app (splash, loading screen) | Content gameplay (levels, enemies, items) |
| UI hệ thống (popup lỗi, dialog xác nhận) | Skin, cosmetic, seasonal content |
| Player prefab cơ bản | Asset theo event/campaign |
| Audio UI feedback (click, swipe) | Nhạc nền, SFX gameplay |
| Scene Boot + MainMenu | Scene Level, Scene Event |
| ScriptableObject config hệ thống | ScriptableObject data content |

> **⚠️ QUAN TRỌNG:** Scripts (`*.cs`) KHÔNG BAO GIỜ là Addressable. Chúng luôn được compile vào build. Chỉ asset (prefab, texture, audio, scene, SO) mới đánh dấu Addressable.

---

## 2. Phân nhóm Addressables Groups

### 2.1 Chiến lược: **Hybrid** (Feature-first + Asset-type sub-groups)

```
📁 Addressables Groups
│
├── 🟢 [Local] Core Assets            ← Build With Player
│   ├── Boot scene, MainMenu scene
│   ├── Player prefab, UI Canvas
│   └── Core audio (UI SFX)
│
├── 🟢 [Local] Core UI                ← Build With Player
│   ├── Popup prefabs cốt lõi
│   └── Icon hệ thống, fonts
│
├── 🔵 [Remote] Gameplay - Characters  ← Download on demand
│   ├── Enemy prefabs
│   ├── NPC prefabs
│   └── Character sprites/textures
│
├── 🔵 [Remote] Gameplay - Environment ← Download on demand
│   ├── Props, obstacles
│   ├── Environment textures
│   └── Environment materials
│
├── 🔵 [Remote] Gameplay - Items       ← Download on demand
│   ├── Collectible prefabs
│   ├── Power-up prefabs
│   └── Item icons
│
├── 🔵 [Remote] Levels - Pack01        ← Download per pack
│   ├── Level_001 ~ Level_010 scenes
│   └── Level-specific assets
│
├── 🔵 [Remote] Levels - Pack02        ← Download per pack
│   ├── Level_011 ~ Level_020 scenes
│   └── Level-specific assets
│
├── 🔵 [Remote] Audio - Music          ← Download khi cần
│   └── Background music tracks
│
├── 🔵 [Remote] Audio - SFX            ← Download khi cần
│   └── Gameplay sound effects
│
├── 🟡 [Remote] LiveOps - Shop         ← Update thường xuyên
│   ├── Shop item data (SO)
│   └── Shop UI prefabs
│
└── 🟡 [Remote] LiveOps - Events       ← Update theo mùa
    ├── Event scenes
    ├── Event prefabs
    └── Event assets
```

### 2.2 Cấu hình cho từng Group

#### Local Groups

| Setting | Giá trị | Lý do |
|---|---|---|
| Build & Load Path | `LocalBuildPath` / `LocalLoadPath` | Đóng gói vào `StreamingAssets` |
| Bundle Mode | `Pack Together` | Ít bundle = ít overhead I/O |
| Bundle Naming | `Filename` | Đơn giản, không cần hash |
| Content Update Restriction | `Cannot Change Post Release` | Không cho phép thay đổi sau build |

#### Remote Groups

| Setting | Giá trị | Lý do |
|---|---|---|
| Build & Load Path | `RemoteBuildPath` / `RemoteLoadPath` | Build ra folder riêng, load từ CDN |
| Bundle Mode | `Pack Together` hoặc `Pack Separately` | Tùy kích thước group |
| Bundle Naming | `Append Hash to Filename` | **BẮT BUỘC** — CDN cache invalidation |
| Content Update Restriction | `Can Change Post Release` | Cho phép OTA update |

---

## 3. Profile Configuration

### 3.1 Ba Profiles

| Profile | Remote Load Path | Mục đích |
|---|---|---|
| **Editor** | `http://localhost:8080/[BuildTarget]` | Test local |
| **Staging** | `https://cdn-staging.yourgame.com/addressables/[BuildTarget]` | QA testing |
| **Production** | `https://cdn.yourgame.com/addressables/v[AppVersion]/[BuildTarget]` | Release |

### 3.2 Lưu ý quan trọng

- **LUÔN** dùng `[BuildTarget]` trong path — Android bundle ≠ iOS bundle
- **LUÔN** bật `Build Remote Catalog = true` — để app kiểm tra content update
- Khi dev trong Editor: dùng **"Use Asset Database (fastest)"** trong Play Mode Script để skip build bundle
- Switch profile trong CI/CD bằng code:

```csharp
AddressableAssetSettings.ActiveProfileId =
    settings.profileSettings.GetProfileId("Production");
```

---

## 4. Naming Convention cho Addressable Keys

### 4.1 Format

```
{category}/{sub-category}/{asset_name}
```

Lowercase, phân tách bằng `/`, dùng `_` cho tên nhiều từ.

### 4.2 Ví dụ

| Loại Asset | Pattern | Ví dụ |
|---|---|---|
| **Prefab** | `prefab/{feature}/{name}` | `prefab/character/player_warrior` |
| **Scene** | `scene/{category}/{name}` | `scene/level/level_001` |
| **Audio** | `audio/{type}/{name}` | `audio/music/theme_main_menu` |
| **Sprite** | `sprite/{feature}/{name}` | `sprite/ui/icon_gold_coin` |
| **Material** | `material/{feature}/{name}` | `material/character/skin_default` |
| **ScriptableObject** | `data/{feature}/{name}` | `data/shop/item_sword_fire` |
| **UI Prefab** | `ui/{screen}/{name}` | `ui/shop/panel_item_detail` |

---

## 5. Content Update Workflow

```
1. Build lần đầu (Full Build) → Upload bundles + catalog lên CDN
2. Khi cần update content:
   a. Addressables → Tools → Check for Content Update Restrictions
   b. Addressables → Build → Update a Previous Build
   c. Upload CHỈ các file thay đổi lên CDN
3. App tự tải catalog mới → so sánh hash → tải bundle mới
```

> **⛔ CẢNH BÁO:** KHÔNG BAO GIỜ xóa bundle cũ trên CDN ngay lập tức. User đang chơi vẫn cần file cũ. Giữ ít nhất 2-3 version trước khi cleanup.

---

## 6. Memory Management

```csharp
// ✅ ĐÚNG — Release khi không cần
Addressables.Release(handle);

// ✅ ĐÚNG — Unload scene khi chuyển level
Addressables.UnloadSceneAsync(sceneHandle);

// ❌ SAI — Destroy KHÔNG release bundle
// Destroy(obj);

// ✅ ĐÚNG — Dùng InstantiateAsync + ReleaseInstance
var handle = Addressables.InstantiateAsync(key);
Addressables.ReleaseInstance(handle.Result);
```

**Rule of thumb**: Mỗi `LoadAssetAsync` / `InstantiateAsync` phải có **đúng 1** `Release` / `ReleaseInstance` tương ứng.
