[System.Serializable]
public struct ItemStack
{
    public ItemData Item;
    public int Amount;

    // Конструктор для удобного создания в коде
    public ItemStack(ItemData item, int amount)
    {
        Item = item;
        Amount = amount;
    }

    // Удобное свойство-помощник, чтобы быстро проверять, пуст ли стак
    public bool IsEmpty => Item == null || Amount <= 0;
}