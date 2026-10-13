using UnityEngine;

public class PlayerChestBuilding : PlayerBuilding
{
    [Header("Inventory")]
    [SerializeField] private InventoryModel chestInventory;

    // WorldSaveManager вызовет этот метод при сохранении
    public override string GetCustomState()
    {
        return chestInventory.CaptureInventoryState();
    }

    // WorldSaveManager вызовет этот метод при загрузке (после спавна сундука)
    public override void LoadCustomState(string stateJson)
    {
        chestInventory.RestoreInventoryState(stateJson);
    }

    public override void Die()
    {
        for (int i = 0; i < chestInventory.Slots.Length; i++)
        {
            ItemStack item = chestInventory.TakeItemFromSlot(i);
            if (!item.IsEmpty)
                DroppedItemsManager.Instance.SpawnDroppedItem(chestInventory.TakeItemFromSlot(i), transform.position);
        }

        base.Die();
    }
}