using UnityEngine;
using static IDamagable;

public interface IDamagable
{
    public enum DamageTypes
    {
        Magic,
        Physic,
        Pure
    }

    void TakeDamage(DamageInfo info);
}

public struct DamageInfo
{
    public float BaseDamage;      // Базовый урон
    public GameObject Sender;     // Кто ударил
    public DamageTypes Type;       // Physic, Magic, Pure

    // Передаем ссылку на само оружие, чтобы жертва могла прочитать из него любые множители
    public WeaponItemData Weapon;

    // Конструктор для удобства создания
    public DamageInfo(float damage, GameObject sender, DamageTypes type, WeaponItemData weapon = null)
    {
        BaseDamage = damage;
        Sender = sender;
        Type = type;
        Weapon = weapon;
    }
}