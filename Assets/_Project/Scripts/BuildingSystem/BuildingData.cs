using UnityEngine.Localization;
using UnityEngine;

// Сохранение информации и постройках (сундуки, печи и тд)
[System.Serializable]
public struct SavedBuildingData
{
    public string BuildingDataID;
    public Vector2Data Position;
    public string CustomState;
    public string InstanceID;
}

[CreateAssetMenu(fileName = "New Building", menuName = "Project/Building")]
public class BuildingData : ScriptableObject
{
    public enum BuildingCategories
    {
        General,
        Production,
        Storage,
        Farming,
        Decor,
        Other
    }

    [field: Header("UI Info")]
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public BuildingCategories Category { get; private set; } = BuildingCategories.Other;

    [field: Header("Localization")]
    [field: SerializeField] public LocalizedString LocalizedName { get; private set; }
    [field: SerializeField] public LocalizedString LocalizedCategory { get; private set; }
    [field: SerializeField] public LocalizedString LocalizedDescription { get; private set; }

    [field: Header("Building Rules")]
    [field: SerializeField] public PlayerBuilding Prefab { get; private set; }
    [field: SerializeField] public Sprite PreviewSprite { get; private set; }
    [field: SerializeField] public Vector2Int GridSize { get; private set; } = new Vector2Int(1, 1);

    [field: Header("Cost")]
    [field: SerializeField] public ItemStack[] BuildCost { get; private set; } // Массив ресурсов, необходимых для постройки

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

    public string GetTranslatedCategory()
    {
        if (!LocalizedCategory.IsEmpty)
            return LocalizedCategory.GetLocalizedString(this);
        return "";
    }
}