using UnityEngine;

namespace Suhdo.Data
{
    [CreateAssetMenu(fileName = "NewItemData", menuName = "Suhdo/Data/Item Data", order = 0)]
    public class ItemData : ScriptableObject
    {
        [Tooltip("Unique ID for this item (e.g., 'fruit_apple')")]
        public string id;

        [Tooltip("Hiển thị tên trong game")]
        public string itemName;

        [Tooltip("Giá trị khi bán hoặc chế tạo")]
        public int price;

        [Tooltip("Prefab hiển thị 3D/2D khi rơi rớt trên đất hoặc khi stack trên lưng")]
        public GameObject prefab;

        [Tooltip("Visual Color (để gán vào particle effect hoặc chỉnh vật liệu)")]
        public Color color = Color.white;
    }
}
