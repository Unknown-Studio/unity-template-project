using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Suhdo.Core
{
    /// <summary>
    /// Contract defines the interface to load, instantiate and unload resources.
    /// It helps decouple concrete implementations like Addressables from other game logic.
    /// </summary>
    public interface IAssetProvider
    {
        /// <summary>
        /// Loads an asset of type T without instantiating it.
        /// </summary>
        UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object;
        UniTask<T> LoadAssetAsync<T>(AssetReference reference) where T : UnityEngine.Object;

        /// <summary>
        /// Instantiates a prefab as a GameObject component.
        /// </summary>
        UniTask<T> InstantiateAsync<T>(string address, Transform parent = null, bool instantiateInWorldSpace = false) where T : Component;
        UniTask<T> InstantiateAsync<T>(AssetReference reference, Transform parent = null, bool instantiateInWorldSpace = false) where T : Component;

        /// <summary>
        /// Instantiates a GameObject directly.
        /// </summary>
        UniTask<GameObject> InstantiateAsync(string address, Transform parent = null, bool instantiateInWorldSpace = false);
        UniTask<GameObject> InstantiateAsync(AssetReference reference, Transform parent = null, bool instantiateInWorldSpace = false);

        /// <summary>
        /// Loads a Scene asynchronously via Addressables.
        /// </summary>
        UniTask<SceneInstance> LoadSceneAsync(string address, LoadSceneMode loadMode = LoadSceneMode.Single);
        UniTask<SceneInstance> LoadSceneAsync(AssetReference reference, LoadSceneMode loadMode = LoadSceneMode.Single);

        /// <summary>
        /// Unloads a loaded Scene instance.
        /// </summary>
        UniTask UnloadSceneAsync(SceneInstance scene);

        /// <summary>
        /// Unloads resources tied to a specific Addressable key.
        /// </summary>
        void Unload(string address);
        void Unload(AssetReference reference);

        /// <summary>
        /// Attempts to release the reference to a specific Loaded Asset or Instance.
        /// </summary>
        void Unload<T>(T asset) where T : UnityEngine.Object;

        /// <summary>
        /// Unloads all assets that were loaded by this provider.
        /// </summary>
        void UnloadAll();

        #region Updater & Dowloader

        /// <summary>
        /// Checks if there are any Addressable Catalog updates available from the remote server.
        /// Updates the catalog if new versions are found.
        /// </summary>
        /// <returns>True if catalogs were updated.</returns>
        UniTask<bool> CheckForCatalogUpdatesAsync();

        /// <summary>
        /// Gets the total download size (in bytes) needed for a given key or label.
        /// </summary>
        UniTask<long> GetDownloadSizeAsync(string key);

        /// <summary>
        /// Gets the total download size (in bytes) needed for a list of keys or labels.
        /// </summary>
        UniTask<long> GetDownloadSizeAsync(System.Collections.Generic.IEnumerable<string> keys);

        /// <summary>
        /// Downloads the dependencies for the given key or label. Provides progress via the callback.
        /// </summary>
        UniTask DownloadDependenciesAsync(string key, System.Action<float> onProgress = null);

        /// <summary>
        /// Downloads the dependencies for a list of keys or labels. Provides progress via the callback.
        /// </summary>
        UniTask DownloadDependenciesAsync(System.Collections.Generic.IEnumerable<string> keys, System.Action<float> onProgress = null);

        #endregion
    }
}
