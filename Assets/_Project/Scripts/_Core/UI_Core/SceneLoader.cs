using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class SceneLoader : Singleton<SceneLoader>
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInit() => _ = Instance;

    public event Action<string> SceneLoaded;

    private LoadingPanel _loadingPanel;

    protected override void Awake()
    {
        base.Awake();

        GameConfig c = Resources.Load<GameConfig>("GameConfig");

        if (c == null || c.LoadingPanelPrefab == null)
        {
            Debug.LogError("Ошибка загрузки GameConfig для SceneLoader!");
            return;
        }

        _loadingPanel = Instantiate(c.LoadingPanelPrefab, transform);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName)
    {
        if (_loadingPanel == null) return;
             
        _loadingPanel.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneLoaded?.Invoke(scene.name);
    }
}