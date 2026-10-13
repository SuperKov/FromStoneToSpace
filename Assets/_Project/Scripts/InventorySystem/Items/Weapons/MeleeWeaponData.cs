using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeWeapon", menuName = "Project/Items/Weapons/Melee")]
public class MeleeWeaponData : WeaponItemData
{
    [field: Header("Melee Weapon Stats")]
    [field: SerializeField] public float AttackRadius { get; private set; } = 0.2f;
    [field: SerializeField] public float AttackSpeed { get; private set; } = 1f;
    [field: SerializeField] public bool Splash { get; private set; } = false;

    private float attackDuration = -1; // кеширую длительность атаки (для анимаций), чтобы каждый раз не считать

    public string GetSpeedGrade()
    {
        return AttackSpeed switch
        {
            >= 2.0f => "Lightning",
            >= 1.2f => "Fast",
            >= 0.8f => "Default",
            _ => "Slow" // default
        };
    }

    public string GetRangeGrade()
    {
        return AttackRadius switch
        {
            >= 1.2f => "Large",
            >= 0.8f => "Big",
            >= 0.4f => "Default",
            _ => "Short" // default
        };
    }

    public override void EquipTo(IWeaponEquipper equipper)
    {
        equipper.EquipMelee(this);
    }

    public float GetAttackDuration()
    {
        if (attackDuration < 0)
            attackDuration = 1 / AttackSpeed;

        return 1 / AttackSpeed;
    }
}