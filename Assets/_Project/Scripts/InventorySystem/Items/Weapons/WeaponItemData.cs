using UnityEngine;

public abstract class WeaponItemData : ItemData
{
    public enum MaterialType
    {
        Flesh,  // Враги, животные
        Wood,   // Деревья, деревянные стены
        Stone,  // Камни, руда, печи
        Dirt    // Земля (для лопаты)
    }

    // Структура бонуса: "К какому материалу" и "Во сколько раз больше урона"
    [System.Serializable]
    public struct MaterialDamageBonus
    {
        public MaterialType TargetMaterial;
        public float Multiplier;
    }

    [field: Header("Weapon Settings")]
    [field: SerializeField] public Sprite View { get; private set; }
    [field: SerializeField] public float Damage { get; private set; } = 1f;
    [field: SerializeField] public IDamagable.DamageTypes DamageType { get; private set; } = IDamagable.DamageTypes.Physic;

    [field: Header("Tool Modifiers")]
    // Список бонусов. Если список пуст — это простое оружие (меч).
    [field: SerializeField] public MaterialDamageBonus[] MaterialBonuses { get; private set; }

    [field: Header("Animator")]
    // Сюда Override Controller для конкретного оружия
    [field: SerializeField] public AnimatorOverrideController WeaponAnimatorOverride {  get; private set; }

    public abstract void EquipTo(IWeaponEquipper equipper);

    public float GetMultiplierFor(MaterialType targetMaterial)
    {
        // Ищем, есть ли у этого оружия бонус к материалу цели
        foreach (MaterialDamageBonus bonus in MaterialBonuses)
        {
            if (bonus.TargetMaterial == targetMaterial)
                return bonus.Multiplier;
        }

        // Если бонуса нет (например, бьем топором по камню) — урон дефолтному x1
        return 1;
    }
}