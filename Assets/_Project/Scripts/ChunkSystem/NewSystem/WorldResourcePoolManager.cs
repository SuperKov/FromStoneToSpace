using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;

public class WorldResourcePoolManager : MonoBehaviour
{
    public static WorldResourcePoolManager Instance { get; private set; }

    private Dictionary<string, ObjectPool<GameObject>> _pools = new();
    private Dictionary<string, GameObject> _prefabs = new();

    private void Awake() => Instance = this;

    public void RegisterPrefab(string resourceID, GameObject prefab)
    {
        if (_pools.ContainsKey(resourceID)) return;

        _prefabs[resourceID] = prefab;

        _pools[resourceID] = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(_prefabs[resourceID], transform),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 50,
            maxSize: 500
        );
    }

    public GameObject GetResource(string resourceID, Vector3 position)
    {
        // Безопасная проверка наличия ключа перед обращением к пулу
        if (!_pools.TryGetValue(resourceID, out ObjectPool<GameObject> pool))
        {
            Debug.LogError($"[WorldResourcePoolManager] Пул для '{resourceID}' не найден! Убедитесь, что ресурс добавлен в конфиг биома.");
            return null;
        }

        GameObject obj = pool.Get();
        obj.transform.position = position;
        return obj;
    }

    public void ReturnToPool(string resourceID, GameObject obj)
    {
        if (_pools.TryGetValue(resourceID, out ObjectPool<GameObject> pool))
        {
            pool.Release(obj);
        }
    }
}