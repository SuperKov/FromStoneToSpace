using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    [Header("Generation System")]
    [SerializeField] private WorldGenerationConfig worldConfig;
    [SerializeField] private WorldRenderer worldRenderer;

    [Header("Interactions & Physics")]
    [Tooltip("Перетащи сюда объект Grid со сцены (на котором висят Tilemap)")]
    [SerializeField] private Grid worldGrid; // <-- НОВОЕ ПОЛЕ ДЛЯ ПРАВИЛЬНОЙ СЕТКИ
    [SerializeField] private ChunkBuyerInteractable buyerPrefab;
    [SerializeField] private LayerMask spawnObstacleLayer;

    public WorldGenerationConfig Config => worldConfig;
    public Vector2Int CenterGrid => _centerGridIndex;
    public bool WorldIsInitialized { get; private set; }
    public List<ChunkData> UnlockedChunks { get; private set; } = new();

    public event Action<double> OnWorldTick;

    private Dictionary<Vector2Int, ChunkBuyerInteractable> _activeBuyers = new();
    private WorldMapData _worldMapData;
    private ChunkData[,] _worldGridData;
    private Vector2Int _centerGridIndex;
    private Transform _bayersContainer;
    private int _worldSeed;

    private void Awake() => Instance = this;

    public void InitializeWorld(int seed)
    {
        _worldSeed = seed;
        _centerGridIndex = new Vector2Int(worldConfig.WorldSizeInChunks.x / 2, worldConfig.WorldSizeInChunks.y / 2);

        WorldGenerator generator = new WorldGenerator(worldConfig, _worldSeed);
        _worldMapData = generator.GenerateWorld();
        _worldGridData = _worldMapData.Chunks;

        RegisterAllResourcesToPool();
        StartCoroutine(LivingWorldTickRoutine());

        WorldIsInitialized = true;
    }

    public void UnlockChunk(Vector2Int gridIndex)
    {
        ChunkData data = _worldGridData[gridIndex.x, gridIndex.y];
        if (data == null || data.IsUnlocked) return;

        data.MarkAsUnlocked();
        data.NextSpawnTime = Time.timeAsDouble + data.ActiveChunk.SpawnInterval;
        OnWorldTick += data.OnChunkTick;
        UnlockedChunks.Add(data);

        if (WorldSaveManager.Instance != null)
            WorldSaveManager.Instance.RegisterChunk(data);

        if (_activeBuyers.TryGetValue(gridIndex, out ChunkBuyerInteractable buyer))
        {
            if (WorldSaveManager.Instance != null)
                WorldSaveManager.Instance.UnregisterChunkBuyer(gridIndex);

            Destroy(buyer.gameObject);
            _activeBuyers.Remove(gridIndex);
        }

        worldRenderer.DrawChunk(data, _worldMapData, worldConfig, _centerGridIndex);

        SpawnNeighborBuyers(gridIndex);
    }

    public void RestoreChunkFromSave(SavedChunkData savedData)
    {
        Vector2Int gridIndex = savedData.GridIndex.ToVector2Int();
        ChunkData data = _worldGridData[gridIndex.x, gridIndex.y];
        if (data == null || data.IsUnlocked) return;

        data.DestroyedObstacles.Clear();
        if (savedData.DestroyedObstacles != null)
        {
            foreach (var obsData in savedData.DestroyedObstacles)
                data.DestroyedObstacles.Add(obsData.ToVector2Int());
        }

        data.MarkAsUnlocked();
        data.NextSpawnTime = Time.timeAsDouble + data.ActiveChunk.SpawnInterval;
        OnWorldTick += data.OnChunkTick;
        UnlockedChunks.Add(data);

        if (_activeBuyers.TryGetValue(gridIndex, out ChunkBuyerInteractable buyer))
        {
            Destroy(buyer.gameObject);
            _activeBuyers.Remove(gridIndex);
        }

        worldRenderer.DrawChunk(data, _worldMapData, worldConfig, _centerGridIndex);

        if (savedData.ActiveResources != null)
        {
            foreach (var resData in savedData.ActiveResources)
                ForceSpawnResource(data, resData.ResourceID, resData.TilePosition.ToVector2Int());
        }
    }

    public void CreateBuyer(Vector2Int gridIndex, ItemStack[] cost = null)
    {
        if (buyerPrefab == null || worldGrid == null) return;

        Vector2Int worldChunkCoord = gridIndex - _centerGridIndex;
        int tileSize = worldConfig.ChunkSizeInTiles;

        int centerTileX = worldChunkCoord.x * tileSize;
        int centerTileY = worldChunkCoord.y * tileSize;

        if (_bayersContainer == null)
        {
            GameObject container = new() { isStatic = true, name = "BayersContainer" };
            _bayersContainer = Instantiate(container, transform).transform;
        }

        // ПРАВИЛЬНОЕ получение физических координат центра 2x2 тайла из Grid
        Vector3 worldPos = worldGrid.GetCellCenterWorld(new Vector3Int(centerTileX, centerTileY, 0));

        ChunkBuyerInteractable buyerObj = Instantiate(buyerPrefab, _bayersContainer);
        buyerObj.transform.SetPositionAndRotation(worldPos, Quaternion.identity);

        ItemStack[] finalCost = cost;

        if (finalCost == null)
        {
            int playerLevel = PlayerEntity.Instance != null ? PlayerEntity.Instance.Level : 0;
            var unlockedItems = WorldSaveManager.Instance != null ? WorldSaveManager.Instance.UnlockedItemsHash : new HashSet<ItemData>();

            ProgressionStage stage = worldConfig.Progression.GetCurrentStage(UnlockedChunks.Count, playerLevel, unlockedItems);

            if (stage != null)
                finalCost = stage.GetRandomItems(UnlockedChunks.Count - 1);
            else
                finalCost = new ItemStack[0];
        }

        buyerObj.Init(gridIndex, finalCost);
        _activeBuyers.Add(gridIndex, buyerObj);

        if (WorldSaveManager.Instance != null)
            WorldSaveManager.Instance.RegisterChunkBuyer(buyerObj);
    }

    private void SpawnNeighborBuyers(Vector2Int centerGridIndex)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int neighbor = centerGridIndex + dir;

            if (neighbor.x < 0 || neighbor.x >= worldConfig.WorldSizeInChunks.x ||
                neighbor.y < 0 || neighbor.y >= worldConfig.WorldSizeInChunks.y) continue;

            if (_worldGridData[neighbor.x, neighbor.y].IsUnlocked || _activeBuyers.ContainsKey(neighbor)) continue;

            CreateBuyer(neighbor);
        }
    }

    private IEnumerator LivingWorldTickRoutine()
    {
        WaitForSeconds wait = new(0.5f);
        while (true)
        {
            yield return wait;
            OnWorldTick?.Invoke(Time.timeAsDouble);
        }
    }

    private void RegisterAllResourcesToPool()
    {
        if (WorldResourcePoolManager.Instance == null || worldConfig == null) return;

        if (worldConfig.DefaultStarterBiome != null)
            RegisterBiomeResources(worldConfig.DefaultStarterBiome);

        if (worldConfig.AllBiomes != null)
        {
            foreach (var biome in worldConfig.AllBiomes)
                RegisterBiomeResources(biome);
        }
    }

    private void RegisterBiomeResources(BiomeData biome)
    {
        if (biome.ProceduralChunk?.Resources != null)
            foreach (var res in biome.ProceduralChunk.Resources)
                if (res.Prefab != null) WorldResourcePoolManager.Instance.RegisterPrefab(res.Prefab.ID, res.Prefab.gameObject);

        if (biome.SpecialChunks != null)
            foreach (var special in biome.SpecialChunks)
                if (special.Resources != null)
                    foreach (var res in special.Resources)
                        if (res.Prefab != null) WorldResourcePoolManager.Instance.RegisterPrefab(res.Prefab.ID, res.Prefab.gameObject);
    }

    // ====================================================================================
    // СИСТЕМА СПАВНА РЕСУРСОВ
    // ====================================================================================

    public void TrySpawnRandomResourceInChunk(ChunkData chunk)
    {
        if (chunk.ActiveChunk.Resources == null || chunk.ActiveChunk.Resources.Length == 0 || worldGrid == null) return;

        int tileSize = worldConfig.ChunkSizeInTiles;
        int halfSize = tileSize / 2;
        Vector2Int worldChunkCoord = chunk.GridIndex - _centerGridIndex;

        int localX = UnityEngine.Random.Range(0, tileSize);
        int localY = UnityEngine.Random.Range(0, tileSize);

        int arrayX = chunk.GridIndex.x * tileSize + localX;
        int arrayY = chunk.GridIndex.y * tileSize + localY;

        TileBase ground = _worldMapData.GlobalGroundMap[arrayX, arrayY];
        if (ground == null || ground == worldConfig.OceanTile) return;

        TileBase obstacle = _worldMapData.GlobalObstaclesMap[arrayX, arrayY];
        Vector2Int localPos = new Vector2Int(localX, localY);
        if (obstacle != null && !chunk.DestroyedObstacles.Contains(localPos)) return;

        int startWorldX = worldChunkCoord.x * tileSize - halfSize;
        int startWorldY = worldChunkCoord.y * tileSize - halfSize;
        Vector3Int globalTilePos = new Vector3Int(startWorldX + localX, startWorldY + localY, 0);

        // ПРАВИЛЬНЫЙ расчет мирового центра для 2x2 тайлов
        Vector3 worldPos = worldGrid.GetCellCenterWorld(globalTilePos);

        Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.4f, spawnObstacleLayer);
        if (hit != null) return;

        int totalTickets = 0;
        foreach (var res in chunk.ActiveChunk.Resources) totalTickets += res.Tickets;

        int randomVal = UnityEngine.Random.Range(0, totalTickets);
        int currentWeight = 0;
        ResourceSpawnChance selectedResource = default;

        foreach (var res in chunk.ActiveChunk.Resources)
        {
            currentWeight += res.Tickets;
            if (randomVal < currentWeight)
            {
                selectedResource = res;
                break;
            }
        }

        if (selectedResource.Prefab == null) return;

        SpawnResourceInstance(chunk, selectedResource.Prefab.ID, globalTilePos, worldPos);
    }

    public void ForceSpawnResource(ChunkData chunk, string resourceID, Vector2Int globalTilePos)
    {
        if (worldGrid == null) return;

        Vector3Int targetTilePos = new Vector3Int(globalTilePos.x, globalTilePos.y, 0);

        // ПРАВИЛЬНЫЙ расчет мирового центра из сохранений
        Vector3 worldPos = worldGrid.GetCellCenterWorld(targetTilePos);

        SpawnResourceInstance(chunk, resourceID, targetTilePos, worldPos);
    }

    private void SpawnResourceInstance(ChunkData chunk, string resourceID, Vector3Int tilePos, Vector3 worldPos)
    {
        GameObject resourceObj = WorldResourcePoolManager.Instance.GetResource(resourceID, worldPos);

        if (resourceObj == null) return;

        Vector2Int tilePosVector2 = new Vector2Int(tilePos.x, tilePos.y);

        if (!chunk.SpawnedResources.ContainsKey(tilePosVector2))
        {
            chunk.SpawnedResources.Add(tilePosVector2, resourceID);
        }

        if (resourceObj.TryGetComponent(out IChunkResource chunkResource))
        {
            chunkResource.Init(tilePosVector2, resourceID, chunk);
        }
    }

    public void RemoveResourceFromWorld(Vector2Int tilePos, string resourceID, ChunkData chunk, GameObject obj)
    {
        chunk.SpawnedResources.Remove(tilePos);

        if (WorldResourcePoolManager.Instance != null)
            WorldResourcePoolManager.Instance.ReturnToPool(resourceID, obj);
    }

    public void RemoveObstacleFromWorld(Vector3 worldPosition)
    {
        ChunkData chunk = GetChunkDataAtWorldPosition(worldPosition);
        if (chunk == null || worldGrid == null) return;

        int tileSize = worldConfig.ChunkSizeInTiles;
        int halfSize = tileSize / 2;

        // ПРАВИЛЬНАЯ конвертация из мировых координат в тайловые с учетом масштаба 2x2
        Vector3Int cellPos = worldGrid.WorldToCell(worldPosition);

        Vector2Int worldChunkCoord = chunk.GridIndex - _centerGridIndex;
        int startWorldX = worldChunkCoord.x * tileSize - halfSize;
        int startWorldY = worldChunkCoord.y * tileSize - halfSize;

        Vector2Int localGridPos = new Vector2Int(cellPos.x - startWorldX, cellPos.y - startWorldY);

        chunk.DestroyedObstacles.Add(localGridPos);
    }

    // ====================================================================================

    public ChunkData GetChunkDataAtGridIndex(Vector2Int chunkCoords)
    {
        if (chunkCoords.x < 0 || chunkCoords.x >= worldConfig.WorldSizeInChunks.x ||
            chunkCoords.y < 0 || chunkCoords.y >= worldConfig.WorldSizeInChunks.y)
        {
            return null;
        }

        return _worldGridData[chunkCoords.x, chunkCoords.y];
    }

    public BiomeData GetBiomeAtWorldPosition(Vector3 worldPosition)
    {
        ChunkData data = GetChunkDataAtWorldPosition(worldPosition);
        return data?.AssignedBiome;
    }

    public ChunkData GetChunkDataAtWorldPosition(Vector3 worldPosition)
    {
        if (_worldGridData == null || worldConfig == null || worldGrid == null) return null;

        int tileSize = worldConfig.ChunkSizeInTiles;
        int halfSize = tileSize / 2;

        // ПРАВИЛЬНАЯ конвертация мировых координат (решает проблему любого масштаба)
        Vector3Int cellPos = worldGrid.WorldToCell(worldPosition);

        // Используем Float Division, чтобы координаты в минусе (-1, -2) округлялись правильно
        int worldChunkX = Mathf.FloorToInt((cellPos.x + halfSize) / (float)tileSize);
        int worldChunkY = Mathf.FloorToInt((cellPos.y + halfSize) / (float)tileSize);

        Vector2Int gridIndex = new Vector2Int(worldChunkX, worldChunkY) + _centerGridIndex;

        return GetChunkDataAtGridIndex(gridIndex);
    }

    public void UnlockAllChunks()
    {
        if (!WorldIsInitialized || _worldGridData == null)
        {
            Debug.LogWarning("[ChunkManager] Невозможно разблокировать мир: он еще не инициализирован.");
            return;
        }

        int width = worldConfig.WorldSizeInChunks.x;
        int height = worldConfig.WorldSizeInChunks.y;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridIndex = new Vector2Int(x, y);

                if (!_worldGridData[x, y].IsUnlocked)
                {
                    UnlockChunk(gridIndex);
                }
            }
        }
    }
}