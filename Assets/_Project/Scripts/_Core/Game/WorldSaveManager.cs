using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System;

public class SaveFileDescriptor
{
    public WorldSaveHeader Header;
    public string RawJson;
    public string FilePath;
}

public class WorldSaveManager : MonoBehaviour, ISaveable
{
    [Serializable]
    private struct WorldEntitiesState
    {
        public List<SavedBuildingData> Buildings;
        public List<SavedChunkData> Chunks;
        public List<SavedChunkBuyerData> ChunkBuyers;
        public List<SavedDroppedItemData> DroppedItems;
        public List<string> UnlockedItems;
        public float WorldTime;
    }

    public static WorldSaveManager Instance { get; private set; }
    public string SaveID => "WorldSave";

    // Публичный доступ к разблокированным предметам для ProgressionConfig
    public List<ItemData> UnlockedItems { get; private set; } = new();
    public HashSet<ItemData> UnlockedItemsHash { get; private set; } = new();

    private Dictionary<string, PlayerBuilding> _activeBuildings = new();
    private Dictionary<Vector2Int, ItemStack[]> _activeChunkBuyers = new();
    private Dictionary<Vector2Int, ChunkData> _activeChunks = new();
    private List<DroppedItem> _activeDroppedItems = new();

    private void Awake() => Instance = this;

    public void RegisterBuilding(PlayerBuilding building) => _activeBuildings[building.InstanceID] = building;
    public void UnregisterBuilding(string instanceId) => _activeBuildings.Remove(instanceId);

    public void RegisterChunk(ChunkData chunk) => _activeChunks[chunk.GridIndex] = chunk;
    public void RegisterChunkBuyer(ChunkBuyerInteractable bayer) => _activeChunkBuyers[bayer.TargetGridIndex] = bayer.ItemsToUnlock;
    public void UnregisterChunkBuyer(Vector2Int gridId) => _activeChunkBuyers.Remove(gridId);

    public void RegisterDroppedItem(DroppedItem item) => _activeDroppedItems.Add(item);
    public void UnregisterDroppedItem(DroppedItem item) => _activeDroppedItems.Remove(item);

    public void AddUnlockedItem(ItemData item)
    {
        if (UnlockedItemsHash.Add(item))
            UnlockedItems.Add(item);
    }

    public string SaveState()
    {
        WorldEntitiesState state = new()
        {
            Buildings = new(),
            Chunks = new(),
            ChunkBuyers = new(),
            DroppedItems = new(),
            UnlockedItems = new(),
            WorldTime = 1
        };

        foreach (var building in _activeBuildings.Values)
            state.Buildings.Add(new SavedBuildingData { InstanceID = building.InstanceID, BuildingDataID = building.Data.ID, Position = new(building.transform.position), CustomState = building.GetCustomState() });

        foreach (var chunk in _activeChunks.Values)
        {
            SavedChunkData chunkData = new() { GridIndex = new(chunk.GridIndex), IsUnlocked = chunk.IsUnlocked, ActiveResources = new(), DestroyedObstacles = new() };
            foreach (var resource in chunk.SpawnedResources) chunkData.ActiveResources.Add(new(resource.Value, new(resource.Key)));
            foreach (var obstacle in chunk.DestroyedObstacles) chunkData.DestroyedObstacles.Add(new(obstacle));
            state.Chunks.Add(chunkData);
        }

        foreach (var buyer in _activeChunkBuyers)
        {
            SavedChunkBuyerData data = new() { GridIndex = new(buyer.Key), Cost = new() };
            foreach (ItemStack item in buyer.Value) data.Cost.Add(new(item.Item.ID, item.Amount));
            state.ChunkBuyers.Add(data);
        }

        foreach (var item in _activeDroppedItems)
            state.DroppedItems.Add(new SavedDroppedItemData(item.Data.Item.ID, item.Data.Amount, new(item.transform.position)));

        foreach (var item in UnlockedItems)
            state.UnlockedItems.Add(item.ID);

        return JsonConvert.SerializeObject(state);
    }

    public void LoadState(string stateJson)
    {
        foreach (var building in _activeBuildings.Values) Destroy(building.gameObject);

        _activeBuildings.Clear(); _activeChunks.Clear(); _activeChunkBuyers.Clear(); UnlockedItems.Clear(); UnlockedItemsHash.Clear();

        WorldEntitiesState state = JsonConvert.DeserializeObject<WorldEntitiesState>(stateJson);

        if (state.UnlockedItems != null)
        {
            foreach (var id in state.UnlockedItems)
            {
                ItemData config = ItemData.GetItemByID(id);
                if (config != null) AddUnlockedItem(config);
            }
        }

        if (state.Buildings != null)
        {
            BuildingData[] allConfigs = Resources.LoadAll<BuildingData>("Buildings");
            foreach (SavedBuildingData data in state.Buildings)
            {
                BuildingData config = allConfigs.FirstOrDefault(b => b.ID == data.BuildingDataID);
                if (config != null && config.Prefab != null)
                {
                    PlayerBuilding building = Instantiate(config.Prefab, data.Position.ToVector2(), Quaternion.identity);
                    building.Init(data.InstanceID);
                    building.LoadCustomState(data.CustomState);
                    RegisterBuilding(building);
                }
            }
        }

        // БОЛЬШЕ НЕТ ChunkManager.Instance.InitializeWorld() ЗДЕСЬ!
        if (ChunkManager.Instance != null && state.Chunks != null)
        {
            foreach (SavedChunkData savedChunk in state.Chunks)
            {
                ChunkManager.Instance.RestoreChunkFromSave(savedChunk);
                ChunkData restoredChunk = ChunkManager.Instance.GetChunkDataAtGridIndex(savedChunk.GridIndex.ToVector2Int());
                if (restoredChunk != null) RegisterChunk(restoredChunk);
            }
        }

        if (ChunkManager.Instance != null && state.ChunkBuyers != null)
        {
            foreach (var buyer in state.ChunkBuyers)
            {
                ItemStack[] cost = new ItemStack[buyer.Cost.Count];
                for (int i = 0; i < buyer.Cost.Count; i++)
                {
                    ItemData itemData = ItemData.GetItemByID(buyer.Cost[i].ItemID);
                    if (itemData != null) cost[i] = new(itemData, buyer.Cost[i].Amount);
                }
                ChunkManager.Instance.CreateBuyer(buyer.GridIndex.ToVector2Int(), cost);
            }
        }

        if (DroppedItemsManager.Instance != null && state.DroppedItems != null)
        {
            foreach (var data in state.DroppedItems)
            {
                ItemData config = ItemData.GetItemByID(data.ItemID);
                if (config != null) DroppedItemsManager.Instance.SpawnDroppedItem(new(config, data.Amount), data.Position.ToVector2(), true);
            }
        }
    }
}