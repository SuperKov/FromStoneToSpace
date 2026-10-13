using UnityEngine;

[CreateAssetMenu(fileName = "NewBow", menuName = "Project/Items/Weapons/Bow")]
public class BowData : WeaponItemData
{
    [field: Header("Bow")]
    [field: SerializeField] public float AimSpeed { get; private set; } = 1f;
    [field: SerializeField] public float MaxAimMultiplier { get; private set; } = 1.5f;
    [field: SerializeField] public float ArrowSpeed { get; private set; } = 4f;

    private const float UIAimSpeedModifier = 10f;

    public override void EquipTo(IWeaponEquipper equipper)
    {
        equipper.EquipBow(this);
    }

    public string GetUIAimSpeed()
    {
        return (AimSpeed * UIAimSpeedModifier).ToString();
    }
}