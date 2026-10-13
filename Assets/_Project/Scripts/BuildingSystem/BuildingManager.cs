using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    private GameConfig _config;

    // Геттер для получения размера клетки, если кому-то понадобится (например для визуала сетки)
    public float CellSize => _config != null ? _config.GridCellSize : 0.5f;


    private void Awake()
    {
        Instance = this;

        _config = Resources.Load<GameConfig>("GameConfig");
        if (_config == null)
        {
            Debug.LogError("Ошибка загрузки GameConfig для BuildingManager!");
        }
    }

    // 1. Метод расчета центров
    public Vector2 GetSnappedCenter(Vector2 rawPosition, Vector2Int gridSize)
    {
        float snapX = SnapCoordinate(rawPosition.x, gridSize.x);
        float snapY = SnapCoordinate(rawPosition.y, gridSize.y);
        return new Vector2(snapX, snapY);
    }

    // 2. Метод проверки физики
    public bool IsPlacementValid(Vector2 centerPos, Vector2Int gridSize)
    {
        float margin = _config.GridCellSize * 0.2f;
        Vector2 boxSize = new Vector2(
            gridSize.x * _config.GridCellSize,
            gridSize.y * _config.GridCellSize
        ) - new Vector2(margin, margin);

        Collider2D hit = Physics2D.OverlapBox(centerPos, boxSize, 0f, _config.ObstacleLayer);
        return hit == null;
    }

    private float SnapCoordinate(float rawValue, int gridLength)
    {
        float cellSize = _config.GridCellSize;

        if (gridLength % 2 == 0)
            return Mathf.Round(rawValue / cellSize) * cellSize;
        else
            return Mathf.Floor(rawValue / cellSize) * cellSize + (cellSize / 2f);
    }
}