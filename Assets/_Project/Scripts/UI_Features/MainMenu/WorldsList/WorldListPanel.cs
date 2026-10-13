using System.Collections.Generic;
using UnityEngine;

public class WorldListPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private WorldUISlot slotPrefab;
    [SerializeField] private Transform slotsContainer;

    private List<WorldUISlot> _spawnedSlots = new(); // нужно для пула слотов миров

    private void OnEnable()
    {
        RefreshWorldList();
    }

    public void RefreshWorldList()
    {
        // 1. Получаем список всех дескрипторов сохранений через SaveSystem
        List<SaveFileDescriptor> allSaves = new();
        allSaves.AddRange(SaveSystem.GetAllSavesDescriptors());

        // 2. Используем пул для синхронизации UI
        UIPoolHelper.UpdatePool(
            dataList: allSaves,
            pool: _spawnedSlots,
            prefab: slotPrefab,
            container: slotsContainer,
            setup: (slot, data) => slot.Init(data, RefreshWorldList)
        );
    }
}