using UnityEngine;

public interface IChunkResource
{
    void Init(Vector2Int tilePos, string resourceID, ChunkData chunk);
}
