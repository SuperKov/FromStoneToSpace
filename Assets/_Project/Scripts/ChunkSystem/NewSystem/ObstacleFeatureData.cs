using UnityEngine.Tilemaps;
using UnityEngine;
using NaughtyAttributes;

public enum ObstacleGenerationType
{
    ContinuousLayer, // Длинные реки, озера, скалы (Шум Перлина)
    ScatteredSingle  // Одиночные объекты: кактусы, редкие камни (Случайный шанс)
}

[CreateAssetMenu(fileName = "NewObstacleFeature", menuName = "Project/World/Obstacle Feature")]
public class ObstacleFeatureData : ScriptableObject
{
    [field: SerializeField] public TileBase ObstacleTile { get; private set; }
    [field: SerializeField] public ObstacleGenerationType GenType { get; private set; } = ObstacleGenerationType.ContinuousLayer;

    [field: Header("Continuous Settings (Rivers/Lakes)")]
    [field: Tooltip("Масштаб узора. Меньше значение - крупнее озера.")]
    [field: ShowIf(nameof(_isContinuousLayer))] [field: SerializeField] public float NoiseScale { get; private set; } = 0.15f;
    [field: Tooltip("Порог появления. 0.6 = вода занимает 40% территории, 0.8 = только 20%.")]
    [field: ShowIf(nameof(_isContinuousLayer))] [field: SerializeField, Range(0f, 1f)] public float NoiseThreshold { get; private set; } = 0.65f;

    [field: Header("Rarity Mask (For rare oasis/volcanos)")]
    [field: Tooltip("0 = спавнится везде по правилу выше. 0.8 = спавнится только в 20% зон биома.")]
    [field: SerializeField, Range(0f, 1f)] public float RarityMaskThreshold { get; private set; } = 0f;
    [field: SerializeField] public float RarityMaskScale { get; private set; } = 0.05f;

    [field: Header("Scattered Settings (Cacti/Boulders)")]
    [field: Tooltip("Шанс появления на любой клетке (0.05 = 5% шанс на тайл)")]
    [field: HideIf(nameof(_isContinuousLayer))] [field: SerializeField, Range(0f, 1f)] public float SpawnChancePerTile { get; private set; } = 0.05f;

    private bool _isContinuousLayer => GenType == ObstacleGenerationType.ContinuousLayer;
}