using Suhdo.Managers.Save;
using UnityEngine;

namespace Suhdo.Managers
{
    /// <summary>
    /// Quản lý tiền tệ trong game. 
    /// Có thể dùng UnityAtoms IntVariable để bind UI, nhưng class này xử lý logic nghiệp vụ.
    /// </summary>
    public class CurrencyManager
    {
        private readonly ISaveManager _saveManager;
        private const string KEY_GOLD = "CURRENCY_GOLD";
        private const string KEY_GEMS = "CURRENCY_GEMS";

        public int Gold { get; private set; }
        public int Gems { get; private set; }

        public CurrencyManager(ISaveManager saveManager)
        {
            _saveManager = saveManager;
            LoadData();
            Debug.Log($"[CurrencyManager] Initialized. Gold: {Gold}, Gems: {Gems}");
        }

        private void LoadData()
        {
            Gold = _saveManager.LoadInt(KEY_GOLD, 0);
            Gems = _saveManager.LoadInt(KEY_GEMS, 0);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            _saveManager.SaveInt(KEY_GOLD, Gold);
            // Publish UnityAtoms Event cập nhật UI nếu cần
        }

        public bool ConsumeGold(int amount)
        {
            if (Gold < amount) return false;
            
            Gold -= amount;
            _saveManager.SaveInt(KEY_GOLD, Gold);
            return true;
        }
        
        // Tương tự cho Gems...
    }
}
