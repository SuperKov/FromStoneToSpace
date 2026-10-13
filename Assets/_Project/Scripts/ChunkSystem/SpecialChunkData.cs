using UnityEngine;

[CreateAssetMenu(fileName = "New Special Chunk", menuName = "Project/World/Chunks/Special Chunk")]
public class SpecialChunkData : BaseChunkData
{
    [field: Header("Manual Design")]
    [field: SerializeField] public SpecialChunkContainer ChunkPrefab { get; private set; }
    [field: SerializeField] public bool SpawnDynamicResources { get; private set; } = true;

    [field: Header("Spawn Rules")]
    [field: SerializeField] public int MaxPerBiome { get; private set; } = 1;
}