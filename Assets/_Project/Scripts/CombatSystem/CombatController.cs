using UnityEngine;
using System;

public abstract class CombatController<T> : MonoBehaviour, ICombatController
{
    public bool IsAttacking { get; protected set; } = false;

    public event Action<bool> AttackCompleted;

    [Header("General")]
    [SerializeField] protected LayerMask attackLayer;

    [Header("Animator")] // Ссылки необязательны для работы — можно оставить пустыми
    [SerializeField] protected Animator animator;
    [SerializeField] protected AnimatorEventRelay eventRelay;
    [SerializeField] private string animationAttackId = "Attack";
    [SerializeField] private string animationAttackDoneId = "Done";

    protected T weaponData;

    protected static readonly int attackTriggerHash = Animator.StringToHash("Attack");
    protected static readonly int returnTriggerHash = Animator.StringToHash("Return");

    protected virtual void OnEnable()
    {
        if (eventRelay != null)
            eventRelay.EventExecuted += HandleAnimationEvent;
    }

    protected virtual void OnDisable()
    {
        if (eventRelay != null)
            eventRelay.EventExecuted -= HandleAnimationEvent;

        InterruptAttack();
    }

    public virtual void SetData(T data)
    {
        weaponData = data;
    }

    public virtual void StartAttack()
    {
        if (IsAttacking) return;
        IsAttacking = true; // Флаг, что идет атака

        // Проверяем: есть ли система анимации?
        if (animator != null || eventRelay != null)
        {
            animator.SetTrigger(attackTriggerHash); // Запуск визуала
        }
        else
        {
            // Анимаций нет (или это прототип). Атака мгновенна
            Debug.LogWarning($"[{name}] Анимации не найдены. Атака выполнена мгновенно.");
            Attack();
        }
    }

    public virtual void StopAttack() {  }

    public abstract void Attack();

    protected virtual void HandleAnimationEvent(string eventId) // Обработчик всех ивентов анимации, которые долетают до боевого контроллера
    {
        // Боевой контроллер реагирует ТОЛЬКО на команду удара. 
        if (eventId == animationAttackId)
            Attack();
        else if (eventId == animationAttackDoneId)
        {
            IsAttacking = false;
        }
    }

    // Безопасное прерывание атаки
    protected virtual void InterruptAttack()
    {
        if (!IsAttacking) return;

        IsAttacking = false;

        // Очищаем триггер в аниматоре, чтобы при следующем доставании меча
        // персонаж не сделал случайный удар, "запомнив" старую команду
        if (animator != null)
        {
            animator.ResetTrigger(attackTriggerHash);

            // принудительно перевести аниматор в Idle, чтобы сбросить анимацию удара
            animator.SetTrigger(returnTriggerHash);
        }

        // Оповещаем другие системы, что атака завершилась ничем (целей не задели)
        AttackCompleted?.Invoke(false);
    }

    protected void InvokeAttackCompleted(bool target)
    {
        AttackCompleted?.Invoke(target);
    }
}