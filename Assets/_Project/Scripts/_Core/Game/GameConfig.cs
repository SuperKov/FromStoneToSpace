using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Project/GameConfig")]
public class GameConfig : ScriptableObject
{
    [field: Header("Item Spawning")]
    [field: SerializeField] public DroppedItem DroppedItemPrefab { get; private set; }

    [field: Header("UI")]
    [field: SerializeField] public LoadingPanel LoadingPanelPrefab { get; private set; }

    [field: Header("Building System")]
    [field: SerializeField] public float GridCellSize { get; private set; } = 0.5f;
    [field: SerializeField] public Vector2 GridCellOffset { get; private set; } = new(0.25f, 0.25f);
    [field: SerializeField] public LayerMask ObstacleLayer { get; private set; }

    [field: Header("World Grid Rules")]
    [field: Tooltip("Размер чанка в тайлах по ширине и высоте (например, 5x5)")]
    [field: SerializeField] public int ChunkSizeInTiles { get; private set; } = 5;
    [field: Tooltip("Размер мира в чанках. ВСЕГДА должно быть нечетным для точного центра!")]
    [field: SerializeField] public Vector2Int WorldSizeInChunks { get; private set; } = new(11, 11);

    [field: Header("Effects")]
    [field: SerializeField] public Material FlashMaterial { get; private set; }

    // Авто-коррекция: если кто то ввел четное число, исправляем на нечетное
    private void OnValidate()
    {
        if (ChunkSizeInTiles <= 0) ChunkSizeInTiles = 1;
        if (WorldSizeInChunks.x % 2 == 0) WorldSizeInChunks += new Vector2Int(1, 0);
        if (WorldSizeInChunks.y % 2 == 0) WorldSizeInChunks += new Vector2Int(0, 1);
    }
}