using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isQuitting = false;

    public static T Instance
    {
        get
        {
            if (_isQuitting) return null;

            if (_instance == null)
            {
                Init();
            }
            return _instance;
        }
    }

    public static void Init()
    {
        if (_instance != null) return;

        GameObject go = new (typeof(T).Name);
        _instance = go.AddComponent<T>();
        _isQuitting = false;

        DontDestroyOnLoad(go);
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        // Если объект удаляется вручную, тоже помечаем
        if (_instance == this)
            _isQuitting = true;
    }
}
