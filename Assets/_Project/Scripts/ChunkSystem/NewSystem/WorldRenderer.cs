using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldRenderer : MonoBehaviour
{
    [Header("Global Tilemaps")]
    [SerializeField] private Tilemap globalGroundTilemap;
    [SerializeField] private Tilemap globalObstaclesTilemap;

    public void DrawChunk(ChunkData chunkData, WorldMapData mapData, WorldGenerationConfig config, Vector2Int centerGridIndex)
    {
        int tileSize = config.ChunkSizeInTiles;
        int halfSize = tileSize / 2;

        Vector2Int gridIndex = chunkData.GridIndex;
        Vector2Int worldChunkCoord = gridIndex - centerGridIndex;

        int startArrayX = gridIndex.x * tileSize;
        int startArrayY = gridIndex.y * tileSize;

        int startWorldX = worldChunkCoord.x * tileSize - halfSize;
        int startWorldY = worldChunkCoord.y * tileSize - halfSize;

        for (int x = 0; x < tileSize; x++)
        {
            for (int y = 0; y < tileSize; y++)
            {
                int arrayX = startArrayX + x;
                int arrayY = startArrayY + y;
                Vector3Int worldPos = new Vector3Int(startWorldX + x, startWorldY + y, 0);

                // Отрисовка земли
                TileBase groundTile = mapData.GlobalGroundMap[arrayX, arrayY];
                if (groundTile != null) globalGroundTilemap.SetTile(worldPos, groundTile);

                // ПРОВЕРКА: Разрушено ли это препятствие игроком ранее?
                Vector2Int localPos = new Vector2Int(x, y);
                if (!chunkData.DestroyedObstacles.Contains(localPos))
                {
                    TileBase obstacleTile = mapData.GlobalObstaclesMap[arrayX, arrayY];
                    if (obstacleTile != null) globalObstaclesTilemap.SetTile(worldPos, obstacleTile);
                }
            }
        }

        // Запекание Спец-чанка
        if (chunkData.ActiveChunk is SpecialChunkData specialData && specialData.ChunkPrefab != null)
        {
            Vector3 worldCenter = globalGroundTilemap.GetCellCenterWorld(
                new Vector3Int(worldChunkCoord.x * tileSize, worldChunkCoord.y * tileSize, 0));

            SpecialChunkContainer container = Instantiate(specialData.ChunkPrefab, worldCenter, Quaternion.identity);

            for (int x = 0; x < tileSize; x++)
            {
                for (int y = 0; y < tileSize; y++)
                {
                    Vector3Int localOffset = new Vector3Int(x - halfSize, y - halfSize, 0);
                    Vector3Int globalPos = new Vector3Int(startWorldX + x, startWorldY + y, 0);

                    if (container.ProtectionMask != null && container.ProtectionMask.HasTile(localOffset))
                        globalObstaclesTilemap.SetTile(globalPos, null);

                    if (container.GroundTilemap != null)
                    {
                        TileBase ground = container.GroundTilemap.GetTile(localOffset);
                        if (ground != null) globalGroundTilemap.SetTile(globalPos, ground);
                    }

                    // Ручные препятствия тоже не восстанавливаются, если игрок их сломал
                    Vector2Int localGridPos = new Vector2Int(x, y);
                    if (!chunkData.DestroyedObstacles.Contains(localGridPos) && container.ObstaclesTilemap != null)
                    {
                        TileBase obs = container.ObstaclesTilemap.GetTile(localOffset);
                        if (obs != null) globalObstaclesTilemap.SetTile(globalPos, obs);
                    }
                }
            }

            if (container.EntitiesContainer != null) container.EntitiesContainer.SetParent(null);
            Destroy(container.gameObject);
        }
    }
}