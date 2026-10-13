using UnityEngine.Tilemaps;
using UnityEngine;

public class SpecialChunkContainer : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap GroundTilemap;
    public Tilemap ObstaclesTilemap;

    [Tooltip("Невидимый слой. Если здесь есть тайл, генератор удалит любые реки и горы в этой точке.")]
    public Tilemap ProtectionMask;

    [Header("Content")]
    [Tooltip("Контейнер для сундуков, врагов и NPC. При запекании мира он будет отвязан от чанка.")]
    public Transform EntitiesContainer;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (GroundTilemap == null) return;

        Gizmos.DrawSphere(GroundTilemap.GetCellCenterWorld(new()), 0.2f);
    }
#endif
}