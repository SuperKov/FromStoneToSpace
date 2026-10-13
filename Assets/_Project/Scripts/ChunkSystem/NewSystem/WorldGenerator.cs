using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class WorldGenerator
{
    private readonly WorldGenerationConfig _config;
    private readonly int _seed;

    private readonly int _widthChunks;
    private readonly int _heightChunks;

    public WorldGenerator(WorldGenerationConfig config, int seed)
    {
        _config = config;
        _seed = seed;

        _widthChunks = config.WorldSizeInChunks.x;
        _heightChunks = config.WorldSizeInChunks.y;
    }

    public WorldMapData GenerateWorld()
    {
        WorldMapData worldData = new WorldMapData();

        BiomeData[,] biomeGrid = GenerateBiomeSkeletons();
        worldData.Chunks = BuildChunkData(biomeGrid);
        PlaceSpecialChunks(worldData.Chunks);

        worldData.GlobalGroundMap = GenerateGroundTiles(worldData.Chunks);
        worldData.GlobalObstaclesMap = GenerateMacroObstacles(worldData.Chunks);

        return worldData;
    }

    private BiomeData[,] GenerateBiomeSkeletons()
    {
        BiomeData[,] biomeGrid = new BiomeData[_widthChunks, _heightChunks];
        Vector2Int center = new Vector2Int(_widthChunks / 2, _heightChunks / 2);
        Queue<Vector2Int> growthQueue = new Queue<Vector2Int>();

        Random.InitState(_seed);

        if (_config.DefaultStarterBiome != null)
        {
            biomeGrid[center.x, center.y] = _config.DefaultStarterBiome;
            growthQueue.Enqueue(center);
        }

        foreach (var biome in _config.AllBiomes)
        {
            if (biome == _config.DefaultStarterBiome) continue;

            List<Vector2Int> validSpots = new List<Vector2Int>();
            for (int x = 0; x < _widthChunks; x++)
            {
                for (int y = 0; y < _heightChunks; y++)
                {
                    if (biomeGrid[x, y] != null) continue;
                    int dist = Mathf.RoundToInt(Vector2Int.Distance(new Vector2Int(x, y), center));
                    if (dist >= biome.MinRadius && dist <= biome.MaxRadius)
                        validSpots.Add(new Vector2Int(x, y));
                }
            }

            if (validSpots.Count > 0)
            {
                Vector2Int seedPos = validSpots[Random.Range(0, validSpots.Count)];
                biomeGrid[seedPos.x, seedPos.y] = biome;
                growthQueue.Enqueue(seedPos);
            }
        }

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (growthQueue.Count > 0)
        {
            Vector2Int current = growthQueue.Dequeue();
            BiomeData currentBiome = biomeGrid[current.x, current.y];

            ShuffleArray(dirs);

            foreach (var dir in dirs)
            {
                Vector2Int neighbor = current + dir;
                if (neighbor.x >= 0 && neighbor.x < _widthChunks && neighbor.y >= 0 && neighbor.y < _heightChunks)
                {
                    if (biomeGrid[neighbor.x, neighbor.y] == null)
                    {
                        biomeGrid[neighbor.x, neighbor.y] = currentBiome;
                        growthQueue.Enqueue(neighbor);
                    }
                }
            }
        }

        // Заполнение пустых дыр
        for (int x = 0; x < _widthChunks; x++)
            for (int y = 0; y < _heightChunks; y++)
                if (biomeGrid[x, y] == null)
                    biomeGrid[x, y] = _config.DefaultStarterBiome;

        return biomeGrid;
    }

    private ChunkData[,] BuildChunkData(BiomeData[,] biomeGrid)
    {
        ChunkData[,] chunks = new ChunkData[_widthChunks, _heightChunks];
        for (int x = 0; x < _widthChunks; x++)
        {
            for (int y = 0; y < _heightChunks; y++)
            {
                BiomeData biome = biomeGrid[x, y];
                int localSeed = _seed ^ (x * 73856093) ^ (y * 19349663);
                chunks[x, y] = new ChunkData(new Vector2Int(x, y), biome, biome.ProceduralChunk, localSeed);
            }
        }
        return chunks;
    }

    private void PlaceSpecialChunks(ChunkData[,] chunks)
    {
        foreach (var biome in _config.AllBiomes)
        {
            if (biome.SpecialChunks == null || biome.SpecialChunks.Length == 0) continue;

            Random.InitState(_seed ^ biome.ID.GetHashCode());

            foreach (var specialChunk in biome.SpecialChunks)
            {
                List<Vector2Int> candidates = new List<Vector2Int>();
                for (int x = 0; x < _widthChunks; x++)
                {
                    for (int y = 0; y < _heightChunks; y++)
                    {
                        if (x == _widthChunks / 2 && y == _heightChunks / 2) continue;
                        if (chunks[x, y].AssignedBiome == biome && chunks[x, y].ActiveChunk is ProceduralChunkData)
                            candidates.Add(new Vector2Int(x, y));
                    }
                }

                for (int i = 0; i < specialChunk.MaxPerBiome; i++)
                {
                    if (candidates.Count == 0) break;
                    int randomIndex = Random.Range(0, candidates.Count);
                    Vector2Int target = candidates[randomIndex];
                    chunks[target.x, target.y].SetChunk(specialChunk);
                    candidates.RemoveAt(randomIndex);
                }
            }
        }
    }

    private TileBase[,] GenerateGroundTiles(ChunkData[,] chunks)
    {
        int tileSize = _config.ChunkSizeInTiles;
        int widthTiles = _widthChunks * tileSize;
        int heightTiles = _heightChunks * tileSize;

        TileBase[,] groundMap = new TileBase[widthTiles, heightTiles];
        float warpOffset = _seed % 1000f;

        for (int x = 0; x < widthTiles; x++)
        {
            for (int y = 0; y < heightTiles; y++)
            {
                float distToEdgeX = Mathf.Min(x, widthTiles - 1 - x);
                float distToEdgeY = Mathf.Min(y, heightTiles - 1 - y);
                float minDistToEdge = Mathf.Min(distToEdgeX, distToEdgeY);

                if (minDistToEdge < _config.OceanFalloffDistance)
                {
                    float oceanChance = 1f - (minDistToEdge / _config.OceanFalloffDistance);
                    Random.InitState(_seed ^ (x * 123) ^ (y * 321));
                    if (Random.value < oceanChance)
                    {
                        groundMap[x, y] = _config.OceanTile;
                        continue;
                    }
                }

                float warpX = Mathf.PerlinNoise(x * _config.BiomeWarpScale + warpOffset, y * _config.BiomeWarpScale) * 2f - 1f;
                float warpY = Mathf.PerlinNoise(x * _config.BiomeWarpScale, y * _config.BiomeWarpScale + warpOffset) * 2f - 1f;

                int warpedX = Mathf.Clamp(x + Mathf.RoundToInt(warpX * _config.BiomeWarpStrength), 0, widthTiles - 1);
                int warpedY = Mathf.Clamp(y + Mathf.RoundToInt(warpY * _config.BiomeWarpStrength), 0, heightTiles - 1);

                ChunkData targetChunk = chunks[warpedX / tileSize, warpedY / tileSize];

                if (targetChunk.ActiveChunk is ProceduralChunkData proceduralData)
                    groundMap[x, y] = proceduralData.GroundTile;
                else
                    groundMap[x, y] = _config.DefaultStarterBiome.ProceduralChunk.GroundTile;
            }
        }

        return groundMap;
    }

    private TileBase[,] GenerateMacroObstacles(ChunkData[,] chunks)
    {
        int tileSize = _config.ChunkSizeInTiles;
        int widthTiles = _widthChunks * tileSize;
        int heightTiles = _heightChunks * tileSize;

        TileBase[,] obstaclesMap = new TileBase[widthTiles, heightTiles];

        for (int x = 0; x < widthTiles; x++)
        {
            for (int y = 0; y < heightTiles; y++)
            {
                int chunkX = x / tileSize;
                int chunkY = y / tileSize;
                ChunkData currentChunk = chunks[chunkX, chunkY];

                if (currentChunk.ActiveChunk is ProceduralChunkData proceduralData && proceduralData.Obstacles != null)
                {
                    foreach (var obstacle in proceduralData.Obstacles)
                    {
                        if (obstacle == null || obstacle.ObstacleTile == null) continue;

                        bool spawn = false;

                        if (obstacle.GenType == ObstacleGenerationType.ContinuousLayer)
                        {
                            float noiseOffset = (_seed ^ obstacle.name.GetHashCode()) % 1000f;
                            float rarityOffset = (_seed ^ (obstacle.name.GetHashCode() * 2)) % 1000f;

                            float rarityNoise = Mathf.PerlinNoise(x * obstacle.RarityMaskScale + rarityOffset, y * obstacle.RarityMaskScale + rarityOffset);
                            if (rarityNoise >= obstacle.RarityMaskThreshold)
                            {
                                float noiseValue = Mathf.PerlinNoise(x * obstacle.NoiseScale + noiseOffset, y * obstacle.NoiseScale + noiseOffset);
                                if (noiseValue > obstacle.NoiseThreshold) spawn = true;
                            }
                        }
                        else if (obstacle.GenType == ObstacleGenerationType.ScatteredSingle)
                        {
                            Random.InitState(_seed ^ (x * 73856) ^ (y * 19349) ^ obstacle.name.GetHashCode());
                            if (Random.value < obstacle.SpawnChancePerTile) spawn = true;
                        }

                        if (spawn)
                        {
                            obstaclesMap[x, y] = obstacle.ObstacleTile;
                            break;
                        }
                    }
                }
            }
        }

        return obstaclesMap;
    }

    private void ShuffleArray(Vector2Int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}