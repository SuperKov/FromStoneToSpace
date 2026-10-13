using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PlayerInventoryPanel : InventoryPanel
{
    [Header("Storage Menu")]
    [SerializeField] private InventoryPanel Storage;
    [SerializeField] private InventoryPanel Equipment;

    [Header("Quick Slots")]
    [SerializeField] private Transform quickSlotsContainer;

    [Header("Hotbar Selection")]
    [SerializeField] private RectTransform selectionFrame;
    [SerializeField] private float animDuration = 0.2f; // Время полета рамки в секундах
    [SerializeField] private Vector3 punch = new(0.4f, 0.4f, 0);
    [SerializeField] private Ease selectionEase = Ease.OutBack;


    private bool _isFrameInitialized = false;
    private PlayerEntity _playerEntity;
    private Tween _moveTween; // Ссылка на текущую анимацию перемещения рамки hotslot
    private Tween _punchTween; // Ссылка на анимацию импульса рамки hotslot

    private void Start()
    {
        if (PlayerEntity.Instance != null)
            SetPlayerEntity(PlayerEntity.Instance);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_playerEntity != null)
            _playerEntity.SelectedSlotChanged -= OnSelectedSlotChanged;
    }

    // 1. Инициализируем модель инвентаря
    public void SetPlayerEntity(PlayerEntity player)
    {
        _playerEntity = player;
        _playerEntity.SelectedSlotChanged += OnSelectedSlotChanged;
        SetupModel(_playerEntity.Inventory);
    }

    protected override InventoryModel GetTransferTarget()
    {
        if (Storage != null && Storage.ActiveModel != null)
            return Storage.ActiveModel;

        if (Equipment != null && Equipment.ActiveModel != null)
            return Equipment.ActiveModel;

        return null;
    }

    protected override Transform GetContainerForSlot(int index)
    {
        if (index >= PlayerEntity.Instance.CurrentHotbarSize)
            return defaultContainer;

        return quickSlotsContainer;
    }

    private void OnSelectedSlotChanged(int selectedIndex)
    {
        if (_uiSlots == null || _uiSlots.Count <= selectedIndex)
        {
            selectionFrame.gameObject.SetActive(false);
            return;
        }

        selectionFrame.gameObject.SetActive(true);

        Vector3 targetPosition = _uiSlots[selectedIndex].transform.position;

        // При первом запуске ставим рамку мгновенно
        if (!_isFrameInitialized)
        {
            Canvas.ForceUpdateCanvases();
            selectionFrame.position = targetPosition;
            _isFrameInitialized = true;
            return;
        }

        // Прерываем предыдущие анимации, если игрок крутит колесико очень быстро
        _moveTween?.Kill();
        _punchTween?.Kill();

        // 1. Анимация перемещения с эффектом пружины (OutBack немного пролетает цель и возвращается)
        _moveTween = selectionFrame.DOMove(targetPosition, animDuration)
            .SetEase(selectionEase);

        // 2. Анимация "импульса" - рамка слегка растягивается при прибытии
        // Сначала сбрасываем масштаб на дефолтный, чтобы избежать искажений при спаме скролла
        selectionFrame.localScale = Vector3.one;

        // эффект удара или фиксации на месте
        _punchTween = selectionFrame.DOPunchScale(punch, animDuration, 1);
    }
}