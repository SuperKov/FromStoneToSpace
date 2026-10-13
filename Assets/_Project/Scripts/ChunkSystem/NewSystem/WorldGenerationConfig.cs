using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WorldGenerationConfig", menuName = "Project/World/Generation Config")]
public class WorldGenerationConfig : ScriptableObject
{
    [field: Header("Dimensions")]
    [field: SerializeField] public Vector2Int WorldSizeInChunks { get; private set; } = new(11, 11);
    [field: SerializeField] public int ChunkSizeInTiles { get; private set; } = 5;

    [field: Header("Progression & Economy")]
    [field: SerializeField] public ProgressionConfig Progression { get; private set; }

    [field: Header("Biomes Setup")]
    [field: SerializeField] public BiomeData DefaultStarterBiome { get; private set; }
    [field: SerializeField] public List<BiomeData> AllBiomes { get; private set; }

    [field: Header("Ocean Generation")]
    [field: SerializeField] public TileBase OceanTile { get; private set; }
    [field: Tooltip("За сколько тайлов до края мира шанс воды начинает расти (Island Falloff)")]
    [field: SerializeField] public float OceanFalloffDistance { get; private set; } = 15f;

    [field: Header("Biome Blending")]
    [field: Tooltip("Сила искажения границ (насколько тайлы заходят друг на друга)")]
    [field: SerializeField] public float BiomeWarpStrength { get; private set; } = 3f;
    [field: Tooltip("Масштаб шума искажения")]
    [field: SerializeField] public float BiomeWarpScale { get; private set; } = 0.1f;

    private void OnValidate()
    {
        // Принудительно делаем размер мира нечетным, чтобы всегда был центральный чанк
        if (WorldSizeInChunks.x % 2 == 0) WorldSizeInChunks = new Vector2Int(WorldSizeInChunks.x + 1, WorldSizeInChunks.y);
        if (WorldSizeInChunks.y % 2 == 0) WorldSizeInChunks = new Vector2Int(WorldSizeInChunks.x, WorldSizeInChunks.y + 1);

        // Защита от нулевых и отрицательных значений
        if (WorldSizeInChunks.x < 1) WorldSizeInChunks = new Vector2Int(1, WorldSizeInChunks.y);
        if (WorldSizeInChunks.y < 1) WorldSizeInChunks = new Vector2Int(WorldSizeInChunks.x, 1);
        if (ChunkSizeInTiles < 1) ChunkSizeInTiles = 1;
    }
}
