using UnityEngine;

public class MeleeCombatController : CombatController<MeleeWeaponData>
{
    [Header("Melee")]
    [SerializeField] private Transform attackPoint; // Точка с коллайдером для урона

    private static readonly int attackSpeedHash = Animator.StringToHash("AttackSpeed");

    public override void Attack()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, weaponData.AttackRadius, attackLayer);

        foreach (Collider2D target in hitTargets)
        {
            if (target.TryGetComponent(out IDamagable damagable))
            {

                damagable.TakeDamage(new(weaponData.Damage,
                    gameObject,
                    weaponData.DamageType,
                    weaponData));

                // Если оружие может бить сплешом, то оно задевает всех
                if (!weaponData.Splash) break;
            }
        }

        // Вызывается при успешной атаке
        InvokeAttackCompleted(hitTargets.Length > 0);
    }

    public override void SetData(MeleeWeaponData data)
    {
        base.SetData(data);

        animator.SetFloat(attackSpeedHash, weaponData.AttackSpeed);
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null && weaponData != null)
                Gizmos.DrawWireSphere(attackPoint.position, weaponData.AttackRadius);
    }
}