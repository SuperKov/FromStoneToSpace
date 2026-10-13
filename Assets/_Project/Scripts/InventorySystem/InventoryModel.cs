using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System;

public class InventoryModel : MonoBehaviour
{
    [Serializable]
    public struct InventorySaveData
    {
        public List<SlotSaveData> SavedSlots;
    }

    [Serializable]
    public struct SlotSaveData
    {
        public int Index;       // В каком слоте лежит
        public string ItemID;   // ID предмета из ScriptableObject
        public int Amount;      // Количество
    }

    public enum TransferType
    {
        All,   // Перенести всё (ЛКМ)
        Half,  // Перенести половину (Колесико)
        One    // Перенести 1 штуку (ПКМ)
    }

    [SerializeField] protected int StartSlotsAmount = 4;

    public ItemStack[] Slots { get; private set; }

    // Событие передает конкретный индекс, чтобы не перерисовывать весь инвентарь
    public event Action<int> OnSlotUpdated;

    protected virtual void Awake()
    {
        Slots = new ItemStack[StartSlotsAmount]; // Начальные слоты
    }

    // имеется ли место для предмета
    public bool HasSpaceFor(ItemStack itemToCheck)
    {
        int amountLeft = itemToCheck.Amount; // Сколько виртуально нужно распределить

        // Шаг 1: Симулируем добавление в уже существующие стаки такого же предмета
        for (int i = 0; i < Slots.Length; i++)
        {
            // ЗАЩИТА: Разрешено ли этому предмету находиться в этом слоте?
            if (!IsItemAllowedInSlot(i, itemToCheck.Item)) continue;

            if (!Slots[i].IsEmpty && Slots[i].Item.ID == itemToCheck.Item.ID)
            {
                int spaceLeft = Slots[i].Item.MaxStackSize - Slots[i].Amount;

                if (spaceLeft > 0)
                {
                    int amountToAdd = Mathf.Min(spaceLeft, amountLeft);
                    amountLeft -= amountToAdd;

                    if (amountLeft <= 0)
                        return true; // Всё поместилось!
                }
            }
        }

        // Шаг 2: Симулируем распределение остатка по пустым слотам
        for (int i = 0; i < Slots.Length; i++)
        {
            // ЗАЩИТА: Разрешено ли этому предмету находиться в этом слоте?
            if (!IsItemAllowedInSlot(i, itemToCheck.Item)) continue;

            if (Slots[i].IsEmpty)
            {
                int amountToAdd = Mathf.Min(amountLeft, itemToCheck.Item.MaxStackSize);
                amountLeft -= amountToAdd;

                if (amountLeft <= 0)
                    return true; // Всё поместилось!
            }
        }

        // Если после всех циклов что-то осталось — значит места нет
        return false;
    }

    // имеются ли нужные предметы в инвентаре
    public bool HasItems(ItemStack[] requiredItems)
    {
        foreach (ItemStack reqItem in requiredItems)
        {
            int availableCount = 0;
            for (int i = 0; i < Slots.Length; i++)
            {
                if (!Slots[i].IsEmpty && Slots[i].Item.ID == reqItem.Item.ID)
                    availableCount += Slots[i].Amount;
            }

            if (availableCount < reqItem.Amount)
                return false;
        }
        return true;
    }

    // количество в инвентаре определенного предмета
    public int GetItemAmount(ItemData requiredItem)
    {
        int amount = 0;

        for (int i = 0; i < Slots.Length; i++)
        {
            if (!Slots[i].IsEmpty && Slots[i].Item.ID == requiredItem.ID)
                amount += Slots[i].Amount;
        }
        return amount;
    }

    // пытается добавить предмет в инвентарь, возвращая его остаток
    public int AddItem(ItemStack itemToAdd)
    {
        int amountLeft = itemToAdd.Amount; // Сколько осталось распихать

        // 1. Сначала ищем неполные стаки ТАКОГО ЖЕ предмета
        for (int i = 0; i < Slots.Length; i++)
        {
            // ЗАЩИТА: Разрешено ли этому предмету находиться в этом слоте?
            if (!IsItemAllowedInSlot(i, itemToAdd.Item)) continue;

            if (!Slots[i].IsEmpty && Slots[i].Item.ID == itemToAdd.Item.ID)
            {
                int spaceLeft = Slots[i].Item.MaxStackSize - Slots[i].Amount;

                if (spaceLeft > 0)
                {
                    // Сколько можем впихнуть в этот слот?
                    int amountToAdd = Mathf.Min(spaceLeft, amountLeft);

                    Slots[i].Amount += amountToAdd;
                    amountLeft -= amountToAdd; // Уменьшаем остаток

                    OnSlotUpdated?.Invoke(i);

                    // Если распихали всё, возвращаем 0 (остатков нет)
                    if (amountLeft <= 0)
                        return 0;
                }
            }
        }

        // 2. Если предмет еще остался, ищем ПУСТЫЕ слоты
        for (int i = 0; i < Slots.Length; i++)
        {
            // ЗАЩИТА: Разрешено ли этому предмету находиться в этом слоте?
            if (!IsItemAllowedInSlot(i, itemToAdd.Item)) continue;

            if (Slots[i].IsEmpty)
            {
                // ИСПРАВЛЕНИЕ БАГА №2: Учитываем максимальный размер стака
                int amountToAdd = Mathf.Min(amountLeft, itemToAdd.Item.MaxStackSize);

                Slots[i] = new ItemStack(itemToAdd.Item, amountToAdd);
                amountLeft -= amountToAdd; // Уменьшаем остаток

                OnSlotUpdated?.Invoke(i);

                // Если распихали всё, возвращаем 0
                if (amountLeft <= 0)
                    return 0;
            }
        }

        // 3. Если мы дошли сюда, значит места нет
        return amountLeft;
    }

    // пытаемся забрать предметы, если успех, то возвращем true
    public bool TryRemoveItems(ItemStack[] requiredItems)
    {
        // 1. ЭТАП ПРОВЕРКИ: Проверяем, хватает ли абсолютно всех ресурсов
        foreach (ItemStack reqItem in requiredItems)
        {
            int availableCount = 0;
            for (int i = 0; i < Slots.Length; i++)
            {
                if (!Slots[i].IsEmpty && Slots[i].Item.ID == reqItem.Item.ID)
                {
                    availableCount += Slots[i].Amount;
                }
            }

            // Если хотя бы одного ресурса не хватает — крафт отменяется
            if (availableCount < reqItem.Amount)
                return false;
        }

        // 2. ЭТАП ИЗЪЯТИЯ: Раз мы дошли сюда, значит ресурсов точно гарантированно хватает
        foreach (ItemStack reqItem in requiredItems)
        {
            int amountLeftToRemove = reqItem.Amount;
            for (int i = 0; i < Slots.Length; i++)
            {
                if (!Slots[i].IsEmpty && Slots[i].Item.ID == reqItem.Item.ID)
                {
                    int toRemove = Mathf.Min(Slots[i].Amount, amountLeftToRemove);

                    Slots[i].Amount -= toRemove;
                    amountLeftToRemove -= toRemove;

                    if (Slots[i].Amount <= 0)
                    {
                        Slots[i] = new ItemStack();
                    }

                    OnSlotUpdated?.Invoke(i);

                    if (amountLeftToRemove <= 0)
                        break;
                }
            }
        }

        return true;
    }

    // забираем предмет из слота, очищая слот и возвращая предметы, которые лежали в нем
    public ItemStack TakeItemFromSlot(int slotId)
    {
        if (slotId >= Slots.Length)
            return new();

        return Slots[slotId];
    }

    // Метод, который вызывается при перетаскивании
    // универсальный метод для перемещения: как внутри одного инвентаря, так и между разными
    public void SwapOrCombineItems(int fromIndex, InventoryModel targetModel, int targetIndex, TransferType transferType = TransferType.All)
    {
        // Базовые защиты
        if (targetModel == null) return;

        // Если это тот же самый инвентарь и тот же самый слот — ничего не делаем
        if (this == targetModel && fromIndex == targetIndex) return;

        ItemStack fromSlot = Slots[fromIndex];
        ItemStack toSlot = targetModel.Slots[targetIndex];

        // ЗАЩИТА: Может ли целевой инвентарь (броня) принять предмет, который мы в него тащим?
        if (!fromSlot.IsEmpty && !targetModel.IsItemAllowedInSlot(targetIndex, fromSlot.Item)) return;

        // ЗАЩИТА: Если слот занят, может ли наш рюкзак принять надетую броню обратно?
        if (!toSlot.IsEmpty && !IsItemAllowedInSlot(fromIndex, toSlot.Item)) return;

        // 1. Вычисляем, сколько штук берем
        int amountToMove = fromSlot.Amount;
        if (transferType == TransferType.Half)
            amountToMove = Mathf.CeilToInt(fromSlot.Amount / 2f);
        else if (transferType == TransferType.One)
            amountToMove = 1;

        // 2. ЗАЩИТА: Запрещаем менять разные предметы местами, если игрок несет не весь стак
        if (transferType != TransferType.All && !toSlot.IsEmpty && fromSlot.Item.ID != toSlot.Item.ID)
            return;

        // 3. ОСНОВНАЯ ЛОГИКА
        // 3.1 Слот пустой - просто все перекидываем
        if (toSlot.IsEmpty)
        {
            targetModel.Slots[targetIndex] = new ItemStack(fromSlot.Item, amountToMove);
            Slots[fromIndex].Amount -= amountToMove;
            if (Slots[fromIndex].Amount <= 0) Slots[fromIndex] = new ItemStack();
        }
        // 3.2 Предметы в слотах одинаковые - пытаемся стакнуть и смотрим на остаток
        else if (fromSlot.Item.ID == toSlot.Item.ID)
        {
            int spaceLeft = toSlot.Item.MaxStackSize - toSlot.Amount;
            int actualMove = Mathf.Min(spaceLeft, amountToMove);

            targetModel.Slots[targetIndex].Amount += actualMove;
            Slots[fromIndex].Amount -= actualMove;

            if (Slots[fromIndex].Amount <= 0) Slots[fromIndex] = new ItemStack();
        }
        // 3.3 Предметы в слотах разные - пытаемся свапнуть
        else if (transferType == TransferType.All)
        {
            // Свап (сработает только если мы переносим предмет целиком)
            targetModel.Slots[targetIndex] = fromSlot;
            Slots[fromIndex] = toSlot;
        }

        // 4. ОПОВЕЩАЕМ UI ОБ ИЗМЕНЕНИЯХ
        OnSlotUpdated?.Invoke(fromIndex);

        // Если целевая модель — это мы сами, вызываем ивент у себя. 
        // Если другая (сундук) — дергаем ивент сундука.
        if (this == targetModel)
            OnSlotUpdated?.Invoke(targetIndex);
        else
            targetModel.OnSlotUpdated?.Invoke(targetIndex);
    }
    
    // быстро перекинуть, к примеру если зажат shift
    public void FastClickTransfer(int fromIndex, InventoryModel targetModel)
    {
        if (targetModel == null || this == targetModel) return;

        ItemStack fromSlot = Slots[fromIndex];
        if (fromSlot.IsEmpty) return;

        // Пытаемся закинуть предмет в целевой инвентарь (AddItem сам найдет стаки или пустые места)
        int remainder = targetModel.AddItem(new ItemStack(fromSlot.Item, fromSlot.Amount));

        // Если влезло всё - очищаем наш слот
        if (remainder == 0)
            Slots[fromIndex] = new ItemStack();
        else
            Slots[fromIndex].Amount = remainder;

        OnSlotUpdated?.Invoke(fromIndex);
    }

    // Базовый инвентарь разрешает класть всё и куда угодно
    public virtual bool IsItemAllowedInSlot(int slotIndex, ItemData item)
    {
        return true;
    }

    // СОХРАНЕНИЕ: ЗАГРУЗКА / ВЫГРУЗКА ИНВЕНТАРЯ. Без использования ISaveable,
    // так как не все сундуки должны сохраняться (сундуки в данжах не должны)
    public string CaptureInventoryState()
    {
        InventorySaveData data = new()
        {
            SavedSlots = new()
        };

        for (int i = 0; i < Slots.Length; i++)
        {
            if (!Slots[i].IsEmpty)
            {
                data.SavedSlots.Add(new SlotSaveData
                {
                    Index = i,
                    ItemID = Slots[i].Item.ID, // Берем ID из ScriptableObject предмета
                    Amount = Slots[i].Amount
                });
            }
        }

        return JsonConvert.SerializeObject(data);
    }

    public void RestoreInventoryState(string jsonState)
    {
        if (string.IsNullOrEmpty(jsonState)) return;

        InventorySaveData data = JsonConvert.DeserializeObject<InventorySaveData>(jsonState);
        if (data.SavedSlots == null) return;

        // 1. Очищаем текущие слоты
        for (int i = 0; i < Slots.Length; i++)
            Slots[i] = new();

        // 2. Загружаем все конфиги предметов
        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");

        // 3. Раскладываем предметы по слотам
        foreach (SlotSaveData savedSlot in data.SavedSlots)
        {
            ItemData item = allItems.FirstOrDefault(i => i.ID == savedSlot.ItemID);
            if (item != null && savedSlot.Index >= 0 && savedSlot.Index < Slots.Length)
                Slots[savedSlot.Index] = new ItemStack(item, savedSlot.Amount);
        }

        for (int i = 0; i < Slots.Length; i++)
            OnSlotUpdated?.Invoke(i);
    }
}