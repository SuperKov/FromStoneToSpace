using UnityEngine;

public class StorageInventoryPanel : InventoryPanel
{
    [SerializeField] private InventoryPanel backpack;

    [Header("Menu Integration")]
    [Tooltip("Ссылка на главное окно, в котором находится инвентарь")]
    [SerializeField] private UIWindow mainMenuWindow;

    [Tooltip("Ссылка на контроллер вкладок главного меню")]
    [SerializeField] private UITabController tabController;

    [Tooltip("Индекс вкладки Инвентаря (обычно 0)")]
    [SerializeField] private int inventoryTabIndex = 0;

    private void Awake()
    {
        // 1. Подписываемся на глобальное событие открытия сундука прямо в Awake.
        // Это важно, так как сам объект хранилища по умолчанию будет выключен (SetActive(false)),
        // и OnEnable не сработает!
        ChestInteractable.ChestOpened += OnChestOpened;

        // 2. Подписываемся на событие ЗАКРЫТИЯ главного окна
        if (mainMenuWindow != null)
            mainMenuWindow.OnWindowClosed.AddListener(HideStorage);

        // При старте игры хранилище должно быть скрыто
        HideStorage();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Обязательно отписываемся от всего при удалении объекта
        ChestInteractable.ChestOpened -= OnChestOpened;

        if (mainMenuWindow != null)
            mainMenuWindow.OnWindowClosed.RemoveListener(HideStorage);
    }

    // Этот метод вызывается, когда игрок кликает по сундуку в мире
    private void OnChestOpened(InventoryModel storageModel)
    {
        // 1. Настраиваем данные
        SetupModel(storageModel);

        // 2. Включаем САМУ ВИЗУАЛЬНУЮ ПАНЕЛЬ хранилища
        gameObject.SetActive(true);

        // 3. Командуем UIManager'у открыть Главное Меню (оно запишется в стек!)
        if (UIManager.Instance != null && mainMenuWindow != null)
            UIManager.Instance.OpenWindow(mainMenuWindow);

        // 4. Переключаем вкладку на Инвентарь
        if (tabController != null)
            tabController.OpenTab(inventoryTabIndex);

        RefreshUI();
    }

    // Этот метод вызывается автоматически, когда Главное Меню закрывается (по ESC или I)
    private void HideStorage()
    {
        gameObject.SetActive(false);
    }

    protected override InventoryModel GetTransferTarget()
    {
        if (backpack != null && backpack.ActiveModel != null)
            return backpack.ActiveModel;

        return null;
    }
}