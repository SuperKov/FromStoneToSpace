using Newtonsoft.Json;
using UnityEngine;
using System;
using static IDamagable;

public class PlayerEntity : Entity, ISaveable
{
    // Структура чистых данных
    [Serializable]
    private struct PlayerSaveState
    {
        public float Health;
        public Vector2Data Position;
        public string InventoryState;
        public string EquipmentState;
    }

    public static PlayerEntity Instance { get; private set; }


    [Header("Inventory")]
    [SerializeField] private InventoryModel inventory;
    [SerializeField] private EquipmentModel equipment;
    [SerializeField] private int maxHotbarSize = 10;

    [Header("Controller")]
    [SerializeField] private PlayerController controller;
    [SerializeField] private PlayerBuildController buildController;

    public PlayerBuildController BuildController => buildController;
    public PlayerController Controller => controller;
    public InventoryModel Inventory => inventory;
    public EquipmentModel Equipment => equipment;
    // Это свойство всегда будет возвращать ПРАВИЛЬНЫЙ текущий размер хотбара
    public int CurrentHotbarSize => Mathf.Min(Inventory.Slots.Length, maxHotbarSize);
    public string SaveID => "Player_Main";

    public InventoryModel CurrentStorage { get; private set; }
    public int SelectedSlotIndex { get; private set; } = 0;
    public int TotalArmor { get; private set; } = 0;
    public int Level { get; private set; } = 0;

    // Ивент, на который будут подписываться UI, Руки и Боевка при прокрутке хотбара
    public event Action<int> SelectedSlotChanged;
    public event Action<int> ArmorChanged;

    protected new void Awake()
    {
        base.Awake();
        Instance = this;
    }

    private void Start()
    {
        InputManager.Instance.UnlockPlayerActions(true);

        SetSelectedSlot(0);
    }

    private void OnEnable()
    {
        inventory.OnSlotUpdated += OnInventorySlotUpdated;
        equipment.OnSlotUpdated += OnRecalculateArmor;
        controller.NumberPressed += OnNumberPressed;
        controller.HotbarScrolled += OnHotbarScroll;
    }

    private void OnDisable()
    {
        inventory.OnSlotUpdated -= OnInventorySlotUpdated;
        equipment.OnSlotUpdated -= OnRecalculateArmor;
        controller.NumberPressed -= OnNumberPressed;
        controller.HotbarScrolled -= OnHotbarScroll;
    }

    // Сохраняем состояние игрока
    public string SaveState()
    {
        PlayerSaveState state = new()
        {
            Health = Health,
            Position = new(transform.position),
            InventoryState = inventory.CaptureInventoryState(),
            EquipmentState = equipment.CaptureInventoryState()
        };

        // Используем JsonConvert
        return JsonConvert.SerializeObject(state);
    }
  
    // Загружаем состояние игрока
    public void LoadState(string stateJson)
    {
        // Распаковываем
        PlayerSaveState state = JsonConvert.DeserializeObject<PlayerSaveState>(stateJson);

        Health = state.Health;

        // опционально: чтобы физика не сошла с ума, можно отключать контроллер на кадр перемещения
        transform.position = state.Position.ToVector2();

        // Загружаем все нужные инвентари
        inventory.RestoreInventoryState(state.InventoryState);
        equipment.RestoreInventoryState(state.EquipmentState);
    }

    // (вызвать экран GameOver вместо отключения объекта)
    public override void Die()
    {
        Debug.Log("Игрок погиб! Шучу ему похуй");
        Health = maxHealth;
        // base.Die(); // Не отключаем объект сразу, чтобы игра не сломалась
    }

    // переопределяем новый метод расчета:
    protected override float CalculateFinalDamage(DamageInfo info)
    {
        // Там автоматически применятся модификаторы Топоров/Кирок (если враг ударит киркой) 
        // и сработает базовый physicDamageResist (если выпил зелье на +20% защиты).
        float damage = base.CalculateFinalDamage(info);

        // ЛОГИКА ИГРОКА: MOBA-броня
        // Накидываем её только поверх физического урона
        if (info.Type == DamageTypes.Physic)
            damage *= ArmorBalance / (ArmorBalance + TotalArmor);

        // 3. всегда должен получать хотя бы 1 единицу урона
        damage = Mathf.Max(1f, damage);

        return damage;
    }

    private void SetSelectedSlot(int index)
    {
        SelectedSlotIndex = index;
        SelectedSlotChanged?.Invoke(SelectedSlotIndex);
    }

    private void OnHotbarScroll(int delta)
    {
        int currentSize = CurrentHotbarSize;
        if (currentSize == 0) return;

        int newIndex = SelectedSlotIndex - delta;

        // Зацикливание математически
        if (newIndex < 0) newIndex = currentSize - 1;
        if (newIndex >= currentSize) newIndex = 0;

        SetSelectedSlot(newIndex);
    }

    private void OnNumberPressed(int id)
    {
        if (CurrentHotbarSize == 0 || id >= CurrentHotbarSize) return;

        SetSelectedSlot(id);
    }

    private void OnInventorySlotUpdated(int updatedSlotIndex)
    {
        // При изменении предмета в каком то слоте смотрим: если в слоте
        // новый предмет, то регистрируем его, если нет - то скип
        ItemData item = Inventory.Slots[updatedSlotIndex].Item;
        if (item != null && WorldSaveManager.Instance != null)
            WorldSaveManager.Instance.AddUnlockedItem(item);

        // Если изменилось содержимое того слота, который мы держим прямо сейчас
        // Принудительно дергаем ивент
        if (updatedSlotIndex == SelectedSlotIndex)
            SelectedSlotChanged?.Invoke(SelectedSlotIndex);
    }

    private void OnRecalculateArmor(int slotIndex)
    {
        TotalArmor = 0;

        // Пробегаемся по всем слотам брони и суммируем защиту
        foreach (ItemStack slot in equipment.Slots)
            if (!slot.IsEmpty && slot.Item is ArmorItemData armor)
                TotalArmor += armor.Protection;

        ArmorChanged?.Invoke(TotalArmor);
    }
}