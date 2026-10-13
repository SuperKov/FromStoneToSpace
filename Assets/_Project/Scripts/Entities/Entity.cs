using UnityEngine;
using System;
using static IDamagable;

public class Entity : MonoBehaviour, IDamagable
{
    public event Action<float, float> HealthChanged;

    public float Health {  get; protected set; }

    [Header("Health")]
    [SerializeField] protected float maxHealth = 5f;

    [Header("Resists")]
    [SerializeField] protected int basePhysicDamageResist = 0;
    [SerializeField] protected float baseMagicDamageResist = 0;
    [SerializeField] protected WeaponItemData.MaterialType BaseMaterial = WeaponItemData.MaterialType.Flesh;

    public float MaxHealth => maxHealth;

    protected const float ArmorBalance = 50f;

    protected void Awake()
    {
        Health = maxHealth;
    }

    public virtual void TakeDamage(DamageInfo info)
    {
        float finalDamage = CalculateFinalDamage(info);

        Health -= finalDamage;

        if (Health <= 0)
        {
            Health = 0;
            HealthChanged?.Invoke(Health, maxHealth);

            Die();

            return;
        }

        HealthChanged?.Invoke(Health, maxHealth);
    }

    public virtual void Die()
    {
        gameObject.SetActive(false);
    }

    // Убираем старый метод TakeDamage(float, GameObject) и переопределяем новый метод расчета:
    protected virtual float CalculateFinalDamage(DamageInfo info)
    {
        // Берем базовый урон, который пришел в посылке
        float damage = info.BaseDamage;

        // 1. ЛОГИКА МАТЕРИАЛА: Применяем бонус инструмента
        if (info.Weapon != null)
        {
            float toolMultiplier = info.Weapon.GetMultiplierFor(BaseMaterial);
            damage *= toolMultiplier;
        }

        // 2. Распределяем логику в зависимости от ТИПА урона
        switch (info.Type)
        {
            case DamageTypes.Physic:
                // ФИЗИКА: Применяем MOBA-броню от экипировки игрока
                float armorMultiplier = 1 - basePhysicDamageResist;
                damage *= armorMultiplier;

                // (Базовый процентный резист из Entity мы здесь ИГНОРИРУЕМ, 
                // так как для игрока баланс строится вокруг экипировки)
                break;

            case DamageTypes.Magic:
                // МАГИЯ: Обычная броня не помогает! 
                damage *= 1 - baseMagicDamageResist;
                break;

            default:
                break;
        }

        return damage;
    }
}