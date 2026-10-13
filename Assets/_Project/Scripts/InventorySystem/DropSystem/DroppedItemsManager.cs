using UnityEngine.Pool;
using UnityEngine;

public class DroppedItemsManager : MonoBehaviour
{
    public static DroppedItemsManager Instance { get; private set; }

    private DroppedItem _prefab;
    private ObjectPool<DroppedItem> _pool;

    private void Awake()
    {
        Instance = this;

        GameConfig config = Resources.Load<GameConfig>("GameConfig");
        if (config == null || config.DroppedItemPrefab == null)
        {
            Debug.LogError("Ошибка загрузки GameConfig или префаба для DroppedItemsManager!");
            return;
        }
        _prefab = config.DroppedItemPrefab;

        // Инициализация пула
        _pool = new ObjectPool<DroppedItem>(
            createFunc: () => Instantiate(_prefab, transform),
            actionOnGet: item => item.gameObject.SetActive(true),
            actionOnRelease: item => item.gameObject.SetActive(false),
            actionOnDestroy: item => Destroy(item.gameObject),
            collectionCheck: true, // Проверка на двойной возврат (только в Editor)
            defaultCapacity: 50,
            maxSize: 200 // Защита от переполнения памяти
        );
    }

    public void SpawnDroppedItem(ItemStack item, Vector2 position, bool fastSpawn = false)
    {
        // Берем объект из пула
        DroppedItem droppedItem = _pool.Get();

        // Передаем пулу метод для возврата объекта, когда он будет собран
        droppedItem.Init(item, position, OnDroppedItemRelease, fastSpawn);

        if (WorldSaveManager.Instance != null)
            WorldSaveManager.Instance.RegisterDroppedItem(droppedItem);
    }

    private void OnDroppedItemRelease(DroppedItem droppedItem)
    {
        _pool.Release(droppedItem);

        if (WorldSaveManager.Instance != null)
            WorldSaveManager.Instance.UnregisterDroppedItem(droppedItem);
    }
}