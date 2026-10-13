using UnityEngine;

public class ChunkResource : MonoBehaviour, IChunkResource
{
    [Tooltip("Уникальный ID для системы сохранений (например: 'tree_oak', 'rock_iron')")]
    [field: SerializeField] public string ID { get; private set; } = "New Resource";

    private Vector2Int _currentTilePos;
    private string _currentId;
    private ChunkData _currentChunk;

    private bool _active;
    private bool _isApplicationQuitting; // Флаг безопасности

    public void Init(Vector2Int tilePos, string resourceID, ChunkData chunk)
    {
        _currentTilePos = tilePos;
        _currentId = resourceID;
        _currentChunk = chunk;

        _active = true;
    }

    // Встроенный метод Unity. Вызывается перед закрытием игры.
    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    private void OnDisable()
    {
        // Если объект не инициализирован пулом ИЛИ игра закрывается - ничего не делаем
        if (!_active || _isApplicationQuitting) return;

        // Если ChunkManager уже уничтожен (при смене сцены), тоже не лезем
        if (ChunkManager.Instance == null) return;

        ChunkManager.Instance.RemoveResourceFromWorld(_currentTilePos, _currentId, _currentChunk, gameObject);
        _active = false;
    }
}