using UnityEngine;

public class PlayerHandController : MonoBehaviour
{
    [SerializeField] private PlayerEntity playerEntity;

    [Header("Rotation Settings")]
    [SerializeField] private float maxRadius = 0.25f; // Максимальная дистанция руки от центра
    [SerializeField] private float smoothTime = 0.1f; // Скорость следования
    [SerializeField] private Transform handPivot;     // Объект, который мы двигаем

    [Header("Flip Settings")]
    [SerializeField] private Transform visualsContainer; // Сюда перетаскиваем объект для отражения

    [Header("View")]
    [SerializeField] private SpriteRenderer toolView;

    private Vector2 _velocity;
    private Vector2 _mousePos;
    private Vector2 _targetPosition;
    
    private void Start()
    {
        // Сразу инициализируем визуальную часть
        OnSelectedSlotChanged(playerEntity.SelectedSlotIndex);
    }

    private void OnEnable()
    {
        playerEntity.SelectedSlotChanged += OnSelectedSlotChanged;
    }

    private void OnDisable()
    {
        playerEntity.SelectedSlotChanged -= OnSelectedSlotChanged;
    }

    private void Update()
    {
        RotateHand();
    }

    private void RotateHand()
    {
        InputManager input = InputManager.Instance;

        // 1. Получаем позицию мыши в мире
        if (input != null && !input.IsPlayerBlocked)
            _mousePos = CameraManager.Instance.GetMouseWorldPosition();

        Vector2 centerPos = transform.position;

        // 2. Вектор от центра к курсору
        Vector2 direction = _mousePos - centerPos;

        // 3. Ограничиваем этот вектор радиусом (ClampMagnitude)
        Vector2 clampedDirection = Vector2.ClampMagnitude(direction, maxRadius);

        // 4. Целевая позиция - это центр + ограниченный вектор
        _targetPosition = centerPos + clampedDirection;

        // 5. Плавное движение без залипаний
        handPivot.position = Vector2.SmoothDamp(
            handPivot.position,
            _targetPosition,
            ref _velocity,
            smoothTime // время сглаживания
        );

        // Поворот руки к курсору (на все 360 градусов)
        HandleRotateHand(direction);
        // Отражение руки по вертикали, чтобы она не была вверх ногами
        HandleFlipHand(direction);
    }

    private void HandleRotateHand(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        handPivot.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void HandleFlipHand(Vector2 direction)
    {
        if (visualsContainer == null) return;

        // Если мышка находится слева от игрока (direction.x < 0), 
        // инвертируем localScale по оси Y (-1), что перевернет руку обратно в нормальное положение.
        // Если справа — возвращаем обычный масштаб (1).
        float scaleY = (direction.x < 0) ? -1f : 1f;

        visualsContainer.localScale = new Vector3(1f, scaleY, 1f);
    }

    public void UpdateView(Sprite sprite)
    {
        toolView.sprite = sprite;
    }

    private void OnSelectedSlotChanged(int slotIndex)
    {
        if (playerEntity.Inventory == null || playerEntity.Inventory.Slots.Length < slotIndex) return;

        ItemStack item = playerEntity.Inventory.Slots[slotIndex];
        if (!item.IsEmpty && item.Item is WeaponItemData weapon)
            UpdateView(weapon.View);
        else
            UpdateView(null);
    }
}