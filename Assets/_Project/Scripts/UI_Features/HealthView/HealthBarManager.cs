using UnityEngine.Pool;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance { get; private set; }

    [SerializeField] private HealthBarView healthBarPrefab;

    private ObjectPool<HealthBarView> _pool;

    private void Awake()
    {
        Instance = this;

        _pool = new ObjectPool<HealthBarView>(
            createFunc: () => Instantiate(healthBarPrefab, transform),
            actionOnGet: (bar) => bar.gameObject.SetActive(true),
            actionOnRelease: (bar) => bar.gameObject.SetActive(false),
            actionOnDestroy: (bar) => Destroy(bar.gameObject),
            defaultCapacity: 20,
            maxSize: 100);
    }

    public HealthBarView GetHealthBar(Transform targetPos, float health, float maxHealth)
    {
        HealthBarView bar = _pool.Get();

        bar.Init(targetPos, health, maxHealth, this);

        return bar;
    }

    public void ReturnToPool(HealthBarView bar)
    {
        _pool.Release(bar);
    }
}