using UnityEngine.Localization;
using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Project/Items/Armor")]
public class ArmorItemData : ItemData
{
    public enum ArmorPart
    {
        Helmet,
        Chestpalce,
        Pants
    }

    [field: Header("Armor Stats")]
    [field: SerializeField] public int Protection { get; private set; } = 1;
    [field: SerializeField] public ArmorPart Part { get; private set; } = ArmorPart.Chestpalce;
    [field: SerializeField] public Sprite View { get; private set; }

    [field: Header("Armor Localization")]
    [field: SerializeField] public LocalizedString LocalizedHelmet { get; private set; }
    [field: SerializeField] public LocalizedString LocalizedChestplace { get; private set; }
    [field: SerializeField] public LocalizedString LocalizedPants { get; private set; }

    public string GetTranslatedPart()
    {
        switch (Part)
        {
            case ArmorPart.Helmet:
                if (!LocalizedHelmet.IsEmpty)
                    return LocalizedHelmet.GetLocalizedString();
                break;
            case ArmorPart.Chestpalce:
                if (!LocalizedChestplace.IsEmpty)
                    return LocalizedChestplace.GetLocalizedString();
                break;
            case ArmorPart.Pants:
                if (!LocalizedPants.IsEmpty)
                    return LocalizedPants.GetLocalizedString();
                break;
        }
        return Part.ToString();
    }
}