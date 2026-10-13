using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIWorkbenchWindow : UIWindow
{
    [Header("References")]
    [SerializeField] private CraftingUISlot slotPrefab;
    [SerializeField] private Transform slotsContainer;

    [Header("Info Panel References")]
    [SerializeField] private RequirementInfoPanel infoPanel;
    [SerializeField] private Button craftButton;

    private List<CraftingUISlot> _spawnedSlots = new();

    private CraftingRecipe[] _currentRecipes;
    private InventoryModel _playerInventory;
    private CraftingRecipe _currentRecipe;

    private void Start()
    {
        craftButton.onClick.AddListener(TryCraft);

        if (PlayerEntity.Instance != null)
            SetupInventory(PlayerEntity.Instance.Inventory);

        WorckbenchStation.CraftStationOpened += OpenCrafting;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        craftButton.onClick.RemoveListener(TryCraft);

        WorckbenchStation.CraftStationOpened -= OpenCrafting;
    }

    // Игрок вызовет это в своем Start()
    public void SetupInventory(InventoryModel inventory)
    {
        _playerInventory = inventory;
    }

    public void OpenCrafting(CraftingRecipe[] recipes)
    {
        _currentRecipes = recipes;
        infoPanel.Hide();
        RefreshUI();

        if (UIManager.Instance != null)
            UIManager.Instance.OpenWindow(this);
    }

    public override void Close()
    {
        base.Close();

        _currentRecipes = null;
        _currentRecipe = null;
    }

    public void SelectRecipe(CraftingRecipe recipe)
    {
        if (_playerInventory == null) return;

        _currentRecipe = recipe;

        RefreshInfoPanel();
    }

    // Этот метод будет вызываться по кнопке UI "Скрафтить"
    public void TryCraft()
    {
        if (_currentRecipe == null || _playerInventory == null) return;

        // 1. Проверяем ресурсы
        if (!_playerInventory.HasItems(_currentRecipe.Ingredients))
        {
            Debug.Log("Недостаточно ресурсов!");
            return;
        }

        // 2. Проверяем, есть ли место в инвентаре для результата!
        if (!_playerInventory.HasSpaceFor(_currentRecipe.Result))
        {
            Debug.Log("Инвентарь полон!");
            return;
        }

        // 3. Забираем ресурсы
        if (_playerInventory.TryRemoveItems(_currentRecipe.Ingredients))
        {
            // 4. Выдаем результат
            _playerInventory.AddItem(_currentRecipe.Result);
            Debug.Log($"Успешно скрафчено: {_currentRecipe.Result.Item.ID} x{_currentRecipe.Result.Amount}");

            // обновляем данные текущего кратфа
            RefreshInfoPanel();
            // обновляем данные всего крафта, чтобы сначала показывались крафты, которые доступны
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        UIPoolHelper.UpdatePool(
                    _currentRecipes,
                    _spawnedSlots,
                    slotPrefab,
                    slotsContainer,
                    // Передаем логику инициализации
                    (slot, recipe) => slot.Init(recipe, onClick: SelectRecipe)
                );
    }

    private void RefreshInfoPanel()
    {
        infoPanel.Show(_currentRecipe.Result.Item.ID,
    _currentRecipe.Ingredients,
    _playerInventory,
    _currentRecipe.Result.Item.Icon);

        craftButton.interactable = _playerInventory.HasItems(_currentRecipe.Ingredients);
    }
}