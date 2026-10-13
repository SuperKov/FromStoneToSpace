using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerBuildController : MonoBehaviour
{
    [SerializeField] private InventoryModel inventory;
    [SerializeField] private PlayerCombatController combat;

    [Header("References")]
    [SerializeField] private BuildingPreview preview;

    [Header("Colors")]
    [SerializeField] private Color validColor = new(0, 1, 0, 0.8f);
    [SerializeField] private Color invalidColor = new(1, 0, 0, 0.8f);

    private BuildingData _currentBuilding;
    private Vector2 _currentCenterPos;
    private bool _canBuildCurrently;

    public bool IsBuildingMode => _currentBuilding != null;

    private void OnEnable()
    {
        InputManager.Instance.Actions.Player.Attack.performed += OnBuild;
        InputManager.Instance.Actions.Player.SecondAttack.performed += OnCancel;
        InputManager.Instance.Actions.UI.Cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Actions.Player.Attack.performed -= OnBuild;
            InputManager.Instance.Actions.Player.SecondAttack.performed -= OnCancel;
            InputManager.Instance.Actions.UI.Cancel.performed -= OnCancel;
        }
    }

    // Вызывается из BuildingMenuPresenter при выборе здания
    public void EnterBuildMode(BuildingData building)
    {
        _currentBuilding = building;
        preview.SetData(building.PreviewSprite, building.GridSize);
        combat.SwitchAttackBlock(true);
    }

    public void ExitBuildMode()
    {
        _currentBuilding = null;
        preview.Hide();
        combat.SwitchAttackBlock(false);
    }

    private void Update()
    {
        if (!IsBuildingMode) return;

        UpdatePreviewState();
    }

    private void UpdatePreviewState()
    {
        Vector2 mousePos = CameraManager.Instance.GetMouseWorldPosition();

        // Обращаемся к менеджеру как к чистому калькулятору
        _currentCenterPos = BuildingManager.Instance.GetSnappedCenter(mousePos, _currentBuilding.GridSize);

        bool isPhysicalValid = BuildingManager.Instance.IsPlacementValid(_currentCenterPos, _currentBuilding.GridSize);
        bool hasResources = inventory.HasItems(_currentBuilding.BuildCost);

        _canBuildCurrently = isPhysicalValid && hasResources;

        preview.UpdateVisuals(_currentCenterPos, _canBuildCurrently, validColor, invalidColor);
    }

    private void OnBuild(InputAction.CallbackContext context)
    {
        if (!IsBuildingMode || !_canBuildCurrently) return;

        // Ресурсы успешно списаны -> Строим
        if (inventory.TryRemoveItems(_currentBuilding.BuildCost))
        {
            // Спавним, получаем PlayerBuilding напрямую
            PlayerBuilding building = Instantiate(_currentBuilding.Prefab, _currentCenterPos, Quaternion.identity);

            // Генерируем уникальный GUID для нового здания
            string newGuid = System.Guid.NewGuid().ToString();

            // Запускаем и ставим на учет
            building.Init(newGuid);

            WorldSaveManager.Instance.RegisterBuilding(building);
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (IsBuildingMode)
            ExitBuildMode();
    }

#if UNITY_EDITOR
    // Дебаг переехал сюда, так как координаты теперь хранятся у игрока
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !IsBuildingMode || BuildingManager.Instance == null) return;

        float cellSize = BuildingManager.Instance.CellSize;
        float margin = cellSize * 0.2f;
        Vector2 boxSize = new Vector2(
            _currentBuilding.GridSize.x * cellSize,
            _currentBuilding.GridSize.y * cellSize
        ) - new Vector2(margin, margin);

        Gizmos.color = _canBuildCurrently ? Color.green : Color.red;
        Gizmos.DrawWireCube(_currentCenterPos, boxSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_currentCenterPos, 0.05f);
    }
#endif
}