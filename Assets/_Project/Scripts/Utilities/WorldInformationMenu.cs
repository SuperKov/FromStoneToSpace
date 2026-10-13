using UnityEngine;
using TMPro;

public class WorldInformationMenu : MonoBehaviour
{
    [SerializeField] private GameObject menu;

    [Header("World")]
    [SerializeField] private TextMeshProUGUI worldName;
    [SerializeField] private TextMeshProUGUI worldSeed;
    [SerializeField] private TextMeshProUGUI lastSave;

    [Header("Player")]
    [SerializeField] private TextMeshProUGUI playerPos;
    [SerializeField] private TextMeshProUGUI currentChunk;
    [SerializeField] private TextMeshProUGUI currentBiome;

    [Header("Cursor")]
    [SerializeField] private TextMeshProUGUI cursorPos;
    [SerializeField] private TextMeshProUGUI cursorChunk;

    private PlayerEntity _player;

    private bool _showingInfo;

    private void Start()
    {
        _player = PlayerEntity.Instance;
    }

    private void LateUpdate()
    {
        if (!_showingInfo) return;

        // Инофрмаци об игроке
        if (_player != null)
        {
            if (ChunkManager.Instance != null)
            {
                currentBiome.text = "Текущий биом: " + ChunkManager.Instance.GetBiomeAtWorldPosition(_player.transform.position).ID;
                currentChunk.text = "Текущий чанк: " + ChunkManager.Instance.GetChunkDataAtWorldPosition(_player.transform.position).GridIndex.ToString();
            }
            else
            {
                currentBiome.text = "ERROR: ChunkManager отсутсвует на сцене!";
                currentChunk.text = "ERROR: ChunkManager отсутсвует на сцене!";
            }
            playerPos.text = "Позиция игрока: " + new Vector2(_player.transform.position.x, _player.transform.position.y).ToString();
        }
        else
        {
            playerPos.text = "ERROR: Игрока нет на сцене!";
            currentChunk.text = "ERROR: Игрока нет на сцене!";
            currentBiome.text = "ERROR: Игрока нет на сцене!";
        }

        // Информация курсора
        if (CameraManager.Instance != null)
        {
            Vector3 cursorPos = CameraManager.Instance.GetMouseWorldPosition();
            
            this.cursorPos.text = "Позиция Курсора: " + new Vector2(cursorPos.x, cursorPos.y).ToString();

            ChunkData chunk = ChunkManager.Instance.GetChunkDataAtWorldPosition(cursorPos);
            cursorChunk.text = "Чанк Курсора: ";
            if (chunk != null)
                cursorChunk.text += chunk.GridIndex.ToString();
            else
                cursorChunk.text += "NULL";
        }
    }

    public void ShowInfo()
    {
        if (GameStateManager.TargetWorld == null) return;

        worldName.text = "Имя мира: " + GameStateManager.TargetWorld.Header.Name;
        worldSeed.text = "Сид мира: " + GameStateManager.TargetWorld.Header.Seed.ToString();
        lastSave.text = "Последнее сохранение: " + GameStateManager.TargetWorld.Header.LastSavedDate;

        _showingInfo = true;

        menu.SetActive(true);
    }

    public void StopShowingInfo()
    {
        menu.SetActive(false);
        _showingInfo = false;
    }

    public void UnlockAllWorld()
    {
        if (ChunkManager.Instance != null)
            ChunkManager.Instance.UnlockAllChunks();
    }
}