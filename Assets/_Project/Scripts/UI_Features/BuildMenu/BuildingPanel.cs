using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private BuildingUISlot slotPrefab;

    [Header("Info Panel References")]
    [SerializeField] private RequirementInfoPanel infoPanel;

    // Списки для пулинга объектов
    private List<BuildingUISlot> _spawnedSlots = new();

    // Все доступные на определенный момент игроку здания
    private BuildingData[] _allBuildings;
    private InventoryModel _playerInventory;

    private void Start()
    {
        if (PlayerEntity.Instance != null)
            SetupInventory(PlayerEntity.Instance.Inventory);

        // Временная загрузка всех существующих зданий, позже здания будут открываться
        // с помощью скиллов или исследований
        _allBuildings = Resources.LoadAll<BuildingData>("Buildings");
    }

    private void OnEnable()
    {
        Init();
    }

    public void SetupInventory(InventoryModel inventory)
    {
        _playerInventory = inventory;
    }

    public void Init()
    {
        if (PlayerEntity.Instance != null && _playerInventory == null)
            SetupInventory(PlayerEntity.Instance.Inventory);

        // Временная загрузка всех существующих зданий, позже здания будут открываться
        // с помощью скиллов или исследований
        if (_allBuildings == null)
            _allBuildings = Resources.LoadAll<BuildingData>("Buildings");

        ShowCategory(BuildingData.BuildingCategories.General);
        infoPanel.Hide();
    }

    public void ShowCategory(BuildingData.BuildingCategories category)
    {
        // 1. Фильтруем список зданий (с помощью Linq)
        var filteredBuildings = _allBuildings
            .Where(b => category == BuildingData.BuildingCategories.General || b.Category == category)
            .ToList();

        // 2. Доверяем всю грязную работу помощнику
        UIPoolHelper.UpdatePool(
            filteredBuildings,
            _spawnedSlots,
            slotPrefab,
            slotsContainer,
            (slot, building) => slot.Init(building, OnBuildingSelected, OnBuildingClicked)
        );
    }

    private void OnBuildingSelected(BuildingData data)
    {
        // Передаем NULL вместо спрайта! Инфо-панель сама скроет иконку
        // так же испльуем передачу description по имени "description:"
        infoPanel.Show(data.GetTranslatedName(), data.BuildCost, _playerInventory,
            description: data.GetTranslatedDescription(),
            additionalInfo: data.GetTranslatedCategory());
    }

    private void OnBuildingClicked(BuildingData data)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.CloseAll();

        PlayerEntity.Instance.BuildController.EnterBuildMode(data);
    }
}