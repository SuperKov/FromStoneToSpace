using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class UISlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    // ссылка на родительскую модель
    public InventoryModel ParentModel { get; private set; }
    public int SlotIndex { get; private set; }

    [SerializeField] private Image itemIcon; // Картинка предмета в слоте
    [SerializeField] private TextMeshProUGUI amountText; // Колво в стаке

    // События для Presenter'а
    public event Action<InventoryModel, int, InventoryModel, int, PointerEventData.InputButton> SlotDropped;
    public event Action<InventoryModel, int> SlotFastClicked;
    public event Action<Sprite, Vector2> DragBegun;
    public event Action<Vector2> Dragging;
    public event Action DragEnded;

    private bool _isEmpty = true;
    private InventoryPanel _inventoryPanel;

    private void OnDisable()
    {
        if (itemIcon != null)
        {
            // Принудительно возвращаем иконке цвет
            itemIcon.color = Color.white;
        }

        // На всякий случай принудительно сообщаем Презентеру, 
        // что перетаскивание прервано, чтобы очистить временные оверлеи
        DragEnded?.Invoke();
    }

    public void Initialize(int index, InventoryModel model, InventoryPanel presenter)
    {
        SlotIndex = index;
        ParentModel = model;
        _inventoryPanel = presenter;
    }

    public void UpdateVisuals(ItemStack item)
    {
        if (item.IsEmpty)
        {
            itemIcon.gameObject.SetActive(false);
            amountText.gameObject.SetActive(false);

            _isEmpty = true;
        }
        else
        {
            itemIcon.sprite = item.Item.Icon;
            itemIcon.gameObject.SetActive(true);

            if (item.Amount > 1)
            {
                amountText.text = "x" + item.Amount;
                amountText.gameObject.SetActive(true);
            }
            else
                amountText.gameObject.SetActive(false);

            _isEmpty = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Если слот пустой, то отмена
        if (_isEmpty || _inventoryPanel == null) return;

        // Опционально: делаем иконку в самом слоте полупрозрачной
        itemIcon.color = Color.gray5;
        DragBegun?.Invoke(itemIcon.sprite, eventData.position);

        // Не забываем отменить выделение при переносе предмета
        itemIcon.transform.localScale = Vector2.one;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isEmpty || _inventoryPanel == null) return;

        Dragging?.Invoke(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemIcon.color = Color.white; // Возвращаем непрозрачность
        DragEnded?.Invoke();
    }

    // Вызывается тогда, когда на слот ОТПУСКАЕТСЯ что-то, а не тогда, когда ЕГО отпускают!!
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent(out UISlot fromSlot))
        {
            SlotDropped?.Invoke(fromSlot.ParentModel, fromSlot.SlotIndex, ParentModel, SlotIndex, eventData.button);
            
            ItemData item = ParentModel.Slots[SlotIndex].Item;
            if (item != null)
                ToolTipManager.Instance.Show(item, _inventoryPanel.gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_inventoryPanel == null) return;

        ItemData item = ParentModel.Slots[SlotIndex].Item;
        if (item == null) return;

        itemIcon.transform.localScale = new(1.2f, 1.2f);

        ToolTipManager.Instance.Show(item, _inventoryPanel.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemIcon.transform.localScale = Vector2.one;
        ToolTipManager.Instance.Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isEmpty || InputManager.Instance == null || _inventoryPanel == null) return;

        // Прячем тултип, чтобы не мешал
        ToolTipManager.Instance.Hide();

        // Проверяем, нажат ли Shift
        bool isShiftPressed = false;
        if (InputManager.Instance != null)
            isShiftPressed = InputManager.Instance.Actions.UI.FastDrag.IsPressed();

        if (eventData.button == PointerEventData.InputButton.Left && isShiftPressed)
            SlotFastClicked?.Invoke(ParentModel, SlotIndex);
    }
}