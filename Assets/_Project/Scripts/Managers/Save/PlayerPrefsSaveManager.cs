using UnityEngine;

namespace Suhdo.Managers.Save
{
    /// <summary>
    /// Implementation cơ bản của ISaveManager sử dụng PlayerPrefs.
    /// Dễ dàng thay thế bằng JSONSaveManager hoặc CloudSaveManager sau này qua DI.
    /// </summary>
    public class PlayerPrefsSaveManager : ISaveManager
    {
        public void SaveInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public int LoadInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);

        public void SaveFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
        public float LoadFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);

        public void SaveString(string key, string value) => PlayerPrefs.SetString(key, value);
        public string LoadString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);

        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public void DeleteAll() => PlayerPrefs.DeleteAll();
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public void Save() => PlayerPrefs.Save();
    }
}
