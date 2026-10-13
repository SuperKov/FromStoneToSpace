public class EquipmentModel : InventoryModel
{
    protected override void Awake()
    {
        StartSlotsAmount = 3;

        base.Awake();
    }

    // Жестко фиксируем индексы: 0 = Шлем, 1 = Нагрудник, 2 = Штаны
    public override bool IsItemAllowedInSlot(int slotIndex, ItemData item)
    {
        if (item == null) return true; // Разрешаем очищать слот

        // Если это не броня — запрещаем
        if (item is not ArmorItemData armor) return false;

        // Распределяем по слотам
        if (slotIndex == 0 && armor.Part == ArmorItemData.ArmorPart.Helmet) return true;
        if (slotIndex == 1 && armor.Part == ArmorItemData.ArmorPart.Chestpalce) return true;
        if (slotIndex == 2 && armor.Part == ArmorItemData.ArmorPart.Pants) return true;

        return false;
    }
}