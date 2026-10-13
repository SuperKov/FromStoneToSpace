using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private MoveController movement;
    [SerializeField] private PlayerCombatController combat;

    public event Action<int> HotbarScrolled;
    public event Action<int> NumberPressed;

    private void OnEnable()
    {
        InputManager.Instance.Actions.Player.Numbers.performed += OnNumbers;
        InputManager.Instance.Actions.Player.Scroll.performed += OnScroll;
        InputManager.Instance.Actions.Player.Attack.performed += OnAttack;
        InputManager.Instance.Actions.Player.Attack.canceled += OnAttack;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Actions.Player.Numbers.performed -= OnNumbers;
            InputManager.Instance.Actions.Player.Scroll.performed -= OnScroll;
            InputManager.Instance.Actions.Player.Attack.performed -= OnAttack;
            InputManager.Instance.Actions.Player.Attack.canceled -= OnAttack;
        }
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector2 inputDir = InputManager.Instance.Actions.Player.Move.ReadValue<Vector2>();

        // 1. Передаем вектор движения
        movement.SetDirection(inputDir);

        InputManager input = InputManager.Instance;
        if (input != null && input.IsPlayerBlocked) return;

        // Поворачиваем персонажа в сторону курсора
        float lookDir = CameraManager.Instance.GetMouseWorldPosition().x - transform.position.x;

        // 2. Управляем поворотом (флипом)
        movement.HandleFlip(lookDir);
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
            combat.StartAttack();
        else if (context.canceled)
            combat.StopAttack(); // Для меча метод пустой, для лука — сбросит IsCharging и выстрелит
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        // Меняется только Y, хз почему сделан вектор 2, а не int или float, но мне пох
        Vector2 scrollValue = context.ReadValue<Vector2>();

        if (scrollValue.y == 0) return;

        HotbarScrolled.Invoke((int)scrollValue.y);
    }

    private void OnNumbers(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        int rawValue = (int)context.ReadValue<float>();

        // Если значение 0 — это математический результат отпускания кнопки (0 * Scale = 0)
        if (rawValue == 0) return;

        // Превращаем цифры с клавиатуры (1..10) в индексы слотов (0..9)
        int slotIndex = rawValue - 1;

        NumberPressed?.Invoke(slotIndex);
    }
}