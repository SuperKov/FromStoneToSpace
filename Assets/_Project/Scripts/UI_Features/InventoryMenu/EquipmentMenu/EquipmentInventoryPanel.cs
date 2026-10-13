using UnityEngine;

public class EquipmentInventoryPanel : InventoryPanel
{
    [SerializeField] private InventoryPanel Backpack;

    private void Start()
    {
        if (PlayerEntity.Instance != null)
            SetupModel(PlayerEntity.Instance.Equipment);
    }

    protected override InventoryModel GetTransferTarget()
    {
        if (Backpack != null && Backpack.ActiveModel != null)
            return Backpack.ActiveModel;

        return null;
    }
}