using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using static InventoryModel;

public class InventoryPanel : MonoBehaviour
{
    public InventoryModel ActiveModel {  get; private set; }

    [Header("UI Setup")]
    [SerializeField] protected UISlot slotPrefab;
    [SerializeField] protected Transform defaultContainer;

    protected List<UISlot> _uiSlots = new();
    protected InventoryModel _model;

    protected virtual void OnEnable()
    {
        if (_model == null) return;
        RefreshUI();
        ActiveModel = _model;
    }

    protected virtual void OnDisable()
    {
        ActiveModel = null;
    }

    // Отписываемся в дестрое от инвентаря, если его передавали в SetupModel
    protected virtual void OnDestroy()
    {
        if (_model != null)
            _model.OnSlotUpdated -= HandleModelSlotUpdate;
    }

    public virtual void RefreshUI()
    {
        for (int i = 0; i < _model.Slots.Length; i++)
            HandleModelSlotUpdate(i);
    }

    // --- ОБРАБОТЧИКИ СОБЫТИЙ ОТ ДАННЫХ (MODEL -> VIEW) ---
    protected void SetupModel(InventoryModel model)
    {
        if (_model != null)
            _model.OnSlotUpdated -= HandleModelSlotUpdate;

        _model = model;
        _model.OnSlotUpdated += HandleModelSlotUpdate;
        if (gameObject.activeInHierarchy)
            ActiveModel = _model;

        SetupUI();
    }

    protected void HandleModelSlotUpdate(int slotIndex)
    {
        // Обновляем только один конкретный слот
        ItemStack currentData = _model.Slots[slotIndex];

        _uiSlots[slotIndex].UpdateVisuals(currentData);
    }

    // Базовая реализация для сундуков — всё летит в одну кучу
    protected virtual Transform GetContainerForSlot(int index) => defaultContainer;

    // Пустышка. Дочерние классы сами решат, куда кидать предметы быстрым нажатием
    protected virtual InventoryModel GetTransferTarget() => null;

    // Устанавливаем / создаем все слоты под модель
    // если слотов меньше, чем есть, то созадем новые, если больше - скрываем лишние
    protected virtual void SetupUI()
    {
        int requiredSlots = _model.Slots.Length;
        int existingSlots = _uiSlots.Count;

        for (int i = 0; i < Mathf.Max(requiredSlots, existingSlots); i++)
        {
            if (i < requiredSlots)
            {
                if (i >= existingSlots)
                {
                    UISlot newSlot = CreateUISlot(i, _model);
                    _uiSlots.Add(newSlot);
                }
                else
                {
                    // Слот уже есть - просто включаем и привязываем к новой модели
                    _uiSlots[i].Initialize(i, _model, this);
                    _uiSlots[i].gameObject.SetActive(true);
                }
            }
            else
                // Лишние слоты прячем
                _uiSlots[i].gameObject.SetActive(false);
        }
    }

    protected virtual UISlot CreateUISlot(int index, InventoryModel targetModel)
    {
        Transform targetContainer = GetContainerForSlot(index);

        UISlot newSlot = Instantiate(slotPrefab, targetContainer);

        newSlot.Initialize(index, targetModel, this);

        newSlot.SlotFastClicked += OnSlotFastClicked;
        newSlot.SlotDropped += OnSlotDropped;
        newSlot.DragBegun += OnSlotDragBegun;
        newSlot.Dragging += OnSlotDragging;
        newSlot.DragEnded += OnSlotDragEnded;

        newSlot.UpdateVisuals(new());

        return newSlot;
    }

    // --- ОБРАБОТЧИКИ СОБЫТИЙ ОТ СЛОТОВ ---
    private void OnSlotDragBegun(Sprite sprite, Vector2 mousePos)
    {
        if (gameObject.activeInHierarchy && DraggableIcon.Instance != null)
            DraggableIcon.Instance.StartDragging(sprite, mousePos);
    }

    private void OnSlotDragging(Vector2 mousePos)
    {
        if (DraggableIcon.Instance != null)
            DraggableIcon.Instance.UpdateDragPosition(mousePos);
    }

    private void OnSlotDragEnded()
    {
        if (DraggableIcon.Instance != null)
            DraggableIcon.Instance.StopDragging();
    }

    // -------

    private void OnSlotDropped(InventoryModel fromModel, int fromIndex, InventoryModel toModel, int toIndex, PointerEventData.InputButton button)
    {
        if (fromModel == null || toModel == null) return;
        if (fromModel.Slots[fromIndex].IsEmpty || !gameObject.activeInHierarchy) return;

        // Определяем тип переноса
        TransferType transferType = TransferType.All;
        if (button == PointerEventData.InputButton.Right)
            transferType = TransferType.One;
        else if (button == PointerEventData.InputButton.Middle)
            transferType = TransferType.Half;

        // Вызываем один единственный метод. Модель сама разберется!
        fromModel.SwapOrCombineItems(fromIndex, toModel, toIndex, transferType);
    }

    private void OnSlotFastClicked(InventoryModel fromModel, int fromIndex)
    {
        InventoryModel targetModel = GetTransferTarget();

        if (targetModel != null)
            fromModel.FastClickTransfer(fromIndex, targetModel);
    }
}