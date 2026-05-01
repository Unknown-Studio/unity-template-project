using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace Suhdo.Data
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Suhdo/Data/Player Stats", order = 1)]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        public FloatReference moveSpeed;

        [Header("Inventory")]
        [Tooltip("Sức chứa tối đa (số lượng trái cây có thể mang theo)")]
        public IntReference capacity;

        [Header("Combat")]
        public FloatReference attackPower;
        public FloatReference attackSpeed;

        [Header("Starting Equipment (Addressables)")]
        [Tooltip("ID hoặc Addressable Key của vũ khí mặc định (ví dụ: 'weapon_chainsaw')")]
        public string defaultWeaponId = "weapon_chainsaw";
    }
}
