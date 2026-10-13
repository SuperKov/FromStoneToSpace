using UnityEngine;

public class SettingsPanel : MonoBehaviour
{
    public void TrySaveWorld()
    {
        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SaveGame(GameStateManager.TargetWorld.Header);
    }

    public void ReloadWorld()
    {
        SceneLoader.Instance.LoadScene("GameWorld");
    }

    public void Quit()
    {
        if (SaveLoadManager.Instance != null && GameStateManager.TargetWorld != null)
            SaveLoadManager.Instance.SaveGame(GameStateManager.TargetWorld.Header);

        SceneLoader.Instance.LoadScene("Menu");
    }

    public void QuitWithouttSave()
    {
        SceneLoader.Instance.LoadScene("Menu");
    }

    public void GenerateWorld()
    {
        if (ChunkManager.Instance != null)
            ChunkManager.Instance.InitializeWorld(1111);
    }
}