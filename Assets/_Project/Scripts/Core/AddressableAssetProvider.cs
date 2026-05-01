using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Suhdo.Core
{
    /// <summary>
    /// Implementation of IAssetProvider using Unity Addressables.
    /// Handles memory management securely by keeping track of handles.
    /// </summary>
    public class AddressableAssetProvider : IAssetProvider, IDisposable
    {
        // Dictionary to track Handle operations by key to prevent duplicate simultaneous loads and to Unload by key
        private readonly Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

        // Dictionary to track Scene handles by key
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _sceneHandles = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();

        // Dictionary to track Instance handles by GameObject
        // Required for Addressables.ReleaseInstance
        private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles = new Dictionary<GameObject, AsyncOperationHandle<GameObject>>();

        #region LoadAssetAsync
        
        public async UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            if (_handles.TryGetValue(address, out var existingHandle))
            {
                if (existingHandle.IsDone)
                    return existingHandle.Result as T;
                
                // Allow awaiting the existing ongoing load operation
                return await existingHandle.Convert<T>().ToUniTask();
            }

            var handle = Addressables.LoadAssetAsync<T>(address);
            _handles[address] = handle;

            var result = await handle.ToUniTask();
            return result;
        }
        
        public UniTask<T> LoadAssetAsync<T>(AssetReference reference) where T : UnityEngine.Object
        {
            return LoadAssetAsync<T>(reference.RuntimeKey.ToString());
        }
        
        #endregion

        #region InstantiateAsync
        
        public async UniTask<T> InstantiateAsync<T>(string address, Transform parent = null, bool instantiateInWorldSpace = false) where T : Component
        {
            var go = await InstantiateAsync(address, parent, instantiateInWorldSpace);
            if (go == null) return null;

            var comp = go.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"[AddressableAssetProvider] Component {typeof(T).Name} not found on instantiated prefab from {address}.");
            }
            return comp;
        }

        public UniTask<T> InstantiateAsync<T>(AssetReference reference, Transform parent = null, bool instantiateInWorldSpace = false) where T : Component
        {
            return InstantiateAsync<T>(reference.RuntimeKey.ToString(), parent, instantiateInWorldSpace);
        }

        public async UniTask<GameObject> InstantiateAsync(string address, Transform parent = null, bool instantiateInWorldSpace = false)
        {
            var handle = Addressables.InstantiateAsync(address, parent, instantiateInWorldSpace);
            var go = await handle.ToUniTask();
            
            if (go != null)
            {
                _instanceHandles[go] = handle;
            }
            else
            {
                Debug.LogError($"[AddressableAssetProvider] Failed to instantiate {address}.");
            }

            return go;
        }

        public UniTask<GameObject> InstantiateAsync(AssetReference reference, Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return InstantiateAsync(reference.RuntimeKey.ToString(), parent, instantiateInWorldSpace);
        }

        #endregion

        #region Scene Loading
        
        public async UniTask<SceneInstance> LoadSceneAsync(string address, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if (_sceneHandles.TryGetValue(address, out var existingHandle))
            {
                if (existingHandle.IsDone)
                    return existingHandle.Result;
                
                return await existingHandle.ToUniTask();
            }

            var handle = Addressables.LoadSceneAsync(address, loadMode);
            _sceneHandles[address] = handle;

            var result = await handle.ToUniTask();
            return result;
        }

        public UniTask<SceneInstance> LoadSceneAsync(AssetReference reference, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            return LoadSceneAsync(reference.RuntimeKey.ToString(), loadMode);
        }

        public async UniTask UnloadSceneAsync(SceneInstance scene)
        {
            AsyncOperationHandle<SceneInstance> handleToRelease = default;
            string keyToRemove = null;

            foreach (var kvp in _sceneHandles)
            {
                if (kvp.Value.IsValid() && kvp.Value.Result.Equals(scene))
                {
                    handleToRelease = kvp.Value;
                    keyToRemove = kvp.Key;
                    break;
                }
            }

            if (handleToRelease.IsValid())
            {
                await Addressables.UnloadSceneAsync(handleToRelease).ToUniTask();
                _sceneHandles.Remove(keyToRemove);
            }
            else
            {
                Debug.LogWarning($"[AddressableAssetProvider] Could not find tracked handle for Scene {scene.Scene.name} to unload.");
            }
        }

        #endregion
        
        #region Unloading Memory
        
        // Unloads by Address string (Good for releasing LoadAssetAsync)
        public void Unload(string address)
        {
            if (_handles.TryGetValue(address, out var handle))
            {
                Addressables.Release(handle);
                _handles.Remove(address);
            }
            else
            {
                Debug.LogWarning($"[AddressableAssetProvider] Could not find handle for key {address} to unload.");
            }
        }

        public void Unload(AssetReference reference)
        {
            Unload(reference.RuntimeKey.ToString());
        }

        // Object-based release logic. Correctly branches for GameObject Instances vs LoadAsset
        public void Unload<T>(T asset) where T : UnityEngine.Object
        {
            if (asset == null) return;

            if (asset is GameObject go && _instanceHandles.TryGetValue(go, out var instanceHandle))
            {
                Addressables.ReleaseInstance(instanceHandle);
                _instanceHandles.Remove(go);
                return;
            }

            if (asset is Component comp && _instanceHandles.TryGetValue(comp.gameObject, out var compInstanceHandle))
            {
                Addressables.ReleaseInstance(compInstanceHandle);
                _instanceHandles.Remove(comp.gameObject);
                return;
            }

            // Fallback: If it's a raw asset (e.g., Sprite, AudioClip, ScriptableObject), Addressables supports `.Release(Object)` directly.
            // Note: Releasing raw assets directly via Addressables.Release does not clear `_handles` dictionary tracking.
            // This is primarily for one-off loaded resources. Prefer `Unload(string address)`.
            Addressables.Release(asset);
        }

        public void UnloadAll()
        {
            // Release loaded assets
            foreach (var kvp in _handles)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }
            _handles.Clear();

            // Release Instantiated GameObjects
            foreach (var kvp in _instanceHandles)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.ReleaseInstance(kvp.Value);
                }
            }
            _instanceHandles.Clear();

            // Release scenes (unloads the scene as well if valid)
            foreach (var kvp in _sceneHandles)
            {
                if (kvp.Value.IsValid())
                {
                    // For synchronous cleanup on quit/dispose, we might just release handle.
                    // Doing a non-async UnloadScene isn't fully supported via handle release alone sometimes, but usually Addressables clears it on destroy.
                    Addressables.Release(kvp.Value); 
                }
            }
            _sceneHandles.Clear();
        }

        public void Dispose()
        {
            UnloadAll();
        }
        
        #endregion

        #region Updater & Dowloader
        
        public async UniTask<bool> CheckForCatalogUpdatesAsync()
        {
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            var catalogsToUpdate = await checkHandle.ToUniTask();

            if (catalogsToUpdate != null && catalogsToUpdate.Count > 0)
            {
                var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);
                await updateHandle.ToUniTask();
                Addressables.Release(updateHandle);
                Addressables.Release(checkHandle);
                return true;
            }

            Addressables.Release(checkHandle);
            return false;
        }

        public async UniTask<long> GetDownloadSizeAsync(string key)
        {
            var handle = Addressables.GetDownloadSizeAsync(key);
            long size = await handle.ToUniTask();
            Addressables.Release(handle);
            return size;
        }

        public async UniTask<long> GetDownloadSizeAsync(IEnumerable<string> keys)
        {
            var handle = Addressables.GetDownloadSizeAsync(keys);
            long size = await handle.ToUniTask();
            Addressables.Release(handle);
            return size;
        }

        public async UniTask DownloadDependenciesAsync(string key, Action<float> onProgress = null)
        {
            var handle = Addressables.DownloadDependenciesAsync(key, false);
            
            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.PercentComplete);
                await UniTask.Yield();
            }

            onProgress?.Invoke(1f);
            
            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"[AddressableAssetProvider] Failed to download dependencies for key: {key}");
            }

            Addressables.Release(handle);
        }

        public async UniTask DownloadDependenciesAsync(IEnumerable<string> keys, Action<float> onProgress = null)
        {
            var handle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union, false);

            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.PercentComplete);
                await UniTask.Yield();
            }

            onProgress?.Invoke(1f);

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("[AddressableAssetProvider] Failed to download dependencies for multiple keys.");
            }

            Addressables.Release(handle);
        }

        #endregion
    }
}
