using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool loadOnStart = true;

    private void Start()
    {
        Instance = this;

        if (InputManager.Instance != null)
            InputManager.Instance.UnlockPlayerActions(fullUnlock: true);

        if (loadOnStart)
            OnGameLoaded();
    }

    private void OnGameLoaded()
    {
        if (GameStateManager.TargetWorld == null) return;

        if (string.IsNullOrEmpty(GameStateManager.TargetWorld.Header.Name))
            GameStateManager.TargetWorld.Header.Name = "Polygon";

        // ВАЖНО: Инициализируем мир только ОДИН раз здесь
        if (ChunkManager.Instance != null)
            ChunkManager.Instance.InitializeWorld(GameStateManager.TargetWorld.Header.Seed);

        if (SaveLoadManager.Instance != null)
        {
            if (!GameStateManager.IsNewWorld)
            {
                SaveLoadManager.Instance.LoadGame(GameStateManager.TargetWorld.Header.Name);
            }
            else
            {
                if (ChunkManager.Instance != null)
                    ChunkManager.Instance.UnlockChunk(ChunkManager.Instance.CenterGrid);

                SaveLoadManager.Instance.SaveGame(GameStateManager.TargetWorld.Header);
                GameStateManager.IsNewWorld = false;
            }
        }
    }
}