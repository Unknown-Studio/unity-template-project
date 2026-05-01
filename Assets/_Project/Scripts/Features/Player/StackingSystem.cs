using System.Collections.Generic;
using UnityEngine;
using Suhdo.Data;
using UnityAtoms.BaseAtoms;
using DG.Tweening; // Rất khuyên dùng DOTween cho game Idle Arcade để làm animation nhảy vào túi

namespace Suhdo.Features.Player
{
    public class StackingSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        
        [Tooltip("Vị trí gốc trên lưng nhân vật để bắt đầu xếp đồ")]
        [SerializeField] private Transform backpackTransform;

        [Header("UnityAtoms - Data Sync")]
        [Tooltip("Kéo thả IntVariable đại diện cho số lượng đồ trong balo (Reactive UI sẽ lắng nghe biến này)")]
        [SerializeField] private IntVariable currentStackCountVariable;

        [Header("Settings")]
        [Tooltip("Khoảng cách tăng dần theo trục Y cho mỗi item")]
        [SerializeField] private float yOffsetPerItem = 0.5f;

        // Danh sách chứa các object đang stack
        private List<GameObject> _stackedItems = new List<GameObject>();

        // Lấy value từ UnityAtoms IntReference thay vì biến thường
        private bool IsFull => _stackedItems.Count >= playerStats.capacity.Value;

        private void Start()
        {
            // Khởi tạo số lượng trên ba lô là 0 cho UI biết
            if (currentStackCountVariable != null)
                currentStackCountVariable.Value = 0;
        }

        /// <summary>
        /// Giao diện chính để nhặt item, thường được gọi từ OnTriggerEnter của Player hoặc Item
        /// </summary>
        public bool TryPickupItem(ItemData itemData, Transform itemWorldTransform)
        {
            if (IsFull)
            {
                Debug.Log("Backpack is FULL! Can't pickup " + itemData.itemName);
                return false;
            }

            GameObject visualEntity = itemWorldTransform.gameObject;
            
            // Disable logic nhặt rớt, va chạm vật lý trên thân item
            Destroy(visualEntity.GetComponent<Collider>());
            if (visualEntity.TryGetComponent(out Rigidbody rb)) Destroy(rb);

            AddToStack(visualEntity, itemData);
            return true;
        }

        private void AddToStack(GameObject itemVisual, ItemData itemData)
        {
            _stackedItems.Add(itemVisual);
            int index = _stackedItems.Count - 1;

            // Xoay và gắn làm con của backpack
            itemVisual.transform.SetParent(backpackTransform);
            
            // Cập nhật lên biến Atoms để các thanh UI Progress Bar tự tự động chạy
            if (currentStackCountVariable != null)
                currentStackCountVariable.Value = _stackedItems.Count;

            // Tính toán vị trí đích đến trên Backpack
            Vector3 targetLocalPos = new Vector3(0, index * yOffsetPerItem, 0);

            itemVisual.transform.localPosition = targetLocalPos;
            itemVisual.transform.localRotation = Quaternion.identity;
        }

        public void SellAllItems()
        {
            foreach (var item in _stackedItems)
            {
                Destroy(item);
            }
            _stackedItems.Clear();

            // Reset UI Atoms variable
            if (currentStackCountVariable != null)
                currentStackCountVariable.Value = 0;

            Debug.Log("[StackingSystem] Sold everything!");
        }
    }
}
