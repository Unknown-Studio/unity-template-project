using System;
using SmartAddresser.Editor.Core.Models.LayoutRules.AddressRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Suhdo.Editor
{
    [Serializable]
    public sealed class LowercasePrefixAddressProvider : AddressProviderAsset
    {
        [SerializeField] private string _prefix = "";
        [SerializeField] private bool _removeExtension = true;

        public string Prefix { get => _prefix; set => _prefix = value; }

        public override void Setup() { }

        public override string Provide(string assetPath, Type assetType, bool isFolder)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;

            string result = assetPath;
            
            // Tìm phần sau Assets/_Project/ (hoặc bỏ prefix thư mục gốc)
            int index = result.IndexOf("_Project/");
            if (index >= 0)
            {
                result = result.Substring(index + "_Project/".Length);
            }

            // Bỏ phần đầu dư thừa dựa trên logic thư mục (ví dụ bỏ "Prefabs/", "Audio/")
            if (result.StartsWith("Prefabs/")) result = result.Substring("Prefabs/".Length);
            else if (result.StartsWith("Audio/")) result = result.Substring("Audio/".Length);
            else if (result.StartsWith("SO_Data/")) result = result.Substring("SO_Data/".Length);
            else if (result.StartsWith("Art/")) result = result.Substring("Art/".Length);

            if (_removeExtension)
            {
                int lastDot = result.LastIndexOf('.');
                if (lastDot > 0) result = result.Substring(0, lastDot);
            }

            result = _prefix + result.ToLower().Replace(' ', '_');
            return result;
        }

        public override string GetDescription() => $"Prefix: {_prefix}, Lowercase: True";
    }
}
