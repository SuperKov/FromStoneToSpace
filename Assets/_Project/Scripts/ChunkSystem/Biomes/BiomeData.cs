using UnityEngine;

[CreateAssetMenu(fileName = "NewBiome", menuName = "Project/World/Biome")]
public class BiomeData : ScriptableObject
{
    [field: Header("General")]
    [field: SerializeField] public string ID { get; private set; } = "New Biome";

    [field: Header("Spawn Rules")]
    [field: SerializeField] public int MinRadius { get; private set; } = 1;
    [field: SerializeField] public int MaxRadius { get; private set; } = 5;
    [field: SerializeField, Range(0, 100)] public int SpawnWeight { get; private set; } = 50;

    [field: Header("Chunk Tamplates")]
    [field: SerializeField] public ProceduralChunkData ProceduralChunk { get; private set; }
    [field: SerializeField] public SpecialChunkData[] SpecialChunks { get; private set; }
}
