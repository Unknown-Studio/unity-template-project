using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressablesProfileSetup
{
    static AddressablesProfileSetup()
    {
        EditorApplication.delayCall += SetupProfiles;
    }

    [MenuItem("Tools/Suhdo/Setup Addressables Profiles")]
    public static void SetupProfiles()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
        }
        if (settings == null)
        {
            Debug.LogError("Addressables settings not found! Please initialize Addressables first.");
            return;
        }

        var profileSettings = settings.profileSettings;

        // Use Default profile as our internal Editor profile
        string editorProfileId = profileSettings.GetProfileId("Default");

        // Add Staging profile
        string stagingProfileId = profileSettings.GetProfileId("Staging");
        if (string.IsNullOrEmpty(stagingProfileId))
        {
            stagingProfileId = profileSettings.AddProfile("Staging", editorProfileId);
        }

        // Add Production profile
        string prodProfileId = profileSettings.GetProfileId("Production");
        if (string.IsNullOrEmpty(prodProfileId))
        {
            prodProfileId = profileSettings.AddProfile("Production", editorProfileId);
        }

        // Configure Editor Profile
        profileSettings.SetValue(editorProfileId, "Remote.BuildPath", "ServerData/[BuildTarget]");
        profileSettings.SetValue(editorProfileId, "Remote.LoadPath", "http://localhost:8080/[BuildTarget]");

        // Configure Staging Profile
        profileSettings.SetValue(stagingProfileId, "Remote.BuildPath", "ServerData/[BuildTarget]");
        profileSettings.SetValue(stagingProfileId, "Remote.LoadPath", "https://cdn-staging.yourgame.com/addressables/[BuildTarget]");

        // Configure Production Profile
        profileSettings.SetValue(prodProfileId, "Remote.BuildPath", "ServerData/[BuildTarget]");
        // Dùng Application.version để tự động lấy version của app
        profileSettings.SetValue(prodProfileId, "Remote.LoadPath", "https://cdn.yourgame.com/addressables/v[UnityEngine.Application.version]/[BuildTarget]");

        // Bật Build Remote Catalog
        settings.BuildRemoteCatalog = true;
        settings.RemoteCatalogBuildPath.SetVariableByName(settings, "Remote.BuildPath");
        settings.RemoteCatalogLoadPath.SetVariableByName(settings, "Remote.LoadPath");
        
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssetIfDirty(settings);
        
        Debug.Log("✅ Addressables Profiles configured successfully.");
    }
}
