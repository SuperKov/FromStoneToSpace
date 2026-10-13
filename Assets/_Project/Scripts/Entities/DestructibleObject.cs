using UnityEngine;

public class DestructibleObject : Entity
{
    [Header("Drop")]
    [SerializeField] protected ItemStack[] DroppedItems;

    public override void Die()
    {
        foreach (ItemStack item in DroppedItems)
            DroppedItemsManager.Instance.SpawnDroppedItem(item, transform.position);

        base.Die();
    }
}