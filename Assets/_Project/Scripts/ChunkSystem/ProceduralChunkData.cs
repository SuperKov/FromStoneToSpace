using UnityEngine.Tilemaps;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chunk Biome", menuName = "Project/World/Chunks/Procedural Chunk")]
public class ProceduralChunkData : BaseChunkData
{
    [field: Header("Procedural Rules")]
    [field: SerializeField] public TileBase GroundTile { get; private set; }

    [field: Header("Macro Features")]
    [field: Tooltip("Список всех препятствий, которые могут тут появиться. Чем выше в списке, тем больше приоритет.")]
    [field: SerializeField] public ObstacleFeatureData[] Obstacles { get; private set; }
}