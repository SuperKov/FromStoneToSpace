using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [field: Header("Main")]
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }

    [field: Header("Localization")]
    [Tooltip("Ссылка на таблицу Items_Names")]
    [field: SerializeField] public LocalizedString LocalizedName { get; private set; }
    [Tooltip("Ссылка на таблицу Items_Types")]
    [field: SerializeField] public LocalizedString LocalizedType { get; private set; }
    [Tooltip("Ссылка на таблицу Items_Stats")]
    [field: SerializeField] public LocalizedString LocalizedStats { get; private set; }
    [Tooltip("Ссылка на таблицу Items_Descriptions")]
    [field: SerializeField] public LocalizedString LocalizedDescription { get; private set; }

    public virtual int MaxStackSize => 1;

    public string GetTranslatedName()
    {
        if (!LocalizedName.IsEmpty)
            return LocalizedName.GetLocalizedString();
        return $"[{ID}]";
    }

    public string GetTranslatedDescription()
    {
        if (!LocalizedDescription.IsEmpty)
            return LocalizedDescription.GetLocalizedString();
        return "";
    }

    public string GetTranslatedStats()
    {
        if (!LocalizedStats.IsEmpty)
            return LocalizedStats.GetLocalizedString(this);
        return "";
    }

    public string GetTranslatedType()
    {
        if (!LocalizedType.IsEmpty)
            return LocalizedType.GetLocalizedString();
        return "";
    }

    //====== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ======

    // Кеширую все предметы, чтобы каждый раз их не доставать из Resources
    private static Dictionary<string, ItemData> _allItems = new();

    public static ItemData GetItemByID(string id)
    {
        if (_allItems == null || _allItems.Count <= 0)
        {
            ItemData[] configs = Resources.LoadAll<ItemData>("Items");
            foreach (ItemData config in configs)
                _allItems.Add(config.ID, config);
        }

        if (_allItems.Count < 1 || !_allItems.TryGetValue(id, out ItemData item)) return null;
        return item;
    }

    //======

    // Переопределяем сравнение, чтобы оно было по ID для скорости
    public override bool Equals(object other)
    {
        if (other is ItemData item)
            return ID == item.ID;
        return false;
    }

    public override int GetHashCode()
    {
        return ID != null ? ID.GetHashCode() : 0;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Если ID пустой (мы только создали предмет), генерируем уникальный ключ
        if (string.IsNullOrEmpty(ID))
            ID = name;
    }
#endif
}
