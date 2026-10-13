using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ItemMagnet : MonoBehaviour
{
    [SerializeField] private InventoryModel inventory;

    [Header("Settings")]
    [SerializeField] private float pickupRadius = 0.4f;

    private CircleCollider2D _collider;

    private void Start()
    {
        if (inventory == null)
        {
            Debug.LogError($"Отсутсвует ссылка на инвентарь в {name}");
            return;
        }

        _collider = GetComponent<CircleCollider2D>();

        _collider.isTrigger = true;
        _collider.radius = pickupRadius;
    }

    // ЗОНА МАГНИТА
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out DroppedItem item) && inventory != null && item.IsReady)
        {
            // Проверяем, есть ли место в инвентаре под этот конкретный ресурс
            if (inventory.HasSpaceFor(item.Data))   
                item.StartAttraction(transform, inventory);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}