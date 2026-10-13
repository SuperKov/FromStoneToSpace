using UnityEngine.Tilemaps;
using UnityEngine;

public class WorldMapData
{
    public ChunkData[,] Chunks;
    public TileBase[,] GlobalGroundMap;
    public TileBase[,] GlobalObstaclesMap;
}