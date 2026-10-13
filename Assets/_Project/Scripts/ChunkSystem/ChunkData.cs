using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResourceSaveData
{
    public string ResourceID;
    public Vector2IntData TilePosition;

    public ResourceSaveData(string resourceID, Vector2IntData tilePosition)
    {
        ResourceID = resourceID;
        TilePosition = tilePosition;
    }
}

[System.Serializable]
public struct SavedChunkData
{
    public Vector2IntData GridIndex;
    public bool IsUnlocked;
    public List<ResourceSaveData> ActiveResources;
    public List<Vector2IntData> DestroyedObstacles; // Храним разрушенные препятствия

    public SavedChunkData(Vector2IntData gridIndex, bool isUnlocked)
    {
        GridIndex = gridIndex;
        IsUnlocked = isUnlocked;
        ActiveResources = new();
        DestroyedObstacles = new();
    }
}

public class ChunkData
{
    public Vector2Int GridIndex { get; private set; }
    public BiomeData AssignedBiome { get; private set; }
    public BaseChunkData ActiveChunk { get; private set; }
    public int LocalChunkSeed { get; private set; }
    public bool IsUnlocked { get; private set; }
    public double NextSpawnTime { get; set; }

    public Dictionary<Vector2Int, string> SpawnedResources { get; private set; } = new();

    // Множество для быстрого поиска разрушенных препятствий (хранит локальные координаты чанка)
    public HashSet<Vector2Int> DestroyedObstacles { get; private set; } = new();

    public ChunkData(Vector2Int index, BiomeData biome, BaseChunkData activeChunk, int seed)
    {
        GridIndex = index;
        AssignedBiome = biome;
        ActiveChunk = activeChunk;
        LocalChunkSeed = seed;
        IsUnlocked = false;
        NextSpawnTime = 0.0;
    }

    public void MarkAsUnlocked() => IsUnlocked = true;
    public void SetChunk(BaseChunkData newChunkData) => ActiveChunk = newChunkData;

    public int GetMaxAllowedResources(int chunkSizeInTiles)
    {
        float totalArea = chunkSizeInTiles * chunkSizeInTiles;
        return Mathf.FloorToInt(totalArea * ActiveChunk.MaxResourceDensity);
    }

    public void OnChunkTick(double currentTime)
    {
        if (ActiveChunk.Resources == null || ActiveChunk.Resources.Length == 0) return;

        int maxResources = GetMaxAllowedResources(ChunkManager.Instance.Config.ChunkSizeInTiles);
        if (SpawnedResources.Count >= maxResources) return;

        if (currentTime >= NextSpawnTime)
        {
            ChunkManager.Instance.TrySpawnRandomResourceInChunk(this);
            NextSpawnTime = currentTime + ActiveChunk.SpawnInterval;
        }
    }
}