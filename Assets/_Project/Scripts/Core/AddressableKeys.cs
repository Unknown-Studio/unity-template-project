namespace Suhdo.Core
{
    /// <summary>
    /// Centralized Addressable asset keys.
    /// Trống hardcode string rải rác trong codebase.
    /// </summary>
    public static class AddressableKeys
    {
        // ── Scenes ──────────────────────────────────────
        public static class Scenes
        {
            public const string Boot       = "scene/core/boot";
            public const string MainMenu   = "scene/core/main_menu";
            public const string Level      = "scene/level/level_{0:D3}"; // Format: level_001
        }

        // ── Prefabs ─────────────────────────────────────
        public static class Prefabs
        {
            public const string Player     = "prefab/character/player";
            public const string Enemy      = "prefab/character/enemy_{0}";  // enemy_slime
        }

        // ── UI ──────────────────────────────────────────
        public static class UI
        {
            public const string ShopPanel  = "ui/shop/panel_shop";
            public const string Popup      = "ui/popup/popup_{0}";  // popup_confirm
        }

        // ── Audio ───────────────────────────────────────
        public static class Audio
        {
            public const string MusicMenu  = "audio/music/theme_main_menu";
            public const string SfxClick   = "audio/sfx/ui_click";
        }

        // ── Data ────────────────────────────────────────
        public static class Data
        {
            public const string LevelData  = "data/level/level_data_{0:D3}";
            public const string ShopItem   = "data/shop/item_{0}";
        }

        // ── Helper ──────────────────────────────────────
        /// <summary>
        /// Format a key template with arguments.
        /// Usage: AddressableKeys.Format(AddressableKeys.Scenes.Level, 5) -> "scene/level/level_005"
        /// </summary>
        public static string Format(string template, params object[] args)
            => string.Format(template, args);
    }
}
